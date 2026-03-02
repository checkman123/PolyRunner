using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class CheckpointSystem : NetworkBehaviour
{
    public static CheckpointSystem Instance { get; private set; }

    [SerializeField] private int totalLaps = 3;

    private readonly Dictionary<PlayerRaceData, int> _lastCheckpoint = new Dictionary<PlayerRaceData, int>();

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void OnPlayerHitCheckpoint(PlayerRaceData player, int index, bool isFinish)
    {
        if (!IsServer) return;
        if (player.hasFinished.Value) return;

        int expected = player.checkpointIndex.Value + 1;

        if (isFinish)
        {
            if (player.checkpointIndex.Value > 0)
            {
                player.currentLap.Value++;
                player.checkpointIndex.Value = 0;

                if (player.currentLap.Value >= totalLaps)
                {
                    player.hasFinished.Value = true;
                    player.finishTime.Value = RaceTimer.Instance != null ? RaceTimer.Instance.ElapsedTime : 0f;
                    RaceManager.Instance?.OnPlayerFinished(player);
                }
            }
            return;
        }

        if (index == expected)
        {
            player.checkpointIndex.Value = index;
        }
    }

    public int TotalLaps => totalLaps;
}