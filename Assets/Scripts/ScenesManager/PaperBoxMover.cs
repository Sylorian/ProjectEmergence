using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] // OnMouseDown 需要碰撞体
public class PapperBoxMover : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("点击后沿 X 轴旋转到多少度")]
    public float targetAngleX = -20f;

    [Tooltip("一个来回的总时长（秒）")]
    public float duration = 0.5f;

    [Tooltip("是否使用平滑缓动，关闭则为匀速")]
    public bool useEase = true;

    [Header("纸张盒子移动设置")]
    [Tooltip("要移动的物体在 ScenePropertyRegistry 里的 ID")]
    public string paperMoverId = "0006";

    [Tooltip("收起状态（paperPosition = false）时的本地坐标")]
    public Vector3 positionClosed = new Vector3(0f, -7f, -5f);

    [Tooltip("展开状态（paperPosition = true）时的本地坐标")]
    public Vector3 positionOpen = new Vector3(0f, 0f, -5f);

    [Tooltip("移动速度")]
    public float moveSpeed = 20f;

    [Header("调试")]
    [Tooltip("是否保留 Slash 键切换（方便测试，可关掉）")]
    public bool allowKeyboardToggle = true;

    // ---- 旋转状态 ----
    private Quaternion _startRotation;
    private bool _isRotating;

    // ---- 移动状态 ----
    private bool _paperPosition = false;
    private Transform _paperMover;
    private Vector3 _targetPosition;

    private void Awake()
    {
        // 记录初始局部旋转，这样不会破坏原本的 Z 轴朝向
        _startRotation = transform.localRotation;
    }

    private void Start()
    {
        _paperMover = ScenePropertyRegistry.GetInstance<Transform>(paperMoverId);
        // 初始处于收起状态
        _targetPosition = positionClosed;
    }

    private void Update()
    {
        // 保留原来的键盘测试入口（可选）
        if (allowKeyboardToggle && Input.GetKeyDown(KeyCode.Slash))
        {
            TogglePaperPosition();
        }

        // 每帧朝当前目标逐步移动
        if (_paperMover != null)
        {
            _paperMover.localPosition = Vector3.MoveTowards(
                _paperMover.localPosition, _targetPosition, moveSpeed * Time.deltaTime);
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
        _targetPosition = _paperPosition ? positionOpen : positionClosed;
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