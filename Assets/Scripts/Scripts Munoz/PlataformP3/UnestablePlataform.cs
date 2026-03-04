using UnityEngine;

public class UnstablePlatform : MonoBehaviour
{
    public float moveAmount = 0.5f;
    public float speed = 2f;

    private Vector3 startPos;
    private bool isStabilized = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (!isStabilized)
        {
            float offset = Mathf.Sin(Time.time * speed) * moveAmount;
            transform.position = startPos + new Vector3(offset, 0, 0);
        }
    }

    public void SetStabilized(bool state)
    {
        isStabilized = state;
    }
}