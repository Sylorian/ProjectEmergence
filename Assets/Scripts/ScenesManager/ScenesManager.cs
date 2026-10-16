using UnityEngine;

public class ScenesManager : MonoBehaviour
{
    [Header("按顺序拖入每个界面的根节点")]
    public Transform[] screens;

    [Header("初始界面")]
    [Tooltip("列表中作为初始界面的索引，从 0 开始")]
    public int startIndex = 0;

    [Header("平滑时间")]
    public float smoothTime = 0.25f;

    [Header("键盘输入")]
    public KeyCode prevKey = KeyCode.A;
    public KeyCode nextKey = KeyCode.D;

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

        if (autoCalculateScreenWidth && cam != null)
        {
            float worldHeight = cam.orthographicSize * 2f;
            screenWidth = worldHeight * cam.aspect;
        }
        else
        {
            screenWidth = manualScreenWidth;
        }

        screenActiveStates = new bool[screens.Length];

        // 初始界面由 startIndex 决定
        currentIndex = Mathf.Clamp(startIndex, 0, screens.Length - 1);
        targetX = screens[currentIndex].position.x;

        var p = transform.position;
        transform.position = new Vector3(targetX, p.y, p.z);

        UpdateActivation(true);
    }

    void Update()
    {
        if (screens == null || screens.Length == 0) return;

        if (Input.GetKeyDown(prevKey)) Prev();
        if (Input.GetKeyDown(nextKey)) Next();

        float x = Mathf.SmoothDamp(
            transform.position.x,
            targetX,
            ref velocityX,
            smoothTime
        );

        var p = transform.position;
        transform.position = new Vector3(x, p.y, p.z);

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

        if (autoActivate) UpdateActivation(false);
    }

    void UpdateActivation(bool force)
    {
        if (screens == null || screens.Length == 0) return;
        if (screenActiveStates == null || screenActiveStates.Length != screens.Length)
            screenActiveStates = new bool[screens.Length];

        float camX = transform.position.x;
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
}