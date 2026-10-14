using UnityEngine;

public class HorizontalScreenSwitcher : MonoBehaviour
{
    [Header("按顺序拖入每个界面的根节点")]
    public Transform[] screens;

    [Header("平滑时间")]
    public float smoothTime = 0.25f;

    [Header("性能优化")]
    [Tooltip("是否自动激活/禁用附近界面")]
    public bool autoActivate = true;

    [Tooltip("前后额外激活的界面数量，1 表示当前+左右各1个")]
    public int activateBuffer = 1;

    [Tooltip("自动计算屏幕世界宽度")]
    public bool autoCalculateScreenWidth = true;

    [Tooltip("手动设置屏幕世界宽度，autoCalculateScreenWidth = false 时生效")]
    public float manualScreenWidth = 19.2f;

    private int currentIndex;
    private float targetX;
    private float velocityX;
    private Vector2 swipeStart;
    private float screenWidth;
    private bool[] screenActiveStates;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        if (screens == null || screens.Length == 0) return;

        // 计算屏幕世界宽度
        if (autoCalculateScreenWidth && cam != null)
        {
            float worldHeight = cam.orthographicSize * 2f;
            screenWidth = worldHeight * cam.aspect;
        }
        else
        {
            screenWidth = manualScreenWidth;
        }

        // 初始化激活状态数组
        screenActiveStates = new bool[screens.Length];

        currentIndex = 0;
        targetX = screens[0].position.x;

        // 相机初始位置
        var p = transform.position;
        transform.position = new Vector3(targetX, p.y, p.z);

        // 初始化激活状态
        UpdateActivation(true);
    }

    void Update()
    {
        if (screens == null || screens.Length == 0) return;

        // 键盘输入
        if (Input.GetKeyDown(KeyCode.A)) Prev();
        if (Input.GetKeyDown(KeyCode.D)) Next();

        // 平滑移动相机
        float x = Mathf.SmoothDamp(
            transform.position.x,
            targetX,
            ref velocityX,
            smoothTime
        );

        var p = transform.position;
        transform.position = new Vector3(x, p.y, p.z);

        // 性能优化：动态激活附近界面
        if (autoActivate) UpdateActivation(false);
    }
    public void Next() => MoveTo(currentIndex + 1);
    public void Prev() => MoveTo(currentIndex - 1);

    public void MoveTo(int index)
    {
        if (screens == null || screens.Length == 0) return;

        index = Mathf.Clamp(index, 0, screens.Length - 1);
        currentIndex = index;
        targetX = screens[currentIndex].position.x;

        // 目标切换时立即更新激活，保证移动过程中相邻界面可见
        if (autoActivate) UpdateActivation(false);
    }

    /// <summary>
    /// 根据相机当前位置，激活附近界面，禁用远处界面。
    /// </summary>
    /// <param name="force">强制刷新所有状态</param>
    void UpdateActivation(bool force)
    {
        if (screens == null || screens.Length == 0) return;
        if (screenActiveStates == null || screenActiveStates.Length != screens.Length)
            screenActiveStates = new bool[screens.Length];

        float camX = transform.position.x;

        // 激活范围：当前页中心 ± (0.5 + activateBuffer) * 屏幕宽度
        float range = screenWidth * (activateBuffer + 0.5f);

        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i] == null) continue;

            bool shouldActive = Mathf.Abs(screens[i].position.x - camX) <= range;

            if (force || screenActiveStates[i] != shouldActive)
            {
                screens[i].gameObject.SetActive(shouldActive);
                screenActiveStates[i] = shouldActive;
            }
        }
    }

    // 可选：在 Scene 视图画线，方便调试排列
    void OnDrawGizmosSelected()
    {
        if (screens == null) return;
        Gizmos.color = Color.green;
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i] == null) continue;
            Gizmos.DrawWireCube(screens[i].position, new Vector3(screenWidth, 10f, 0f));
        }
    }
}