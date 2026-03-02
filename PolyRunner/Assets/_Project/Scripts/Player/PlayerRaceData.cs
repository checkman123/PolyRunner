using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerRaceData : NetworkBehaviour
{
    public readonly SyncVar<int> currentLap = new SyncVar<int>(0);
    public readonly SyncVar<int> checkpointIndex = new SyncVar<int>(0);
    public readonly SyncVar<float> distanceToNext = new SyncVar<float>(float.MaxValue);
    public readonly SyncVar<bool> hasFinished = new SyncVar<bool>(false);
    public readonly SyncVar<float> finishTime = new SyncVar<float>(0f);
    public readonly SyncVar<string> playerName = new SyncVar<string>("Player");

    public int Rank { get; set; }

    public int GetRaceScore(int totalLaps)
    {
        if (hasFinished.Value) return int.MaxValue;
        return currentLap.Value * 10000 + checkpointIndex.Value * 100 + (int)(100f - Mathf.Clamp(distanceToNext.Value, 0, 100f));
    }
}