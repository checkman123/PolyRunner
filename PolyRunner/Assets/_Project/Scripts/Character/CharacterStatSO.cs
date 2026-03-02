using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "PolyRunner/Character Stat")]
public class CharacterStatSO : ScriptableObject
{
    [Header("Identity")]
    public string characterName = "Default";
    public int characterId = 0;

    [Header("Movement")]
    public float maxSpeed = 12f;
    public float acceleration = 18f;
    public float deceleration = 20f;

    [Header("Jump")]
    public float jumpForce = 9f;
    public float doubleJumpForce = 7f;

    [Header("Air")]
    public float airControl = 0.6f;

    [Header("Sprint")]
    public float sprintMultiplier = 1.5f;
    public float maxStamina = 3f;
    public float staminaRegen = 1f;

    [Header("Dash")]
    public float dashForce = 18f;
    public float dashCooldown = 1.2f;

    [Header("Physics")]
    public float mass = 1f;
}