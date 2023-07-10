using UnityEngine;

public class Singleton<TType> : MonoBehaviour where TType : Singleton<TType>
{
    public static TType Instance{ get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        
        Instance = this as TType;
    }
}
