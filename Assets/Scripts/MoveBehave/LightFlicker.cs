using UnityEngine;
using UnityEngine.Rendering.Universal;   // 旧版本可能是 UnityEngine.Experimental.Rendering.Universal

[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [Header("强度范围")]
    [SerializeField] private float minIntensity = 3f;
    [SerializeField] private float maxIntensity = 5.5f;

    [Header("波动速度（Hz，每秒完整波动次数）")]
    [SerializeField] private float pulseSpeed = 1.5f;

    private Light2D _light;
    private float   _phase;

    private void Awake()
    {
        _light = GetComponent<Light2D>();
    }

    private void Update()
    {
        // 相位累加：pulseSpeed 次/秒 × 2π
        _phase += Time.deltaTime * pulseSpeed * Mathf.PI * 2f;

        // sin 值 (-1~1) 映射到 0~1，再映射到 3~5.5
        float t = (Mathf.Sin(_phase) + 1f) * 0.5f;
        _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}