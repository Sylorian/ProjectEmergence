using UnityEngine;

public class BoxTransforMotor : MonoBehaviour
{
    [Header("速度")]
    public float moveSpeed = 5f;

    private Vector3 logicalPosition;
    private Vector3 targetPosition;
    private bool isMoving;

    private const float MoveEpsilon = 0.0005f;

    void Awake()
    {
        logicalPosition = transform.localPosition;
    }

    void Update()
    {
        if (!isMoving) return;

        logicalPosition = Vector3.MoveTowards(
            logicalPosition, targetPosition, moveSpeed * Time.deltaTime);

        if ((logicalPosition - targetPosition).sqrMagnitude <= MoveEpsilon * MoveEpsilon)
        {
            logicalPosition = targetPosition;
            isMoving = false;
        }

        transform.localPosition = logicalPosition;
    }

    public void MoveTo(Vector3 localPosition)
    {
        targetPosition = localPosition;
        isMoving = true;
    }

    public void Stop()
    {
        isMoving = false;
    }
}