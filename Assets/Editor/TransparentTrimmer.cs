#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TransparentTrimmer : EditorWindow
{
    int padding = 0;
    float alphaThreshold = 0f;
    Object inputFolder;
    Object outputFolder;

    [MenuItem("Tools/Transparent Trimmer")]
    static void Open() => GetWindow<TransparentTrimmer>();

    void OnGUI()
    {
        inputFolder = EditorGUILayout.ObjectField("输入文件夹", inputFolder, typeof(Object), false);
        outputFolder = EditorGUILayout.ObjectField("输出文件夹", outputFolder, typeof(Object), false);
        padding = EditorGUILayout.IntField("Padding", padding);
        alphaThreshold = EditorGUILayout.Slider("Alpha 阈值", alphaThreshold, 0f, 1f);

        if (GUILayout.Button("开始裁切"))
            Run();
    }

    void Run()
    {
        string inPath = AssetDatabase.GetAssetPath(inputFolder);
        string outPath = AssetDatabase.GetAssetPath(outputFolder);
        var files = Directory.GetFiles(inPath, "*.png", SearchOption.AllDirectories);

        foreach (var f in files)
        {
            byte[] bytes = File.ReadAllBytes(f);
            Texture2D src = new Texture2D(2, 2);
            src.LoadImage(bytes);   // 内存可读，不改导入设置

            Color[] pixels = src.GetPixels();
            int w = src.width, h = src.height;
            int minX = w, minY = h, maxX = -1, maxY = -1;
            byte aThresh = (byte)(alphaThreshold * 255);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a * 255 > aThresh)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < 0) { Debug.Log($"跳过全透明: {f}"); continue; }

            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(w - 1, maxX + padding);
            maxY = Mathf.Min(h - 1, maxY + padding);

            int cw = maxX - minX + 1;
            int ch = maxY - minY + 1;

            Color[] cropped = src.GetPixels(minX, minY, cw, ch);
            Texture2D dst = new Texture2D(cw, ch, TextureFormat.RGBA32, false);
            dst.SetPixels(cropped);
            dst.Apply();

            string name = Path.GetFileName(f);
            File.WriteAllBytes(Path.Combine(outPath, name), dst.EncodeToPNG());
            DestroyImmediate(src);
            DestroyImmediate(dst);
        }

        AssetDatabase.Refresh();
        Debug.Log($"裁切完成，处理 {files.Length} 张");
    }
}
#endif