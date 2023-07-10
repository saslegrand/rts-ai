using UnityEngine;

namespace RTS.UI
{
    public class Billboard : MonoBehaviour
    {
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            transform.rotation = _cam.transform.rotation;
        }
    }
}

