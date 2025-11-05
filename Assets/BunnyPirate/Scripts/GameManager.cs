using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{

  public static void LoadScene(string scene)
  {
    SceneManager.LoadScene(scene);
  }
}
