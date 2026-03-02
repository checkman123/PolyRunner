using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HudUI : MonoBehaviour
{
    private UIDocument _doc;
    private Label _lapLabel;
    private Label _rankLabel;
    private Label _timerLabel;
    private Label _countdownLabel;
    private VisualElement _countdownOverlay;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        var root = _doc.rootVisualElement;

        _lapLabel = root.Q<Label>("lap-label");
        _rankLabel = root.Q<Label>("rank-label");
        _timerLabel = root.Q<Label>("timer-label");
        _countdownLabel = root.Q<Label>("countdown-label");
        _countdownOverlay = root.Q<VisualElement>("countdown-overlay");
    }

    private void Update()
    {
        if (RaceTimer.Instance != null)
        {
            float t = RaceTimer.Instance.ElapsedTime;
            _timerLabel.text = $"{(int)(t / 60):00}:{t % 60:00.0}";
        }

        if (RaceManager.Instance != null)
        {
            var state = RaceManager.Instance.CurrentState;
            _countdownOverlay.style.display = state == RaceState.Countdown ? DisplayStyle.Flex : DisplayStyle.None;
            if (state == RaceState.Countdown)
            {
                float cd = RaceManager.Instance.Countdown;
                _countdownLabel.text = cd > 0.5f ? ((int)cd + 1).ToString() : "GO!";
            }
        }

        // Local player stats
        var localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            _lapLabel.text = $"Lap {localPlayer.currentLap.Value + 1}/{CheckpointSystem.Instance?.TotalLaps}";
            _rankLabel.text = $"#{localPlayer.Rank}";
        }
    }

    private PlayerRaceData FindLocalPlayer()
    {
        foreach (var p in FindObjectsByType<PlayerRaceData>(FindObjectsSortMode.None))
            if (p.IsOwner) return p;
        return null;
    }
}