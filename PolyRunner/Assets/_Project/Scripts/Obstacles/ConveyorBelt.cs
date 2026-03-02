using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private Vector3 beltDirection = Vector3.forward;
    [SerializeField] private float speed = 5f;

    private void OnCollisionStay(Collision col)
    {
        if (col.rigidbody != null)
            col.rigidbody.AddForce(beltDirection.normalized * speed, ForceMode.Acceleration);
    }
}