using UnityEngine;

public class BoxTransforMotor : MonoBehaviour
{
    [Header("速度")]
    public float moveSpeed = 5f;
    private Vector3 logicalPosition;
    private float logicalAngleX;      
    private Vector3 targetPosition;
    private float targetAngleX;       
    private bool isMoving;
    private bool isPausing;
    private float nextPauseTime;
    private float pauseEndTime;

    void Awake()
    {
        logicalPosition = transform.localPosition;
        logicalAngleX = transform.eulerAngles.x;   // 取 X 轴
    }

    void Update()
    {
        // 移动和旋转都完成，直接应用精确位置并返回
        if (!isMoving)
        {
            transform.localPosition = logicalPosition;
            return;
        }
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;

        // 如果当前没有在移动，从当前位置开始
        if (!isMoving)
        {
            logicalPosition = transform.localPosition;
        }
        isMoving = true;
    }
}