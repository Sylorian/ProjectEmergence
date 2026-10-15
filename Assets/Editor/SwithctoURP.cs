#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SwithctoURP
{
    private const string MatPath = "Assets/SpriteLitDefault.mat";

    [MenuItem("Tools/2D URP/替换选中对象为 Sprite-Lit-Default")]
    public static void ReplaceSelected()
    {
        var mat = GetOrCreateLitMaterial();
        if (mat == null) return;
        foreach (var go in Selection.gameObjects)
            ReplaceInHierarchy(go, mat);
        AssetDatabase.SaveAssets();
        Debug.Log("选中对象的 SpriteRenderer 材质已替换。");
    }

    [MenuItem("Tools/2D URP/替换场景中所有 SpriteRenderer")]
    public static void ReplaceAllInScene()
    {
        var mat = GetOrCreateLitMaterial();
        if (mat == null) return;

        var all = Object.FindObjectsOfType<SpriteRenderer>(true); // 包括隐藏对象
        foreach (var sr in all)
        {
            Undo.RecordObject(sr, "Replace Sprite Material");
            sr.sharedMaterial = mat;
            EditorUtility.SetDirty(sr);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"场景中 {all.Length} 个 SpriteRenderer 的材质已替换。");
    }

    private static void ReplaceInHierarchy(GameObject root, Material mat)
    {
        var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            Undo.RecordObject(sr, "Replace Sprite Material");
            sr.sharedMaterial = mat;
            EditorUtility.SetDirty(sr);
        }
    }

    private static Material GetOrCreateLitMaterial()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        if (mat != null) return mat;

        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        if (shader == null)
        {
            Debug.LogError("找不到 Sprite-Lit-Default Shader。请确认 URP 已安装，并且 URP Asset 使用 2D Renderer。");
            return null;
        }

        mat = new Material(shader);
        AssetDatabase.CreateAsset(mat, MatPath);
        AssetDatabase.SaveAssets();
        return mat;
    }
}
#endif