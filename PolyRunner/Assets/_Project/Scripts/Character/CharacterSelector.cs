using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class CharacterSelector : NetworkBehaviour
{
    [SerializeField] private CharacterRegistry registry;

    private readonly SyncVar<int> _selectedCharId = new SyncVar<int>(0);

    public CharacterStatSO GetCurrentStats()
    {
        return registry.GetById(_selectedCharId.Value);
    }

    [ServerRpc(RequireOwnership = true)]
    public void RequestCharacter(int characterId)
    {
        if (registry.GetById(characterId) != null)
            _selectedCharId.Value = characterId;
    }
}