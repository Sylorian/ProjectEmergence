using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PackBagMover : MonoBehaviour
{
    private float shakeDuration = 0.7f;
    private float shakeMagnitude = 0.6f;
    private float shakeFrequency = 15f;
    private Vector3 shakeAxis = new Vector3(1, 1, 0);

    private bool isShow = true;
    private Vector3 CheckPoint = new Vector3(0, 6.2f, -12);
    private Vector3 targetPosition = new Vector3(0f, 0f, -12f);
    private float moveSpeed = 33f;
    private bool enableShake = true;

    private bool isBusy = false;              // 是否正在移动/抖动中
    private Coroutine currentRoutine;
    private Transform frameTransform;

    private void Start()
    {
        frameTransform = ScenePropertyRegistry.GetInstance<Transform>("0007");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            Debug.Log("触发");
            // 正在移动中：直接忽略这次点击
            if (isBusy) return;

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
            transform.position = targetPosition;
        }
        isBusy = false;
    }

    private IEnumerator RoutineBack()
    {
        try
        {
            if (Vector3.Distance(transform.position, targetPosition) < shakeMagnitude * 2f)
                transform.position = targetPosition;

            while (Vector3.Distance(transform.position, CheckPoint) > 0.01f)
            {
                    frameTransform.localPosition = Vector3.MoveTowards(
                        frameTransform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime / 5f);

                transform.position = Vector3.MoveTowards(
                    transform.position, CheckPoint, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = CheckPoint;
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
            if (Vector3.Distance(transform.position, targetPosition) < shakeMagnitude * 2f)
                transform.position = targetPosition;

            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                    frameTransform.localPosition = Vector3.MoveTowards(
                        frameTransform.localPosition, new Vector3(0, -1, 0), moveSpeed * Time.deltaTime / 5f);

                transform.position = Vector3.MoveTowards(
                    transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPosition;

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

                    Vector3 offset = new Vector3(nx, ny, 0f);
                    offset = Vector3.Scale(offset, shakeAxis);

                    transform.position = targetPosition + offset * shakeMagnitude * damping;
                    yield return null;
                }

                transform.position = targetPosition;
            }
        }
        finally
        {
            isBusy = false;
            currentRoutine = null;
        }
    }
}