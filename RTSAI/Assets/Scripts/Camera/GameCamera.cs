using RTS.Extensions;
using UnityEngine;


namespace RTS.Cameras
{
	public class GameCamera : MonoBehaviour
	{
		[SerializeField] private Camera _gameCamera;
		[SerializeField] private Transform _zoomedOutPosition;
		[SerializeField] private Transform _zoomedInPosition;

		[SerializeField] private bool _enableMoveLimits = true;
		[SerializeField] private int _keyboardSpeedModifier = 20;
		[SerializeField] private int _moveSpeed = 5;
		[SerializeField] private float _zoomSpeed = 100;
		[SerializeField] private float _terrainBorder = 100f;

		[SerializeField] private AnimationCurve _moveSpeedFromZoomCurve = new ();

		// Movement
		private float _cameraMarginSize;
		private Vector2Int _screenSize;
		private Vector2 _cameraMaxBound;
		private Vector2 _cameraXMargin;
		private Vector2 _cameraYMargin;
		private Vector2 _cameraFrameMove;
		private Vector3 _move = Vector3.zero;

		private readonly Vector2 _cameraRemapMargin = new (0.5f, 1.5f);

		// Zoom
		private float _realZoomSpeed;
		private float _zoomRatio = 0.5f;

		private Vector3 _terrainSize = Vector3.zero;
		private float _moveSpeedFromZoomModifier;

		public Camera MainCamera => _gameCamera;

#region Camera Update

#region Zoom
		public void Zoom(float value)
		{
			_zoomRatio += _realZoomSpeed * Mathf.Sign(value) * Time.unscaledDeltaTime;
			_zoomRatio = Mathf.Clamp(_zoomRatio, 0f, 1f);

			_gameCamera.transform.position = Vector3.Lerp(_zoomedOutPosition.position, _zoomedInPosition.position, _zoomRatio);
			_gameCamera.transform.forward = transform.position - _gameCamera.transform.position;

			ComputeSpeedFromZoomModifier();
		}

		private void ComputeSpeedFromZoomModifier()
		{
			float zoomRatio = Mathf.Clamp(1f - Vector3.Distance(_gameCamera.transform.position, _zoomedOutPosition.position) / Vector3.Distance(_zoomedOutPosition.position, _zoomedInPosition.position), 0f, 1f);

			_moveSpeedFromZoomModifier = _moveSpeedFromZoomCurve.Evaluate(zoomRatio);
		}

		private void ComputeRealZoomSpeed()
		{
			_realZoomSpeed = _zoomSpeed.Remap(0f, Vector3.Distance(_zoomedOutPosition.position, _zoomedInPosition.position), 0f, 1f);
		}
#endregion

#region Movement
		private void UpdateMoveCamera()
		{
			_cameraFrameMove.x = 0f;
			_cameraFrameMove.y = 0f;

			Vector2 input = Input.mousePosition;
			input.x = Mathf.Clamp(input.x, 0f, Screen.width);
			input.y = Mathf.Clamp(input.y, 0f, Screen.height);
			

			if (!input.x.Between(_cameraMarginSize, _cameraMaxBound.x))
			{
				_cameraFrameMove.x = input.x > _screenSize.x * 0.5f ? input.x.Remap(_cameraXMargin, _cameraRemapMargin) : -input.x.Remap(_cameraMarginSize, 0, _cameraRemapMargin);
			}

			if (!input.y.Between(_cameraMarginSize, _cameraMaxBound.y))
			{
				_cameraFrameMove.y = input.y > _screenSize.y * 0.5f ? input.y.Remap(_cameraYMargin, _cameraRemapMargin) : -input.y.Remap(_cameraMarginSize, 0, _cameraRemapMargin);
			}

			MouseMove(_cameraFrameMove);
		}

		private void MouseMove(Vector2 move)
		{
			if (Mathf.Approximately(move.sqrMagnitude, 0f))
				return;

			MoveHorizontal(move.x);
			MoveVertical(move.y);
		}

		public void KeyboardMoveHorizontal(float value)
		{
			if ((int)(transform.rotation.y / 90f) % 2 == 0)
				MoveHorizontal(value * _keyboardSpeedModifier);
			else
				MoveVertical(value * _keyboardSpeedModifier);
		}

		public void KeyboardMoveVertical(float value)
		{
		MoveVertical(value * _keyboardSpeedModifier);
		}

		private void MoveHorizontal(float value)
		{
			_move += transform.right * (value * _moveSpeed * _moveSpeedFromZoomModifier * Time.unscaledDeltaTime);
		}

		private void MoveVertical(float value)
		{
			_move += transform.forward * (value * _moveSpeed * _moveSpeedFromZoomModifier * Time.unscaledDeltaTime);
		}
#endregion

		private void UpdateCameraData()
		{
			if (_screenSize.x == Screen.width && _screenSize.y == Screen.height) return;

			_screenSize.x = Screen.width;
			_screenSize.y = Screen.height;
			ComputeCameraData();
		}

		private void ComputeCameraData()
		{
			_cameraMarginSize = Mathf.Min(_screenSize.x, _screenSize.y) * 0.025f;

			_cameraMaxBound = new Vector2(_screenSize.x - _cameraMarginSize, _screenSize.y - _cameraMarginSize);

			_cameraXMargin = new Vector2(_cameraMaxBound.x, _screenSize.x);
			_cameraYMargin = new Vector2(_cameraMaxBound.y, _screenSize.y);
		}


		// Direct focus on one entity (no smooth)
		public void FocusEntity(BaseEntity entity)
		{
			if (!entity)
				return;

			Vector3 newPos = entity.transform.position;
			newPos.y = 0;

			transform.position = newPos;
		}
#endregion

#region Unity Callbacks

#if UNITY_EDITOR
		private void OnValidate()
		{
			ComputeRealZoomSpeed();
			ComputeSpeedFromZoomModifier();
		}
#endif

		private void Start()
		{
			_terrainSize = GameServices.GetTerrainSize();
			ComputeRealZoomSpeed();
			Zoom(0f);
			
			UpdateCameraData();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Keypad4))
			{
				Vector3 rotationEulerAngles = transform.rotation.eulerAngles;
				rotationEulerAngles = new Vector3(rotationEulerAngles.x, rotationEulerAngles.y + 90f, rotationEulerAngles.z);

				transform.rotation = Quaternion.Euler(rotationEulerAngles);
			}
			else if (Input.GetKeyDown(KeyCode.Keypad6))
			{
				Vector3 rotationEulerAngles = transform.rotation.eulerAngles;
				rotationEulerAngles = new Vector3(rotationEulerAngles.x, rotationEulerAngles.y - 90f, rotationEulerAngles.z);

				transform.rotation = Quaternion.Euler(rotationEulerAngles);
			}
			
			UpdateCameraData();
			//UpdateMoveCamera();

			if (_move == Vector3.zero) return;

			transform.position += _move;
			_move = Vector3.zero;

			if (!_enableMoveLimits) return;

			// Clamp camera position (max height, terrain bounds)
			Vector3 newPos = transform.position;
			newPos.x = Mathf.Clamp(newPos.x, _terrainBorder, _terrainSize.x - _terrainBorder);
			newPos.z = Mathf.Clamp(newPos.z, _terrainBorder, _terrainSize.z - _terrainBorder);
			transform.position = newPos;
		}
#endregion
	}
}
