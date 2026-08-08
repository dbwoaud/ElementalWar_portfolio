using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance => instance;
    public static bool HasInstance => instance != null;

    protected virtual void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if(instance == this)
           instance = null;
    }
}
