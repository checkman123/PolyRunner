using UnityEngine;

[CreateAssetMenu(fileName = "CharacterRegistry", menuName = "PolyRunner/Character Registry")]
public class CharacterRegistry : ScriptableObject
{
    public CharacterStatSO[] characters;

    public CharacterStatSO GetById(int id)
    {
        foreach (var c in characters)
            if (c.characterId == id) return c;
        return characters.Length > 0 ? characters[0] : null;
    }
}