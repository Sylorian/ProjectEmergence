using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 挂在带 Light2D 的对象上，提供颜色、强度、半径、开关的运行时设置接口。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Light2D))]
public class URP2DLightController : MonoBehaviour
{
    [Header("灯光设置")]
    [SerializeField] private Color lightColor = new Color(1f, 0.85f, 0.6f, 1f); // 灯光颜色（默认暖色）
    [SerializeField] private float intensity = 1f;   // 灯光强度
    [SerializeField] private float outerRadius = 5f; // 点光外半径，仅 Point/Freeform 有效
    [SerializeField] private bool isOn = true;       // 灯光是否开启

    [Header("启动")]
    [SerializeField] private bool applyOnStart = true; // 场景开始时把上面的参数应用到 Light2D

    private Light2D cachedLight;

    /// <summary>本控制器驱动的 Light2D。</summary>
    public Light2D TargetLight
    {
        get
        {
            if (cachedLight == null) cachedLight = GetComponent<Light2D>();
            return cachedLight;
        }
    }

    public Color LightColor => lightColor;
    public float Intensity => intensity;
    public float OuterRadius => outerRadius;
    public bool IsOn => isOn;

    private void Awake() => cachedLight = GetComponent<Light2D>();

    private void Start()
    {
        if (applyOnStart) ApplyAll();
    }

    /// <summary>把面板上的参数全部应用到 Light2D。</summary>
    public void ApplyAll()
    {
        SetColor(lightColor);
        SetIntensity(intensity);
        SetRadius(outerRadius);
        SetOn(isOn);
    }

    /// <summary>设置灯光颜色。</summary>
    public void SetColor(Color value)
    {
        lightColor = value;
        if (TargetLight != null) TargetLight.color = value;
    }

    /// <summary>设置灯光强度，负值按 0 处理。</summary>
    public void SetIntensity(float value)
    {
        intensity = Mathf.Max(0f, value);
        if (TargetLight != null) TargetLight.intensity = intensity;
    }

    /// <summary>设置点光外半径，负值按 0 处理。</summary>
    public void SetRadius(float value)
    {
        outerRadius = Mathf.Max(0f, value);
        if (TargetLight != null) TargetLight.pointLightOuterRadius = outerRadius;
    }

    /// <summary>开灯 / 关灯。</summary>
    public void SetOn(bool value)
    {
        isOn = value;
        if (TargetLight != null) TargetLight.enabled = value;
    }

    /// <summary>切换开关状态。</summary>
    public void Toggle() => SetOn(!isOn);
}
