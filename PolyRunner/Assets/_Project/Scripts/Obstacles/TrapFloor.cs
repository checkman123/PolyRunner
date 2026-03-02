using UnityEngine;

public class TrapFloor : MonoBehaviour
{
    [SerializeField] private float openDelay = 0.3f;
    [SerializeField] private float closeDelay = 2f;

    private bool _open;
    private Collider _col;
    private MeshRenderer _rend;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _rend = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_open || other.GetComponent<PlayerMovement>() == null) return;
        Invoke(nameof(Open), openDelay);
        Invoke(nameof(Close), openDelay + closeDelay);
    }

    private void Open()
    {
        _open = true;
        _col.enabled = false;
        if (_rend) _rend.enabled = false;
    }

    private void Close()
    {
        _open = false;
        _col.enabled = true;
        if (_rend) _rend.enabled = true;
    }
}