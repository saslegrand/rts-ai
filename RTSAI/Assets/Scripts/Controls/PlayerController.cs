using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using RTS.Cameras;
using RTS.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTS
{
    public sealed class PlayerController : TeamController
    {
        private enum InputMode
        {
            Orders,
            FactoryPositioning
        }

        [SerializeField] private GameObject _targetCursorPrefab;
        [SerializeField] private float _targetCursorFloorOffset = 0.2f;
        [SerializeField] private EventSystem _sceneEventSystem;

        [SerializeField] private GameCamera _gameCameraRef;

        [SerializeField, Range(0f, 1f)]
        private float _factoryPreviewTransparency = 0.3f;

        // Build Menu UI
        [SerializeField] private MenuController _playerMenuController;
        [SerializeField] private Collider _underMapCollider;

        private PointerEventData _menuPointerEventData;

        // Selection
        private Vector3 _selectionStart = Vector3.zero;
        private Vector3 _selectionEnd = Vector3.zero;
        private bool _selectionStarted;
        private LineRenderer _selectionLineRenderer;
        private GameObject _targetCursor;

        // Factory build
        private InputMode _currentInputMode = InputMode.Orders;
        private int _wantedFactoryId;
        private GameObject _wantedFactoryPreview;
        private Shader _previewShader;

        // Mouse events
        private Action _onMouseLeftPressed;
        private Action _onMouseLeft;
        private Action _onMouseLeftReleased;
        private Action _onMouseRightPressed;
        private Action _onUnitActionStart;
        private Action _onUnitActionEnd;
        private Action _onCameraDragMoveStart;
        private Action _onCameraDragMoveEnd;

        private Action<Vector3> _onFactoryPositioned;
        private Action<float> _onCameraZoom;
        private Action<float> _onCameraMoveHorizontal;
        private Action<float> _onCameraMoveVertical;
        private Action<int> _onUseSquadAtIndex;

        // Keyboard events
        private Action _onFocusBasePressed;
        private Action _onCancelBuildPressed;
        private Action _onDestroyEntityPressed;
        private Action _onCancelFactoryPositioning;
        private Action _onSelectAllPressed;
        private Action[] _onCategoryPressed;


        private bool _isShiftBtPressed;
        private bool _isCtrlBtPressed;

        // Constants
        private const float SELECTION_BOX_HEIGHT = 50f;

        GameObject GetTargetCursor()
        {
            if (_targetCursor == null)
            {
                _targetCursor = Instantiate(_targetCursorPrefab);
                _targetCursor.name = _targetCursor.name.Replace("(Clone)", "");
            }
            return _targetCursor;
        }
        void SetTargetCursorPosition(Vector3 pos)
        {
            SetTargetCursorVisible(true);
            pos.y += _targetCursorFloorOffset;
            GetTargetCursor().transform.position = pos;
        }
        void SetTargetCursorVisible(bool isVisible)
        {
            GetTargetCursor().SetActive(isVisible);
        }
        void SetCameraFocusOnMainFactory()
        {
            if (FactoryList.Count > 0)
                _gameCameraRef.FocusEntity(FactoryList[0]);
        }
        void CancelCurrentBuild()
        {
            if (_selectedFactory)
                _selectedFactory.CancelCurrentBuild();
        }

        #region MonoBehaviour methods
        protected override void Awake()
        {
            base.Awake();

            _armyController = new PlayerArmyController();

            _playerMenuController.Controller = GetComponent<TeamController>();

            _onBuildPointsUpdated += _playerMenuController.UpdateBuildPointsUI;
            _onCaptureTarget += _playerMenuController.UpdateCapturedTargetsUI;

            _selectionLineRenderer = GetComponent<LineRenderer>();

            if (_sceneEventSystem == null)
            {
                Debug.LogWarning("EventSystem not assigned in PlayerController, searching in current scene...");
                _sceneEventSystem = FindObjectOfType<EventSystem>();
            }
            // Set up the new Pointer Event
            _menuPointerEventData = new PointerEventData(_sceneEventSystem);
        }

        protected override void Start()
        {
            base.Start();

            _previewShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

            // left click : selection
            _onMouseLeftPressed += StartSelection;
            _onMouseLeft += UpdateSelection;
            _onMouseLeftReleased += EndSelection;

            _onMouseRightPressed += StartAction;

            // right click : Unit actions (move / attack / capture ...)
            _onUnitActionEnd += ComputeUnitsAction;

            _onCameraZoom += _gameCameraRef.Zoom;
            _onCameraMoveHorizontal += _gameCameraRef.KeyboardMoveHorizontal;
            _onCameraMoveVertical += _gameCameraRef.KeyboardMoveVertical;

            // Gameplay shortcuts
            _onFocusBasePressed += SetCameraFocusOnMainFactory;
            _onCancelBuildPressed += CancelCurrentBuild;

            _onCancelFactoryPositioning += ExitFactoryBuildMode;

            _onFactoryPositioned += (floorPos) =>
            {
                if (RequestFactoryBuild(_wantedFactoryId, floorPos))
                {
                    ExitFactoryBuildMode();
                }
            };

            // Destroy selected unit command
            _onDestroyEntityPressed += () =>
            {
                if (_armyController.CurrentSelectedSquad == null)
                    return;

                Unit[] unitsToBeDestroyed = _armyController.CurrentSelectedSquad.GetAllUnits().ToArray();
                foreach (Unit unit in unitsToBeDestroyed)
                {
                    ((IDamageable)unit).Destroy();
                }

                if (_selectedFactory)
                {
                    Factory factoryRef = _selectedFactory;
                    UnselectCurrentFactory();
                    factoryRef.Destroy();
                }
            };

            // Selection shortcuts
            _onSelectAllPressed += SelectAllUnits;

            _onCategoryPressed = new Action[9];
            for (int i = 0; i < _onCategoryPressed.Length; i++)
            {
                // store typeId value for event closure
                int typeId = i;
                _onCategoryPressed[i] += () =>
                {
                    SelectAllUnitsByTypeId(typeId);
                };
            }

            _onUseSquadAtIndex += UseSquadAtIndex;
        }
        protected override void Update()
        {
            _isShiftBtPressed = Input.GetKey(KeyCode.LeftShift);
            _isCtrlBtPressed = Input.GetKey(KeyCode.LeftControl);

            switch (_currentInputMode)
            {
                case InputMode.FactoryPositioning:
                    UpdateFactoryPositioningInput();
                    break;
                case InputMode.Orders:
                    UpdateSelectionInput();
                    UpdateActionInput();
                    break;
            }

            UpdateCameraInput();
        }
        #endregion

        #region Update methods
        void UpdateFactoryPositioningInput()
        {
            Vector3 floorPos = ProjectFactoryPreviewOnFloor();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _onCancelFactoryPositioning?.Invoke();
            }
            if (Input.GetMouseButtonDown(0))
            {
                _onFactoryPositioned?.Invoke(floorPos);
            }
        }
        void UpdateSelectionInput()
        {
            // Update keyboard inputs

            if (Input.GetKeyDown(KeyCode.A))
                _onSelectAllPressed?.Invoke();

            for (int i = 0; i < _onCategoryPressed.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Keypad1 + i))// || Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    _onCategoryPressed[i]?.Invoke();
                    break;
                }
            }

            // Update mouse inputs
#if UNITY_EDITOR
            if (EditorWindow.focusedWindow != EditorWindow.mouseOverWindow)
                return;
#endif
            if (Input.GetMouseButtonDown(0))
                _onMouseLeftPressed?.Invoke();
            if (Input.GetMouseButton(0))
                _onMouseLeft?.Invoke();
            if (Input.GetMouseButtonUp(0))
                _onMouseLeftReleased?.Invoke();
            if (Input.GetMouseButtonDown(1))
                _onMouseRightPressed?.Invoke();

        }
        void UpdateActionInput()
        {
            if (Input.GetKeyDown(KeyCode.Delete))
                _onDestroyEntityPressed?.Invoke();

            // cancel build
            if (Input.GetKeyDown(KeyCode.C))
                _onCancelBuildPressed?.Invoke();

            // Contextual unit actions (attack / capture ...)
            if (Input.GetMouseButtonDown(1))
                _onUnitActionStart?.Invoke();
            if (Input.GetMouseButtonUp(1))
                _onUnitActionEnd?.Invoke();

            if (Input.GetKeyDown(KeyCode.Alpha1))
                _onUseSquadAtIndex?.Invoke(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                _onUseSquadAtIndex?.Invoke(1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                _onUseSquadAtIndex?.Invoke(2);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                _onUseSquadAtIndex?.Invoke(3);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                _onUseSquadAtIndex?.Invoke(4);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                _onUseSquadAtIndex?.Invoke(5);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                _onUseSquadAtIndex?.Invoke(6);
            if (Input.GetKeyDown(KeyCode.Alpha8))
                _onUseSquadAtIndex?.Invoke(7);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                _onUseSquadAtIndex?.Invoke(8);
            if (Input.GetKeyDown(KeyCode.Alpha0))
                _onUseSquadAtIndex?.Invoke(9);
        }
        void UpdateCameraInput()
        {
            // Camera focus

            if (Input.GetKeyDown(KeyCode.F))
                _onFocusBasePressed?.Invoke();

            // Camera movement inputs

            // keyboard move (arrows)
            float hValue = Input.GetAxis("Horizontal");
            if (hValue != 0)
                _onCameraMoveHorizontal?.Invoke(hValue);
            float vValue = Input.GetAxis("Vertical");
            if (vValue != 0)
                _onCameraMoveVertical?.Invoke(vValue);

            // zoom in / out (ScrollWheel)
            float scrollValue = Input.GetAxis("Mouse ScrollWheel");
            if (scrollValue != 0)
                _onCameraZoom?.Invoke(scrollValue);

            // drag move (mouse button)
            if (Input.GetMouseButtonDown(2))
                _onCameraDragMoveStart?.Invoke();
            if (Input.GetMouseButtonUp(2))
                _onCameraDragMoveEnd?.Invoke();
        }
        #endregion

        #region Unit selection methods
        private void StartAction()
        {
            if (!_selectedFactory)
                return;

            if (_armyController.CurrentSelectedSquad != null)
                return;

            Ray ray = _gameCameraRef.MainCamera.ScreenPointToRay(Input.mousePosition);

            int factoryMask = 1 << LayerMask.NameToLayer("Factory");
            int floorMask = 1 << LayerMask.NameToLayer("Floor");
            int targetMask = 1 << LayerMask.NameToLayer("Target");

            // factory selection
            if (Physics.Raycast(ray, out RaycastHit raycastInfo, Mathf.Infinity, factoryMask))
            {
                Factory factory = raycastInfo.transform.GetComponent<Factory>();
                if (factory != null && factory == _selectedFactory)
                {
                    _selectedFactory.UpdateBanner(true, Vector3.zero);
                }
            }
            // unit selection / deselection
            else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, targetMask))
            {
                // Nothing, impossible to spawn on targetMask
            }
            else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorMask))
            {
                // Set banner on and place it at position
                _selectedFactory.UpdateBanner(false, raycastInfo.point);
            }
        }

        void StartSelection()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            
            // Hide target cursor
            SetTargetCursorVisible(false);

            Ray ray = _gameCameraRef.MainCamera.ScreenPointToRay(Input.mousePosition);

            int factoryMask = 1 << LayerMask.NameToLayer("Factory");
            int unitMask = 1 << LayerMask.NameToLayer("Unit");

            // *** Ignore Unit selection when clicking on UI ***
            // Set the Pointer Event Position to that of the mouse position
            _menuPointerEventData.position = Input.mousePosition;

            if (!_isShiftBtPressed && !_isCtrlBtPressed)
            {
                UnselectAllUnits();
            }

            //Create a list of Raycast Results
            List<RaycastResult> results = new();
            _playerMenuController.BuildMenuRaycaster.Raycast(_menuPointerEventData, results);
            if (results.Count > 0)
                return;

            // factory selection
            if (Physics.Raycast(ray, out RaycastHit raycastInfo, Mathf.Infinity, factoryMask))
            {
                Factory factory = raycastInfo.transform.GetComponent<Factory>();
                if (factory != null)
                {
                    if (factory.GetTeam() == _team && _selectedFactory != factory)
                    {
                        UnselectCurrentFactory();
                        SelectFactory(factory);
                    }
                }
            }
            // unit selection / deselection
            else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, unitMask))
            {
                UnselectCurrentFactory();

                Unit selectedUnit = raycastInfo.transform.GetComponent<Unit>();
                if (selectedUnit != null && selectedUnit.GetTeam() == _team)
                {
                    if (_isShiftBtPressed)
                    {
                        _armyController.UnselectUnit(selectedUnit);
                    }
                    else if (_isCtrlBtPressed)
                    {
                        _armyController.SelectUnit(selectedUnit);
                    }
                    else
                    {
                        UnselectAllUnits();
                        _armyController.SelectUnit(selectedUnit);
                    }
                }
            }
            else if (_underMapCollider.Raycast(ray, out raycastInfo, Mathf.Infinity))
            {
                UnselectCurrentFactory();
                _selectionLineRenderer.enabled = true;

                _selectionStarted = true;

                _selectionStart.x = raycastInfo.point.x;
                _selectionStart.y = 1f;
                _selectionStart.z = raycastInfo.point.z;
            }
        }

        /*
         * Multi selection methods
         */
        private void UpdateSelection()
        {
            if (_selectionStarted == false)
                return;

            Ray ray = _gameCameraRef.MainCamera.ScreenPointToRay(Input.mousePosition);

            if (_underMapCollider.Raycast(ray, out RaycastHit raycastInfo, Mathf.Infinity))
            {
                _selectionEnd = raycastInfo.point;
            }

            _selectionLineRenderer.SetPosition(0, new Vector3(_selectionStart.x, _selectionStart.y, _selectionStart.z));
            _selectionLineRenderer.SetPosition(1, new Vector3(_selectionStart.x, _selectionStart.y, _selectionEnd.z));
            _selectionLineRenderer.SetPosition(2, new Vector3(_selectionEnd.x, _selectionStart.y, _selectionEnd.z));
            _selectionLineRenderer.SetPosition(3, new Vector3(_selectionEnd.x, _selectionStart.y, _selectionStart.z));
        }
        void EndSelection()
        {
            if (_selectionStarted == false)
                return;

            UpdateSelection();
            _selectionLineRenderer.enabled = false;
            Vector3 center = (_selectionStart + _selectionEnd) / 2f;
            Vector3 size = Vector3.up * SELECTION_BOX_HEIGHT + _selectionEnd - _selectionStart;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);
            size.z = Mathf.Abs(size.z);

            UnselectCurrentFactory();

            if (!_isShiftBtPressed && !_isCtrlBtPressed)
                UnselectAllUnits();

            int unitLayerMask = 1 << LayerMask.NameToLayer("Unit");
            int factoryLayerMask = 1 << LayerMask.NameToLayer("Factory");
            Collider[] colliders = Physics.OverlapBox(center, size / 2f, Quaternion.identity, unitLayerMask | factoryLayerMask, QueryTriggerInteraction.Ignore);
            foreach (Collider col in colliders)
            {
                //Debug.Log("collider name = " + col.gameObject.name);
                ISelectable selectedEntity = col.transform.GetComponent<ISelectable>();

                if (selectedEntity.GetTeam() != GetTeam())
                    continue;

                switch (selectedEntity)
                {
                    case Unit unit when _isShiftBtPressed:
                        _armyController.SelectUnit(unit);
                        break;
                    case Unit unit when _isCtrlBtPressed:
                        _armyController.UnselectUnit(unit);
                        break;
                    case Unit unit:
                        _armyController.SelectUnit(unit);
                        break;
                    case Factory entity:
                        {
                            // Select only one factory at a time
                            if (!_selectedFactory)
                                SelectFactory(entity);
                            break;
                        }
                }
            }

            _selectionStarted = false;
            _selectionStart = Vector3.zero;
            _selectionEnd = Vector3.zero;
        }

        private void UseSquadAtIndex(int index)
        {
            if (_isShiftBtPressed || _isCtrlBtPressed)
                _armyController.SaveSquad(index);
            else
            {
                _armyController.SelectSquad(index);
            }
        }


        #endregion

        #region Factory / build methods
        public override void SelectFactory(Factory factory)
        {
            if (!factory || factory.IsUnderConstruction)
                return;

            base.SelectFactory(factory);

            _playerMenuController.UpdateFactoryMenu(_selectedFactory, RequestUnitBuild, EnterFactoryBuildMode);
        }
        public override void UnselectCurrentFactory()
        {
            //Debug.Log("UnselectCurrentFactory");

            if (_selectedFactory)
            {
                _playerMenuController.UnregisterBuildButtons(_selectedFactory.AvailableUnitsCount, _selectedFactory.AvailableFactoriesCount);
            }

            _playerMenuController.HideFactoryMenu();

            base.UnselectCurrentFactory();
        }
        void EnterFactoryBuildMode(int factoryId)
        {
            if (_selectedFactory.GetFactoryCost(factoryId) > TotalBuildPoints)
                return;

            _currentInputMode = InputMode.FactoryPositioning;

            _wantedFactoryId = factoryId;

            // Create factory preview

            // Load factory prefab for preview
            GameObject factoryPrefab = _selectedFactory.GetFactoryPrefab(factoryId);
            if (!factoryPrefab)
            {
                Debug.LogWarning("Invalid factory prefab for factoryId " + factoryId);
            }

            if (_wantedFactoryPreview is not null)
                Destroy(_wantedFactoryPreview);

            _wantedFactoryPreview = Instantiate(factoryPrefab.transform.GetChild(0).gameObject); // Quick and dirty access to mesh GameObject
            _wantedFactoryPreview.name = _wantedFactoryPreview.name.Replace("(Clone)", "_Preview");
            // Set transparency on materials
            foreach (MeshRenderer meshRend in _wantedFactoryPreview.GetComponentsInChildren<MeshRenderer>())
            {
                Material mat = meshRend.material;
                mat.shader = _previewShader;
                Color col = mat.color;
                col.a = _factoryPreviewTransparency;
                mat.color = col;
            }

            // Project mouse position on ground to position factory preview
            ProjectFactoryPreviewOnFloor();
        }

        private void ExitFactoryBuildMode()
        {
            _currentInputMode = InputMode.Orders;
            Destroy(_wantedFactoryPreview);
        }
        Vector3 ProjectFactoryPreviewOnFloor()
        {
            if (_currentInputMode == InputMode.Orders)
            {
                Debug.LogWarning("Wrong call to ProjectFactoryPreviewOnFloor : CurrentInputMode = " + _currentInputMode.ToString());
                return Vector3.zero;
            }

            Vector3 floorPos = Vector3.zero;
            Ray ray = _gameCameraRef.MainCamera.ScreenPointToRay(Input.mousePosition);
            int floorMask = 1 << LayerMask.NameToLayer("Floor");

            if (!Physics.Raycast(ray, out RaycastHit raycastInfo, Mathf.Infinity, floorMask)) return floorPos;

            floorPos = raycastInfo.point;
            _wantedFactoryPreview.transform.position = floorPos;
            return floorPos;
        }
        #endregion

        #region Entity targetting (attack / capture) and movement methods
        private void ComputeUnitsAction()
        {
            int damageableMask = (1 << LayerMask.NameToLayer("Unit")) | (1 << LayerMask.NameToLayer("Factory"));
            int targetMask = 1 << LayerMask.NameToLayer("Target");
            int floorMask = 1 << LayerMask.NameToLayer("Floor");
            Ray ray = _gameCameraRef.MainCamera.ScreenPointToRay(Input.mousePosition);

            // Set unit / factory attack target
            if (Physics.Raycast(ray, out RaycastHit raycastInfo, Mathf.Infinity, damageableMask))
            {
                BaseEntity other = raycastInfo.transform.GetComponent<BaseEntity>();

                if (!other) return;

                Vector3 newPos = other.transform.position;

                if (other.GetTeam() != GetTeam())
                {
                    _armyController.MoveAttackCurrentSquad(newPos);
                }
                else if (other.NeedsRepairing())
                {
                    // Direct call to repairing task $$$ to be improved by AI behaviour
                    //foreach (Unit unit in _selectedUnitList)
                    //    unit.SetRepairTarget(other);
                }
            }
            // Set capturing target
            else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, targetMask))
            {
                TargetBuilding target = raycastInfo.transform.GetComponent<TargetBuilding>();

                if (!target) return;

                SetTargetCursorPosition(target.transform.position);
                _armyController.MoveCurrentSquad(target.transform.position, _isShiftBtPressed);

                if (target.GetTeam() != GetTeam())
                {
                    _armyController.SetCaptureTarget(target);
                }
            }
            // Set unit move target
            else if (Physics.Raycast(ray, out raycastInfo, Mathf.Infinity, floorMask))
            {
                Vector3 newPos = raycastInfo.point;
                SetTargetCursorPosition(newPos);

                _armyController.MoveCurrentSquad(newPos, _isShiftBtPressed);
            }
        }
        #endregion
    }
}
