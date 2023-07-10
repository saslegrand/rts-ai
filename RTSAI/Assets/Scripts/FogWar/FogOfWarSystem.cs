using UnityEngine;

namespace RTS.FogOfWar
{
    public class FogOfWarSystem : MonoBehaviour
    {
        public int GridWidth;
        public int GridHeight;

        [SerializeField] protected FogOfWarTexture _fogTexture;
        [SerializeField] protected Camera _fogCamera;
        [SerializeField] protected Transform _fogQuadParent;

        private Grid VisibilityGrid;
        private Grid PreviousVisibilityGrid;
        private Vector2 TextureScale;

        public void Init()
        {
            Vector3 fogLocalScale = _fogQuadParent.localScale;
            TextureScale = new Vector2(fogLocalScale.x / GridWidth,
                                       fogLocalScale.y / GridHeight);
            _fogTexture.CreateTexture(GridWidth, GridHeight, TextureScale);

            VisibilityGrid = new Grid(GridWidth, GridHeight, 0);
            PreviousVisibilityGrid = new Grid(GridWidth, GridHeight, 0);

            _fogQuadParent.gameObject.SetActive(true);
        }

        private Vector2Int GetPositionInGrid(Vector2 p)
        {
            Vector3 fogLocalScale = _fogQuadParent.localScale; 
            return new Vector2Int
            {
                x = Mathf.RoundToInt(p.x * GridWidth / fogLocalScale.x),
                y = Mathf.RoundToInt(p.y * GridHeight / fogLocalScale.y)
            };
        }

        private void SetCell(int team, int x, int y)
        {
            if (!VisibilityGrid.Contains(x, y))
                return;
            if ((VisibilityGrid.Values[x + y * VisibilityGrid.Height] & team) > 0)
                return;

            VisibilityGrid.Values[x + y * VisibilityGrid.Width] |= team;
            PreviousVisibilityGrid.Values[x + y * PreviousVisibilityGrid.Width] |= team;
        }

        public void ClearVisibility()
        {
            VisibilityGrid.Clear();
        }

        public void UpdateVisions(EntityVisibility[] visibilities)
        {
            foreach (EntityVisibility v in visibilities)
            {
                Vector2Int gridPos = GetPositionInGrid(v.Position);

                int radius = Mathf.FloorToInt(v.VisionRange / TextureScale.x) - 1;
                if (radius <= 0)
                    return;

                int x = radius;
                int y = 0;
                int xTemp = 1 - (radius * 2);
                int yTemp = 0;
                int radiusTemp = 0;

                while (x >= y)
                {
                    for (int j = gridPos.x - x; j <= gridPos.x + x; ++j)
                    {
                        SetCell(1 << (int)v.Team, j, gridPos.y + y);
                        SetCell(1 << (int)v.Team, j, gridPos.y - y);
                    }
                    for (int j = gridPos.x - y; j <= gridPos.x + y; ++j)
                    {
                        SetCell(1 << (int)v.Team, j, gridPos.y + x);
                        SetCell(1 << (int)v.Team, j, gridPos.y - x);
                    }

                    ++y;
                    radiusTemp += yTemp;
                    yTemp += 2;

                    if (((radiusTemp * 2) + xTemp) > 0)
                    {
                        x--;
                        radiusTemp += xTemp;
                        xTemp += 2;
                    }
                }
            }
        }

        public void UpdateTextures(int team)
        {
            _fogTexture.SetTexture(VisibilityGrid, PreviousVisibilityGrid, team);
        }

        public bool IsVisible(int team, Vector2 position)
        {
            Vector2Int posGrid = GetPositionInGrid(position);
            return VisibilityGrid.IsValue(team, posGrid.x, posGrid.y);
        }

        public bool WasVisible(int team, Vector2 position)
        {
            Vector2Int posGrid = GetPositionInGrid(position);
            return PreviousVisibilityGrid.IsValue(team, posGrid.x, posGrid.y);
        }
    }
}