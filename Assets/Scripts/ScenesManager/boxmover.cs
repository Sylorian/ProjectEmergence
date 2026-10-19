using UnityEngine;

/// <summary>
/// 子对象：沿父对象本地坐标先向右移动，减速到 0 后，再返回本地起点。
/// 全程只执行一次，结束后停在初始本地位置。
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

    [Tooltip("到达终点后停留时长")]
    public float holdAtEnd = 0f;

    [Header("其它")]
    public bool playOnStart = true;

    public bool IsFinished { get; private set; }

    private enum State { Idle, MovingRight, HoldEnd, MovingLeft, Finished }

    private State state = State.Idle;
    private float elapsed;
    private Vector3 startLocalPos;
    private Vector3 endLocalPos;

    void Awake()
    {
        startLocalPos = transform.localPosition;
        endLocalPos = startLocalPos + moveDirection.normalized * distance;
    }

    void Start()
    {
        if (playOnStart) Begin();
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
                    state = holdAtEnd > 0f ? State.HoldEnd : State.MovingLeft;
                }
                else
                {
                    float t = elapsed / oneWayDuration;
                    transform.localPosition = Vector3.LerpUnclamped(startLocalPos, endLocalPos, EaseOutQuad(t));
                }
                break;

            case State.HoldEnd:
                elapsed += Time.deltaTime;
                if (elapsed >= holdAtEnd) { elapsed = 0f; state = State.MovingLeft; }
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
                    transform.localPosition = Vector3.LerpUnclamped(endLocalPos, startLocalPos, EaseOutQuad(t));
                }
                break;

            case State.Finished:
                IsFinished = true;
                state = State.Idle;
                break;
        }
    }

    /// <summary>从本地起点开始执行一次完整流程</summary>
    public void Begin()
    {
        transform.localPosition = startLocalPos;
        elapsed = 0f;
        IsFinished = false;
        state = State.MovingRight;
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