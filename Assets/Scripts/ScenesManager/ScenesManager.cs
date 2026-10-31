using System.Collections;
using System.Collections.Generic;
using Hyperplasia.Core.Data;
using Hyperplasia.Runtime;
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

    // 角色越界阈值
    private const float BoundaryX = 18.1f;

    // 箱子对象 ID
    private readonly string[] boxIds = { "0005", "0004", "0003", "0002" };

    private string[] sortinglayer = new string[]
    {
        "BedroomObject","StoreroomObject","LivingroomObject","HallwayObject","KitchenObject","WashroomObject"
    };

    /// <summary>界面根节点名称 → 数据层房间名（与 GameData 中的 room 字段一致）。</summary>
    private static readonly Dictionary<string, RoomEnum> RoomNameByScreen = new()
    {
        { "HouseBedroom", RoomEnum.Bedroom },
        { "HouseStoreroom", RoomEnum.Storeroom },
        { "HouseLivingroom", RoomEnum.Livingroom },
        { "HouseHallway", RoomEnum.Hallway },
        { "HouseKitchen", RoomEnum.Kitchen },
        { "HouseWashroom", RoomEnum.Washroom }
    };

    private int currentIndex;
    private float targetX;
    private float velocityX;
    private float screenWidth;
    private bool[] screenActiveStates;
    private bool isTransitioning;   // 切换流程进行中，防止重复触发
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

        currentIndex = Mathf.Clamp(startIndex, 0, screens.Length - 1);
        SyncCurrentRoom();
        targetX = screens[currentIndex].position.x;

        var p = transform.position;
        transform.position = new Vector3(targetX, p.y, p.z);

        UpdateActivation(true);
    }

    void Update()
    {
        if (screens == null || screens.Length == 0) return;

        var character = ScenePropertyRegistry.GetInstance<Transform>("01");

        // 角色穿墙检测：启动完整的「右移 → 切屏 → 左移」协程
        if (!isTransitioning && character != null)
        {
            float cx = character.localPosition.x;

            if (cx < -BoundaryX)
            {
                // 从左侧穿出 → 从右侧进入 → 相机左移一个界面
                character.localPosition = new Vector3(13.5f, -2f, 1f);
                StartCoroutine(TransitionRoutine(-1));
            }
            else if (cx > BoundaryX)
            {
                // 从右侧穿出 → 从左侧进入 → 相机右移一个界面
                character.localPosition = new Vector3(-13.5f, -2f, 1f);
                StartCoroutine(TransitionRoutine(+1));
            }
        }

        // 相机平滑
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

    /// <summary>
    /// 切换流程：
    ///   1) 所有箱子先向右移动并等待完成
    ///   2) 相机切换到目标界面并等待到位
    ///   3) 所有箱子最后向左返回
    /// </summary>
    private IEnumerator TransitionRoutine(int direction)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        List<boxmover> movers = GetBoxMovers();
        float timeout = MaxOneWayDuration(movers) + 1f;

        // ── 1. 先执行：箱子向右移动 ──
        for (int i = 0; i < movers.Count; i++) movers[i].BeginRight();
        yield return WaitUntilOrTimeout(() => AreAllRightFinished(movers), timeout);

        // ── 2. 再执行：相机切换界面 ──
        MoveTo(currentIndex + direction);
        yield return WaitUntilOrTimeout(
            () => Mathf.Abs(transform.position.x - targetX) <= 0.01f,
            5f);

        var p = transform.position;
        transform.position = new Vector3(targetX, p.y, p.z);
        velocityX = 0f;

        // ── 3. 最后执行：箱子向左返回 ──
        for (int i = 0; i < movers.Count; i++) movers[i].BeginLeft();
        yield return WaitUntilOrTimeout(() => AreAllLeftFinished(movers), timeout);

        isTransitioning = false;
    }

    private IEnumerator WaitUntilOrTimeout(System.Func<bool> condition, float timeout)
    {
        float t = 0f;
        while (!condition() && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    private static bool AreAllRightFinished(List<boxmover> movers)
    {
        for (int i = 0; i < movers.Count; i++)
            if (movers[i] != null && !movers[i].IsRightFinished) return false;
        return true;
    }

    private static bool AreAllLeftFinished(List<boxmover> movers)
    {
        for (int i = 0; i < movers.Count; i++)
            if (movers[i] != null && movers[i].IsMovingLeft) return false;
        return true;
    }

    private static float MaxOneWayDuration(List<boxmover> movers)
    {
        float d = 1f;
        for (int i = 0; i < movers.Count; i++)
            if (movers[i] != null) d = Mathf.Max(d, movers[i].oneWayDuration);
        return d;
    }

    private List<boxmover> GetBoxMovers()
    {
        var list = new List<boxmover>();
        for (int i = 0; i < boxIds.Length; i++)
        {
            var tr = ScenePropertyRegistry.GetInstance<Transform>(boxIds[i]);
            if (tr == null) continue;
            var bm = tr.GetComponent<boxmover>();
            if (bm != null) list.Add(bm);
        }
        return list;
    }

    /// <summary>公开接口：让所有箱子执行一次完整流程（不移动相机）</summary>
    public void BoxMove()
    {
        var movers = GetBoxMovers();
        for (int i = 0; i < movers.Count; i++) movers[i].Begin();
    }

    public void Next() => MoveTo(currentIndex + 1);
    public void Prev() => MoveTo(currentIndex - 1);

    public void MoveTo(int index)
    {
        if (screens == null || screens.Length == 0) return;

        index = Mathf.Clamp(index, 0, screens.Length - 1);
        currentIndex = index;
        SyncCurrentRoom();

        var character = ScenePropertyRegistry.GetInstance<Transform>("01");
        if (character != null && index < sortinglayer.Length)
        {
            var sr = character.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingLayerName = sortinglayer[index];
        }

        targetX = screens[currentIndex].position.x;

        if (autoActivate) UpdateActivation(false);
    }

    /// <summary>把当前界面同步到数据层的 GameSession.CurrentRoom。</summary>
    private void SyncCurrentRoom()
    {
        if (screens == null || screens.Length == 0) return;
        if (currentIndex < 0 || currentIndex >= screens.Length) return;

        Transform screen = screens[currentIndex];
        if (screen == null) return;

        if (!RoomNameByScreen.TryGetValue(screen.name, out RoomEnum room))
        {
            Debug.LogWarning($"[ScenesManager] 界面 {screen.name} 未配置对应的房间名", screen);
            return;
        }

        var session = GameBootstrap.Session;
        if (session != null)
            session.CurrentRoom = room;
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