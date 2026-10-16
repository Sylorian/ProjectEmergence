using UnityEngine;

public class TransforMotor : MonoBehaviour
{
    [Header("速度")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    [Header("抖动")]
    public float shakeAmplitude = 0.1f;
    public float shakeFrequency = 20f;
    public float shakeRotationAmplitude = 5f;

    [Header("停顿")]
    public float pauseIntervalMin = 0.5f;
    public float pauseIntervalMax = 1.5f;
    public float pauseDurationMin = 0.1f;
    public float pauseDurationMax = 0.4f;

    [Header("到达阈值")]
    public float positionThreshold = 0.01f;
    public float angleThreshold = 0.1f;

    private Vector3 logicalPosition;
    private float logicalAngleX;      // 改为 X 轴角度
    private Vector3 targetPosition;
    private float targetAngleX;       // 改为 X 轴角度
    private bool isMoving;
    private bool isRotating;
    private bool isPausing;
    private float nextPauseTime;
    private float pauseEndTime;

    
    void Awake()
    {
        logicalPosition = transform.position;
        logicalAngleX = transform.eulerAngles.x;   // 取 X 轴
    }

    void Update()
    {

        
        // 移动和旋转都完成，直接应用精确位置并返回
        if (!isMoving && !isRotating)
        {
            transform.position = logicalPosition;
            transform.rotation = Quaternion.Euler(logicalAngleX, 0f, 0f);   // 绕 X 轴
            return;
        }

        // 停顿逻辑
        if (isPausing)
        {
            if (Time.time >= pauseEndTime)
            {
                isPausing = false;
                nextPauseTime = Time.time + Random.Range(pauseIntervalMin, pauseIntervalMax);
            }
        }
        else
        {
            if (Time.time >= nextPauseTime)
            {
                isPausing = true;
                pauseEndTime = Time.time + Random.Range(pauseDurationMin, pauseDurationMax);
            }
        }

        // 非停顿期间推进逻辑位置和角度
        if (!isPausing)
        {
            if (isMoving)
            {
                logicalPosition = Vector3.MoveTowards(logicalPosition, targetPosition, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(logicalPosition, targetPosition) <= positionThreshold)
                {
                    logicalPosition = targetPosition;
                    isMoving = false;
                }
            }

            if (isRotating)
            {
                logicalAngleX = Mathf.MoveTowardsAngle(logicalAngleX, targetAngleX, rotateSpeed * Time.deltaTime);
                if (Mathf.Abs(Mathf.DeltaAngle(logicalAngleX, targetAngleX)) <= angleThreshold)
                {
                    logicalAngleX = targetAngleX;
                    isRotating = false;
                }
            }
        }

        // 应用视觉抖动
        Vector3 shakeOffset = Vector3.zero;
        float angleOffset = 0f;

        if (!isPausing)
        {
            float t = Time.time * shakeFrequency;
            float nx = (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f;
            float ny = (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f;
            shakeOffset = new Vector3(nx, ny, 0f) * shakeAmplitude;

            float nr = (Mathf.PerlinNoise(t, 123f) - 0.5f) * 2f;
            angleOffset = nr * shakeRotationAmplitude;
        }

        transform.position = logicalPosition + shakeOffset;
        transform.rotation = Quaternion.Euler(logicalAngleX + angleOffset, 0f, 0f);   // 绕 X 轴
        
    }

    /// <summary>
    /// 移动到指定位置并绕 X 轴旋转到指定角度，自动包含抖动和停顿效果。
    /// </summary>
    public void MoveTo(Vector3 position, float angleX)
    {
        targetPosition = position;
        targetAngleX = angleX;

        // 如果当前没有在移动，从当前位置开始
        if (!isMoving && !isRotating)
        {
            logicalPosition = transform.position;
            logicalAngleX = transform.eulerAngles.x;
        }

        isMoving = true;
        isRotating = true;
        isPausing = false;
        nextPauseTime = Time.time + Random.Range(pauseIntervalMin, pauseIntervalMax);
    }
    
    
}