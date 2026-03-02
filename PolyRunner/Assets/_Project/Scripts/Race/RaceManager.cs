using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public enum RaceState { Waiting, Countdown, Racing, Finished }

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance { get; private set; }

    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private float resultsDelay = 5f;
    [SerializeField] private List<Transform> spawnPoints;

    private readonly SyncVar<RaceState> _state = new SyncVar<RaceState>(RaceState.Waiting);
    private readonly SyncVar<float> _countdownValue = new SyncVar<float>(3f);

    private List<PlayerRaceData> _finishers = new List<PlayerRaceData>();

    public RaceState CurrentState => _state.Value;
    public float Countdown => _countdownValue.Value;

    private void Awake() => Instance = this;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(RaceFlow());
    }

    [Server]
    private IEnumerator RaceFlow()
    {
        _state.Value = RaceState.Waiting;
        yield return new WaitForSeconds(2f);

        _state.Value = RaceState.Countdown;
        float t = countdownDuration;
        while (t > 0f)
        {
            _countdownValue.Value = t;
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }
        _countdownValue.Value = 0f;

        _state.Value = RaceState.Racing;
        RaceTimer.Instance?.StartTimer();
        RaceRanking.Instance?.SetTotalLaps(CheckpointSystem.Instance != null ? CheckpointSystem.Instance.TotalLaps : 3);

        // Update ranking every 0.5s
        while (_state.Value == RaceState.Racing)
        {
            RaceRanking.Instance?.UpdateRanking();
            yield return new WaitForSeconds(0.5f);
        }
    }

    [Server]
    public void OnPlayerFinished(PlayerRaceData player)
    {
        _finishers.Add(player);
        if (_finishers.Count >= 1) // Could gate on all players finishing
        {
            // Continue until all finish or timeout
        }
        // If all done
        // StartCoroutine(ShowResults());
    }

    public Transform GetSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return transform;
        return spawnPoints[Mathf.Clamp(index, 0, spawnPoints.Count - 1)];
    }
}