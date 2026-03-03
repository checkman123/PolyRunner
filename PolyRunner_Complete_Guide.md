# 🏁 PolyRunner — Complete Unity 6.3 + FishNet 4.6 Project Guide

> **Stack:** Unity 6.3 · FishNet 4.6 · Rigidbody Physics · New Input System · Cinemachine · UI Toolkit (UXML + USS)
>
> A fun, chaotic multiplayer arcade obstacle racing game. Fast, silly, and enjoyable with friends. Max 12 players per race.

---

## Table of Contents

1. [Folder Structure](#1-folder-structure)
2. [Scene Hierarchy](#2-scene-hierarchy)
3. [Core Architecture Overview](#3-core-architecture-overview)
4. [C# Scripts](#4-c-scripts)
   - [Character System](#41-character-system)
   - [Player System](#42-player-system)
   - [Race System](#43-race-system)
   - [Track System](#44-track-system)
   - [Obstacles](#45-obstacles)
   - [Network](#46-network)
   - [UI Scripts](#47-ui-scripts)
5. [UI Toolkit Files](#5-ui-toolkit-files)
   - [UXML](#51-uxml)
   - [USS](#52-uss)
6. [Input Actions](#6-input-actions)
7. [Inspector Setup](#7-inspector-setup)
8. [Step-by-Step Implementation Guide](#8-step-by-step-implementation-guide)
9. [Wall Running](#9-wall-running)
10. [Adding New Content](#10-adding-new-content)
11. [Quick Reference Checklist](#11-quick-reference-checklist)

---

## 1. Folder Structure

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Character/
│   │   │   ├── CharacterStatSO.cs
│   │   │   ├── CharacterRegistry.cs
│   │   │   └── CharacterSelector.cs
│   │   ├── Player/
│   │   │   ├── PlayerMovement.cs
│   │   │   ├── PlayerNetworkSync.cs
│   │   │   ├── PlayerRaceData.cs
│   │   │   ├── PlayerInputHandler.cs
│   │   │   └── WallRunHandler.cs
│   │   ├── Race/
│   │   │   ├── RaceManager.cs
│   │   │   ├── CheckpointSystem.cs
│   │   │   ├── Checkpoint.cs
│   │   │   ├── RaceRanking.cs
│   │   │   └── RaceTimer.cs
│   │   ├── Track/
│   │   │   ├── TrackGenerator.cs
│   │   │   ├── TrackChunk.cs
│   │   │   └── TrackChunkDatabase.cs
│   │   ├── Obstacles/
│   │   │   ├── MovingPlatform.cs
│   │   │   ├── RotatingBar.cs
│   │   │   ├── FallingPlatform.cs
│   │   │   ├── SwingingHammer.cs
│   │   │   ├── PushBlock.cs
│   │   │   ├── TrapFloor.cs
│   │   │   ├── JumpPad.cs
│   │   │   └── ConveyorBelt.cs
│   │   ├── Network/
│   │   │   ├── GameNetworkManager.cs
│   │   │   └── LobbyManager.cs
│   │   └── UI/
│   │       ├── MainMenuUI.cs
│   │       ├── LobbyUI.cs
│   │       ├── HudUI.cs
│   │       ├── RaceResultsUI.cs
│   │       └── CharacterSelectUI.cs
│   ├── ScriptableObjects/
│   │   └── Characters/
│   │       ├── Char_Default.asset
│   │       ├── Char_Speedy.asset
│   │       └── Char_Heavy.asset
│   ├── Prefabs/
│   │   ├── Player/
│   │   │   └── PlayerPrefab.prefab
│   │   ├── Track/
│   │   │   ├── Chunk_Straight.prefab
│   │   │   ├── Chunk_Curve.prefab
│   │   │   ├── Chunk_Obstacles.prefab
│   │   │   └── Chunk_Jump.prefab
│   │   └── Obstacles/
│   │       ├── MovingPlatform.prefab
│   │       ├── RotatingBar.prefab
│   │       ├── FallingPlatform.prefab
│   │       ├── SwingingHammer.prefab
│   │       ├── PushBlock.prefab
│   │       ├── TrapFloor.prefab
│   │       ├── JumpPad.prefab
│   │       └── ConveyorBelt.prefab
│   ├── UI/
│   │   ├── UXML/
│   │   │   ├── MainMenu.uxml
│   │   │   ├── Lobby.uxml
│   │   │   ├── CharacterSelect.uxml
│   │   │   ├── HUD.uxml
│   │   │   └── RaceResults.uxml
│   │   └── USS/
│   │       ├── Common.uss
│   │       ├── MainMenu.uss
│   │       ├── HUD.uss
│   │       └── RaceResults.uss
│   ├── Input/
│   │   └── PlayerInputActions.inputactions
│   └── Scenes/
│       ├── MainMenu.unity
│       ├── Lobby.unity
│       └── Race.unity
```

---

## 2. Scene Hierarchy

### MainMenu.unity
```
MainMenu
├── UIDocument          → MainMenu.uxml
├── NetworkManager
│   ├── FishNet NetworkManager component
│   └── GameNetworkManager.cs
└── AudioListener
```

### Lobby.unity
```
Lobby
├── UIDocument          → Lobby.uxml
├── UIDocument          → CharacterSelect.uxml
└── LobbyManager
```

### Race.unity
```
Race
├── RaceManager
│   ├── RaceManager.cs
│   ├── RaceRanking.cs
│   ├── RaceTimer.cs
│   └── CheckpointSystem.cs
├── TrackRoot           → [Generated Chunks at Runtime]
├── CheckpointRoot      → [Generated Checkpoints at Runtime]
├── SpawnPoints
│   └── SpawnPoint_0 .. SpawnPoint_11   (12 slots)
├── CinemachineRoot
│   └── CM_PlayerCamera  (CinemachineCamera)
├── UIDocument          → HUD.uxml       + HudUI.cs
├── UIDocument          → RaceResults.uxml + RaceResultsUI.cs
└── DirectionalLight
```

---

## 3. Core Architecture Overview

### Network Flow
```
Server boots
  → TrackGenerator runs (seed synced to all clients)
  → Players connect & join lobby
  → CharacterSelector validates character choice (server authoritative)
  → PlayerSpawner spawns PlayerPrefab per connection
  → RaceManager runs countdown
  → Race loop begins
  → RaceRanking updates every 0.5s
  → Players finish → Results screen
```

### Key Design Principles

| Principle | Detail |
|-----------|--------|
| Server authoritative | RaceManager, TrackGenerator, Ranking all run on server |
| Client smoothing | PlayerNetworkSync interpolates position/rotation for remote players |
| ScriptableObject stats | CharacterStatSO drives all movement values — no hardcoding |
| Modular obstacles | Each obstacle is a self-contained prefab + script |
| No MonoBehaviour bloat | Each system has one clear responsibility |

### System Relationships
```
CharacterStatSO ──────► PlayerMovement      (reads stats at spawn)
                  └───► WallRunHandler      (reads wall run stats)

RaceManager ──► CheckpointSystem ──► RaceRanking ──► HudUI
TrackGenerator ──► TrackChunk ──► Obstacles (auto-initialized as children)
PlayerInputHandler ──► PlayerMovement ──► PlayerNetworkSync
```

---

## 4. C# Scripts

---

### 4.1 Character System

#### `CharacterStatSO.cs`
```csharp
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

    [Header("Wall Run")]
    public float wallRunSpeed = 14f;
    public float wallRunGravity = 2f;
    public float wallRunDuration = 1.8f;
    public float wallRunJumpForce = 10f;
    public float wallRunJumpUpForce = 6f;
    public float wallRunMinSpeed = 4f;
    public float wallDetectDist = 0.75f;
}
```

---

#### `CharacterRegistry.cs`
```csharp
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
```

---

#### `CharacterSelector.cs`
```csharp
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
```

---

### 4.2 Player System

#### `PlayerInputHandler.cs`
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput   { get; private set; }
    public bool JumpPressed    { get; private set; }
    public bool JumpHeld       { get; private set; }
    public bool SprintHeld     { get; private set; }
    public bool DashPressed    { get; private set; }
    public bool SlideHeld      { get; private set; }

    private PlayerInputActions _actions;

    private void Awake()  => _actions = new PlayerInputActions();
    private void OnEnable()  => _actions.Enable();
    private void OnDisable() => _actions.Disable();

    private void Update()
    {
        MoveInput   = _actions.Player.Move.ReadValue<Vector2>();
        JumpHeld    = _actions.Player.Jump.IsPressed();
        SprintHeld  = _actions.Player.Sprint.IsPressed();
        SlideHeld   = _actions.Player.Slide.IsPressed();
        JumpPressed = _actions.Player.Jump.WasPressedThisFrame();
        DashPressed = _actions.Player.Dash.WasPressedThisFrame();
    }
}
```

---

#### `PlayerMovement.cs`
```csharp
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    // ── References ────────────────────────────────────────────────────────────
    private CharacterStatSO    _stats;
    private Rigidbody          _rb;
    private PlayerInputHandler _input;
    private WallRunHandler     _wallRun;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool  _isGrounded;
    private bool  _canDoubleJump;
    private int   _jumpCount;
    private float _stamina;
    private float _dashCooldownTimer;
    private bool  _isSliding;
    private bool  _gravityOverridden;

    // ── Exposed for WallRunHandler ────────────────────────────────────────────
    public bool IsGrounded => _isGrounded;

    // ── Inspector ─────────────────────────────────────────────────────────────
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float     groundRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float     wallCheckDist = 0.6f;
    [SerializeField] private LayerMask wallMask;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb      = GetComponent<Rigidbody>();
        _input   = GetComponent<PlayerInputHandler>();
        _wallRun = GetComponent<WallRunHandler>();
    }

    public void ApplyStats(CharacterStatSO stats)
    {
        _stats        = stats;
        _rb.mass      = stats.mass;
        _stamina      = stats.maxStamina;
        _wallRun?.ApplyStats(stats);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner) _input.enabled = false;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || _stats == null) return;

        CheckGround();
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleSlide();
        RegenStamina();
    }

    // ── Ground check ──────────────────────────────────────────────────────────
    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);
        if (_isGrounded)
        {
            _jumpCount    = 0;
            _canDoubleJump = true;
        }
    }

    // ── Movement ──────────────────────────────────────────────────────────────
    private void HandleMovement()
    {
        Vector3 camForward = Camera.main != null
            ? Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized
            : transform.forward;
        Vector3 camRight = Camera.main != null
            ? Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized
            : transform.right;

        Vector3 dir = (camForward * _input.MoveInput.y + camRight * _input.MoveInput.x).normalized;

        bool  isSprinting = _input.SprintHeld && _stamina > 0f && !_isSliding;
        float targetSpeed = _stats.maxSpeed * (isSprinting ? _stats.sprintMultiplier : 1f);
        if (isSprinting) _stamina -= Time.fixedDeltaTime;
        _stamina = Mathf.Clamp(_stamina, 0f, _stats.maxStamina);

        float control = _isGrounded ? 1f : _stats.airControl;

        if (dir.sqrMagnitude > 0.01f)
        {
            Vector3 targetVel = dir * targetSpeed;
            Vector3 current   = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            Vector3 delta     = (targetVel - current) * _stats.acceleration * control * Time.fixedDeltaTime;
            _rb.AddForce(delta, ForceMode.VelocityChange);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 12f * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 current = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(-current * _stats.deceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        // Clamp horizontal speed
        Vector3 hVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        if (hVel.magnitude > targetSpeed)
        {
            Vector3 clamped = hVel.normalized * targetSpeed;
            _rb.linearVelocity = new Vector3(clamped.x, _rb.linearVelocity.y, clamped.z);
        }
    }

    // ── Jump ──────────────────────────────────────────────────────────────────
    private void HandleJump()
    {
        // Wall run handles its own jump
        if (_wallRun != null && _wallRun.IsWallRunning) return;
        if (!_input.JumpPressed) return;

        // Wall jump (legacy, when not using WallRunHandler)
        bool touchingWall =
            Physics.Raycast(transform.position,  transform.right, wallCheckDist, wallMask) ||
            Physics.Raycast(transform.position, -transform.right, wallCheckDist, wallMask);

        if (!_isGrounded && touchingWall)
        {
            Vector3 wallNormal = Physics.Raycast(transform.position, -transform.right, wallCheckDist, wallMask)
                ? transform.right : -transform.right;
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce((wallNormal + Vector3.up).normalized * _stats.jumpForce, ForceMode.VelocityChange);
            return;
        }

        if (_isGrounded)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _stats.jumpForce, ForceMode.VelocityChange);
            _jumpCount = 1;
        }
        else if (_canDoubleJump && _jumpCount == 1)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * _stats.doubleJumpForce, ForceMode.VelocityChange);
            _canDoubleJump = false;
            _jumpCount     = 2;
        }
    }

    // ── Dash ──────────────────────────────────────────────────────────────────
    private void HandleDash()
    {
        _dashCooldownTimer -= Time.fixedDeltaTime;
        if (!_input.DashPressed || _dashCooldownTimer > 0f) return;

        Vector3 dir = Vector3.zero;
        if (Camera.main != null)
        {
            Vector3 cf = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            Vector3 cr = Vector3.ProjectOnPlane(Camera.main.transform.right,   Vector3.up).normalized;
            dir = (cf * _input.MoveInput.y + cr * _input.MoveInput.x).normalized;
        }
        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f, _rb.linearVelocity.z);
        _rb.AddForce(dir.normalized * _stats.dashForce, ForceMode.VelocityChange);
        _dashCooldownTimer = _stats.dashCooldown;
    }

    // ── Slide ─────────────────────────────────────────────────────────────────
    private void HandleSlide()
    {
        _isSliding = _input.SlideHeld && _isGrounded;
        if (_isSliding)
            _rb.AddForce(transform.forward * 5f, ForceMode.Acceleration);
    }

    // ── Stamina regen ─────────────────────────────────────────────────────────
    private void RegenStamina()
    {
        if (!_input.SprintHeld)
            _stamina = Mathf.Min(_stamina + _stats.staminaRegen * Time.fixedDeltaTime, _stats.maxStamina);
    }

    // ── Public API (called by WallRunHandler and Obstacles) ───────────────────
    public void ApplyKnockback(Vector3 force) =>
        _rb.AddForce(force, ForceMode.Impulse);

    public void SetGravityOverride(bool active)
    {
        _gravityOverridden = active;
        _rb.useGravity     = !active;
    }

    public void RestoreDoubleJump()
    {
        _canDoubleJump = true;
        _jumpCount     = 1;
    }

    public void ApplySpeedBoost(float multiplier, float duration) =>
        StartCoroutine(SpeedBoostRoutine(multiplier, duration));

    private System.Collections.IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        float orig = _stats.maxSpeed;
        _stats.maxSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        _stats.maxSpeed = orig;
    }
}
```

---

#### `PlayerNetworkSync.cs`
```csharp
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerNetworkSync : NetworkBehaviour
{
    [SerializeField] private float smoothSpeed = 15f;

    private Vector3    _targetPos;
    private Quaternion _targetRot;
    private Rigidbody  _rb;

    private readonly SyncVar<Vector3>    _serverPos = new SyncVar<Vector3>();
    private readonly SyncVar<Quaternion> _serverRot = new SyncVar<Quaternion>();

    private void Awake() => _rb = GetComponent<Rigidbody>();

    private void OnEnable()
    {
        _serverPos.OnChange += OnPosChanged;
        _serverRot.OnChange += OnRotChanged;
    }

    private void OnDisable()
    {
        _serverPos.OnChange -= OnPosChanged;
        _serverRot.OnChange -= OnRotChanged;
    }

    private void OnPosChanged(Vector3 prev, Vector3 next, bool asServer) => _targetPos = next;
    private void OnRotChanged(Quaternion prev, Quaternion next, bool asServer) => _targetRot = next;

    [Server]
    private void FixedUpdate()
    {
        if (_rb == null) return;
        _serverPos.Value = _rb.position;
        _serverRot.Value = _rb.rotation;
    }

    private void Update()
    {
        if (IsOwner || IsServer) return;
        transform.position = Vector3.Lerp(transform.position, _targetPos, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, smoothSpeed * Time.deltaTime);
    }
}
```

---

#### `PlayerRaceData.cs`
```csharp
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerRaceData : NetworkBehaviour
{
    public readonly SyncVar<int>    currentLap     = new SyncVar<int>(0);
    public readonly SyncVar<int>    checkpointIndex = new SyncVar<int>(0);
    public readonly SyncVar<float>  distanceToNext  = new SyncVar<float>(float.MaxValue);
    public readonly SyncVar<bool>   hasFinished     = new SyncVar<bool>(false);
    public readonly SyncVar<float>  finishTime      = new SyncVar<float>(0f);
    public readonly SyncVar<string> playerName      = new SyncVar<string>("Player");

    public int Rank { get; set; }

    public int GetRaceScore(int totalLaps)
    {
        if (hasFinished.Value) return int.MaxValue;
        return currentLap.Value * 10000
             + checkpointIndex.Value * 100
             + (int)(100f - Mathf.Clamp(distanceToNext.Value, 0, 100f));
    }
}
```

---

#### `WallRunHandler.cs`
```csharp
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
public class WallRunHandler : NetworkBehaviour
{
    [Header("Detection Layers")]
    [SerializeField] private LayerMask wallMask;

    private Rigidbody          _rb;
    private PlayerMovement     _movement;
    private PlayerInputHandler _input;
    private CharacterStatSO    _stats;

    private bool    _isWallRunning;
    private bool    _onLeftWall;
    private bool    _onRightWall;
    private float   _wallRunTimer;
    private bool    _wallJumpedThisRun;
    private bool    _canWallRun = true;
    private Vector3 _wallNormal;

    public bool    IsWallRunning => _isWallRunning;
    public Vector3 WallNormal    => _wallNormal;

    private void Awake()
    {
        _rb       = GetComponent<Rigidbody>();
        _movement = GetComponent<PlayerMovement>();
        _input    = GetComponent<PlayerInputHandler>();
    }

    public void ApplyStats(CharacterStatSO stats) => _stats = stats;

    private void FixedUpdate()
    {
        if (!IsOwner || _stats == null) return;

        DetectWalls(out bool leftHit, out bool rightHit, out RaycastHit wallHit);
        UpdateWallRunState(leftHit, rightHit, wallHit);
        ApplyWallRunPhysics();
        HandleWallJump();
    }

    // ── Wall detection ────────────────────────────────────────────────────────
    private void DetectWalls(out bool leftHit, out bool rightHit, out RaycastHit hit)
    {
        float dist = _stats.wallDetectDist;
        leftHit  = Physics.Raycast(transform.position, -transform.right, out RaycastHit lHit, dist, wallMask);
        rightHit = Physics.Raycast(transform.position,  transform.right, out RaycastHit rHit, dist, wallMask);
        hit = rightHit ? rHit : lHit;
    }

    // ── State machine ─────────────────────────────────────────────────────────
    private void UpdateWallRunState(bool leftHit, bool rightHit, RaycastHit wallHit)
    {
        bool playerMovingForward = _input.MoveInput.y > 0.2f;
        bool hasEnoughSpeed      = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude
                                   >= _stats.wallRunMinSpeed;
        bool isAirborne          = !_movement.IsGrounded;
        bool wallPresent         = leftHit || rightHit;

        if (!_isWallRunning && wallPresent && isAirborne &&
            playerMovingForward && hasEnoughSpeed && _canWallRun)
        {
            StartWallRun(leftHit, rightHit, wallHit);
        }

        if (_isWallRunning)
        {
            _wallRunTimer -= Time.fixedDeltaTime;

            bool timedOut  = _wallRunTimer <= 0f;
            bool wallGone  = !wallPresent;
            bool grounded  = _movement.IsGrounded;
            bool movedAway = _input.MoveInput.y < 0.1f;

            if (timedOut || wallGone || grounded || movedAway)
                StopWallRun(wallGone);
        }
    }

    private void StartWallRun(bool leftHit, bool rightHit, RaycastHit wallHit)
    {
        _isWallRunning     = true;
        _onLeftWall        = leftHit && !rightHit;
        _onRightWall       = rightHit;
        _wallRunTimer      = _stats.wallRunDuration;
        _wallJumpedThisRun = false;
        _wallNormal        = wallHit.normal;

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _movement.SetGravityOverride(true);
    }

    private void StopWallRun(bool wallGone)
    {
        _isWallRunning = false;
        _onLeftWall    = false;
        _onRightWall   = false;
        _movement.SetGravityOverride(false);

        if (!wallGone) StartCoroutine(WallRunCooldown());
    }

    // ── Physics ───────────────────────────────────────────────────────────────
    private void ApplyWallRunPhysics()
    {
        if (!_isWallRunning) return;

        // Stick to wall
        _rb.AddForce(-_wallNormal * 4f, ForceMode.Acceleration);

        // Run along wall surface
        Vector3 wallForward = Vector3.Cross(_wallNormal, Vector3.up).normalized;
        if (Vector3.Dot(wallForward, transform.forward) < 0f)
            wallForward = -wallForward;

        Vector3 targetVel   = wallForward * _stats.wallRunSpeed;
        Vector3 currentHVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velDelta    = (targetVel - currentHVel) * 8f * Time.fixedDeltaTime;
        _rb.AddForce(velDelta, ForceMode.VelocityChange);

        // Reduced gravity
        _rb.AddForce(Vector3.down * _stats.wallRunGravity, ForceMode.Acceleration);
    }

    // ── Wall jump ─────────────────────────────────────────────────────────────
    private void HandleWallJump()
    {
        if (!_isWallRunning || _wallJumpedThisRun || !_input.JumpPressed) return;

        _wallJumpedThisRun = true;

        Vector3 jumpDir = (_wallNormal * _stats.wallRunJumpForce) +
                          (Vector3.up   * _stats.wallRunJumpUpForce);

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x * 0.5f, 0f, _rb.linearVelocity.z * 0.5f);
        _rb.AddForce(jumpDir, ForceMode.VelocityChange);

        _movement.RestoreDoubleJump();
        StopWallRun(true);
    }

    // ── Cooldown ──────────────────────────────────────────────────────────────
    private System.Collections.IEnumerator WallRunCooldown()
    {
        _canWallRun = false;
        yield return new WaitForSeconds(0.4f);
        _canWallRun = true;
    }
}
```

---

### 4.3 Race System

#### `Checkpoint.cs`
```csharp
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int  checkpointIndex;
    public bool isFinishLine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerRaceData>(out var data))
            CheckpointSystem.Instance?.OnPlayerHitCheckpoint(data, checkpointIndex, isFinishLine);
    }
}
```

---

#### `CheckpointSystem.cs`
```csharp
using FishNet.Object;
using UnityEngine;

public class CheckpointSystem : NetworkBehaviour
{
    public static CheckpointSystem Instance { get; private set; }

    [SerializeField] private int totalLaps = 3;
    public int TotalLaps => totalLaps;

    private void Awake() => Instance = this;

    [Server]
    public void OnPlayerHitCheckpoint(PlayerRaceData player, int index, bool isFinish)
    {
        if (!IsServer || player.hasFinished.Value) return;

        if (isFinish)
        {
            if (player.checkpointIndex.Value > 0)
            {
                player.currentLap.Value++;
                player.checkpointIndex.Value = 0;

                if (player.currentLap.Value >= totalLaps)
                {
                    player.hasFinished.Value = true;
                    player.finishTime.Value  = RaceTimer.Instance != null
                        ? RaceTimer.Instance.ElapsedTime : 0f;
                    RaceManager.Instance?.OnPlayerFinished(player);
                }
            }
            return;
        }

        int expected = player.checkpointIndex.Value + 1;
        if (index == expected)
            player.checkpointIndex.Value = index;
    }
}
```

---

#### `RaceTimer.cs`
```csharp
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

    [Server] public void StartTimer() => _running = true;
    [Server] public void StopTimer()  => _running = false;

    private void FixedUpdate()
    {
        if (IsServer && _running)
            _elapsed.Value += Time.fixedDeltaTime;
    }
}
```

---

#### `RaceRanking.cs`
```csharp
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using UnityEngine;

public class RaceRanking : NetworkBehaviour
{
    public static RaceRanking Instance { get; private set; }

    private readonly List<PlayerRaceData> _players = new List<PlayerRaceData>();
    private int _totalLaps;

    private void Awake() => Instance = this;

    public void RegisterPlayer(PlayerRaceData p)   => _players.Add(p);
    public void UnregisterPlayer(PlayerRaceData p) => _players.Remove(p);
    public void SetTotalLaps(int laps)             => _totalLaps = laps;

    [Server]
    public void UpdateRanking()
    {
        var sorted = _players
            .OrderByDescending(p => p.hasFinished.Value
                ? int.MaxValue
                : p.GetRaceScore(_totalLaps))
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
            sorted[i].Rank = i + 1;
    }

    public List<PlayerRaceData> GetRanking() =>
        _players.OrderBy(p => p.Rank).ToList();
}
```

---

#### `RaceManager.cs`
```csharp
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public enum RaceState { Waiting, Countdown, Racing, Finished }

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance { get; private set; }

    [SerializeField] private float           countdownDuration = 3f;
    [SerializeField] private List<Transform> spawnPoints;

    private readonly SyncVar<RaceState> _state          = new SyncVar<RaceState>(RaceState.Waiting);
    private readonly SyncVar<float>     _countdownValue = new SyncVar<float>(3f);
    private readonly List<PlayerRaceData> _finishers    = new List<PlayerRaceData>();

    public RaceState CurrentState => _state.Value;
    public float     Countdown    => _countdownValue.Value;

    private void Awake() => Instance = this;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(RaceFlow());
    }

    [Server]
    private IEnumerator RaceFlow()
    {
        _state.Value = RaceState.Waiting;
        yield return new WaitForSeconds(2f);

        _state.Value = RaceState.Countdown;
        float t = countdownDuration;
        while (t > 0f)
        {
            _countdownValue.Value = t;
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }
        _countdownValue.Value = 0f;

        _state.Value = RaceState.Racing;
        RaceTimer.Instance?.StartTimer();
        RaceRanking.Instance?.SetTotalLaps(
            CheckpointSystem.Instance != null ? CheckpointSystem.Instance.TotalLaps : 3);

        while (_state.Value == RaceState.Racing)
        {
            RaceRanking.Instance?.UpdateRanking();
            yield return new WaitForSeconds(0.5f);
        }
    }

    [Server]
    public void OnPlayerFinished(PlayerRaceData player)
    {
        _finishers.Add(player);
    }

    public Transform GetSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return transform;
        return spawnPoints[Mathf.Clamp(index, 0, spawnPoints.Count - 1)];
    }
}
```

---

### 4.4 Track System

#### `TrackChunk.cs`
```csharp
using UnityEngine;

public class TrackChunk : MonoBehaviour
{
    public Transform entryNode;
    public Transform exitNode;
    [Range(0f, 1f)] public float difficultyTier = 0f;
}
```

---

#### `TrackChunkDatabase.cs`
```csharp
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
        float acc  = 0f;
        foreach (var c in eligible)
        {
            acc += c.weight;
            if (roll <= acc) return c.prefab;
        }
        return eligible[0].prefab;
    }
}
```

---

#### `TrackGenerator.cs`
```csharp
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class TrackGenerator : NetworkBehaviour
{
    [SerializeField] private TrackChunkDatabase database;
    [SerializeField] private int                chunksPerLap = 12;
    [SerializeField] private int                totalLaps    = 3;
    [SerializeField] private Transform          trackRoot;

    private readonly SyncVar<int>     _seed          = new SyncVar<int>(0);
    private readonly List<TrackChunk> _spawnedChunks = new List<TrackChunk>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        _seed.Value = Random.Range(1000, 99999);
        GenerateTrack();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsServer) GenerateTrack();
    }

    private void GenerateTrack()
    {
        foreach (var c in _spawnedChunks)
            if (c != null) Destroy(c.gameObject);
        _spawnedChunks.Clear();

        var     rng        = new System.Random(_seed.Value);
        Vector3    pos     = Vector3.zero;
        Quaternion rot     = Quaternion.identity;
        int        total   = chunksPerLap * totalLaps;

        for (int i = 0; i < total; i++)
        {
            float      difficulty = Mathf.Clamp01((float)i / total);
            TrackChunk prefab     = database.GetRandom(rng, difficulty);
            TrackChunk chunk      = Instantiate(prefab, trackRoot);

            Vector3 entryOffset = chunk.transform.position - chunk.entryNode.position;
            chunk.transform.position = pos + entryOffset;
            chunk.transform.rotation = rot;

            pos = chunk.exitNode.position;
            rot = chunk.exitNode.rotation;

            _spawnedChunks.Add(chunk);
        }
    }
}
```

---

### 4.5 Obstacles

#### `MovingPlatform.cs`
```csharp
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 moveOffset = new Vector3(3f, 0, 0);
    [SerializeField] private float   speed      = 2f;

    private Vector3 _startPos;
    private float   _t;

    private void Start() => _startPos = transform.position;

    private void FixedUpdate()
    {
        _t += Time.fixedDeltaTime * speed;
        float ping = Mathf.PingPong(_t, 1f);
        transform.position = Vector3.Lerp(_startPos, _startPos + moveOffset, ping);
    }
}
```

---

#### `RotatingBar.cs`
```csharp
using UnityEngine;

public class RotatingBar : MonoBehaviour
{
    [SerializeField] private float   rotationSpeed = 90f;
    [SerializeField] private Vector3 axis          = Vector3.up;

    private void FixedUpdate() =>
        transform.Rotate(axis, rotationSpeed * Time.fixedDeltaTime, Space.World);
}
```

---

#### `FallingPlatform.cs`
```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay  = 0.8f;
    [SerializeField] private float resetTime  = 4f;

    private Rigidbody  _rb;
    private Vector3    _startPos;
    private Quaternion _startRot;
    private bool       _triggered;

    private void Awake()
    {
        _rb            = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _startPos       = transform.position;
        _startRot       = transform.rotation;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (_triggered || col.gameObject.GetComponent<PlayerMovement>() == null) return;
        _triggered = true;
        Invoke(nameof(Fall),  fallDelay);
        Invoke(nameof(Reset), resetTime);
    }

    private void Fall()  => _rb.isKinematic = false;

    private void Reset()
    {
        _rb.isKinematic    = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(_startPos, _startRot);
        _triggered = false;
    }
}
```

---

#### `SwingingHammer.cs`
```csharp
using UnityEngine;

public class SwingingHammer : MonoBehaviour
{
    [SerializeField] private float swingAngle = 60f;
    [SerializeField] private float swingSpeed = 1.5f;

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;
        transform.localRotation = Quaternion.Euler(angle, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerMovement>(out var pm))
            pm.ApplyKnockback(transform.forward * 12f + Vector3.up * 5f);
    }
}
```

---

#### `PushBlock.cs`
```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushBlock : MonoBehaviour
{
    [SerializeField] private float pushForce = 15f;

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.TryGetComponent<PlayerMovement>(out var pm))
        {
            Vector3 dir = (col.transform.position - transform.position).normalized;
            pm.ApplyKnockback(dir * pushForce);
        }
    }
}
```

---

#### `TrapFloor.cs`
```csharp
using UnityEngine;

public class TrapFloor : MonoBehaviour
{
    [SerializeField] private float openDelay  = 0.3f;
    [SerializeField] private float closeDelay = 2f;

    private bool         _open;
    private Collider     _col;
    private MeshRenderer _rend;

    private void Awake()
    {
        _col  = GetComponent<Collider>();
        _rend = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_open || other.GetComponent<PlayerMovement>() == null) return;
        Invoke(nameof(Open),  openDelay);
        Invoke(nameof(Close), openDelay + closeDelay);
    }

    private void Open()
    {
        _open         = true;
        _col.enabled  = false;
        if (_rend) _rend.enabled = false;
    }

    private void Close()
    {
        _open         = false;
        _col.enabled  = true;
        if (_rend) _rend.enabled = true;
    }
}
```

---

#### `JumpPad.cs`
```csharp
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] private float launchForce = 20f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;
            rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
        }
    }
}
```

---

#### `ConveyorBelt.cs`
```csharp
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private Vector3 beltDirection = Vector3.forward;
    [SerializeField] private float   speed         = 5f;

    private void OnCollisionStay(Collision col)
    {
        if (col.rigidbody != null)
            col.rigidbody.AddForce(beltDirection.normalized * speed, ForceMode.Acceleration);
    }
}
```

---

### 4.6 Network

#### `GameNetworkManager.cs`
```csharp
using FishNet.Managing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : MonoBehaviour
{
    public static GameNetworkManager Instance { get; private set; }

    [SerializeField] private NetworkManager fishNetManager;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartHost()
    {
        fishNetManager.ServerManager.StartConnection();
        fishNetManager.ClientManager.StartConnection();
        SceneManager.LoadScene("Race");
    }

    public void StartClient(string address)
    {
        fishNetManager.ClientManager.StartConnection(address);
        SceneManager.LoadScene("Race");
    }

    public void Disconnect()
    {
        fishNetManager.ClientManager.StopConnection();
        fishNetManager.ServerManager.StopConnection();
        SceneManager.LoadScene("MainMenu");
    }
}
```

---

#### `LobbyManager.cs`
```csharp
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private int minPlayersToStart = 1;

    private readonly SyncList<string> _playerNames = new SyncList<string>();
    public IReadOnlyList<string> PlayerNames => _playerNames;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _playerNames.OnChange += OnListChanged;
    }

    private void OnListChanged(SyncListOperation op, int index,
        string prev, string next, bool asServer)
    {
        LobbyUI.Instance?.RefreshPlayerList(_playerNames);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestJoin(string playerName)
    {
        if (_playerNames.Count < 12)
            _playerNames.Add(playerName);
    }

    [Server]
    public void TryStartRace()
    {
        if (_playerNames.Count >= minPlayersToStart)
            StartRaceObserversRpc();
    }

    [ObserversRpc]
    private void StartRaceObserversRpc() =>
        UnityEngine.SceneManagement.SceneManager.LoadScene("Race");
}
```

---

#### `PlayerSpawner.cs`
```csharp
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private RaceManager   raceManager;

    private int _spawnIndex = 0;

    private void Start()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnClientConnected;
    }

    private void OnClientConnected(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Transform spawnPoint = raceManager.GetSpawnPoint(_spawnIndex++);
            NetworkObject nob = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            InstanceFinder.ServerManager.Spawn(nob, conn);
        }
    }
}
```

---

### 4.7 UI Scripts

#### `MainMenuUI.cs`
```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuUI : MonoBehaviour
{
    private UIDocument _doc;
    private TextField  _addressField;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        var root = _doc.rootVisualElement;

        _addressField = root.Q<TextField>("address-field");

        root.Q<Button>("host-btn").clicked +=
            () => GameNetworkManager.Instance?.StartHost();

        root.Q<Button>("join-btn").clicked +=
            () => GameNetworkManager.Instance?.StartClient(_addressField.value);
    }
}
```

---

#### `LobbyUI.cs`
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    private UIDocument     _doc;
    private VisualElement  _playerList;

    private void Awake()
    {
        Instance    = this;
        _doc        = GetComponent<UIDocument>();
        _playerList = _doc.rootVisualElement.Q<VisualElement>("player-list");
    }

    public void RefreshPlayerList(IReadOnlyList<string> names)
    {
        _playerList.Clear();
        foreach (var n in names)
        {
            var lbl = new Label(n);
            lbl.AddToClassList("player-entry");
            _playerList.Add(lbl);
        }
    }
}
```

---

#### `HudUI.cs`
```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class HudUI : MonoBehaviour
{
    private UIDocument    _doc;
    private Label         _lapLabel;
    private Label         _rankLabel;
    private Label         _timerLabel;
    private Label         _countdownLabel;
    private VisualElement _countdownOverlay;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        var root = _doc.rootVisualElement;

        _lapLabel         = root.Q<Label>("lap-label");
        _rankLabel        = root.Q<Label>("rank-label");
        _timerLabel       = root.Q<Label>("timer-label");
        _countdownLabel   = root.Q<Label>("countdown-label");
        _countdownOverlay = root.Q<VisualElement>("countdown-overlay");
    }

    private void Update()
    {
        // Timer
        if (RaceTimer.Instance != null)
        {
            float t = RaceTimer.Instance.ElapsedTime;
            _timerLabel.text = $"{(int)(t / 60):00}:{t % 60:00.0}";
        }

        // Countdown overlay
        if (RaceManager.Instance != null)
        {
            var state = RaceManager.Instance.CurrentState;
            _countdownOverlay.style.display =
                state == RaceState.Countdown ? DisplayStyle.Flex : DisplayStyle.None;

            if (state == RaceState.Countdown)
            {
                float cd = RaceManager.Instance.Countdown;
                _countdownLabel.text = cd > 0.5f ? ((int)cd + 1).ToString() : "GO!";
            }
        }

        // Local player stats
        var local = FindLocalPlayer();
        if (local != null)
        {
            _lapLabel.text  = $"Lap {local.currentLap.Value + 1}/{CheckpointSystem.Instance?.TotalLaps}";
            _rankLabel.text = $"#{local.Rank}";
        }
    }

    private PlayerRaceData FindLocalPlayer()
    {
        foreach (var p in FindObjectsByType<PlayerRaceData>(FindObjectsSortMode.None))
            if (p.IsOwner) return p;
        return null;
    }
}
```

---

#### `RaceResultsUI.cs`
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RaceResultsUI : MonoBehaviour
{
    private UIDocument    _doc;
    private VisualElement _resultsList;
    private VisualElement _root;

    private void Awake()
    {
        _doc         = GetComponent<UIDocument>();
        _root        = _doc.rootVisualElement;
        _resultsList = _root.Q<VisualElement>("results-list");
        _root.style.display = DisplayStyle.None;
    }

    public void Show(List<PlayerRaceData> ranking)
    {
        _root.style.display = DisplayStyle.Flex;
        _resultsList.Clear();

        for (int i = 0; i < ranking.Count; i++)
        {
            var row = new Label(
                $"#{i + 1}  {ranking[i].playerName.Value}  {ranking[i].finishTime.Value:0.0}s");
            row.AddToClassList("result-row");
            _resultsList.Add(row);
        }
    }
}
```

---

#### `CharacterSelectUI.cs`
```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private CharacterRegistry registry;

    private UIDocument       _doc;
    private VisualElement    _container;
    private CharacterSelector _selector;

    private void Awake()
    {
        _doc       = GetComponent<UIDocument>();
        _container = _doc.rootVisualElement.Q<VisualElement>("char-container");
    }

    private void Start()
    {
        foreach (var nob in FindObjectsByType<CharacterSelector>(FindObjectsSortMode.None))
        {
            if (nob.IsOwner) { _selector = nob; break; }
        }
        BuildUI();
    }

    private void BuildUI()
    {
        _container.Clear();
        foreach (var ch in registry.characters)
        {
            int id  = ch.characterId;
            var btn = new Button(() => _selector?.RequestCharacter(id));
            btn.text = ch.characterName;
            btn.AddToClassList("char-btn");
            _container.Add(btn);
        }
    }
}
```

---

## 5. UI Toolkit Files

### 5.1 UXML

#### `HUD.uxml`
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="hud-root" class="hud-root">
    <ui:Label name="lap-label"   text="Lap 1/3" class="hud-label top-left"/>
    <ui:Label name="rank-label"  text="#1"       class="hud-label top-right"/>
    <ui:Label name="timer-label" text="00:00.0"  class="hud-label top-center"/>
    <ui:VisualElement name="countdown-overlay" class="countdown-overlay">
      <ui:Label name="countdown-label" text="3" class="countdown-text"/>
    </ui:VisualElement>
  </ui:VisualElement>
</ui:UXML>
```

#### `MainMenu.uxml`
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="menu-root" class="menu-root">
    <ui:Label text="POLY RUNNER" class="title"/>
    <ui:TextField name="address-field" label="Server IP" value="localhost" class="input-field"/>
    <ui:Button name="host-btn" text="HOST GAME" class="menu-btn"/>
    <ui:Button name="join-btn" text="JOIN GAME" class="menu-btn"/>
  </ui:VisualElement>
</ui:UXML>
```

#### `Lobby.uxml`
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="lobby-root" class="lobby-root">
    <ui:Label text="LOBBY" class="title"/>
    <ui:VisualElement name="player-list" class="player-list"/>
    <ui:Button name="start-btn" text="START RACE" class="menu-btn"/>
  </ui:VisualElement>
</ui:UXML>
```

#### `CharacterSelect.uxml`
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="charselect-root" class="charselect-root">
    <ui:Label text="SELECT CHARACTER" class="title"/>
    <ui:VisualElement name="char-container" class="char-container"/>
  </ui:VisualElement>
</ui:UXML>
```

#### `RaceResults.uxml`
```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
  <ui:VisualElement name="results-root" class="results-root">
    <ui:Label text="RACE RESULTS" class="title"/>
    <ui:VisualElement name="results-list" class="results-list"/>
    <ui:Button name="menu-btn" text="MAIN MENU" class="menu-btn"/>
  </ui:VisualElement>
</ui:UXML>
```

---

### 5.2 USS

#### `Common.uss`
```css
.title {
    font-size: 42px;
    color: #FFFFFF;
    -unity-font-style: bold;
    margin-bottom: 20px;
    -unity-text-align: upper-center;
}

.menu-btn {
    width: 280px;
    height: 52px;
    font-size: 20px;
    margin: 8px auto;
    background-color: #FF5722;
    color: white;
    border-radius: 8px;
    border-width: 0;
}

.menu-btn:hover {
    background-color: #FF7043;
}

.input-field {
    width: 280px;
    margin: 8px auto;
    font-size: 16px;
    color: white;
    background-color: #333333;
    border-radius: 6px;
}
```

#### `HUD.uss`
```css
.hud-root {
    position: absolute;
    top: 0; left: 0;
    width: 100%;
    height: 100%;
}

.hud-label {
    font-size: 24px;
    color: white;
    position: absolute;
    -unity-font-style: bold;
    background-color: rgba(0,0,0,0.45);
    padding: 4px 10px;
    border-radius: 6px;
}

.top-left   { top: 16px; left: 16px; }
.top-right  { top: 16px; right: 16px; }
.top-center { top: 16px; left: 50%; translate: -50% 0; }

.countdown-overlay {
    position: absolute;
    top: 0; left: 0;
    width: 100%; height: 100%;
    align-items: center;
    justify-content: center;
    background-color: rgba(0,0,0,0.3);
}

.countdown-text {
    font-size: 120px;
    color: #FFEB3B;
    -unity-font-style: bold;
}
```

#### `MainMenu.uss`
```css
.menu-root {
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 100%;
    background-color: #1A1A2E;
}
```

#### `RaceResults.uss`
```css
.results-root {
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 100%;
    background-color: rgba(0,0,0,0.85);
}

.results-list {
    width: 400px;
    margin: 20px 0;
}

.result-row {
    font-size: 22px;
    color: white;
    margin: 6px 0;
    padding: 6px 12px;
    background-color: rgba(255,255,255,0.1);
    border-radius: 4px;
}
```

---

## 6. Input Actions

Create `PlayerInputActions.inputactions` in `Assets/_Project/Input/`.

| Action | Type | Keyboard | Gamepad |
|--------|------|----------|---------|
| Move | Value / Vector2 | WASD | Left Stick |
| Jump | Button | Space | South (A/Cross) |
| Sprint | Button | Left Shift | Left Stick Press |
| Dash | Button | Left Alt | East (B/Circle) |
| Slide | Button | Left Ctrl | Right Stick Press |

**Steps:**
1. `Assets > Create > Input Actions`
2. Name it `PlayerInputActions`
3. Add Action Map: `Player`
4. Add each action from the table above
5. Check `Generate C# Class` in the Inspector
6. Click **Apply** — this generates `PlayerInputActions.cs` automatically

---

## 7. Inspector Setup

### PlayerPrefab Component Layout
```
PlayerPrefab
├── Rigidbody
│   ├── Mass = 1 (overridden by CharacterStatSO at runtime)
│   ├── Drag = 0
│   ├── Angular Drag = 8
│   ├── Freeze Rotation: X = true, Z = true
│   ├── Interpolate = Interpolate
│   └── Collision Detection = Continuous Dynamic
├── CapsuleCollider (height=2, radius=0.4)
├── NetworkObject (FishNet)
├── NetworkTransform (FishNet)
├── PlayerMovement
│   ├── groundCheck → GroundCheck child
│   ├── groundRadius = 0.25
│   ├── groundMask = Ground layer
│   ├── wallCheckDist = 0.6
│   └── wallMask = Wall layer
├── PlayerInputHandler
├── PlayerNetworkSync
│   └── smoothSpeed = 15
├── PlayerRaceData
├── CharacterSelector
│   └── registry → CharacterRegistry asset
└── WallRunHandler
    └── wallMask = Wall layer

Children:
└── GroundCheck  (empty Transform, local Y = -0.9)
```

### RaceManager GameObject
```
RaceManager
├── RaceManager.cs
│   ├── countdownDuration = 3
│   └── spawnPoints → assign 12 SpawnPoint transforms
├── RaceRanking.cs
├── RaceTimer.cs
├── CheckpointSystem.cs
│   └── totalLaps = 3
└── TrackGenerator.cs
    ├── database → TrackChunkDatabase asset
    ├── chunksPerLap = 12
    ├── totalLaps = 3
    └── trackRoot → TrackRoot GameObject
```

### TrackChunk Prefab Structure
```
Chunk_Name (TrackChunk.cs)
├── entryNode → child empty at chunk entrance
├── exitNode  → child empty at chunk exit
├── Geometry  → platforms, ramps, etc.
└── Obstacles → child obstacle prefabs
```

### Checkpoint Setup
- Add `Checkpoint.cs` to a trigger collider GameObject
- Set `IsTrigger = true`
- Assign sequential `checkpointIndex` starting from 1
- The last checkpoint before the start line = `isFinishLine = true`
- Place `CheckpointRoot` as parent, add checkpoints as children

### Cinemachine Camera
- Add `CinemachineCamera` to `CM_PlayerCamera`
- Set `Follow` and `LookAt` at runtime (PlayerSpawner assigns local player transform)
- Add `CinemachineOrbitalFollow` for third-person orbit
- Add `CinemachineRotationComposer` for look-at behaviour

---

## 8. Step-by-Step Implementation Guide

### Step 1 — Create the Unity Project

1. Open **Unity Hub**
2. Click **New Project**
3. Select **3D (URP)** template
4. Name: `PolyRunner`
5. Target Unity version: **6.3.x**
6. Click **Create Project**

---

### Step 2 — Install Required Packages

Open `Window > Package Manager`.

**Install from Unity Registry:**
- `Input System` — search by name, click Install
- `Cinemachine` — search by name, click Install

URP and UI Toolkit are included by default in Unity 6 with the URP template.

---

### Step 3 — Install FishNet 4.6

**Option A — OpenUPM (recommended):**
1. `Edit > Project Settings > Package Manager`
2. Under **Scoped Registries**, click `+`
3. Fill in:
   - Name: `OpenUPM`
   - URL: `https://package.openupm.com`
   - Scope: `com.firstgeargames.fishnet`
4. Click **Save**
5. In Package Manager → **My Registries** tab → find FishNet → **Install**

**Option B — GitHub:**
1. Download the latest FishNet 4.6 `.unitypackage` from [github.com/FirstGearGames/FishNet](https://github.com/FirstGearGames/FishNet)
2. `Assets > Import Package > Custom Package`
3. Select the downloaded file → Import All

---

### Step 4 — Enable New Input System

1. `Edit > Project Settings > Player`
2. Scroll to **Other Settings**
3. Find **Active Input Handling**
4. Set to **Both** (supports both old and new)
5. Unity will prompt a restart — click **Apply and Restart**

---

### Step 5 — Create Folder Structure

In the Project window, create the following folders under `Assets/`:

```
_Project/Scripts/Character
_Project/Scripts/Player
_Project/Scripts/Race
_Project/Scripts/Track
_Project/Scripts/Obstacles
_Project/Scripts/Network
_Project/Scripts/UI
_Project/ScriptableObjects/Characters
_Project/Prefabs/Player
_Project/Prefabs/Track
_Project/Prefabs/Obstacles
_Project/UI/UXML
_Project/UI/USS
_Project/Input
_Project/Scenes
```

---

### Step 6 — Create Input Actions Asset

1. Right-click `Assets/_Project/Input/` → `Create > Input Actions`
2. Name it `PlayerInputActions`
3. Double-click to open the Input Actions editor
4. Click `+` to add Action Map → name it `Player`
5. Add actions one by one:

| Action | Action Type | Control Type |
|--------|-------------|--------------|
| Move | Value | Vector2 |
| Jump | Button | — |
| Sprint | Button | — |
| Dash | Button | — |
| Slide | Button | — |

6. For **Move**: add binding → `WASD Composite` (keyboard) and `Left Stick` (gamepad)
7. For **Jump**: add binding → `Space` (keyboard) and `Gamepad South Button`
8. For **Sprint**: add binding → `Left Shift` and `Left Stick Press`
9. For **Dash**: add binding → `Left Alt` and `Gamepad East Button`
10. For **Slide**: add binding → `Left Ctrl` and `Right Stick Press`
11. In the Inspector with the asset selected: check **Generate C# Class**
12. Click **Apply** — `PlayerInputActions.cs` is generated automatically

---

### Step 7 — Create All C# Scripts

Create each script file in its correct folder (see Section 4). Paste the code from this guide exactly.

Key scripts and their locations:

| Script | Folder |
|--------|--------|
| CharacterStatSO.cs | Scripts/Character/ |
| CharacterRegistry.cs | Scripts/Character/ |
| CharacterSelector.cs | Scripts/Character/ |
| PlayerInputHandler.cs | Scripts/Player/ |
| PlayerMovement.cs | Scripts/Player/ |
| PlayerNetworkSync.cs | Scripts/Player/ |
| PlayerRaceData.cs | Scripts/Player/ |
| WallRunHandler.cs | Scripts/Player/ |
| Checkpoint.cs | Scripts/Race/ |
| CheckpointSystem.cs | Scripts/Race/ |
| RaceTimer.cs | Scripts/Race/ |
| RaceRanking.cs | Scripts/Race/ |
| RaceManager.cs | Scripts/Race/ |
| TrackChunk.cs | Scripts/Track/ |
| TrackChunkDatabase.cs | Scripts/Track/ |
| TrackGenerator.cs | Scripts/Track/ |
| MovingPlatform.cs | Scripts/Obstacles/ |
| RotatingBar.cs | Scripts/Obstacles/ |
| FallingPlatform.cs | Scripts/Obstacles/ |
| SwingingHammer.cs | Scripts/Obstacles/ |
| PushBlock.cs | Scripts/Obstacles/ |
| TrapFloor.cs | Scripts/Obstacles/ |
| JumpPad.cs | Scripts/Obstacles/ |
| ConveyorBelt.cs | Scripts/Obstacles/ |
| GameNetworkManager.cs | Scripts/Network/ |
| LobbyManager.cs | Scripts/Network/ |
| PlayerSpawner.cs | Scripts/Network/ |
| MainMenuUI.cs | Scripts/UI/ |
| LobbyUI.cs | Scripts/UI/ |
| HudUI.cs | Scripts/UI/ |
| RaceResultsUI.cs | Scripts/UI/ |
| CharacterSelectUI.cs | Scripts/UI/ |

---

### Step 8 — Create Character ScriptableObjects

1. Right-click `Assets/_Project/ScriptableObjects/Characters/`
2. `Create > PolyRunner > Character Stat`
3. Create three assets with these values:

**Char_Default**
| Field | Value |
|-------|-------|
| characterName | Default |
| characterId | 0 |
| maxSpeed | 12 |
| acceleration | 18 |
| jumpForce | 9 |
| doubleJumpForce | 7 |
| airControl | 0.6 |
| sprintMultiplier | 1.5 |
| maxStamina | 3 |
| dashForce | 18 |
| mass | 1 |
| wallRunSpeed | 14 |
| wallRunDuration | 1.8 |
| wallRunJumpForce | 10 |

**Char_Speedy**
| Field | Value |
|-------|-------|
| characterName | Speedy |
| characterId | 1 |
| maxSpeed | 16 |
| acceleration | 22 |
| jumpForce | 8 |
| mass | 0.7 |
| wallRunSpeed | 17 |
| wallRunDuration | 2.2 |
| wallRunMinSpeed | 3 |

**Char_Heavy**
| Field | Value |
|-------|-------|
| characterName | Heavy |
| characterId | 2 |
| maxSpeed | 9 |
| jumpForce | 11 |
| doubleJumpForce | 9 |
| mass | 1.8 |
| wallRunSpeed | 11 |
| wallRunGravity | 3.5 |
| wallRunDuration | 1.2 |

4. Create `CharacterRegistry` asset: right-click → `Create > PolyRunner > Character Registry`
5. Drag all three character assets into the `Characters` array in the Inspector

---

### Step 9 — Create Scenes

1. `File > New Scene` → Save as `MainMenu` in `Assets/_Project/Scenes/`
2. Repeat for `Lobby` and `Race`
3. `File > Build Settings` → drag all three scenes into **Scenes In Build** in this order:
   - 0: MainMenu
   - 1: Lobby
   - 2: Race

---

### Step 10 — Configure FishNet NetworkManager

In **MainMenu.unity**:

1. Create empty GameObject → name it `NetworkManager`
2. Add component: `FishNet.Managing.NetworkManager`
3. Add component: `Tugboat` (FishNet's UDP transport)
4. In the NetworkManager Inspector → assign `Tugboat` to the **Transport** slot
5. Set `Server Tick Rate = 30`
6. Create another empty GameObject → name it `GameNetworkManager`
7. Add `GameNetworkManager.cs`
8. Assign the `NetworkManager` GameObject to the **Fish Net Manager** field
9. Create a UIDocument GameObject → assign `MainMenu.uxml`
10. Add `MainMenuUI.cs` to the UIDocument GameObject

---

### Step 11 — Build the Player Prefab

1. `GameObject > 3D Object > Capsule` — rename to `PlayerPrefab`
2. Set CapsuleCollider: Height=2, Radius=0.4
3. Add **Rigidbody** — configure per the Inspector Setup section above
4. Add **NetworkObject** (FishNet)
5. Add **NetworkTransform** (FishNet)
6. Add all player scripts: `PlayerMovement`, `PlayerInputHandler`, `PlayerNetworkSync`, `PlayerRaceData`, `CharacterSelector`, `WallRunHandler`
7. Create child empty → name `GroundCheck` → set local position Y = -0.9
8. In `PlayerMovement` Inspector:
   - Assign `GroundCheck` transform
   - Set `groundRadius = 0.25`
   - Set **Ground Mask** to your ground layer
9. In `CharacterSelector` Inspector → assign `CharacterRegistry` asset
10. In `WallRunHandler` Inspector → set **Wall Mask** to your Wall layer
11. Drag the GameObject into `Assets/_Project/Prefabs/Player/` to create the prefab
12. In FishNet NetworkManager → **Spawnable Prefabs** → add `PlayerPrefab`

---

### Step 12 — Create Layer Setup

1. `Edit > Project Settings > Tags and Layers`
2. Add these layers:
   - `Ground` (used by PlayerMovement ground check)
   - `Wall` (used by WallRunHandler detection)
   - `Player` (used for player colliders)
3. Assign the `Ground` layer to all floor/platform geometry
4. Assign the `Wall` layer to all vertical wall geometry

---

### Step 13 — Create Track Chunk Prefabs

Create at least 4 chunk variants. For each:

1. Create empty GameObject → name `Chunk_Straight` (or Curve, Obstacles, Jump)
2. Add `TrackChunk.cs`
3. Create child empty → name `EntryNode` → position at chunk start (local 0,0,0)
4. Create child empty → name `ExitNode` → position at chunk end (e.g. local 0,0,20)
5. Assign `EntryNode` and `ExitNode` in the `TrackChunk` Inspector
6. Add floor geometry as children (Planes or Cubes with Ground layer)
7. Add obstacle prefabs as children
8. Set `difficultyTier` (0 = easy start, 0.5+ = harder)
9. Save as prefab in `Assets/_Project/Prefabs/Track/`

Then create the database:
1. Right-click → `Create > PolyRunner > Track Chunk Database`
2. Add each chunk prefab with a weight (higher = appears more often) and `minDifficulty`

---

### Step 14 — Create Obstacle Prefabs

For each obstacle type create a prefab in `Assets/_Project/Prefabs/Obstacles/`:

| Obstacle | Collider Type | Rigidbody | Trigger |
|----------|--------------|-----------|---------|
| MovingPlatform | Box | No | No |
| RotatingBar | Box | No | No |
| FallingPlatform | Box | Yes (Kinematic start) | No |
| SwingingHammer | Box | No | Yes |
| PushBlock | Box | Yes (Dynamic) | No |
| TrapFloor | Box | No | Yes |
| JumpPad | Box | No | Yes |
| ConveyorBelt | Box | No | No |

Attach the matching script to each prefab.

---

### Step 15 — Set Up Race Scene

In **Race.unity**:

1. Create empty → `RaceManager` → add `RaceManager`, `RaceRanking`, `RaceTimer`, `CheckpointSystem`, `TrackGenerator`
2. Create empty → `TrackRoot` → assign to `TrackGenerator`
3. Create empty → `SpawnPoints` → add 12 child empties named `SpawnPoint_0` through `SpawnPoint_11`, position them in a grid near the start
4. Create empty → `CheckpointRoot` (checkpoints placed here)
5. Add checkpoints along first lap layout manually (or generate them from TrackChunk)
6. Create empty → `CinemachineRoot` → add `CinemachineCamera` component
   - Add `CinemachineOrbitalFollow` for orbit
   - Add `CinemachineRotationComposer` for aim
7. Create two UIDocument GameObjects:
   - One with `HUD.uxml` + `HudUI.cs`
   - One with `RaceResults.uxml` + `RaceResultsUI.cs`
8. Create empty → `PlayerSpawner` → add `PlayerSpawner.cs`
   - Assign `PlayerPrefab` NetworkObject
   - Assign `RaceManager` reference

**Camera follow — add to PlayerSpawner or a dedicated script:**
```csharp
// Call this after spawning local player:
var vcam = FindFirstObjectByType<CinemachineCamera>();
if (vcam != null && nob.IsOwner)
{
    vcam.Follow = nob.transform;
    vcam.LookAt = nob.transform;
}
```

---

### Step 16 — Create UXML and USS Files

1. Right-click `Assets/_Project/UI/UXML/` → `Create > UI Toolkit > UI Document`
2. Create one file per screen: `MainMenu.uxml`, `Lobby.uxml`, `CharacterSelect.uxml`, `HUD.uxml`, `RaceResults.uxml`
3. Open each file and paste the UXML from Section 5.1
4. Right-click `Assets/_Project/UI/USS/` → `Create > UI Toolkit > Style Sheet`
5. Create: `Common.uss`, `HUD.uss`, `MainMenu.uss`, `RaceResults.uss`
6. Paste the USS from Section 5.2
7. In each `UIDocument` component → set the **Source Asset** to the matching `.uxml` file

---

### Step 17 — Test Multiplayer Locally

**Option A — ParrelSync (easiest for development):**
1. Install ParrelSync: `Window > Package Manager > + > Add by git URL`
   - URL: `https://github.com/VeriorPies/ParrelSync.git?path=/ParrelSync`
2. `ParrelSync > Clones Manager > Add Clone`
3. Open the clone in a second Unity Editor window
4. Press **Play** in the main editor → click **Host Game**
5. Press **Play** in the clone → enter `localhost` → click **Join Game**

**Option B — Standalone Builds:**
1. `File > Build Settings > Build`
2. Run two instances of the `.exe`
3. First instance: Host Game
4. Second instance: enter `localhost` → Join Game

---

### Step 18 — Test With Up to 12 Players

1. In FishNet NetworkManager → set `Max Connections = 12`
2. Build the project (`File > Build Settings > Build And Run`)
3. For **LAN multiplayer**:
   - Host machine runs the game and clicks **Host**
   - Other machines on the same WiFi/LAN run the game
   - They enter the host's **local IP** (e.g. `192.168.1.5`) → Join
4. Find your local IP: run `ipconfig` (Windows) or `ifconfig` (Mac/Linux) in terminal

---

## 9. Wall Running

Wall running is implemented as a **self-contained component** (`WallRunHandler.cs`) that extends `PlayerMovement` cleanly. Removing the component from the prefab disables the feature entirely with no side effects.

### How It Works

```
Player approaches wall while airborne + moving forward + above min speed
  → WallRunHandler detects wall via Raycast (left and right)
  → StartWallRun():
      - Zeros vertical velocity (clean attach)
      - Tells PlayerMovement to disable gravity (SetGravityOverride)
      - Begins timer
  → ApplyWallRunPhysics() each FixedUpdate:
      - Pushes player into wall (stick force)
      - Drives forward along wall surface (Cross product of normal + up)
      - Applies reduced gravity (wallRunGravity, not full Physics gravity)
  → Player presses Jump:
      - Launches away from wall (wallNormal * jumpForce + up * upForce)
      - Restores double jump as a reward
      - Exits wall run
  → Exit conditions: timer expired, wall gone, landed, stopped moving forward
```

### Wall Run Stats Per Character

| Stat | Default | Speedy | Heavy |
|------|---------|--------|-------|
| wallRunSpeed | 14 | 17 | 11 |
| wallRunGravity | 2 | 1.5 | 3.5 |
| wallRunDuration | 1.8s | 2.2s | 1.2s |
| wallRunJumpForce | 10 | 11 | 9 |
| wallRunJumpUpForce | 6 | 7 | 5 |
| wallRunMinSpeed | 4 | 3 | 5 |
| wallDetectDist | 0.75 | 0.75 | 0.8 |

Heavy slides off walls faster and needs more speed to initiate. Speedy sticks longer and needs less speed — character identity is preserved through the wall run system.

### Wall Run Inspector Setup

1. Add `WallRunHandler.cs` to PlayerPrefab (alongside PlayerMovement)
2. Set **Wall Mask** to the `Wall` layer
3. Assign the `Wall` layer to all vertical wall geometry in your track chunks
4. Walls must be approximately vertical — intentional, ramps won't trigger wall runs

### Changes Made to `PlayerMovement.cs` for Wall Run

Three additions were made to the existing PlayerMovement — nothing was changed or removed:

```csharp
// 1. New field
private WallRunHandler _wallRun;
public bool IsGrounded => _isGrounded;  // expose for WallRunHandler

// 2. In Awake()
_wallRun = GetComponent<WallRunHandler>();

// 3. In ApplyStats()
_wallRun?.ApplyStats(stats);

// 4. In HandleJump() — early return while wall running
if (_wallRun != null && _wallRun.IsWallRunning) return;

// 5. Two new public methods
public void SetGravityOverride(bool active) { ... }
public void RestoreDoubleJump() { ... }
```

---

## 10. Adding New Content

### Adding a New Character

1. Right-click `ScriptableObjects/Characters/` → `Create > PolyRunner > Character Stat`
2. Set a unique `characterId` (increment from the last one)
3. Fill in all stat fields
4. Open `CharacterRegistry` → add the new asset to the `characters` array
5. Done — no code changes required

### Adding a New Obstacle

1. Create prefab geometry
2. Add the matching obstacle script (or create a new one following the same single-responsibility pattern)
3. Configure Collider (Trigger or not per table in Step 14)
4. Place inside a TrackChunk prefab as a child
5. Done

### Adding a New Track Chunk

1. Duplicate an existing chunk prefab
2. Redesign the geometry and obstacle layout
3. Reposition `EntryNode` and `ExitNode`
4. Set `difficultyTier`
5. Open `TrackChunkDatabase` → add the new chunk with a weight and `minDifficulty`
6. Done

### Adding a New Movement Ability

Follow the WallRunHandler pattern:
1. Create a new component script (e.g. `GrappleHandler.cs`)
2. Add `ApplyStats(CharacterStatSO stats)` method
3. Add relevant fields to `CharacterStatSO`
4. In `PlayerMovement.Awake()` grab the reference
5. In `PlayerMovement.ApplyStats()` forward the call
6. The new component manages its own physics and state

---

## 11. Quick Reference Checklist

```
Project Setup
  [ ] Unity 6.3 project created (URP template)
  [ ] Input System package installed and enabled
  [ ] Cinemachine package installed
  [ ] FishNet 4.6 installed (OpenUPM or GitHub)
  [ ] Folder structure created

Input
  [ ] PlayerInputActions.inputactions created with all 5 actions
  [ ] C# class generation enabled and applied

Character System
  [ ] CharacterStatSO assets created (min 3: Default, Speedy, Heavy)
  [ ] CharacterRegistry asset created with all characters assigned

Player Prefab
  [ ] Rigidbody configured (freeze rotation X,Z; interpolate; continuous dynamic)
  [ ] All player scripts attached
  [ ] GroundCheck child empty at Y = -0.9
  [ ] Ground and Wall layer masks assigned
  [ ] Prefab registered in FishNet Spawnable Prefabs

Track
  [ ] At least 4 TrackChunk prefabs created with EntryNode and ExitNode
  [ ] TrackChunkDatabase asset created with chunks and weights
  [ ] TrackGenerator assigned database and trackRoot

Obstacles
  [ ] All 8 obstacle prefabs created with correct scripts and colliders

Race Scene
  [ ] RaceManager GameObject with all race scripts
  [ ] 12 SpawnPoint transforms under SpawnPoints
  [ ] CheckpointRoot with checkpoints (sequential index, one isFinishLine)
  [ ] CinemachineCamera configured
  [ ] Two UIDocument GameObjects (HUD, RaceResults)
  [ ] PlayerSpawner configured

UI
  [ ] All 5 UXML files created and assigned to UIDocuments
  [ ] All 4 USS files created

Network
  [ ] FishNet NetworkManager configured (Tugboat transport, tick rate 30)
  [ ] Max Connections = 12
  [ ] GameNetworkManager in MainMenu scene

Scenes
  [ ] All 3 scenes in Build Settings in correct order (0=MainMenu, 1=Lobby, 2=Race)

Testing
  [ ] Tested locally with 2 players (ParrelSync or two builds)
```
