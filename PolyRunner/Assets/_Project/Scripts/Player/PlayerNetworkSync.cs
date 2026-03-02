using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerNetworkSync : NetworkBehaviour
{
    [SerializeField] private float smoothSpeed = 15f;

    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private Rigidbody _rb;

    private readonly SyncVar<Vector3> _serverPos = new SyncVar<Vector3>();
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