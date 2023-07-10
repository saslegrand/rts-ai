using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI
{
	public class BuildableUnit : MonoBehaviour
	{
		[SerializeField] private Button[] _buildButton = new Button[8];

		private Factory _selectedFactory;

		public Button[] Buttons => _buildButton;
		public Image[] Thumbnails { get; private set; }
		public TMP_Text[] CostText { get; private set; }

		private void Awake()
		{
			Thumbnails = new Image[_buildButton.Length];
			CostText = new TMP_Text[_buildButton.Length];
			
			for (var index = 0; index < _buildButton.Length; index++)
			{
				Button button = _buildButton[index];
				Thumbnails[index] = button.GetComponentsInChildren<Image>().Last();
				CostText[index] = button.GetComponentsInChildren<TMP_Text>().First();
				button.gameObject.SetActive(false);
			}
		}
	}
}
