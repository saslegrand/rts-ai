using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI
{
	public class InQueueUnit : MonoBehaviour
	{
		[SerializeField] private Button[] _queue = new Button[8];
		[SerializeField] private Slider _progress;

		private Image[] _thumbnails;
		private Factory _selectedFactory;

		public void SetFactoryDisplay(Factory factory)
		{
			_selectedFactory = factory;
		}

		private void Awake()
		{
			_thumbnails = new Image[_queue.Length];

			for (var index = 0; index < _queue.Length; index++)
			{
				Button button = _queue[index];
				_thumbnails[index] = button.GetComponentsInChildren<Image>().Last();
				button.gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			var i = 0;
			_progress.value = 0f;

			if (_selectedFactory.CurrentUnitInBuild >= 0)
			{
				_progress.value = _selectedFactory.CurrentBuildPercent;
				_queue[i].gameObject.SetActive(true);
				_thumbnails[i].sprite = _selectedFactory.GetBuildableUnitData(_selectedFactory.CurrentUnitInBuild).Thumbnail;
				i++;
			}
			
			foreach (int unit in _selectedFactory.UnitQueue)
			{
				if (i > _queue.Length) return;
				
				_queue[i].gameObject.SetActive(true);
				_thumbnails[i].sprite = _selectedFactory.GetBuildableUnitData(unit).Thumbnail;
				
				i++;
			}

			for (;i < _queue.Length; i++)
			{
				_queue[i].gameObject.SetActive(false);
			}
		}
	}
}
