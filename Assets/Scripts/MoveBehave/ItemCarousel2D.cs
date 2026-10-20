using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D 单物品循环展示
/// 所有物品都是 itemRoot 的子物体，按子物体顺序循环
/// E = 下一个（向上切换），Q = 上一个（向下切换）
/// 展示中：localRotation = (0,0,0)
/// 隐藏中：localRotation = (0,90,0)
/// </summary>
public class ItemCarousel2D : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("物品的父对象，其所有子物体按顺序循环")]
    public Transform itemRoot;

    [Header("动画")]
    [Tooltip("切换时当前物品水平移动的距离")]
    public float moveDistance = 1.5f;
    [Tooltip("水平移动时长（秒）")]
    public float moveDuration = 0.15f;
    [Tooltip("Y 轴旋转时长（秒）")]
    public float rotateDuration = 0.2f;

    [Header("输入")]
    public KeyCode nextKey = KeyCode.E;
    public KeyCode prevKey = KeyCode.Q;

    private readonly List<Transform> items = new List<Transform>();
    private Vector3[] basePositions;
    private int currentIndex = 0;
    private bool isAnimating = false;

    private static readonly Quaternion HiddenRot = Quaternion.Euler(0f, 90f, 0f);
    private static readonly Quaternion ShownRot  = Quaternion.identity;

    void Start()
    {
        if (itemRoot == null) { Debug.LogError("[ItemCarousel2D] 未指定 itemRoot"); enabled = false; return; }

        items.Clear();
        foreach (Transform child in itemRoot) items.Add(child);

        if (items.Count == 0) { Debug.LogWarning("[ItemCarousel2D] itemRoot 下没有子物体"); enabled = false; return; }

        basePositions = new Vector3[items.Count];
        for (int i = 0; i < items.Count; i++)
            basePositions[i] = items[i].localPosition;

        // 初始化：当前展示，其余侧转隐藏
        for (int i = 0; i < items.Count; i++)
        {
            items[i].localPosition = basePositions[i];
            items[i].localRotation = (i == currentIndex) ? ShownRot : HiddenRot;
        }
    }

    void Update()
    {
        if (isAnimating || items.Count <= 1) return;

        if (Input.GetKeyDown(nextKey))      StartCoroutine(SwitchCoroutine(+1));
        else if (Input.GetKeyDown(prevKey)) StartCoroutine(SwitchCoroutine(-1));
    }

    /// <summary>direction: +1 = 下一个(E，旧物向左移出)，-1 = 上一个(Q，旧物向右移出)</summary>
    IEnumerator SwitchCoroutine(int direction)
    {
        isAnimating = true;

        int oldIndex = currentIndex;
        int newIndex = (currentIndex + direction + items.Count) % items.Count;
        currentIndex = newIndex;

        Transform oldItem = items[oldIndex];
        Transform newItem = items[newIndex];

        Vector3 oldBase = basePositions[oldIndex];
        Vector3 newBase = basePositions[newIndex];
        float sign = Mathf.Sign(direction);

        // 1) 当前物品水平轻微移动
        Vector3 oldOutPos = oldBase + new Vector3(-moveDistance * sign, 0f, 0f);
        yield return LerpLocalPosition(oldItem, oldBase, oldOutPos, moveDuration);

        // 2) 旧物旋转到 90°（隐藏）；同时新物从 90° 转回 0° 并从另一侧滑入
        Vector3 newInPos = newBase + new Vector3(moveDistance * sign * 0.6f, 0f, 0f);
        newItem.localPosition = newInPos;
        newItem.localRotation = HiddenRot;

        StartCoroutine(LerpLocalPosition(newItem, newInPos, newBase, rotateDuration));
        StartCoroutine(LerpLocalRotation(newItem, HiddenRot, ShownRot, rotateDuration));
        yield return LerpLocalRotation(oldItem, ShownRot, HiddenRot, rotateDuration);

        // 3) 收尾（旧物已隐藏，位置复位无感）
        oldItem.localPosition = oldBase;
        oldItem.localRotation = HiddenRot;
        newItem.localPosition = newBase;
        newItem.localRotation = ShownRot;

        isAnimating = false;
    }

    IEnumerator LerpLocalPosition(Transform tr, Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f) { tr.localPosition = to; yield break; }
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            tr.localPosition = Vector3.Lerp(from, to, EaseOut(Mathf.Clamp01(t / duration)));
            yield return null;
        }
        tr.localPosition = to;
    }

    IEnumerator LerpLocalRotation(Transform tr, Quaternion from, Quaternion to, float duration)
    {
        if (duration <= 0f) { tr.localRotation = to; yield break; }
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            tr.localRotation = Quaternion.Slerp(from, to, EaseInOut(Mathf.Clamp01(t / duration)));
            yield return null;
        }
        tr.localRotation = to;
    }

    static float EaseOut(float t)   => 1f - (1f - t) * (1f - t);
    static float EaseInOut(float t) => t * t * (3f - 2f * t);
}