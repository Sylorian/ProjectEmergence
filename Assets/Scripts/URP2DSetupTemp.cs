#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 临时脚本：生成两个 URP 2D 灯光预制体，并校验 URP 管线资源。用完即删。
/// </summary>
public static class URP2DSetupTemp
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string PointLightPath = PrefabFolder + "/URP2D_PointLight.prefab";
    private const string GlobalLightPath = PrefabFolder + "/URP2D_GlobalLight.prefab";
    private const string RendererPath = "Assets/Settings/Renderer2D.asset";

    public static void Run()
    {
        int exitCode = 0;
        try
        {
            VerifySetup();

            CreateLightPrefab(PointLightPath, "URP2D_PointLight", Light2D.LightType.Point,
                new Color(1f, 0.85f, 0.6f, 1f), 1f, 5f, true);

            CreateLightPrefab(GlobalLightPath, "URP2D_GlobalLight", Light2D.LightType.Global,
                new Color(0.75f, 0.8f, 1f, 1f), 0.3f, 5f, false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("URP2D_SETUP_OK");
        }
        catch (Exception e)
        {
            Debug.LogError("URP2D_SETUP_FAILED: " + e);
            exitCode = 1;
        }

        EditorApplication.Exit(exitCode);
    }

    private static void VerifySetup()
    {
        RenderPipelineAsset pipeline = GraphicsSettings.defaultRenderPipeline;
        Debug.Log("URP2D_PIPELINE=" + (pipeline == null
            ? "NULL"
            : pipeline.GetType().FullName + " @" + AssetDatabase.GetAssetPath(pipeline)));

        ScriptableObject rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererPath);
        Debug.Log("URP2D_RENDERER=" + (rendererData == null ? "NULL" : rendererData.GetType().FullName));

        if (rendererData == null) return;

        // 校验 2D Renderer 的 shader 引用是否齐全，缺失会导致灯光不渲染
        var so = new SerializedObject(rendererData);
        string[] shaderProps =
        {
            "m_ShapeLightShader", "m_PointLightShader",
            "m_ShapeLightVolumeShader", "m_PointLightVolumeShader",
            "m_BlitShader", "m_CopyDepthShader", "m_SamplingShader"
        };

        foreach (string name in shaderProps)
        {
            SerializedProperty prop = so.FindProperty(name);
            if (prop == null)
            {
                Debug.Log("URP2D_SHADER " + name + " = <no property>");
                continue;
            }

            Debug.Log("URP2D_SHADER " + name + " = " +
                (prop.objectReferenceValue == null ? "NULL" : prop.objectReferenceValue.name));
        }
    }

    private static void CreateLightPrefab(string path, string name, Light2D.LightType type,
        Color color, float intensity, float radius, bool withFlicker)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("URP2D_SKIP_EXISTS " + path);
            return;
        }

        var go = new GameObject(name);
        try
        {
            var light = go.AddComponent<Light2D>();
            light.lightType = type;
            light.color = color;
            light.intensity = intensity;
            if (type == Light2D.LightType.Point) light.pointLightOuterRadius = radius;

            var controller = go.AddComponent<URP2DLightController>();
            SetProperty(controller, "lightColor", color);
            SetProperty(controller, "intensity", intensity);
            SetProperty(controller, "outerRadius", radius);
            SetProperty(controller, "isOn", true);
            SetProperty(controller, "applyOnStart", true);

            if (withFlicker)
            {
                var flicker = go.AddComponent<URP2DLightFlicker>();
                SetProperty(flicker, "flickerEnabled", false); // 默认关闭，需要时在面板上勾选
                SetProperty(flicker, "baseIntensity", intensity);
            }

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Debug.Log("URP2D_PREFAB_OK " + path);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    private static void SetProperty(UnityEngine.Object target, string propertyName, object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning("URP2D_PROP_MISSING " + target.GetType().Name + "." + propertyName);
            return;
        }

        switch (value)
        {
            case Color c: prop.colorValue = c; break;
            case float f: prop.floatValue = f; break;
            case bool b: prop.boolValue = b; break;
            default:
                Debug.LogWarning("URP2D_PROP_TYPE " + propertyName);
                return;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
