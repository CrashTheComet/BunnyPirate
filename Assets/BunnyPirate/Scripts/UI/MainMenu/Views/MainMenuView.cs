using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour, IMainMenu
{
  [SerializeField] Button PlayButton;

  public event Action OnPlay;

  void Awake()
  {
    PlayButton.onClick.AddListener(
() => { OnPlay?.Invoke(); }
    );
  }
}
