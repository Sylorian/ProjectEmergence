using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using Hyperplasia.Runtime;

/// <summary>
/// 融合版：挂在按钮对象上（需要有 Collider2D 才能响应 OnMouseDown）。
/// 点击后：按钮做旋转反馈 + 触发原 PackBagMover 的“显示/隐藏”位移抖动逻辑。
/// 位移目标从场景里按 Key 取：0008 = 背包主体，0007 = frame。
/// </summary>
[RequireComponent(typeof(Collider2D))]
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

    [Header("按钮旋转设置")]
    [SerializeField] private float targetAngleX = -20f;
    [SerializeField] private float duration     = 0.5f;
    [SerializeField] private bool  useEase      = true;

    private Transform bagTransform;    // 0008
    private Transform frameTransform;  // 0007

    private Quaternion startRotation;
    private bool isRotating;

    private bool isShow = true;
    private bool isBusy;
    private Coroutine currentRoutine;

    private void Awake()
    {
        // 记录初始局部旋转，不破坏原本的 Z 轴朝向
        startRotation = transform.localRotation;
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
        GameBootstrap.TyperPresenter.OnInventoryToggled(!isShow);
    }

    /// <summary>统一入口：旋转反馈 + 切换背包显隐</summary>
    public void Trigger()
    {
        if (isBusy || isRotating) return;   // 移动中 / 旋转中忽略连点

        if (isShow)
        {
            isShow = false;
            Play();
        }
        else
        {
            isShow = true;
            Back();
        }

        StartCoroutine(RotateRoutine());
    }

    public void Toggle(bool open)
    {
        if (isBusy || isRotating) return;
        if (open == isShow) return;
        if (open)
        {
            isShow = true;
            Play();
        }
        else
        {
            isShow = false;
            Back();
        }
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
            if (Vector3.Distance(bagTransform.position, targetPosition) < shakeMagnitude * 2f)
                bagTransform.localPosition = targetPosition;

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
            if (Vector3.Distance(bagTransform.localPosition, targetPosition) < shakeMagnitude * 2f)
                bagTransform.localPosition = targetPosition;

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

    // ---------------- 按钮旋转反馈 ----------------

    private IEnumerator RotateRoutine()
    {
        isRotating = true;

        float half = duration * 0.5f;
        Quaternion targetRotation = startRotation * Quaternion.Euler(targetAngleX, 0f, 0f);

        yield return RotateFromTo(startRotation, targetRotation, half);
        yield return RotateFromTo(targetRotation, startRotation, half);

        transform.localRotation = startRotation;
        isRotating = false;
    }

    private IEnumerator RotateFromTo(Quaternion from, Quaternion to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            if (useEase) k = k * k * (3f - 2f * k);
            transform.localRotation = Quaternion.Slerp(from, to, k);
            yield return null;
        }
        transform.localRotation = to;   // 精确到位，避免下一段起点跳变
    }
}