using System;

public interface IMainMenu
{
  event Action OnPlay;
}

public class MainMenuModel
{
  private readonly string playString = "2_GameScene";

  public void LoadGameplayScene()
  {
    GameManager.LoadScene(playString);
  }
}

public class MainMenuPresenter
{
  MainMenuModel _model;
  IMainMenu _view;

  public MainMenuPresenter(MainMenuModel model, IMainMenu view)
  {
    _model = model;
    _view = view;

    view.OnPlay += LoadGameplayScene;
  }

  private void LoadGameplayScene()
  {
    _model.LoadGameplayScene();
  }
}
