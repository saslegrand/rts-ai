#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SaveThumbnail : MonoBehaviour
{
	[SerializeField] private Camera _camera;
	[SerializeField] private string _folder;
	[SerializeField] private GameObject _container;

	private GameObject[] _unitToCapture;

	private void Start()
	{
		int childCount = _container.transform.childCount;
		_unitToCapture = new GameObject[childCount];
		
		for (var i = 0; i < childCount; i++)
		{
			_unitToCapture[i] = _container.transform.GetChild(i).gameObject;
		}

		foreach (GameObject unit in _unitToCapture)
		{
			unit.gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.G)) return;

		foreach (GameObject unit in _unitToCapture)
		{
			unit.gameObject.SetActive(true);
			CreateThumbnail(unit);
			unit.gameObject.SetActive(false);
		}
	}

	private void CreateThumbnail(GameObject toCapture)
	{
		const int Side = 512;
     
		_camera.aspect = 1.0f;
		// recall that the height is now the "actual" size from now on
     
		RenderTexture tempRT = new (Side,Side, 32 );
		// the 24 can be 0,16,24, formats like
		// RenderTextureFormat.Default, ARGB32 etc.
     
		_camera.targetTexture = tempRT;
		_camera.Render();
     
		RenderTexture.active = tempRT;
		Texture2D virtualPhoto = new (Side, Side, TextureFormat.ARGB32, false);
		// false, meaning no need for mipmaps
		
		virtualPhoto.ReadPixels( new Rect(0, 0, Side,Side), 0, 0);
		
		RenderTexture.active = null;
		_camera.targetTexture = null;
		// consider ... Destroy(tempRT);

		byte[] bytes = virtualPhoto.EncodeToPNG();

		var path = $"{_folder}/{toCapture.name}.png";
		System.IO.File.WriteAllBytes(path, bytes);
		
		AssetDatabase.Refresh();
		AssetDatabase.ImportAsset(path);
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

		if (!importer) return;
		
		importer.textureType = TextureImporterType.Sprite;
		AssetDatabase.WriteImportSettingsIfDirty(path);
	}
}
#endif
