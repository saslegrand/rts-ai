using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RTS
{
    using FogOfWar;
    using System.Collections.Generic;

    public class TargetBuilding : MonoBehaviour
    {
        [SerializeField] private float _captureGaugeStart = 100f;
        [SerializeField] private float _captureGaugeSpeed = 1f;
        [SerializeField] private int _buildPointsPerTick = 5;
        [SerializeField] private float _delayBetweenTick = 5;
        private float _tickTimer;
        [SerializeField] private GameObject _blueFlag;
        [SerializeField] private GameObject _redFlag;
        [SerializeField] private TMP_Text[] _teamCounts;

        private Image _gaugeImage;
        private Image _minimapImage;

        public float PhysicalRadius { get; protected set; }

        private int[] _teamScore;
        private float _captureGaugeValue;
        private ETeam _owningTeam = ETeam.Neutral;
        private ETeam _capturingTeam = ETeam.Neutral;
        private List<Unit> _unitsCapturing;
        public ETeam GetTeam() { return _owningTeam; }

        private EntityVisibility _visibility;
        public Action OnCaptured;

        public EntityVisibility Visibility
        {
            get
            {
                if (_visibility == null)
                {
                    _visibility = GetComponent<EntityVisibility>();
                }
                return _visibility;
            }
        }


        #region MonoBehaviour methods
        void Start()
        {
            _unitsCapturing = new List<Unit>();
            GetComponentInChildren<MeshRenderer>();

            if (TryGetComponent(out Collider col))
            {
                CapsuleCollider caps = col as CapsuleCollider;
                SphereCollider sphere = col as SphereCollider;
                BoxCollider box = col as BoxCollider;
                if (caps != null)
                    PhysicalRadius = caps.radius;
                else if (sphere != null)
                    PhysicalRadius = sphere.radius;
                else if (box != null)
                {
                    Vector3 boxSize = box.size;
                    PhysicalRadius = Mathf.Max(boxSize.x, boxSize.z) * 0.5f;
                }
            }

            _gaugeImage = GetComponentInChildren<Image>();
            if (_gaugeImage)
                _gaugeImage.fillAmount = 0f;
            _captureGaugeValue = _captureGaugeStart;
            _teamScore = new int[2];
            _teamScore[0] = 0;
            _teamScore[1] = 0;

            Transform minimapTransform = transform.Find("MinimapCanvas");
            if (minimapTransform != null)
                _minimapImage = minimapTransform.GetComponentInChildren<Image>();
        }
        void Update()
        {
            UpdateCapturePointsOverTime();
            UpdateCaptureFill();
        }

        private void UpdateCapturePointsOverTime()
        {
            if (_owningTeam == ETeam.Neutral)
                return;

            if (_tickTimer < _delayBetweenTick)
            {
                _tickTimer += Time.deltaTime;
                return;
            }

            _tickTimer -= _delayBetweenTick;

            TeamController teamController = GameServices.GetControllerByTeam(_owningTeam);
            if (teamController != null)
                teamController.GainPoints(_buildPointsPerTick);

        }

        private void UpdateCaptureFill()
        {
            if (_capturingTeam == _owningTeam || _capturingTeam == ETeam.Neutral)
                return;

            if (_teamScore[(int)GameServices.GetOpponent(_capturingTeam)] > 0)
                return;

            _captureGaugeValue -= _teamScore[(int)_capturingTeam] * _captureGaugeSpeed * Time.deltaTime;

            _gaugeImage.fillAmount = 1f - _captureGaugeValue / _captureGaugeStart;

            if (_captureGaugeValue <= 0f)
            {
                _captureGaugeValue = 0f;
                Captured(_capturingTeam);
            }
        }

        #endregion

        public Vector3 GetPhysicalPosition(Vector3 other)
        {
            Vector3 pos = transform.position;
            Vector3 dir = (other - pos).normalized;
            return pos + dir * (PhysicalRadius * 0.9f);
        }


        private void OnTriggerEnter(Collider other)
        {
            Unit unit = other.GetComponent<Unit>();

            if (unit == null)
                return;

            if (!unit.IsAlive)
                return;

            RegisterUnit(unit);
            unit.OnUnitDeadEvent += UnregisterUnit;
        }

        private void OnTriggerExit(Collider other)
        {
            Unit unit = other.GetComponent<Unit>();

            if (unit == null)
                return;

            if (!unit.IsAlive)
                return;

            unit.OnUnitDeadEvent -= UnregisterUnit;
            UnregisterUnit(unit);
        }

        private void RegisterUnit(Unit unit)
        {
            if (_unitsCapturing.Contains(unit))
                return;

            OnCaptured += () => unit.StopCapture();
            _unitsCapturing.Add(unit);
            AddCaptureForce(unit.GetUnitData.Cost, unit.GetTeam());
        }

        private void UnregisterUnit(Unit unit)
        {
            if (!_unitsCapturing.Contains(unit))
                return;

            OnCaptured -= () => unit.StopCapture();
            _unitsCapturing.Remove(unit);
            AddCaptureForce(-unit.GetUnitData.Cost, unit.GetTeam());
        }

        public void AddCaptureForce(int addValue, ETeam team)
        {
            int i = (int)team;
            ETeam opponent = GameServices.GetOpponent(team);
            int o = (int)opponent;

            _teamScore[i] += addValue;
            _teamCounts[i].text = _teamScore[i].ToString();
            _teamCounts[i].color = GameServices.GetTeamColor(team);
            _teamCounts[o].color = GameServices.GetTeamColor(opponent);

            if (_teamScore[i] > 0 && _teamScore[o] <= 0 && _capturingTeam != team && _owningTeam != team)
            {
                StartCapturing(team);
            }
            else if (_teamScore[o] > 0 && _teamScore[i] <= 0 && _capturingTeam != opponent && _owningTeam != team)
            {
                StartCapturing(opponent);
            }
            else if (_teamScore[i] <= 0 && _teamScore[o] <= 0)
            {
                ResetCapture();
            }

        }

        public void StartCapturing(ETeam team)
        {
            ResetCapture();
            _capturingTeam = team;
            _gaugeImage.color = GameServices.GetTeamColor(_capturingTeam);
        }

        /*
        #region Capture methods
        public void StartCapture(Unit unit)
        {
            if (unit == null)
                return;

            
            ETeam opponent = GameServices.GetOpponent(unit.GetTeam());

            if (_capturingTeam == ETeam.Neutral)
            {
                if (_teamScore[(int)opponent] == 0)
                {
                    _capturingTeam = unit.GetTeam();
                    _gaugeImage.color = GameServices.GetTeamColor(_capturingTeam);
                }
            }
            else
            {
                if (_owningTeam == opponent && _teamScore[(int)opponent] <= 0 && _teamScore[(int)unit.GetTeam()] <= 0)
                {
                    ResetCapture();
                    _capturingTeam = unit.GetTeam();
                    _gaugeImage.color = GameServices.GetTeamColor(_capturingTeam);
                }
            }

            _teamScore[(int)unit.GetTeam()] += unit.Cost;
        }
        public void StopCapture(Unit unit)
        {
            if (unit == null)
                return;

            _teamScore[(int)unit.GetTeam()] -= unit.Cost;
            if (_teamScore[(int)unit.GetTeam()] == 0)
            {
                ETeam opponentTeam = GameServices.GetOpponent(unit.GetTeam());
                if (_teamScore[(int)opponentTeam] == 0)
                {
                    ResetCapture();
                }
                else
                {
                    _capturingTeam = opponentTeam;
                    _gaugeImage.color = GameServices.GetTeamColor(_capturingTeam);
                }
            }
        }
        #endregion
        */
        void ResetCapture()
        {
            _captureGaugeValue = _captureGaugeStart;
            _capturingTeam = ETeam.Neutral;
            _gaugeImage.fillAmount = 0f;
        }
        
        private void Captured(ETeam newTeam)
        {
            Debug.Log("target captured by " + newTeam);

            _tickTimer = 0.0f;

            ResetCapture();
            _owningTeam = newTeam;
            if (Visibility) { Visibility.Team = _owningTeam; }
            if (_minimapImage) { _minimapImage.color = GameServices.GetTeamColor(_owningTeam); }

            bool isBlueTeam = newTeam == ETeam.Blue;

            _blueFlag.SetActive(isBlueTeam);
            _redFlag.SetActive(!isBlueTeam);

            OnCaptured?.Invoke();
        }
        
        
    }
}