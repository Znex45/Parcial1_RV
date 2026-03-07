using UnityEngine;

public class UnstablePlatform : MonoBehaviour
{
    public float moveAmount = 0.5f;
    public float speed = 2f;
    public float recoveryDelay = 2f;

    private Vector3 startPos;
    private bool isStabilized = false;
    private float cooldownTimer = 0f;
    private float internalTime = 0f;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {

        if (isStabilized)
        {
            cooldownTimer = recoveryDelay;
            return;
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }
        internalTime += Time.deltaTime;
        float offset = Mathf.Sin(internalTime * speed) * moveAmount;
        transform.position = startPos + new Vector3(offset, 0, 0);
    }

    public void SetStabilized(bool state)
    {
        isStabilized = state;

    }
}