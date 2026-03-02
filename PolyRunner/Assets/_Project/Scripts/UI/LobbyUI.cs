using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    private UIDocument _doc;
    private VisualElement _playerList;

    private void Awake()
    {
        Instance = this;
        _doc = GetComponent<UIDocument>();
        _playerList = _doc.rootVisualElement.Q<VisualElement>("player-list");
    }

    public void RefreshPlayerList(IReadOnlyList<string> names)
    {
        _playerList.Clear();
        foreach (var n in names)
        {
            var lbl = new Label(n);
            lbl.AddToClassList("player-entry");
            _playerList.Add(lbl);
        }
    }
}