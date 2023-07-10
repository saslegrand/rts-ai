using RTS.Extensions;
using UnityEngine;

namespace RTS.Cameras
{
	public class MinimapCamera : MonoBehaviour
	{
		[SerializeField] private RectTransform _minimapButton;
		[SerializeField] private GameCamera _gameCamera;
		[SerializeField] private Camera _minimapCamera;
		[SerializeField] private BoxCollider _boxCollider;
		[SerializeField] private LineRenderer _cameraViewportRenderer;

		private Vector2 _minimapXBounds;
		private Vector2 _minimapYBounds;
		private Vector2 _minimapXRemap;
		private Vector2 _minimapYRemap;

		private void Start()
		{
			Rect miniMapButtonRect = _minimapButton.rect;
			Vector3 mapPosition = _minimapButton.transform.position;
			_minimapXBounds = new Vector2(mapPosition.x + miniMapButtonRect.xMin, mapPosition.x + miniMapButtonRect.xMax);
			_minimapYBounds = new Vector2(mapPosition.y + miniMapButtonRect.yMin, mapPosition.y + miniMapButtonRect.yMax);

			Vector3 cameraPosition = _minimapCamera.transform.position;
			float cameraSize = _minimapCamera.orthographicSize;
			_minimapXRemap = new Vector2(cameraPosition.x - cameraSize, cameraPosition.x + cameraSize);
			_minimapYRemap = new Vector2(cameraPosition.z - cameraSize, cameraPosition.z + cameraSize);
		}

		private void Update()
		{
			CalculateMapFrustum();
		}

		public void OnClickButton()
		{
			Vector3 input = Input.mousePosition;
			input.x = input.x.Remap(_minimapXBounds, _minimapXRemap);
			input.z = input.y.Remap(_minimapYBounds, _minimapYRemap);
			input.y = 100;

			Ray ray = new Ray(input, Vector3.down);

			if (!_boxCollider.Raycast(ray, out RaycastHit hit, 110f)) return;

			if (hit.collider != _boxCollider)
				return;
				
			Vector3 newPosition = hit.point;
			newPosition.y = 0f;
			_gameCamera.transform.position = newPosition;
		}

		private void CalculateMapFrustum()
		{
			Vector3 topLeftPos = GetCameraFrustumPosition(new Vector3(0, Screen.height));
			Vector3 topRightPos = GetCameraFrustumPosition(new Vector3(Screen.width, Screen.height));
			Vector3 bottomLeftPos = GetCameraFrustumPosition(new Vector3(0, 0));
			Vector3 bottomRightPos = GetCameraFrustumPosition(new Vector3(Screen.width, 0));

			topLeftPos.y = topRightPos.y = bottomLeftPos.y = bottomRightPos.y = 99f;
			
			_cameraViewportRenderer.SetPositions(new []{topLeftPos, topRightPos, bottomRightPos, bottomLeftPos});
		}

		private Vector3 GetCameraFrustumPosition(Vector3 position)
		{
			Ray positionRay = _gameCamera.MainCamera.ScreenPointToRay(position);

			return _boxCollider.Raycast(positionRay, out RaycastHit hit, 2000f) ? hit.point : new Vector3();
		}
	}
}
