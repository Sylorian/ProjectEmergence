using System.Collections;
using UnityEngine;
using Hyperplasia.Runtime;
using Hyperplasia.Presenter;

[RequireComponent(typeof(Collider2D))] // OnMouseDown 需要碰撞体
public class ViewInkButton : MonoBehaviour
{
    public enum DisableMode
    {
        SetActive,      // 整个 targetObject 启用/禁用（最彻底，"全部禁用"用这个）
        CanvasEnabled   // 只控制 Canvas 组件，物体本身还可见
    }

    [Header("目标设置")]
    [Tooltip("要操作的对象：控制它的启用/禁用，并让对象沿 Y 轴旋转")]
    public GameObject targetObject;

    [Tooltip("实际旋转的对象（留空则默认用 targetObject）")]
    public GameObject rotateTarget;

    [Header("启用/禁用设置")]
    [Tooltip("SetActive：整个对象禁用；CanvasEnabled：只禁用 Canvas")]
    public DisableMode disableMode = DisableMode.CanvasEnabled; // 根据需求默认只控制 Canvas

    [Tooltip("开始时目标对象是否处于禁用状态")]
    public bool startDisabled = true;

    [Tooltip("CanvasEnabled 模式下，是否也控制 targetObject 自身的 Canvas（建议开启）")]
    public bool includeSelfCanvas = true;

    [Header("旋转设置")]
    [Tooltip("关闭状态时本地 Y 轴的角度（初始角度）")]
    public float targetAngleY = 90f;

    [Tooltip("单程旋转时长（秒）")]
    public float rotateDuration = 0.5f;

    [Tooltip("是否使用平滑缓动，关闭则为匀速")]
    public bool useEase = true;

    private Quaternion _closedRotation;  // 关闭状态旋转（y = targetAngleY）
    private Quaternion _openRotation;    // 打开状态旋转（y = 0）
    private bool _isOpen;                // 当前是否处于展开状态（false = 关闭）
    private bool _isRotating;            // 旋转中锁，防止连点打断

    private void Awake()
    {
        if (rotateTarget == null) rotateTarget = targetObject;

        if (rotateTarget != null)
        {
            // 场景中初始旋转应为 y = 90°（关闭状态）
            _closedRotation = rotateTarget.transform.localRotation;
            // 打开状态：绕本地 Y 轴旋转 -targetAngleY（从 90° 转到 0°）
            _openRotation = _closedRotation * Quaternion.Euler(0f, -targetAngleY, 0f);

            // 确保初始就是关闭角度
            rotateTarget.transform.localRotation = _closedRotation;
        }

        // 开始时禁用（Canvas 或整个对象）
        if (startDisabled)
            SetTargetEnabled(false);
    }

    private void OnMouseDown()
    {
        GameBootstrap.TyperPresenter.OnInkToggled(!_isOpen);
    }

    public void Toggle(bool open)
    {
        if (_isRotating) return; // 旋转中不响应
        _isOpen = open;
        StartCoroutine(TriggerRoutine(_isOpen));
    }

    private IEnumerator TriggerRoutine(bool open)
    {
        _isRotating = true;

        if (open)
        {
            // ① 第一次点击：先启用 Canvas，再转到 0°
            SetTargetEnabled(true);
            yield return null;          // 给 Canvas 一帧初始化时间
            yield return RotateTo(true);
        }
        else
        {
            // ② 第二次点击：先转回 90°，再禁用 Canvas
            yield return RotateTo(false);
            SetTargetEnabled(false);
        }

        _isRotating = false;
    }

    private IEnumerator RotateTo(bool open)
    {
        if (rotateTarget == null) yield break;

        Transform t = rotateTarget.transform;
        Quaternion from = t.localRotation; // 用当前朝向作为起点，更稳
        Quaternion to = open ? _openRotation : _closedRotation;

        float elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / rotateDuration);
            if (useEase) k = k * k * (3f - 2f * k); // SmoothStep 缓动
            t.localRotation = Quaternion.Slerp(from, to, k);
            yield return null;
        }
        t.localRotation = to; // 精确到位
    }

    private void SetTargetEnabled(bool enabled)
    {
        if (targetObject == null) return;

        if (disableMode == DisableMode.SetActive)
        {
            // 整个对象启用/禁用（子物体一起禁用/启用）
            targetObject.SetActive(enabled);
        }
        else
        {
            // 只切换 Canvas 的 enabled
            Canvas[] canvases = targetObject.GetComponentsInChildren<Canvas>(true);
            foreach (var c in canvases)
            {
                if (!includeSelfCanvas && c.gameObject == targetObject) continue;
                c.enabled = enabled;
            }
        }
    }
}