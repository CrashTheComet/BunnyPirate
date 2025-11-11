using UnityEngine;
using UnityEngine.UI;

public class MapSpaceButton : MonoBehaviour
{
    [SerializeField] GameMap _gameMap;

    public string spaceName;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetName(string name)
    {
        ClearEvents();

        spaceName = name;

        if (button == null)
            button = GetComponent<Button>();
        if (_gameMap == null)
            _gameMap = FindFirstObjectByType<GameMap>();

        button.onClick.AddListener(() => 
        { 
            _gameMap.SelectMapSpace(name); 
        });
    }

    private void ClearEvents()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
    }
}
