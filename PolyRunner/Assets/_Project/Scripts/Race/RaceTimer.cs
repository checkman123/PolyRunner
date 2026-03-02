using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class RaceTimer : NetworkBehaviour
{
    public static RaceTimer Instance { get; private set; }

    private readonly SyncVar<float> _elapsed = new SyncVar<float>(0f);
    private bool _running;

    public float ElapsedTime => _elapsed.Value;

    private void Awake() => Instance = this;

    [Server]
    public void StartTimer() => _running = true;

    [Server]
    public void StopTimer() => _running = false;

    private void FixedUpdate()
    {
        if (IsServer && _running)
            _elapsed.Value += Time.fixedDeltaTime;
    }
}