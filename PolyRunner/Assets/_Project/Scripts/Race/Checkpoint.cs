using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;
    public bool isFinishLine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerRaceData>(out var data))
            CheckpointSystem.Instance?.OnPlayerHitCheckpoint(data, checkpointIndex, isFinishLine);
    }
}