using UnityEngine;

public class BoxTransforMotor : MonoBehaviour
{
    [Header("移动范围")]
    public Vector3 pointA = Vector3.zero;
    public Vector3 pointB = new Vector3(5f, 0f, 0f);

    [Header("运动参数")]
    public float duration = 2f;
    public bool smoothAtEnds = true;
    
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
    
    public Vector3 GetPingPongPosition(Vector3 a, Vector3 b, float dur, bool smooth)
    {
        float safeDur = Mathf.Max(0.01f, dur);
        float t = Mathf.PingPong(Time.time / safeDur, 1f);

        if (smooth)
            t = Mathf.SmoothStep(0f, 1f, t);

        return Vector3.Lerp(a, b, t);
    }
}
