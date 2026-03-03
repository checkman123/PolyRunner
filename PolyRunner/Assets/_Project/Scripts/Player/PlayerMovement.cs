using FishNet.Object;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    private CharacterStatSO _stats;
    private Rigidbody _rb;
    private PlayerInputHandler _input;

    // State
    private bool _isGrounded;
    private bool _canDoubleJump;
    private bool _hasWallJumped;
    private int _jumpCount;
    private float _stamina;
    private float _dashCooldownTimer;

    // Slide
    private bool _isSliding;

    // Ground check
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;

    // Wall jump
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDist = 0.6f;
    [SerializeField] private LayerMask wallMask;

    private WallRunHandler _wallRun;
    private bool _gravityOverridden;

    // Expose grounded state for WallRunHandler
    public bool IsGrounded => _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputHandler>();
        _wallRun = GetComponent<WallRunHandler>();
    }

    public void ApplyStats(CharacterStatSO stats)
    {
        _stats = stats;
        _rb.mass = stats.mass;
        _stamina = stats.maxStamina;
        _wallRun?.ApplyStats(stats);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner) _input.enabled = false;
        var vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = transform;
            vcam.LookAt = transform;
        }
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

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);
        if (_isGrounded)
        {
            _jumpCount = 0;
            _canDoubleJump = true;
            _hasWallJumped = false;
        }
    }

    private void HandleMovement()
    {
        Vector3 camForward = Camera.main != null
            ? Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized
            : transform.forward;
        Vector3 camRight = Camera.main != null
            ? Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized
            : transform.right;

        Vector3 dir = (camForward * _input.MoveInput.y + camRight * _input.MoveInput.x).normalized;

        bool isSprinting = _input.SprintHeld && _stamina > 0f && !_isSliding;
        float targetSpeed = _stats.maxSpeed * (isSprinting ? _stats.sprintMultiplier : 1f);

        if (isSprinting) _stamina -= Time.fixedDeltaTime;
        _stamina = Mathf.Clamp(_stamina, 0f, _stats.maxStamina);

        float control = _isGrounded ? 1f : _stats.airControl;

        if (dir.sqrMagnitude > 0.01f)
        {
            Vector3 targetVel = dir * targetSpeed;
            Vector3 current = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            Vector3 delta = (targetVel - current) * _stats.acceleration * control * Time.fixedDeltaTime;
            _rb.AddForce(delta, ForceMode.VelocityChange);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 current = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            Vector3 brake = -current * _stats.deceleration * Time.fixedDeltaTime;
            _rb.AddForce(brake, ForceMode.VelocityChange);
        }

        // Clamp horizontal speed
        Vector3 hVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        if (hVel.magnitude > targetSpeed)
        {
            Vector3 clamped = hVel.normalized * targetSpeed;
            _rb.linearVelocity = new Vector3(clamped.x, _rb.linearVelocity.y, clamped.z);
        }
    }

    private void HandleJump()
    {
        // Wall run handles its own jump
        if (_wallRun != null && _wallRun.IsWallRunning) return;

        if (!_input.JumpPressed) return;

        // Wall jump
        bool touchingWall = Physics.Raycast(transform.position, transform.right, wallCheckDist, wallMask) ||
                            Physics.Raycast(transform.position, -transform.right, wallCheckDist, wallMask);

        if (!_isGrounded && touchingWall && !_hasWallJumped)
        {
            Vector3 wallNormal = Physics.Raycast(transform.position, -transform.right, wallCheckDist, wallMask)
                ? transform.right : -transform.right;
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            _rb.AddForce((wallNormal + Vector3.up).normalized * _stats.jumpForce, ForceMode.VelocityChange);
            _hasWallJumped = true;
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
            _jumpCount = 2;
        }
    }

    private void HandleDash()
    {
        _dashCooldownTimer -= Time.fixedDeltaTime;
        if (!_input.DashPressed || _dashCooldownTimer > 0f) return;

        Vector3 dir = new Vector3(_input.MoveInput.x, 0, _input.MoveInput.y);
        if (dir.sqrMagnitude < 0.01f) dir = transform.forward;

        // Transform dir relative to camera
        if (Camera.main != null)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
            dir = (camForward * _input.MoveInput.y + camRight * _input.MoveInput.x).normalized;
        }

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.5f, _rb.linearVelocity.z);
        _rb.AddForce(dir.normalized * _stats.dashForce, ForceMode.VelocityChange);
        _dashCooldownTimer = _stats.dashCooldown;
    }

    private void HandleSlide()
    {
        _isSliding = _input.SlideHeld && _isGrounded;
        if (_isSliding)
        {
            _rb.AddForce(transform.forward * 5f, ForceMode.Acceleration);
        }
    }

    private void RegenStamina()
    {
        if (!_input.SprintHeld)
            _stamina = Mathf.Min(_stamina + _stats.staminaRegen * Time.fixedDeltaTime, _stats.maxStamina);
    }

    public void ApplyKnockback(Vector3 force)
    {
        _rb.AddForce(force, ForceMode.Impulse);
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        float orig = _stats.maxSpeed;
        _stats.maxSpeed *= multiplier;
        yield return new WaitForSeconds(duration);
        _stats.maxSpeed = orig;
    }

    /// <summary>Called by WallRunHandler to suppress built-in gravity.</summary>
    public void SetGravityOverride(bool active)
    {
        _gravityOverridden = active;
        _rb.useGravity = !active;
    }

    /// <summary>Wall jump rewards the player with their double jump back.</summary>
    public void RestoreDoubleJump()
    {
        _canDoubleJump = true;
        _jumpCount = 1;
    }
}