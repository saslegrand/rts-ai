using UnityEngine;

namespace RTS.FogOfWar
{
	public class FogOfWarManager : MonoBehaviour
	{
		private PlayerController _controller;
		public ETeam Team => _controller.GetTeam();

		[SerializeField] private FogOfWarSystem _FOWSystem;

		public FogOfWarSystem GetFogOfWarSystem => _FOWSystem;

		[SerializeField]
		private float _updateFrequency = 0.05f;

		private float _lastUpdateDate;


		void Start()
		{
			_controller = FindObjectOfType<PlayerController>();
			_FOWSystem.Init();
		}

		private void Update()
		{
			if (Time.time - _lastUpdateDate > _updateFrequency)
			{
				_lastUpdateDate = Time.time;
				UpdateVisibilityTextures();
				UpdateFactoriesVisibility();
				UpdateUnitVisibility();
				UpdateBuildingVisibility();
			}
		}

		private void UpdateVisibilityTextures()
		{
			_FOWSystem.ClearVisibility();
			_FOWSystem.UpdateVisions(FindObjectsOfType<EntityVisibility>());
			_FOWSystem.UpdateTextures(1 << (int)Team);
		}

		private void UpdateUnitVisibility()
		{
			foreach (Unit unit in GameServices.GetControllerByTeam(Team).UnitList)
			{
				if (unit.Visibility == null) continue;

				unit.Visibility.SetVisible(true);
			}

			foreach (Unit unit in GameServices.GetControllerByTeam(Team.GetOpponent()).UnitList)
			{
				if (unit.Visibility == null) continue;

				unit.Visibility.SetVisible( _FOWSystem.IsVisible(1 << (int)Team, unit.Visibility.Position) );
			}
		}

		private void UpdateBuildingVisibility()
		{
			foreach (TargetBuilding building in GameServices.GetTargetBuildings())
			{
				if (building.Visibility == null) { continue; }

				building.Visibility.SetVisibleUI( _FOWSystem.IsVisible(1 << (int)Team, building.Visibility.Position) );

				if (_FOWSystem.WasVisible(1 << (int)Team, building.Visibility.Position))
				{
					building.Visibility.SetVisibleDefault(true);
				}
				else
				{
					building.Visibility.SetVisible(false);
				}
			}
		}

		private void UpdateFactoriesVisibility()
		{
			foreach (Factory factory in GameServices.GetControllerByTeam(Team).FactoryList)
			{
				factory.Visibility?.SetVisible(true);
			}

			foreach (Factory factory in GameServices.GetControllerByTeam(Team.GetOpponent()).FactoryList)
			{
				if (_FOWSystem.IsVisible(1 << (int)Team, factory.Visibility.Position))
				{
					factory.Visibility.SetVisibleUI(true);
				}
				else
				{
					factory.Visibility.SetVisibleUI(false);
				}

				if (_FOWSystem.WasVisible(1 << (int)Team, factory.Visibility.Position))
				{
					factory.Visibility.SetVisibleDefault(true);
				}
				else
				{
					factory.Visibility.SetVisible(false);
				}
			}
		}
	}
}