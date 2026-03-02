using UnityEngine;

[System.Serializable]
public struct WeightedChunk
{
    public TrackChunk prefab;
    [Range(0f, 1f)] public float weight;
    public float minDifficulty;
}

[CreateAssetMenu(fileName = "TrackChunkDatabase", menuName = "PolyRunner/Track Chunk Database")]
public class TrackChunkDatabase : ScriptableObject
{
    public WeightedChunk[] chunks;

    public TrackChunk GetRandom(System.Random rng, float difficulty)
    {
        var eligible = System.Array.FindAll(chunks, c => c.minDifficulty <= difficulty);
        if (eligible.Length == 0) return chunks[0].prefab;

        float total = 0f;
        foreach (var c in eligible) total += c.weight;
        float roll = (float)rng.NextDouble() * total;
        float acc = 0f;
        foreach (var c in eligible)
        {
            acc += c.weight;
            if (roll <= acc) return c.prefab;
        }
        return eligible[0].prefab;
    }
}