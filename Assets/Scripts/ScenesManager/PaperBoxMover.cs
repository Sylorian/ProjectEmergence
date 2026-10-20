using System.Collections;
using UnityEngine;
using Hyperplasia.Runtime;

[RequireComponent(typeof(Collider2D))] // OnMouseDown 需要碰撞体
public class PaperBoxMover : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("点击后沿 X 轴旋转到多少度")]
    public float targetAngleX = -20f;

    [Tooltip("一个来回的总时长（秒）")]
    public float duration = 0.5f;

    [Tooltip("是否使用平滑缓动，关闭则为匀速")]
    public bool useEase = true;

    [Header("纸张盒子移动设置")]
    [Tooltip("要移动的物体在 GameObjectRegistry 里的 ID")]
    public int paperMoverId = 1002;

    [Tooltip("收起状态（paperPosition = false）时的本地坐标")]
    public Vector3 positionClosed = new Vector3(0f, -7f, -5f);

    [Tooltip("展开状态（paperPosition = true）时的本地坐标")]
    public Vector3 positionOpen = new Vector3(0f, 0f, -5f);

    [Tooltip("输出区域在 GameObjectRegistry 里的 ID")]
    public int OutputRectId = 1001;

    [Tooltip("展开状态下的顶部边距")]
    public float TopMarginOpen = 120f;

    [Tooltip("收起状态下的顶部边距")]
    public float TopMarginClosed = 820f;

    [Tooltip("纸张盒子移动速度")]
    public float paperMoveSpeed = 20f;

    [Tooltip("输出区域改变速度")]
    public float outputRectChangeSpeed = 2000f;

    // ---- 旋转状态 ----
    private Quaternion _startRotation;
    private bool _isRotating = false;

    // ---- 移动状态 ----
    private bool _paperPosition = false;
    private Transform _paperTransform;
    private RectTransform _outputRect;
    private Vector3 _targetPaperPosition;
    private float _targetOutputRectTopMargin;

    private void Awake()
    {
        // 记录初始局部旋转，这样不会破坏原本的 Z 轴朝向
        _startRotation = transform.localRotation;
    }

    private void Start()
    {
        _paperTransform = GameBootstrap.GetComponentById<Transform>(paperMoverId);
        _outputRect = GameBootstrap.GetComponentById<RectTransform>(OutputRectId);
        // 初始处于收起状态
        _targetPaperPosition = positionClosed;
        _targetOutputRectTopMargin = TopMarginClosed;
        if (_paperTransform != null)
            _paperTransform.localPosition = _targetPaperPosition;
        if (_outputRect != null)
            _outputRect.offsetMax = new Vector2(_outputRect.offsetMax.x, -_targetOutputRectTopMargin);
    }

    private void Update()
    {
        // 每帧朝当前目标逐步移动
        if (_paperTransform != null)
        {
            _paperTransform.localPosition = Vector3.MoveTowards(
                _paperTransform.localPosition, _targetPaperPosition, paperMoveSpeed * Time.deltaTime);
        }

        if (_outputRect != null)
        {
            var targetOffsetMax = new Vector2(_outputRect.offsetMax.x, -_targetOutputRectTopMargin);
            _outputRect.offsetMax = Vector2.MoveTowards(_outputRect.offsetMax, targetOffsetMax, outputRectChangeSpeed * Time.deltaTime);
        }
    }

    private void OnMouseDown()
    {
        // 切换纸张盒子位置
        TogglePaperPosition();

        // 触发旋转动画（正在旋转时忽略连点）
        if (!_isRotating)
        {
            StartCoroutine(RotateRoutine());
        }
    }

    /// <summary>切换纸张盒子的目标位置</summary>
    private void TogglePaperPosition()
    {
        _paperPosition = !_paperPosition;
        _targetPaperPosition = _paperPosition ? positionOpen : positionClosed;
        _targetOutputRectTopMargin = _paperPosition ? TopMarginOpen : TopMarginClosed;
    }

    private IEnumerator RotateRoutine()
    {
        _isRotating = true;

        float half = duration * 0.5f;                                   // 单程时间
        Quaternion targetRotation =
            _startRotation * Quaternion.Euler(targetAngleX, 0f, 0f);     // 绕本地 X 轴

        // ① 0 → targetAngleX
        yield return RotateFromTo(_startRotation, targetRotation, half);

        // ② targetAngleX → 0
        yield return RotateFromTo(targetRotation, _startRotation, half);

        transform.localRotation = _startRotation; // 精确回到初始状态
        _isRotating = false;
    }

    private IEnumerator RotateFromTo(Quaternion from, Quaternion to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            if (useEase) k = k * k * (3f - 2f * k); // SmoothStep 缓动
            transform.localRotation = Quaternion.Slerp(from, to, k);
            yield return null;
        }
    }
}