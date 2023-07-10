using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI
{

	public class SelectedFactory : MonoBehaviour
	{
		[SerializeField] private TMP_Text _name;
		[SerializeField] private TMP_Text _health;
		[SerializeField] private Image _factoryDisplayBackground;
		[SerializeField] private Image _factoryDisplayImage;

		private Factory _selectedFactory;
		
		public void SetFactoryDisplay(Factory factory)
		{
			_selectedFactory = factory;
			_selectedFactory.OnHpUpdated += UpdateHP;
			_factoryDisplayImage.sprite = factory.FactoryData.Thumbnail;
			_name.text = factory.FactoryData.Caption;
		}

		private void UpdateHP()
		{
			_health.text = $"{_selectedFactory.Hp} / {_selectedFactory.Hp_Max}";
		}
	}
}
