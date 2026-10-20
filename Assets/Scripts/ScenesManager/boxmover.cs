using UnityEngine;

/// <summary>
/// 子对象：沿父对象本地坐标移动。
/// 支持两种模式：
///   1) Begin()      —— 完整流程：右移 → 停留 → 左移（原有行为）
///   2) BeginRight() —— 只右移到终点后停住，等外部调用 BeginLeft() 再返回
/// </summary>
public class boxmover : MonoBehaviour
{
    [Header("位移设置")]
    [Tooltip("沿本地坐标移动的距离")]
    public float distance = 3f;

    [Tooltip("移动方向（父对象本地空间，默认本地 X 轴向右）")]
    public Vector3 moveDirection = Vector3.right;

    [Header("时间设置（秒）")]
    [Tooltip("单程时长：向右、向左各用这个时间")]
    public float oneWayDuration = 1f;

    [Tooltip("到达终点后停留时长（仅 Begin() 完整流程时生效）")]
    public float holdAtEnd = 0f;

    public bool IsFinished { get; private set; }      // 完整流程结束
    public bool IsRightFinished { get; private set; } // 右移已到达终点
    public bool IsMovingRight => state == State.MovingRight;
    public bool IsMovingLeft => state == State.MovingLeft;

    private enum State { Idle, MovingRight, HoldEnd, MovingLeft, Finished }

    private State state = State.Idle;
    private float elapsed;
    private Vector3 startLocalPos;
    private Vector3 endLocalPos;
    private Vector3 leftStartPos;
    private bool autoReturn;   // true = 到终点后自动返回

    void Awake()
    {
        startLocalPos = transform.localPosition;
        endLocalPos = startLocalPos + moveDirection.normalized * distance;
        leftStartPos = endLocalPos;
    }
    void Update()
    {
        switch (state)
        {
            case State.MovingRight:
                elapsed += Time.deltaTime;
                if (elapsed >= oneWayDuration)
                {
                    transform.localPosition = endLocalPos;
                    elapsed = 0f;
                    IsRightFinished = true;

                    if (autoReturn)
                    {
                        // 完整流程：停留（可选）后自动返回
                        if (holdAtEnd > 0f) state = State.HoldEnd;
                        else EnterMovingLeft();
                    }
                    else
                    {
                        // 手动模式：停在终点，等待外部 BeginLeft()
                        state = State.HoldEnd;
                    }
                }
                else
                {
                    float t = elapsed / oneWayDuration;
                    transform.localPosition = Vector3.LerpUnclamped(startLocalPos, endLocalPos, EaseOutQuad(t));
                }
                break;

            case State.HoldEnd:
                if (!autoReturn) break;   // 手动模式：无限等待 BeginLeft()
                elapsed += Time.deltaTime;
                if (elapsed >= holdAtEnd) { elapsed = 0f; EnterMovingLeft(); }
                break;

            case State.MovingLeft:
                elapsed += Time.deltaTime;
                if (elapsed >= oneWayDuration)
                {
                    transform.localPosition = startLocalPos;
                    elapsed = 0f;
                    state = State.Finished;
                }
                else
                {
                    float t = elapsed / oneWayDuration;
                    transform.localPosition = Vector3.LerpUnclamped(leftStartPos, startLocalPos, EaseOutQuad(t));
                }
                break;

            case State.Finished:
                IsFinished = true;
                state = State.Idle;
                break;
        }
    }

    private void EnterMovingLeft()
    {
        leftStartPos = transform.localPosition;
        elapsed = 0f;
        state = State.MovingLeft;
    }

    /// <summary>完整流程：右移 → 停留 → 左移（原本的行为）</summary>
    public void Begin()
    {
        transform.localPosition = startLocalPos;
        elapsed = 0f;
        IsFinished = false;
        IsRightFinished = false;
        autoReturn = true;
        state = State.MovingRight;
    }

    /// <summary>第一步：只向右移动，到达终点后停住</summary>
    public void BeginRight()
    {
        transform.localPosition = startLocalPos;
        elapsed = 0f;
        IsFinished = false;
        IsRightFinished = false;
        autoReturn = false;
        state = State.MovingRight;
    }

    /// <summary>第三步：从当前位置向左返回起点</summary>
    public void BeginLeft()
    {
        if (state == State.MovingLeft) return;
        IsFinished = false;
        EnterMovingLeft();
    }

    /// t∈[0,1]：t=0 速度最大，t=1 速度为 0
    private static float EaseOutQuad(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t);
    }

    void OnValidate()
    {
        oneWayDuration = Mathf.Max(0.01f, oneWayDuration);
    }
}