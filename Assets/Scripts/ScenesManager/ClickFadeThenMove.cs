using System;
using System.Collections;
using System.Collections.Generic;
using Hyperplasia.Core.Flow;
using Hyperplasia.Runtime;
using UnityEngine;

/// <summary>
/// 输入 START 并回车、输入状态机离开 StartState 后开始：
/// 阶段1：两个物体淡出消失
/// 阶段2：四个物体两两一组移动到目标点后销毁
/// 挂在一个始终激活的物体上（父物体或空物体）
/// </summary>
[DisallowMultipleComponent]
public class ClickFadeThenMove : MonoBehaviour
{
    [Serializable]
    public class MoveEntry
    {
        public GameObject obj;      // 要移动的物体
        public Transform target;    // 目标位置（场景里放个空物体当目标点）
    }

    [Header("阶段1：淡出消失的物体")]
    [SerializeField] private GameObject fadeObjA;
    [SerializeField] private GameObject fadeObjB;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool fadeSimultaneous = true;   // true=两个同时淡出
    [SerializeField] private bool destroyAfterFade;          // true=销毁，false=SetActive(false)

    [Header("阶段2：移动后销毁（组内并行，各自目标点）")]
    [SerializeField] private MoveEntry[] groupA = new MoveEntry[2];
    [SerializeField] private MoveEntry[] groupB = new MoveEntry[2];
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private bool moveGroupsSimultaneous;     // 两组是否同时移动

    private bool _triggered;

    private void LateUpdate()
    {
        if (_triggered) return;

        InputController controller = GameBootstrap.Controller;
        if (controller == null || controller.State == InputState.Start) return;

        _triggered = true;

        Canvas childCanvas = FindChildCanvas();
        if (childCanvas != null)
            Destroy(childCanvas.gameObject);

        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // ---------- 阶段1：淡出 ----------
        if (fadeSimultaneous)
        {
            Coroutine fadeA = StartCoroutine(FadeOut(fadeObjA));
            Coroutine fadeB = StartCoroutine(FadeOut(fadeObjB));
            yield return fadeA;
            yield return fadeB;
        }
        else
        {
            yield return FadeOut(fadeObjA);
            yield return FadeOut(fadeObjB);
        }

        // ---------- 阶段2：移动 ----------
        if (moveGroupsSimultaneous)
        {
            Coroutine moveA = StartCoroutine(MoveGroup(groupA));
            Coroutine moveB = StartCoroutine(MoveGroup(groupB));
            yield return moveA;
            yield return moveB;
        }
        else
        {
            yield return MoveGroup(groupA);
            yield return MoveGroup(groupB);
        }
    }

    // ================= 淡出 =================
    private IEnumerator FadeOut(GameObject obj)
    {
        if (obj == null) yield break;

        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        // 没有 CanvasGroup 就处理所有 SpriteRenderer（含子物体），适合多部件角色
        SpriteRenderer[] spriteRenderers = canvasGroup == null
            ? obj.GetComponentsInChildren<SpriteRenderer>(true)
            : null;

        float duration = Mathf.Max(0.0001f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(canvasGroup, spriteRenderers, 1f - Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        SetAlpha(canvasGroup, spriteRenderers, 0f);

        if (destroyAfterFade)
            Destroy(obj);
        else
            obj.SetActive(false);   // 淡出后隐藏
    }

    private static void SetAlpha(CanvasGroup canvasGroup, SpriteRenderer[] spriteRenderers, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            return;
        }

        if (spriteRenderers == null) return;

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    // ================= 移动 =================
    private IEnumerator MoveGroup(MoveEntry[] entries)
    {
        if (entries == null) yield break;

        List<Coroutine> running = new();
        foreach (MoveEntry entry in entries)
        {
            if (entry == null || entry.obj == null || entry.target == null) continue;
            running.Add(StartCoroutine(MoveAndDestroy(entry.obj, entry.target.position)));
        }

        foreach (Coroutine routine in running)
            yield return routine;   // 组内并行，等全部到达
    }

    private IEnumerator MoveAndDestroy(GameObject obj, Vector3 targetPosition)
    {
        if (obj == null) yield break;

        Transform objTransform = obj.transform;
        Vector3 startPosition = objTransform.position;
        float duration = Mathf.Max(0.0001f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            objTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        objTransform.position = targetPosition;
        Destroy(obj);   // 到达后销毁
    }

    /// <summary>找到挂载物体下方第一个 Canvas（不包含自身）。</summary>
    private Canvas FindChildCanvas()
    {
        Canvas[] canvases = GetComponentsInChildren<Canvas>(true); // true = 包含未激活的
        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject != gameObject)
                return canvas;
        }

        return null;
    }
}
