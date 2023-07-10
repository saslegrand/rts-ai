using System;
using RTS.AI.Debugger;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.AI.Tools
{
    public class InfluenceMapDebugger : UIDebugger
    {
        [Header("Map")]
        [SerializeField] private InfluenceMap _influenceMap;
        [SerializeField] private Image _displayImage;
        [SerializeField] private int _textureResolution = 200;

        [Header("Other")] 
        [SerializeField] private bool _shouldUpdateTexture = true;
        [SerializeField] private AIController _aiController;

        private Texture2D _mapTexture;
    
        // Start is called before the first frame update
        void Start()
        {
            _influenceMap.InfluenceMapUpdated += OnInfluenceMapUpdated;
        }

        private void OnInfluenceMapUpdated()
        {
            if (!_shouldUpdateTexture || !_displayImage.transform.parent.gameObject.activeSelf)
                return;

            if (!_mapTexture)
                _mapTexture = new Texture2D(_textureResolution, _textureResolution, TextureFormat.ARGB32, false);

            for (int pixelYIndex = 0; pixelYIndex < _textureResolution; pixelYIndex++)
            {
                float yPercent = pixelYIndex / (float)(_textureResolution - 1);
                for (int pixelXIndex = 0; pixelXIndex < _textureResolution; pixelXIndex++)
                {
                    float xPercent = pixelXIndex / (float)(_textureResolution - 1);
                    InfluenceMapNode node = _influenceMap.GetNodeFromPercentage(xPercent, yPercent);

                    float power = node.GetScore(ETeam.Red) / (float)_influenceMap.MaxCellScore;
                    Color aiColor = GameServices.GetTeamColor(ETeam.Red) * power;
                    
                    float opponentPower = node.GetScore(ETeam.Blue) / (float)_influenceMap.MaxCellScore;
                    Color opponentColor = GameServices.GetTeamColor(ETeam.Blue) * opponentPower;
                    
                    Color pixelColor = aiColor + opponentColor;
                    float alpha = pixelColor.a < 1f ? pixelColor.a : 1f;
                    pixelColor *= 0.5f;
                    pixelColor.a = alpha;

                    //pixelColor /= node.Scores.Length;
                    _mapTexture.SetPixel(pixelXIndex, pixelYIndex, pixelColor);
                }
            }
        
            _mapTexture.Apply();

            _displayImage.material.mainTexture = _mapTexture;
        }

        public override void UpdateUI()
        {
            
        }
    }
}