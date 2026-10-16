using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal; // 需要 URP 包

public class BatchSetSortingLayer : EditorWindow
{
    private string layerName = "Environment";

    [MenuItem("Tools/批量设置 Sorting Layer")]
    public static void ShowWindow()
    {
        GetWindow<BatchSetSortingLayer>("批量设置 Sorting Layer");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量设置 Sorting Layer", EditorStyles.boldLabel);
        layerName = EditorGUILayout.TextField("Sorting Layer 名称", layerName);

        EditorGUILayout.Space();

        if (GUILayout.Button("应用到选中的 SpriteRenderer"))
        {
            ApplyToSpriteRenderers();
        }

        if (GUILayout.Button("应用到选中的 Light2D 的 Target Sorting Layers"))
        {
            ApplyToLight2D();
        }

        EditorGUILayout.HelpBox(
            "注意：Sorting Layer 必须先在 Edit > Project Settings > Tags and Layers 中创建好。\n" +
            "如果 Light2D 的下拉列表仍不刷新，可以尝试重新打开窗口或重启 Unity。",
            MessageType.Info);
    }

    private bool IsSortingLayerExist(string name)
    {
        foreach (var layer in SortingLayer.layers)
        {
            if (layer.name == name) return true;
        }
        return false;
    }

    private void ApplyToSpriteRenderers()
    {
        var selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("请先在 Hierarchy 中选中至少一个对象。");
            return;
        }

        if (!IsSortingLayerExist(layerName))
        {
            Debug.LogWarning($"Sorting Layer '{layerName}' 不存在，请先在 Tags and Layers 中创建。");
            return;
        }

        int count = 0;
        foreach (var go in selected)
        {
            var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in renderers)
            {
                Undo.RecordObject(sr, "Batch Set Sorting Layer");
                sr.sortingLayerName = layerName;
                EditorUtility.SetDirty(sr);
                count++;
            }
        }
        Debug.Log($"已修改 {count} 个 SpriteRenderer 的 Sorting Layer 为 {layerName}");
    }

    private void ApplyToLight2D()
    {
        var selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("请先在 Hierarchy 中选中至少一个对象。");
            return;
        }

        if (!IsSortingLayerExist(layerName))
        {
            Debug.LogWarning($"Sorting Layer '{layerName}' 不存在，请先在 Tags and Layers 中创建。");
            return;
        }

        int layerId = SortingLayer.NameToID(layerName);
        int count = 0;

        foreach (var go in selected)
        {
            var lights = go.GetComponentsInChildren<Light2D>(true);
            foreach (var light in lights)
            {
                Undo.RecordObject(light, "Batch Set Light2D Target Sorting Layers");

                // 通过 SerializedObject 修改 Light2D 的私有序列化字段 m_ApplyToSortingLayers
                var so = new SerializedObject(light);
                var prop = so.FindProperty("m_ApplyToSortingLayers");
                if (prop != null)
                {
                    prop.arraySize = 1;
                    prop.GetArrayElementAtIndex(0).intValue = layerId;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(light);
                    count++;
                }
                else
                {
                    Debug.LogWarning($"无法找到 Light2D 的 Target Sorting Layers 字段，请检查 URP 版本。");
                }
            }
        }
        Debug.Log($"已修改 {count} 个 Light2D 的 Target Sorting Layers 为 {layerName}");
    }
}