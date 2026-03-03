using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
public class WallRunHandler : NetworkBehaviour
{
    [Header("Detection Layers")]
    [SerializeField] private LayerMask wallMask;

    private Rigidbody _rb;
    private PlayerMovement _movement;
    private PlayerInputHandler _input;
    private CharacterStatSO _stats;

    private bool _isWallRunning;
    private bool _onLeftWall;
    private bool _onRightWall;
    private float _wallRunTimer;
    private bool _wallJumpedThisRun;
    private bool _canWallRun = true;
    private Vector3 _wallNormal;

    public bool IsWallRunning => _isWallRunning;
    public Vector3 WallNormal => _wallNormal;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _movement = GetComponent<PlayerMovement>();
        _input = GetComponent<PlayerInputHandler>();
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

    private void DetectWalls(out bool leftHit, out bool rightHit, out RaycastHit hit)
    {
        float dist = _stats.wallDetectDist;

        leftHit = Physics.Raycast(transform.position, -transform.right, out RaycastHit lHit, dist, wallMask);
        rightHit = Physics.Raycast(transform.position, transform.right, out RaycastHit rHit, dist, wallMask);

        hit = rightHit ? rHit : lHit;
    }

    private void UpdateWallRunState(bool leftHit, bool rightHit, RaycastHit wallHit)
    {
        bool playerMovingForward = _input.MoveInput.y > 0.2f;
        bool hasEnoughSpeed = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z).magnitude >= _stats.wallRunMinSpeed;
        bool isAirborne = !_movement.IsGrounded;
        bool wallPresent = leftHit || rightHit;

        if (!_isWallRunning && wallPresent && isAirborne &&
            playerMovingForward && hasEnoughSpeed && _canWallRun)
        {
            StartWallRun(leftHit, rightHit, wallHit);
        }

        if (_isWallRunning)
        {
            _wallRunTimer -= Time.fixedDeltaTime;

            bool timedOut = _wallRunTimer <= 0f;
            bool wallGone = !wallPresent;
            bool grounded = _movement.IsGrounded;
            bool movedAway = _input.MoveInput.y < 0.1f;

            if (timedOut || wallGone || grounded || movedAway)
                StopWallRun(wallGone);
        }
    }

    private void StartWallRun(bool leftHit, bool rightHit, RaycastHit wallHit)
    {
        _isWallRunning = true;
        _onLeftWall = leftHit && !rightHit;
        _onRightWall = rightHit;
        _wallRunTimer = _stats.wallRunDuration;
        _wallJumpedThisRun = false;
        _wallNormal = wallHit.normal;

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _movement.SetGravityOverride(true);
    }

    private void StopWallRun(bool wallGone)
    {
        _isWallRunning = false;
        _onLeftWall = false;
        _onRightWall = false;

        _movement.SetGravityOverride(false);

        if (!wallGone) StartCoroutine(WallRunCooldown());
    }

    private void ApplyWallRunPhysics()
    {
        if (!_isWallRunning) return;

        _rb.AddForce(-_wallNormal * 4f, ForceMode.Acceleration);

        Vector3 wallForward = Vector3.Cross(_wallNormal, Vector3.up).normalized;
        if (Vector3.Dot(wallForward, transform.forward) < 0f)
            wallForward = -wallForward;

        Vector3 targetVel = wallForward * _stats.wallRunSpeed;
        Vector3 currentHVel = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        Vector3 velDelta = (targetVel - currentHVel) * 8f * Time.fixedDeltaTime;
        _rb.AddForce(velDelta, ForceMode.VelocityChange);

        _rb.AddForce(Vector3.down * _stats.wallRunGravity, ForceMode.Acceleration);
    }

    private void HandleWallJump()
    {
        if (!_isWallRunning || _wallJumpedThisRun) return;
        if (!_input.JumpPressed) return;

        _wallJumpedThisRun = true;

        Vector3 jumpDir = (_wallNormal * _stats.wallRunJumpForce) +
                          (Vector3.up * _stats.wallRunJumpUpForce);

        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x * 0.5f, 0f, _rb.linearVelocity.z * 0.5f);
        _rb.AddForce(jumpDir, ForceMode.VelocityChange);

        _movement.RestoreDoubleJump();
        StopWallRun(true);
    }

    private System.Collections.IEnumerator WallRunCooldown()
    {
        _canWallRun = false;
        yield return new WaitForSeconds(0.4f);
        _canWallRun = true;
    }
}