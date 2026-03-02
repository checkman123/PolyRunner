using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 0.8f;
    [SerializeField] private float resetTime = 4f;

    private Rigidbody _rb;
    private Vector3 _startPos;
    private Quaternion _startRot;
    private bool _triggered;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _startPos = transform.position;
        _startRot = transform.rotation;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (_triggered || col.gameObject.GetComponent<PlayerMovement>() == null) return;
        _triggered = true;
        Invoke(nameof(Fall), fallDelay);
        Invoke(nameof(Reset), resetTime);
    }

    private void Fall()
    {
        _rb.isKinematic = false;
    }

    private void Reset()
    {
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(_startPos, _startRot);
        _triggered = false;
    }
}