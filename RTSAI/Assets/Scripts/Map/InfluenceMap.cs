using System;
using RTS.Extensions;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RTS.AI.Tools
{
    public class InfluenceMapNode
    {
        public readonly int[] Scores = new int[EnumExtension.GetCount<ETeam>()];

        public void Reset()
        {
            for (int i = 0; i < Scores.Length; i++)
                Scores[i] = 0;
        }
        
        public void AddScore(int score, ETeam team)
        {
            Scores[(int)team + 1] += score;
        }

        public int GetScore(ETeam team)
        {
            return Scores[(int)team + 1];
        }
    }
    
    public class InfluenceMap : Singleton<InfluenceMap>
    {
        [Header("Update")]
        [SerializeField] private bool _shouldAutoUpdate;
        [SerializeField] private float _updateFrequency = 0.1f;
        
        [Header("Details")]
        [SerializeField] private float _size = 20f;
        [SerializeField] private int _resolution = 10;
        [SerializeField] private bool _drawGrid;
        [SerializeField, Tooltip("Only for play mode")] 
        private bool _drawInfluence;

        [Header("Propagation")] 
        [SerializeField] private int _maxCellScore = -1;
        [SerializeField] private int _scoreThreshold = 20;
        [SerializeField] private DropOffType _dropOffType = DropOffType.Linear;

        public event Action InfluenceMapUpdated;

        private bool _isRunning;
        private float _updateTimer;

        private float _cellSize;
        private Vector3 _downLeftPos;

        private InfluenceMapNode[] _graph;
        private readonly List<Unit> _influenceSources = new List<Unit>();
        
        private int[] _tempTeamGlobalInfluences;
        private int[] _teamGlobalInfluences = new int[EnumExtension.GetCount<ETeam>()];

        public InfluenceMapNode[] Graph => _graph;
        public int MaxCellScore => _maxCellScore;

        private enum DropOffType
        {
            Linear,
            Square,
            SquareRoot
        }
        
        #region Unity Callbacks
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            
            CreateGraph();

            float halfSize = _size * 0.5f;
            _downLeftPos = transform.position + Vector3.left * halfSize - Vector3.forward * halfSize;
            _cellSize = _size / _resolution;

            _isRunning = true;

            if (_shouldAutoUpdate)
                AutoUpdateMap().Forget();
        }

        private async UniTask AutoUpdateMap()
        {
            while (_isRunning)
            {
                ComputeMap();
                await UniTask.WhenAny(UniTask.Delay(TimeSpan.FromSeconds(_updateFrequency)),
                    UniTask.WaitUntil(() => !_isRunning));
            }
        }

        private void OnDestroy()
        {
            _isRunning = false;
        }
        #endregion


        #region Graph
        private void CreateGraph()
        {
            _graph = new InfluenceMapNode[_resolution * _resolution];

            for (int i = 0; i < _resolution * _resolution; i++)
                _graph[i] = new InfluenceMapNode();
        }

        private Vector3 GetPositionFromGridCoords(Vector2Int coords)
        {
            float halfCell = _cellSize * 0.5f;
            float xPos = _downLeftPos.x + coords.x * _cellSize + halfCell;
            float zPos = _downLeftPos.z + coords.y * _cellSize + halfCell;

            return new Vector3(xPos, 0f, zPos);
        }
        
        private Vector2Int GetGridCoordsFromPosition(Vector3 pos)
        {
            int xCoords = Mathf.FloorToInt((pos.x - _downLeftPos.x) / _cellSize);
            int zCoords = Mathf.FloorToInt((pos.z - _downLeftPos.z) / _cellSize);
        
            xCoords = Mathf.Clamp(xCoords, 0, _resolution - 1);
            zCoords = Mathf.Clamp(zCoords, 0, _resolution - 1);
        
            return new Vector2Int(xCoords, zCoords);
        }

        private InfluenceMapNode GetNodeAt(Vector2Int coords)
        {
            return GetNodeAt(coords.x, coords.y);
        }

        private InfluenceMapNode GetNodeAt(Vector3 pos)
        {
            Vector2Int coords = GetGridCoordsFromPosition(pos);
            return GetNodeAt(coords.x, coords.y);
        }
        
        private InfluenceMapNode GetNodeAt(int x, int y)
        {
            int nodeIndex = x + y * _resolution;
            if (nodeIndex < 0 || nodeIndex >= _graph.Length)
                return null;

            return _graph[nodeIndex];
        }
        
        private bool IsPointOnMap(Vector3 pos)
        {
            return pos.x.Between(_downLeftPos.x, _downLeftPos.x + _size) &&
                   pos.z.Between(_downLeftPos.z, _downLeftPos.z + _size);
        }

        public InfluenceMapNode GetNodeFromPercentage(float widthPercent, float heightPercent)
        {
            widthPercent = Mathf.Clamp(widthPercent, 0f, 1f);
            heightPercent = Mathf.Clamp(heightPercent, 0f, 1f);

            Vector3 widthOffset = Vector3.right * widthPercent * _size;
            Vector3 heightOffset = Vector3.forward * heightPercent * _size;

            return GetNodeAt(_downLeftPos + widthOffset + heightOffset);
        }
        
        public void ComputeMap()
        {
            // Reset the graph nodes to their initial state
            foreach (InfluenceMapNode node in _graph)
                node.Reset();

            _tempTeamGlobalInfluences = new int[EnumExtension.GetCount<ETeam>()];

            Unit[] copyInfluenceSources = _influenceSources.ToArray();
            foreach (Unit influenceSource in copyInfluenceSources)
            {
                Vector3 sourcePosition = influenceSource.transform.position;
                if (IsPointOnMap(sourcePosition))
                {
                    Vector2Int coords = GetGridCoordsFromPosition(sourcePosition);
                    ApplyInfluenceAndPropagate(influenceSource, coords);
                }
            }

            _teamGlobalInfluences = _tempTeamGlobalInfluences;
            _tempTeamGlobalInfluences = null;
            
            InfluenceMapUpdated?.Invoke();
        }
        #endregion
        
        
        #region Influence
        public float GetTeamInfluencePercentage(ETeam team)
        {
            int teamInfluence = _teamGlobalInfluences[(int)team + 1];
            int totalInfluence = _teamGlobalInfluences.Sum();

            return totalInfluence != 0 ? teamInfluence / (float)totalInfluence : 0f;
        }

        public float GetTeamPossessionPercentage(ETeam team)
        {
            int teamInfluence = _teamGlobalInfluences[(int)team + 1];
            int graphTotalInfluence = _resolution * _resolution * _maxCellScore;

            return graphTotalInfluence != 0 ? teamInfluence / (float)graphTotalInfluence : 0f;
        }

        public void AddInfluenceSource(Unit influenceSource)
        {
            if (_influenceSources.Contains(influenceSource))
                return;
            
            _influenceSources.Add(influenceSource);
        }

        public void RemoveInfluenceSource(Unit influenceSource)
        {
            if (!_influenceSources.Contains(influenceSource))
                return;

            _influenceSources.Remove(influenceSource);
        }

        private void AddNodeScoreAtCoords(int coordX, int coordY, int score, ETeam team)
        {
            InfluenceMapNode node = GetNodeAt(coordX, coordY);
            int nodeScore = node.GetScore(team);

            int newScore = score;
            int scoreThreshold = _maxCellScore - nodeScore;

            if (newScore > scoreThreshold)
                newScore = scoreThreshold;
            
            _tempTeamGlobalInfluences[(int)team + 1] += newScore;
            node.AddScore(newScore, team);
        }

        private void ApplyInfluenceAndPropagate(Unit source, Vector2Int coords)
        {
            ETeam sourceTeam = source.GetTeam();

            // Clamp influencer score to a max cell score
            int sourceScore = source.GetUnitData.Influence;
            AddNodeScoreAtCoords(coords.x, coords.y, sourceScore, sourceTeam);

            int lowRes = _resolution - 1;
            for (int radius = 1; radius <= source.GetUnitData.InfluenceRadius; radius++)
            {                
                int newScore = CalculateInfluenceDropOff(sourceScore, radius);

                for (int i = -radius; i <= radius; i++)
                {
                    int coordX = coords.x + i;
                    int coordY = coords.y + radius;
                    if (coordY >= 0 && coordY <= lowRes)
                    {
                        if (coordX >= 0 && coordX <= lowRes)
                            AddNodeScoreAtCoords(coordX, coordY, newScore, sourceTeam);
                    }

                    coordY = coords.y - radius;
                    if (coordY >= 0 && coordY <= lowRes)
                    {
                        if (coordX >= 0 && coordX <= lowRes)
                            AddNodeScoreAtCoords(coordX, coordY, newScore, sourceTeam);
                    }
                }
                
                for (int i = -radius + 1; i <= radius - 1; i++)
                {
                    int coordY = coords.y + i;
                    int coordX = coords.x + radius;
                    if (coordX >= 0 && coordX <= lowRes)
                    {
                        if (coordY >= 0 && coordY <= lowRes)
                            AddNodeScoreAtCoords(coordX, coordY, newScore, sourceTeam);
                    }

                    coordX = coords.x - radius;
                    if (coordX >= 0 && coordX <= lowRes)
                    {
                        if (coordY >= 0 && coordY <= lowRes)
                            AddNodeScoreAtCoords(coordX, coordY, newScore, sourceTeam);
                    }
                }

                if (newScore <= _scoreThreshold)
                    break;
            }
        }
        
        private int CalculateInfluenceDropOff(int influence, int distance)
        {
            float threshold = 1f + distance;
            switch (_dropOffType)
            {
                default:
                case DropOffType.Linear:
                    return Mathf.RoundToInt(influence / threshold);
                case DropOffType.Square:
                    return Mathf.RoundToInt(influence / (threshold * threshold));
                case DropOffType.SquareRoot:
                    return Mathf.RoundToInt(influence / Mathf.Sqrt(threshold));
            }
        }
        #endregion
        

        #region Draw
        private void OnDrawGizmos()
        {
            if (_drawGrid)
            {
                Gizmos.color = Color.black;

                float halfSize = _size * 0.5f;
                Vector3 downLeftPos = transform.position + Vector3.left * halfSize - Vector3.forward * halfSize;
                float cellSize = _size / _resolution;

                for (int z = 0; z <= _resolution; z++)
                {
                    Vector3 startPos = downLeftPos + Vector3.forward * cellSize * z;
                    Vector3 endPos = startPos + Vector3.right * _size;
                    Gizmos.DrawLine(startPos, endPos);
                }
            
                for (int x = 0; x <= _resolution; x++)
                {
                    Vector3 startPos = downLeftPos + Vector3.right * cellSize * x;
                    Vector3 endPos = startPos + Vector3.forward * _size;
                    Gizmos.DrawLine(startPos, endPos);
                }
            }

            if (_drawInfluence)
            {
                if (_graph == null || _graph.Length == 0 || _graph[0] == null)
                    return;
                
                float halfSize = _size * 0.5f;
                Vector3 downLeftPos = transform.position + Vector3.left * halfSize - Vector3.forward * halfSize;
                float cellSize = _size / _resolution;

                for (int z = 0; z < _resolution; z++)
                {
                    for (int x = 0; x < _resolution; x++)
                    {
                        int blueScore = _graph[x + z * _resolution].Scores[(int)ETeam.Blue];
                        int redScore = _graph[x + z * _resolution].Scores[(int)ETeam.Red];
                    
                        if (blueScore == 0 && redScore == 0)
                            continue;

                        float xPos = downLeftPos.x + cellSize * 0.5f + x * cellSize;
                        float zPos = downLeftPos.z + cellSize * 0.5f + z * cellSize;
                        Vector3 pos = new Vector3(xPos, transform.position.y, zPos);

                        Color blueColor = Color.blue * ((blueScore - _scoreThreshold) / (float)_maxCellScore);
                        Color redColor = Color.red * ((redScore - _scoreThreshold) / (float)_maxCellScore);

                        Gizmos.color = blueColor + redColor;
                        Debug.Log(blueColor + redColor);
                        Gizmos.DrawCube(pos + Vector3.up * 0.2f, new Vector3(cellSize, 0.2f, cellSize));
                    }
                }
            }
        }
        #endregion
    }
}