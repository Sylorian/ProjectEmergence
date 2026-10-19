using System;
using System.Collections;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

public class PackBagMover : MonoBehaviour
{

    [SerializeField] private float shakeDuration = 0.8f;   // 抖动持续时间
    [SerializeField] private float shakeMagnitude = 0.15f; // 初始抖动幅度
    [SerializeField] private float shakeFrequency = 25f;   // 抖动频率（越大越"快"）
    [SerializeField] private Vector3 shakeAxis = new Vector3(1, 1, 0); // 2D项目把Z设为0

    private bool isRunning;
    private bool isShow = true;
    private Vector3 CheckPoint = new Vector3(0, 6.2f, -12);
    private Vector3 targetPosition = new Vector3(0f, 0f, -12f);
    private float moveSpeed = 33f;
    private bool enableShake = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            if (isShow)
            {
                isShow = !isShow;
                Play();
            }
            else
            {
                isShow = !isShow;
                Back();
            }
        }
    }

    public void Play()
    {
        if (isRunning) return;
        StartCoroutine(Routine());
    }

    public void Back()
    {
        StartCoroutine(RoutineBack());
    }

    private IEnumerator RoutineBack()
    {
        while (Vector3.Distance(transform.position, CheckPoint) > 0.01f)
        {
            var frame_ = ScenePropertyRegistry.GetInstance<Transform>("0006");
            frame_.transform.position =
                Vector3.MoveTowards(frame_.position, new Vector3(0, 0, 0), moveSpeed * Time.deltaTime / 5);
            transform.position = Vector3.MoveTowards(
                transform.position, CheckPoint, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = CheckPoint;
    }

    private IEnumerator Routine()
    {
        isRunning = true;

        // ---------- 阶段 1：移动到终点 ----------
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            var frame_ = ScenePropertyRegistry.GetInstance<Transform>("0006");
            frame_.transform.position =
                Vector3.MoveTowards(frame_.position, new Vector3(0, -1, 0), moveSpeed * Time.deltaTime / 5);
            transform.position = Vector3.MoveTowards(
                transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;

        // ---------- 阶段 2：抖动衰减 ----------
        if (enableShake)
        {
            float elapsed = 0f;
            float seed = Random.value * 100f; // Perlin 采样起点，避免每次都一样

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / shakeDuration);

                // 二次衰减：一开始抖得猛，结尾平滑归零
                float damping = (1f - t) * (1f - t);

                // Perlin 噪声返回 0~1，映射到 -1~1
                float nx = Mathf.PerlinNoise(seed, elapsed * shakeFrequency) * 2f - 1f;
                float ny = Mathf.PerlinNoise(seed + 50f, elapsed * shakeFrequency) * 2f - 1f;

                Vector3 offset = new Vector3(nx, ny, 0f);
                offset = Vector3.Scale(offset, shakeAxis); // 只在指定轴上抖

                transform.position = targetPosition + offset * shakeMagnitude * damping;
                yield return null;
            }

            transform.position = targetPosition; // 精确归位，防止残留偏移
        }
        isRunning = false;

    }
}