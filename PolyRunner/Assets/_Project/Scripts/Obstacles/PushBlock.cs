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