using System;
using UnityEngine;

namespace RTS.FogOfWar
{
    public class FogOfWarTexture : MonoBehaviour
    {
        public Texture2D Texture;

        [SerializeField] private Color _greyColor = new (0.5f, 0.5f, 0.5f, 1.0f);
        [SerializeField] protected Color _whiteColor = new (1.0f, 1.0f, 1.0f, 1.0f);
        [SerializeField] private Color _startColor = new Color(0, 0, 0, 1.0f);

        [SerializeField] private SpriteRenderer _spriteRenderer;

        [SerializeField] private float _interpolateSpeed = 50f;

        [NonSerialized] private Color[] _colors;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void CreateTexture(int width, int height, Vector2 scale)
        {
            Texture = new Texture2D(width, height);
            _spriteRenderer.sprite = Sprite.Create(Texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1);
            _spriteRenderer.transform.localScale = scale;

            int size = width * height;
            _colors = new Color[size];
            for (int i = 0; i < size; ++i)
                _colors[i] = _startColor;

            Texture.SetPixels(_colors);
            Texture.Apply();
        }

        public void SetTexture(Grid visibleGrid, Grid previousVisibleGrid, int team)
        {
            for (int i = 0; i < visibleGrid.Size; ++i)
            {
                bool isVisible = (visibleGrid.Get(i) & team) == team;
                bool wasVisible = (previousVisibleGrid.Get(i) & team) == team;

                Color newColor = _startColor;
                if (isVisible)
                    newColor = _whiteColor;
                else if (wasVisible)
                    newColor = _greyColor;

                newColor.r = Mathf.Lerp(_colors[i].r, newColor.r, Time.deltaTime * _interpolateSpeed);
                _colors[i] = newColor;
            }

            Texture.SetPixels(_colors);
            Texture.Apply();
        }
    }
}