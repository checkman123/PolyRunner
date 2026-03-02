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