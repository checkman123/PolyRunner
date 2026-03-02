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