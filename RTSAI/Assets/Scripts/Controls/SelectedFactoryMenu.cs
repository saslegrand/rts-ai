using System;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI
{
	public class SelectedFactoryMenu : MonoBehaviour
	{
		[SerializeField] private SelectedFactory _selectedFactoryDisplay;
		[SerializeField] private InQueueUnit _inQueueUnitDisplay;
		[SerializeField] private BuildableUnit _unitBuildMenu;
		[SerializeField] private BuildableUnit _factoryBuildMenu;
		[SerializeField] private Button _cancelBuildButton;

		public void UnregisterBuildButtons(int availableUnitsCount, int availableFactoriesCount)
		{
			// unregister build buttons
			for (int i = 0; i < availableUnitsCount; i++)
			{
				_unitBuildMenu.Buttons[i].onClick.RemoveAllListeners();
			}
			for (int i = 0; i < availableFactoriesCount; i++)
			{
				_factoryBuildMenu.Buttons[i].onClick.RemoveAllListeners();
			}
		}

		public void UpdateFactoryMenu(Factory selectedFactory, Func<int, bool> requestUnitBuildMethod, Action<int> enterFactoryBuildModeMethod)
		{
			if (_selectedFactoryDisplay)
				_selectedFactoryDisplay.SetFactoryDisplay(selectedFactory);
			if (_inQueueUnitDisplay)
				_inQueueUnitDisplay.SetFactoryDisplay(selectedFactory);

			// Unit build buttons
			// register available buttons
			SetUnitBuildButtons(selectedFactory, requestUnitBuildMethod);

			// activate Cancel button
			_cancelBuildButton.onClick.AddListener(() =>
			{
				if (selectedFactory)
					selectedFactory.CancelCurrentBuild();
			});

			// Factory build buttons
			// register available buttons
			SetFactoryBuildButtons(selectedFactory, enterFactoryBuildModeMethod);
		}

		private void SetFactoryBuildButtons(Factory selectedFactory, Action<int> enterFactoryBuildModeMethod)
		{
			if (!selectedFactory || !_factoryBuildMenu) return;
            
			int iterationMax = Mathf.Min(selectedFactory.AvailableFactoriesCount, _factoryBuildMenu.Buttons.Length);

			int i = 0;
			for (; i < iterationMax; i++)
			{
				_factoryBuildMenu.Buttons[i].gameObject.SetActive(true);

				int index = i; // capture index value for event closure
				_factoryBuildMenu.Buttons[i].onClick.AddListener(() => { enterFactoryBuildModeMethod(index); });

				FactoryDataScriptable data = selectedFactory.GetBuildableFactoryData(i);

				_factoryBuildMenu.Thumbnails[i].sprite = data.Thumbnail;

				_factoryBuildMenu.CostText[i].text = $"{data.Cost}";
			}

			// hide remaining buttons
			for (; i < _factoryBuildMenu.Buttons.Length; i++)
			{
				_factoryBuildMenu.Buttons[i].gameObject.SetActive(false);
			}
		}

		private void SetUnitBuildButtons(Factory selectedFactory, Func<int, bool> requestUnitBuildMethod)
		{
			if (!selectedFactory || !_unitBuildMenu) return;
            
			int iterationMax = Mathf.Min(selectedFactory.AvailableUnitsCount, _unitBuildMenu.Buttons.Length);

			int i = 0;
			for (; i < iterationMax; i++)
			{
				_unitBuildMenu.Buttons[i].gameObject.SetActive(true);

				int index = i; // capture index value for event closure

				_unitBuildMenu.Buttons[i].onClick.AddListener(() =>
				{
					requestUnitBuildMethod(index);
				});

				UnitDataScriptable data = selectedFactory.GetBuildableUnitData(i);

				_unitBuildMenu.Thumbnails[i].sprite = data.Thumbnail;

				_unitBuildMenu.CostText[i].text = $"{data.Cost}";
			}

			// hide remaining buttons
			for (; i < _unitBuildMenu.Buttons.Length; i++)
			{
				_unitBuildMenu.Buttons[i].gameObject.SetActive(false);
			}
		}
	}
}
