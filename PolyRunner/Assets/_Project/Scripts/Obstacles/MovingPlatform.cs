using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 moveOffset = new Vector3(3f, 0, 0);
    [SerializeField] private float speed = 2f;

    private Vector3 _startPos;
    private float _t;

    private void Start() => _startPos = transform.position;

    private void FixedUpdate()
    {
        _t += Time.fixedDeltaTime * speed;
        float ping = Mathf.PingPong(_t, 1f);
        transform.position = Vector3.Lerp(_startPos, _startPos + moveOffset, ping);
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.rigidbody != null)
        {
            Vector3 delta = transform.position - transform.position; // compensated below
            col.rigidbody.MovePosition(col.rigidbody.position + (transform.position - _startPos) * 0.02f);
        }
    }
}