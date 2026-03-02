using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class RaceRanking : NetworkBehaviour
{
    public static RaceRanking Instance { get; private set; }

    private List<PlayerRaceData> _players = new List<PlayerRaceData>();
    private int _totalLaps;

    private void Awake() => Instance = this;

    public void RegisterPlayer(PlayerRaceData p) => _players.Add(p);
    public void UnregisterPlayer(PlayerRaceData p) => _players.Remove(p);

    public void SetTotalLaps(int laps) => _totalLaps = laps;

    [Server]
    public void UpdateRanking()
    {
        var sorted = _players
            .OrderByDescending(p => p.hasFinished.Value ? int.MaxValue : p.GetRaceScore(_totalLaps))
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
            sorted[i].Rank = i + 1;
    }

    public List<PlayerRaceData> GetRanking() => _players.OrderBy(p => p.Rank).ToList();
}