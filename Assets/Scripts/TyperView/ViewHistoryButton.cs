using UnityEngine;
using Hyperplasia.Runtime;
using Hyperplasia.View;

[RequireComponent(typeof(Collider2D))] // OnMouseDown 需要碰撞体
[RequireComponent(typeof(ButtonPunch))]
public class ViewHistoryButton : MonoBehaviour
{
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

    // ---- 移动状态 ----
    private bool _paperPosition = false;
    private bool _initialized = false;
    private Transform _paperTransform;
    private RectTransform _outputRect;
    private Vector3 _targetPaperPosition;
    private float _targetOutputRectTopMargin;

    private ButtonPunch _punch;

    private void Awake()
    {
        // 按钮旋转反馈统一交给 ButtonPunch
        _punch = GetComponent<ButtonPunch>();
        if (_punch == null)
            _punch = gameObject.AddComponent<ButtonPunch>();
    }

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        if (!_initialized) TryInitialize();

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

    /// <summary>
    /// 解析注册表中的纸张 / 输出区域并初始化到收起状态。
    /// GameObjectRegistry 与按钮脚本的 Start 执行顺序不确定，
    /// 所以允许在 Update 中重试，避免注册表尚未写入时引用永久为空。
    /// </summary>
    private void TryInitialize()
    {
        if (_paperTransform == null)
            _paperTransform = GameBootstrap.GetComponentById<Transform>(paperMoverId);
        if (_outputRect == null)
            _outputRect = GameBootstrap.GetComponentById<RectTransform>(OutputRectId);

        if (_initialized) return;
        if (_paperTransform == null && _outputRect == null) return;

        // 还没点过按钮时，首次拿到目标就同步到初始（收起）状态
        if (!_paperPosition)
        {
            _targetPaperPosition = positionClosed;
            _targetOutputRectTopMargin = TopMarginClosed;
            if (_paperTransform != null)
                _paperTransform.localPosition = _targetPaperPosition;
            if (_outputRect != null)
                _outputRect.offsetMax = new Vector2(_outputRect.offsetMax.x, -_targetOutputRectTopMargin);
        }

        _initialized = true;
    }

    private void OnMouseDown()
    {
        GameBootstrap.TyperPresenter.OnHistoryToggled();
    }

    /// <summary>主动切换：播放按钮 Punch 并移动纸张（鼠标点击 / 键盘 H 共用）。</summary>
    public void Toggle()
    {
        Toggle(!_paperPosition, true);
    }

    /// <summary>被动同步状态（其他按钮切换时由 Presenter 调用），不播放 Punch。</summary>
    public void Toggle(bool open)
    {
        Toggle(open, false);
    }

    public void Toggle(bool open, bool playPunch)
    {
        if (playPunch) _punch.Punch();

        if (open == _paperPosition) return;
        TogglePaperPosition(open);
    }

    /// <summary>切换纸张盒子的目标位置</summary>
    private void TogglePaperPosition(bool open)
    {
        _paperPosition = open;
        _targetPaperPosition = _paperPosition ? positionOpen : positionClosed;
        _targetOutputRectTopMargin = _paperPosition ? TopMarginOpen : TopMarginClosed;
    }
}
