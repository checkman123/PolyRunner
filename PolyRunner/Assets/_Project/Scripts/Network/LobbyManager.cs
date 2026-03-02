using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private int minPlayersToStart = 1;

    private readonly SyncList<string> _playerNames = new SyncList<string>();
    public IReadOnlyList<string> PlayerNames => _playerNames;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _playerNames.OnChange += OnListChanged;
    }

    private void OnListChanged(SyncListOperation op, int index, string prev, string next, bool asServer)
    {
        LobbyUI.Instance?.RefreshPlayerList(_playerNames);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestJoin(string playerName)
    {
        if (_playerNames.Count < 12)
            _playerNames.Add(playerName);
    }

    [Server]
    public void TryStartRace()
    {
        if (_playerNames.Count >= minPlayersToStart)
            StartRaceObserversRpc();
    }

    [ObserversRpc]
    private void StartRaceObserversRpc()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Race");
    }
}