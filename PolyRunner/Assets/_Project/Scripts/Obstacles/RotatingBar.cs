using UnityEngine;

public class RotatingBar : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Vector3 axis = Vector3.up;

    private void FixedUpdate()
    {
        transform.Rotate(axis, rotationSpeed * Time.fixedDeltaTime, Space.World);
    }
}