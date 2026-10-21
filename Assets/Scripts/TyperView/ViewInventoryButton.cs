using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Hyperplasia.Runtime;
using Hyperplasia.View;

/// <summary>
/// 背包按钮：点击后切换背包显隐（位移 + 抖动），并交给 ButtonPunch 播放按钮旋转反馈。
/// 位移目标从场景里按 Key 取：0008 = 背包主体，0007 = frame。
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ButtonPunch))]
public class ViewInventoryButton : MonoBehaviour
{
    [Header("场景对象 Key")]
    [SerializeField] private string bagKey   = "0008";  // 原 PackBagMover 挂载对象
    [SerializeField] private string frameKey = "0007";  // frame

    [Header("位移设置")]
    [SerializeField] private Vector3 checkPoint      = new Vector3(0f, 6.2f, -12f);
    [SerializeField] private Vector3 targetPosition  = new Vector3(0f, 0f, -12f);
    [SerializeField] private float   moveSpeed       = 33f;

    [Header("抖动设置")]
    [SerializeField] private bool    enableShake    = true;
    [SerializeField] private float   shakeDuration  = 0.7f;
    [SerializeField] private float   shakeMagnitude = 0.6f;
    [SerializeField] private float   shakeFrequency = 15f;
    [SerializeField] private Vector3 shakeAxis      = new Vector3(1f, 1f, 0f);

    private Transform bagTransform;    // 0008
    private Transform frameTransform;  // 0007

    private bool isShow = true;
    private bool isBusy;
    private Coroutine currentRoutine;

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
        bagTransform   = ScenePropertyRegistry.GetInstance<Transform>(bagKey);
        frameTransform = ScenePropertyRegistry.GetInstance<Transform>(frameKey);

        if (bagTransform == null)
        {
            Debug.LogError($"[{nameof(ViewInventoryButton)}] 找不到场景对象 \"{bagKey}\"，脚本已禁用");
            enabled = false;
        }
    }

    // 点击触发
    private void OnMouseDown()
    {
        GameBootstrap.TyperPresenter.OnInventoryToggled();
    }

    /// <summary>主动切换：播放按钮 Punch 并切换背包显隐（鼠标点击 / 键盘 I 共用）。</summary>
    public void Toggle()
    {
        Toggle(!isShow, true);
    }

    /// <summary>被动同步状态（其他按钮切换时由 Presenter 调用），不播放 Punch。</summary>
    public void Toggle(bool open)
    {
        Toggle(open, false);
    }

    /// <summary>
    /// 切换到指定显隐状态。
    /// 若位移协程正在播放且请求的是相反状态，会停掉当前协程，
    /// 然后从当前位置继续用动画平滑过渡到请求的状态。
    /// </summary>
    public void Toggle(bool open, bool playPunch)
    {
        if (playPunch) _punch.Punch();

        if (isBusy)
        {
            if (open == isShow) return; // 已经在朝这个状态走
            StopCurrentMotion();        // 停掉协程，从当前位置重新过渡
        }
        else if (open == isShow)
        {
            return;
        }

        isShow = open;
        if (isShow) Play();
        else Back();
    }

    /// <summary>停掉当前位移协程，保留当前位置，供后续平滑过渡。</summary>
    private void StopCurrentMotion()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        isBusy = false;
    }

    public void Play()
    {
        if (isBusy) return;
        isBusy = true;
        currentRoutine = StartCoroutine(Routine());
    }

    public void Back()
    {
        if (isBusy) return;
        isBusy = true;
        currentRoutine = StartCoroutine(RoutineBack());
    }

    public void Stop()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
            if (bagTransform != null) bagTransform.localPosition = targetPosition;
        }
        isBusy = false;
    }

    // ---------------- 位移 / 抖动（作用于 0008） ----------------

    private IEnumerator RoutineBack()
    {
        try
        {
            while (Vector3.Distance(bagTransform.localPosition, checkPoint) > 0.01f)
            {
                if (frameTransform != null)
                {
                    frameTransform.localPosition = Vector3.MoveTowards(
                        frameTransform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime / 5f);
                }

                bagTransform.localPosition = Vector3.MoveTowards(
                    bagTransform.localPosition, checkPoint, moveSpeed * Time.deltaTime);
                yield return null;
            }

            bagTransform.localPosition = checkPoint;
        }
        finally
        {
            isBusy = false;
            currentRoutine = null;
        }
    }

    private IEnumerator Routine()
    {
        try
        {
            while (Vector3.Distance(bagTransform.localPosition, targetPosition) > 0.01f)
            {
                if (frameTransform != null)
                {
                    frameTransform.localPosition = Vector3.MoveTowards(
                        frameTransform.localPosition, new Vector3(0f, -1f, 0f), moveSpeed * Time.deltaTime / 5f);
                }

                bagTransform.localPosition = Vector3.MoveTowards(
                    bagTransform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            bagTransform.localPosition = targetPosition;

            if (enableShake)
            {
                float elapsed = 0f;
                float seed = Random.value * 100f;

                while (elapsed < shakeDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / shakeDuration);
                    float damping = (1f - t) * (1f - t);

                    float nx = Mathf.PerlinNoise(seed, elapsed * shakeFrequency) * 2f - 1f;
                    float ny = Mathf.PerlinNoise(seed + 50f, elapsed * shakeFrequency) * 2f - 1f;

                    Vector3 offset = Vector3.Scale(new Vector3(nx, ny, 0f), shakeAxis);
                    bagTransform.localPosition = targetPosition + offset * shakeMagnitude * damping;
                    yield return null;
                }

                bagTransform.localPosition = targetPosition;
            }
        }
        finally
        {
            isBusy = false;
            currentRoutine = null;
        }
    }
}
