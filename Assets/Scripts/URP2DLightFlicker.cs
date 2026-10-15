using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 挂在带 Light2D 的对象上，让灯光强度随时间闪烁，频率与幅度可调。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Light2D))]
public class URP2DLightFlicker : MonoBehaviour
{
    [Header("闪烁")]
    [SerializeField] private bool flickerEnabled = true; // 是否启用闪烁

    [Tooltip("每秒强度波动的次数")]
    [SerializeField] private float frequency = 8f;

    [Tooltip("强度波动幅度，相对基础强度的比例。0.25 表示上下浮动 25%")]
    [Range(0f, 1f)]
    [SerializeField] private float amplitude = 0.25f;

    [Tooltip("0 = 不规则的噪声闪烁，1 = 平滑的正弦闪烁")]
    [Range(0f, 1f)]
    [SerializeField] private float smoothness = 0.5f;

    [Header("基础强度")]
    [Tooltip("闪烁围绕的中心强度")]
    [SerializeField] private float baseIntensity = 1f;

    [Tooltip("勾选后改用 Light2D 当前的强度作为中心强度")]
    [SerializeField] private bool useLightIntensityAsBase = true;

    [Tooltip("随机相位，避免多个灯光同步闪烁")]
    [SerializeField] private bool randomizePhase = true;

    private Light2D cachedLight;
    private float noiseSeed;
    private float phaseOffset;

    private void Awake()
    {
        cachedLight = GetComponent<Light2D>();

        if (useLightIntensityAsBase && cachedLight != null) baseIntensity = cachedLight.intensity;

        noiseSeed = randomizePhase ? Random.value * 100f : 0f;
        phaseOffset = randomizePhase ? Random.value * Mathf.PI * 2f : 0f;
    }

    private void Update()
    {
        if (!flickerEnabled || cachedLight == null) return;
        cachedLight.intensity = EvaluateIntensity(Time.time);
    }

    /// <summary>计算某个时刻的闪烁强度。</summary>
    public float EvaluateIntensity(float time)
    {
        float phase = (time + phaseOffset) * frequency;

        float noise = Mathf.PerlinNoise(noiseSeed, phase);           // 不规则分量
        float wave = Mathf.Sin(phase * Mathf.PI * 2f) * 0.5f + 0.5f; // 平滑分量
        float blend = Mathf.Lerp(noise, wave, smoothness);
        float scale = Mathf.Lerp(1f - amplitude, 1f + amplitude, blend);

        return Mathf.Max(0f, baseIntensity * scale);
    }

    /// <summary>运行时开关闪烁，关闭时灯光保持当前强度。</summary>
    public void SetFlickerEnabled(bool value) => flickerEnabled = value;

    /// <summary>运行时修改闪烁频率。</summary>
    public void SetFrequency(float value) => frequency = Mathf.Max(0f, value);

    /// <summary>运行时修改闪烁幅度。</summary>
    public void SetAmplitude(float value) => amplitude = Mathf.Clamp01(value);

    /// <summary>把 Light2D 当前强度记作新的闪烁中心。</summary>
    public void CaptureBaseIntensity()
    {
        if (cachedLight != null) baseIntensity = cachedLight.intensity;
    }
}
