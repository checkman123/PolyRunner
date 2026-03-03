using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour
{
    [Header("Spike Movement")]
    [SerializeField] private Transform spikeModel;
    [SerializeField] private float upHeight = 1.5f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float upTime = 1.5f;
    [SerializeField] private float downTime = 2f;

    [Header("Stun Duration")]
    [SerializeField] private float stunDuration = 1.5f;

    private Vector3 _downPos;
    private Vector3 _upPos;
    private bool _isUp;

    private void Start()
    {
        _downPos = spikeModel.localPosition;
        _upPos = _downPos + Vector3.up * upHeight;

        StartCoroutine(SpikeRoutine());
    }

    private IEnumerator SpikeRoutine()
    {
        while (true)
        {
            // Move Up
            yield return MoveSpike(_upPos);
            _isUp = true;

            yield return new WaitForSeconds(upTime);

            // Move Down
            yield return MoveSpike(_downPos);
            _isUp = false;

            yield return new WaitForSeconds(downTime);
        }
    }

    private IEnumerator MoveSpike(Vector3 target)
    {
        while (Vector3.Distance(spikeModel.localPosition, target) > 0.01f)
        {
            spikeModel.localPosition = Vector3.MoveTowards(
                spikeModel.localPosition,
                target,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isUp) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        StartCoroutine(StunPlayer(player));
    }

    private IEnumerator StunPlayer(PlayerMovement player)
    {
        player.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(-player.transform.forward * 5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(stunDuration);

        player.enabled = true;
    }
}