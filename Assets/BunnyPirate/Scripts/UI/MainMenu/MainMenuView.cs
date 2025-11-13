using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
  [SerializeField] Button _playButton;
  [SerializeField] string _playScene;

  void Awake()
  {
    _playButton.onClick.AddListener(
() => { GameManager.LoadScene(_playScene); }
    );
  }
}
