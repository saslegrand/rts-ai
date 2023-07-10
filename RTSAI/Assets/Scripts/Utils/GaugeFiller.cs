using UnityEngine;

public class GaugeFiller : MonoBehaviour
{
    public void SetGaugeValue(float value)
    {
        value = Mathf.Clamp(value, 0f, 1f);
        
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(value, scale.y, scale.z);
    }
}
