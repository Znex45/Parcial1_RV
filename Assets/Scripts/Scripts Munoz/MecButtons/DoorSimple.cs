using UnityEngine;
using System.Collections;

public class DoorSimple : MonoBehaviour
{
    [Header("Movimiento")]
    public Vector3 openOffset = new Vector3(0, 5, 0);
    public float speed = 2f;

    [Header("Tiempo Abierta")]
    public float openDuration = 3f; // tiempo que permanece abierta

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isOpening = false;
    private bool isClosing = false;

    [Header("Comportamiento")]
    public bool autoClose = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (isOpening)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                openPosition,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, openPosition) < 0.01f)
            {
                isOpening = false;

                if (autoClose)
                {
                    StartCoroutine(CloseAfterDelay());
                }
            }
        }

        if (isClosing)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                closedPosition,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, closedPosition) < 0.01f)
            {
                isClosing = false;
            }
        }
    }

    public void OpenDoor()
    {
        if (isOpening || isClosing) return;

        isOpening = true;

        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(openDuration);
        isClosing = true;
    }
}