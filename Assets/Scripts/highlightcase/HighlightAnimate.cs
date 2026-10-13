using System.Collections;
using UnityEngine;

/// 形状跳动动画：弹起 → 保持 → 回落。
/// 通过 PlayBounce() 从外部调用。
[DisallowMultipleComponent]
public class HighlightAnimate : MonoBehaviour
{
    [Header("===== 缩放动画 =====")]
    [Tooltip("弹跳峰值增量（0.2 = 最多放大到 1.2 倍）")]
    [Range(0f, 0.6f)] public float scalePeak = 0.18f;

    [Tooltip("弹起上升时间（秒）")]
    [Range(0.02f, 0.4f)] public float riseTime = 0.08f;

    [Tooltip("到达峰值后保持多久再回落（秒）")]
    [Range(0f, 2f)] public float holdTime = 0.3f;

    [Tooltip("回落时间（秒）")]
    [Range(0.05f, 1f)] public float fallTime = 0.18f;

    // ---------- 内部 ----------
    private Vector3 baseScale;
    private Coroutine animRoutine;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnDisable()
    {
        StopAnim();
        transform.localScale = baseScale;
    }

    // ---------- 公开方法 ----------

    /// <summary>播放一次完整的弹跳动画：弹起 → 保持 holdTime → 回落。</summary>
    public void PlayBounce()
    {
        StopAnim();
        animRoutine = StartCoroutine(BounceRoutine());
    }

    /// <summary>立即停止动画并复位到原始缩放。</summary>
    public void ResetScale()
    {
        StopAnim();
        transform.localScale = baseScale;
    }

    // ---------- 协程 ----------

    private IEnumerator BounceRoutine()
    {
        float startMul = CurrentMul();
        float peakMul = 1f + scalePeak;

        yield return ScaleTo(startMul, peakMul, riseTime);

        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        yield return ScaleTo(peakMul, 1f, fallTime);

        transform.localScale = baseScale;
        animRoutine = null;
    }

    // ---------- 工具 ----------

    private void StopAnim()
    {
        if (animRoutine != null)
        {
            StopCoroutine(animRoutine);
            animRoutine = null;
        }
    }

    /// <summary>把缩放从 fromMul 平滑插值到 toMul（相对 baseScale 的倍率）。</summary>
    private IEnumerator ScaleTo(float fromMul, float toMul, float duration)
    {
        if (duration <= 0f)
        {
            transform.localScale = baseScale * toMul;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duration), 3f); // ease out cubic
            transform.localScale = baseScale * Mathf.Lerp(fromMul, toMul, e);
            yield return null;
        }
        transform.localScale = baseScale * toMul;
    }

    /// <summary>当前缩放相对 baseScale 的倍率。</summary>
    private float CurrentMul()
    {
        return baseScale.x != 0f ? transform.localScale.x / baseScale.x : 1f;
    }
    
    // 静态调用
    public static void HighlightCall(string id)
    {
        var bouncer = ScenePropertyRegistry.GetInstance<Transform>(id).GetComponent<HighlightAnimate>();
        if (bouncer != null)
            bouncer.PlayBounce();
        else
            Debug.LogWarning( id + "上没有 BounceAnimate 组件");
    }
}