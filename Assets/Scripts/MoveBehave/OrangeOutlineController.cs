using System;
using Hyperplasia.Runtime;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class OrangeOutlineController : MonoBehaviour
{
    [ColorUsage(true, true)] // 开启 HDR，为后续可能的 Bloom 发光做准备
    public Color outlineColor = new Color(1.0f, 0.5f, 0.0f, 1.0f); // 橙色

    [Range(0, 10)]
    public float outlineWidth = 2.0f;

    private SpriteRenderer _spriteRenderer;
    private Material _outlineMaterial;

    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        GameBootstrap.Instance.Session.Typer.OrangeOutline += ApplyOutline;
        Invoke(nameof(ApplyOutline), 0f);
    }

    void OnDisable()
    {
        GameBootstrap.Instance.Session.Typer.OrangeOutline -= ApplyOutline;
        CancelInvoke(nameof(ApplyOutline));
    }

    /// <summary>
    /// 应用描边（事件处理方法，签名与 Action 匹配：无参无返回值）。
    /// </summary>
    public void ApplyOutline()
    {
        if (_spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer 未获取到，无法应用描边。");
            return;
        }

        if (_spriteRenderer.sprite == null)
        {
            Debug.LogWarning("SpriteRenderer 上没有 Sprite，无法应用描边。");
            return;
        }

        // 如果材质不是我们的描边材质，则创建新实例
        if (_outlineMaterial == null || !_outlineMaterial.shader.name.Contains("OrangeOutline"))
        {
            Shader outlineShader = Shader.Find("Custom/2D/OrangeOutline");
            if (outlineShader == null)
            {
                Debug.LogError("找不到描边 Shader，请确认 Shader 文件已创建且路径正确。");
                return;
            }

            _outlineMaterial = new Material(outlineShader);
            _spriteRenderer.material = _outlineMaterial;
        }

        // 应用参数
        _outlineMaterial.SetColor(OutlineColorId, outlineColor);
        _outlineMaterial.SetFloat(OutlineWidthId, outlineWidth);
    }
    void OnValidate()
    {
        if (_outlineMaterial != null)
        {
            _outlineMaterial.SetColor(OutlineColorId, outlineColor);
            _outlineMaterial.SetFloat(OutlineWidthId, outlineWidth);
        }
    }

    void OnDestroy()
    {
        // 保险起见，如果 OnDisable 未正常执行也能退订
        GameBootstrap.Instance.Session.Typer.OrangeOutline -= ApplyOutline;

        // 清理动态创建的材质实例，防止内存泄漏
        if (_outlineMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(_outlineMaterial);
            else
                DestroyImmediate(_outlineMaterial);
        }
    }
}