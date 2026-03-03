using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class TrackGenerator : NetworkBehaviour
{
    [SerializeField] private TrackChunkDatabase database;
    [SerializeField] private int chunksPerLap = 12;
    [SerializeField] private int totalLaps = 3;
    [SerializeField] private Transform trackRoot;

    private readonly SyncVar<int> _seed = new SyncVar<int>(0);
    private List<TrackChunk> _spawnedChunks = new List<TrackChunk>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        _seed.Value = Random.Range(1000, 99999);
        GenerateTrack();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsServerInitialized) GenerateTrack();
    }

    private void GenerateTrack()
    {
        foreach (var c in _spawnedChunks)
            if (c != null) Destroy(c.gameObject);
        _spawnedChunks.Clear();

        var rng = new System.Random(_seed.Value);
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;
        float difficulty = 0f;
        int total = chunksPerLap * totalLaps;

        for (int i = 0; i < total; i++)
        {
            difficulty = Mathf.Clamp01((float)i / total);
            TrackChunk prefab = database.GetRandom(rng, difficulty);
            TrackChunk chunk = Instantiate(prefab, trackRoot);

            // Align chunk entry to current position
            Vector3 entryOffset = chunk.transform.position - chunk.entryNode.position;
            chunk.transform.position = pos + entryOffset;
            chunk.transform.rotation = rot;

            // Next position is this chunk's exit
            pos = chunk.exitNode.position;
            rot = chunk.exitNode.rotation;

            _spawnedChunks.Add(chunk);
        }
    }
}