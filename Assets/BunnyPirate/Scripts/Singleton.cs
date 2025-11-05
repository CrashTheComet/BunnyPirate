using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
  private static T _instance;
  [SerializeField] bool destroyOnLoad = false;
  public static T instance => _instance;

  protected virtual void Awake()
  {
    if (_instance == null)
    {
      _instance = (T)this;
      if (transform.parent == null && !destroyOnLoad)
        DontDestroyOnLoad(this);
      return;
    }
    Destroy(gameObject);
  }

  public static bool verify => _instance != null;
}
