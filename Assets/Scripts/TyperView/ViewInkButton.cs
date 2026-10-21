using UnityEngine;
using Hyperplasia.Runtime;
using Hyperplasia.View;

[RequireComponent(typeof(Collider2D))] // OnMouseDown 需要碰撞体
[RequireComponent(typeof(ButtonPunch))]
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

    private ButtonPunch _punch;

    /// <summary>墨盒当前是否处于展开状态。</summary>
    public bool IsOpen => _punch != null && _punch.IsOpen;

    private void Awake()
    {
        // 旋转表现统一交给 ButtonPunch；这里只负责目标启停。
        _punch = GetComponent<ButtonPunch>();
        if (_punch == null)
            _punch = gameObject.AddComponent<ButtonPunch>();

        Transform target = rotateTarget != null
            ? rotateTarget.transform
            : (targetObject != null ? targetObject.transform : null);
        _punch.ConfigureFlip(target);

        _punch.FlipStarted += OnFlipStarted;
        _punch.FlipFinished += OnFlipFinished;

        // 开始时禁用（Canvas 或整个对象）
        if (startDisabled)
            SetTargetEnabled(false);
    }

    private void OnDestroy()
    {
        if (_punch == null) return;
        _punch.FlipStarted -= OnFlipStarted;
        _punch.FlipFinished -= OnFlipFinished;
    }

    private void OnMouseDown()
    {
        GameBootstrap.TyperPresenter.OnInkToggled();
    }

    /// <summary>主动切换：播放按钮 Punch 并切换墨盒（鼠标点击 / 键盘 C 共用）。</summary>
    public void Toggle()
    {
        Toggle(!IsOpen, true);
    }

    /// <summary>被动同步状态（其他按钮切换时由 Presenter 调用），不播放 Punch。</summary>
    public void Toggle(bool open)
    {
        Toggle(open, false);
    }

    public void Toggle(bool open, bool playPunch)
    {
        if (playPunch) _punch.Punch();
        _punch.SetOpen(open);
    }

    private void OnFlipStarted(bool open)
    {
        // 展开时先启用目标，等 ButtonPunch 给出一帧初始化时间后再旋转
        if (open) SetTargetEnabled(true);
    }

    private void OnFlipFinished(bool open)
    {
        // 收起动画播完后再禁用目标
        if (!open) SetTargetEnabled(false);
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
