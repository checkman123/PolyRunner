using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RaceResultsUI : MonoBehaviour
{
    private UIDocument _doc;
    private VisualElement _resultsList;
    private VisualElement _root;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        _root = _doc.rootVisualElement;
        _resultsList = _root.Q<VisualElement>("results-list");
        _root.style.display = DisplayStyle.None;
    }

    public void Show(List<PlayerRaceData> ranking)
    {
        _root.style.display = DisplayStyle.Flex;
        _resultsList.Clear();

        for (int i = 0; i < ranking.Count; i++)
        {
            var row = new Label($"#{i + 1}  {ranking[i].playerName.Value}  {ranking[i].finishTime.Value:0.0}s");
            row.AddToClassList("result-row");
            _resultsList.Add(row);
        }
    }
}