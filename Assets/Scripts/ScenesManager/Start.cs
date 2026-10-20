using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 左键点击 → 阶段1：两个物体淡出消失
///           → 阶段2：四个物体两两一组移动到目标点后销毁
/// 挂在一个始终激活的物体上（父物体或空物体）
/// </summary>
public class ClickFadeThenMove : MonoBehaviour
{
    [System.Serializable]
    public class MoveEntry
    {
        public GameObject obj;      // 要移动的物体
        public Transform target;    // 目标位置（场景里放个空物体当目标点）
    }

    [Header("点击设置")]
    [SerializeField] private bool ignoreClickOnUI = false;

    [Header("阶段1：淡出消失的物体")]
    [SerializeField] private GameObject fadeObjA;
    [SerializeField] private GameObject fadeObjB;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool fadeSimultaneous = true;   // true=两个同时淡出
    [SerializeField] private bool destroyAfterFade = false;  // true=销毁，false=SetActive(false)

    [Header("阶段2：移动后销毁（组内并行，各自目标点）")]
    [SerializeField] private MoveEntry[] groupA = new MoveEntry[2];
    [SerializeField] private MoveEntry[] groupB = new MoveEntry[2];
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private bool moveGroupsSimultaneous = false; // 两组是否同时移动

    private bool triggered = false;
    

    private void Update()
    {
        if (triggered) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (ignoreClickOnUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        triggered = true;
        Destroy(FindChildCanvas().gameObject);
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // ---------- 阶段1：淡出 ----------
        if (fadeSimultaneous)
        {
            Coroutine a = StartCoroutine(FadeOut(fadeObjA));
            Coroutine b = StartCoroutine(FadeOut(fadeObjB));
            yield return a;
            yield return b;
        }
        else
        {
            yield return FadeOut(fadeObjA);
            yield return FadeOut(fadeObjB);
        }

        // ---------- 阶段2：移动 ----------
        if (moveGroupsSimultaneous)
        {
            Coroutine a = StartCoroutine(MoveGroup(groupA));
            Coroutine b = StartCoroutine(MoveGroup(groupB));
            yield return a;
            yield return b;
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

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        // 没有 CanvasGroup 就处理所有 SpriteRenderer（含子物体），适合多部件角色
        SpriteRenderer[] srs = cg == null ? obj.GetComponentsInChildren<SpriteRenderer>(true) : null;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(cg, srs, 1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        SetAlpha(cg, srs, 0f);

        if (destroyAfterFade) Destroy(obj);
        else obj.SetActive(false);   // 淡出后隐藏
    }

    private void SetAlpha(CanvasGroup cg, SpriteRenderer[] srs, float a)
    {
        if (cg != null) { cg.alpha = a; return; }
        if (srs == null) return;
        foreach (var s in srs)
        {
            var c = s.color; c.a = a; s.color = c;
        }
    }

    // ================= 移动 =================
    private IEnumerator MoveGroup(MoveEntry[] entries)
    {
        if (entries == null) yield break;

        List<Coroutine> running = new List<Coroutine>();
        foreach (var e in entries)
        {
            if (e == null || e.obj == null || e.target == null) continue;
            running.Add(StartCoroutine(MoveAndDestroy(e.obj, e.target.position)));
        }
        foreach (var c in running) yield return c;   // 组内并行，等全部到达
    }

    private IEnumerator MoveAndDestroy(GameObject obj, Vector3 targetPos)
    {
        if (obj == null) yield break;

        Vector3 start = obj.transform.position;
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / moveDuration));
            obj.transform.position = Vector3.Lerp(start, targetPos, k);
            yield return null;
        }
        obj.transform.position = targetPos;

        
        Destroy(obj);   // 到达后销毁
    }
    
    private Canvas FindChildCanvas()
    {
        Canvas[] all = GetComponentsInChildren<Canvas>(true); // true = 包含未激活的
        foreach (var c in all)
        {
            if (c.gameObject != gameObject)
                return c;
        }
        return null;
    }
}