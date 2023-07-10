using UnityEngine;

namespace RTS
{
    public enum ETeam
    {
        Neutral = -1,
    	Blue = 0,
    	Red = 1
    }

    [RequireComponent(typeof(GameState))]
    public class GameServices : MonoBehaviour
    {
        [Header("Terrain")]
        [SerializeField, Tooltip("Unplayable terrain border size")]
        private float _nonPlayableBorder = 100f;
        [SerializeField, Tooltip("Playable bounds size if no terrain is found")]
        private float _defaultPlayableBoundsSize = 100f;
        
        [Header("Global")]
        [SerializeField, HideInInspector]
        private float _timeScale = 1f;

        private static GameServices _instance;

        private TeamController[] _controllersArray;
        private TargetBuilding[] _targetBuildingArray;
        private GameState _currentGameState;

        private Terrain _currentTerrain;
        private Bounds _playableBounds;

        public System.Action<float> TimeScaleChanged;

        public float TimeScale => _timeScale;

        #region Static methods
        public static GameServices GetGameServices()
        {
            return _instance;
        }
        public static GameState GetGameState()
        {
            return _instance._currentGameState;
        }
        public static TeamController GetControllerByTeam(ETeam team)
        {
            if (_instance._controllersArray.Length < (int)team)
                return null;
            return _instance._controllersArray[(int)team];
        }

        public static ETeam GetOpponent(ETeam team)
        {
            return _instance._currentGameState.GetOpponent(team);
        }

        public static TargetBuilding[] GetTargetBuildings() { return _instance._targetBuildingArray; }

        // return RGB color struct for each team
        public static Color GetTeamColor(ETeam team)
        {
            switch (team)
            {
                case ETeam.Blue:
                    return Color.blue;
                case ETeam.Red:
                    return Color.red;
                //case Team.Green:
                //    return Color.green;
                default:
                    return Color.grey;
            }
        }
        public static float GetNonPlayableBorder => _instance._nonPlayableBorder;
        public static Terrain GetTerrain => _instance._currentTerrain;
        
        public static Bounds GetPlayableBounds()
        {
            return _instance._playableBounds;
        }
        public static Vector3 GetTerrainSize()
        {
            return _instance.TerrainSize;
        }
        
        public static void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = 0.02f * timeScale;
            
            if (_instance)
                _instance.TimeScaleChanged?.Invoke(timeScale);
        }
        
        public static bool IsPosInPlayableBounds(Vector3 pos)
        {
            if (GetPlayableBounds().Contains(pos))
                return true;

            return false;
        }
        public Vector3 TerrainSize
        {
            get
            {
                if (_currentTerrain)
                    return _currentTerrain.terrainData.bounds.size;
                return new Vector3(_defaultPlayableBoundsSize, 10.0f, _defaultPlayableBoundsSize);
            }
        }

        #endregion

        #region MonoBehaviour methods
        void Awake()
        {
            _instance = this;

            // Retrieve controllers from scene for each team
            _controllersArray = new TeamController[2];
            foreach (TeamController controller in FindObjectsOfType<TeamController>())
            {
                _controllersArray[(int)controller.GetTeam()] = controller;
            }

            // Store TargetBuildings
            _targetBuildingArray = FindObjectsOfType<TargetBuilding>();

            // Store GameState ref
            if (_currentGameState == null)
                _currentGameState = GetComponent<GameState>();

            // Assign first found terrain
            foreach (Terrain terrain in FindObjectsOfType<Terrain>())
            {
                _currentTerrain = terrain;
                //Debug.Log("terrainData " + _currentTerrain.terrainData.bounds.ToString());
                break;
            }

            if (_currentTerrain)
            {
                _playableBounds = _currentTerrain.terrainData.bounds;
                Vector3 clampedOne = new Vector3(1f, 0f, 1f);
                Vector3 heightReduction = Vector3.up * 0.1f; // $$ hack : this is to prevent selection / building in high areas
                _playableBounds.SetMinMax(_playableBounds.min + clampedOne * _nonPlayableBorder / 2f, _playableBounds.max - clampedOne * _nonPlayableBorder / 2f - heightReduction);
            }
            else
            {
                Debug.LogWarning("could not find terrain asset in scene, setting default PlayableBounds");
                Vector3 clampedOne = new Vector3(1f, 0f, 1f);
                _playableBounds.SetMinMax(new Vector3(-_defaultPlayableBoundsSize, -10.0f, -_defaultPlayableBoundsSize) + clampedOne * _nonPlayableBorder / 2f,
                                            new Vector3(_defaultPlayableBoundsSize, 10.0f, _defaultPlayableBoundsSize) - clampedOne * _nonPlayableBorder / 2f);
            }
            
            SetTimeScale(_timeScale);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                _timeScale = Mathf.Clamp(_timeScale + 1, 1, 10);
                SetTimeScale(_timeScale);
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _timeScale = Mathf.Clamp(_timeScale - 1, 1, 10);
                SetTimeScale(_timeScale);
            }

        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_playableBounds.center, _playableBounds.size);
        }
        #endregion
    }
}