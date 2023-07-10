using UnityEngine;
using UnityEngine.UI;

namespace RTS.UI
{
    public class Lifebar : MonoBehaviour
    {
        [SerializeField] private Transform _lifebar;
        [SerializeField] private RawImage _image;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private TMPro.TMP_Text _text;

        public void SetPercent(int life, int lifeMax)
        {
            float percent = Mathf.Clamp01(life / (float)lifeMax);
            Vector3 scale = _lifebar.localScale;
            scale.x = percent;
            _lifebar.localScale = scale;
            _image.color = _gradient.Evaluate(percent);

            _text.text = life.ToString() + " / " + lifeMax.ToString();
        }

    }
}