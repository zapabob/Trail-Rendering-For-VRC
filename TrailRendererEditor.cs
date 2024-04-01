// TrailRendererEditor.cs
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrailRenderer))]
public class TrailRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TrailRenderer trailRenderer = (TrailRenderer)target;

        if (GUILayout.Button("Export VCC Package"))
        {
            ExportVCCPackage(trailRenderer);
        }
    }

    private void ExportVCCPackage(TrailRenderer trailRenderer)
    {
        string assetPath = AssetDatabase.GetAssetPath(trailRenderer);
        string folderPath = System.IO.Path.GetDirectoryName(assetPath);
        string prefabPath = System.IO.Path.Combine(folderPath, "TrailRenderer.prefab");

        PrefabUtility.SaveAsPrefabAsset(trailRenderer.gameObject, prefabPath);

        string vccFilePath = System.IO.Path.Combine(folderPath, "TrailRenderer.vcc");
        AssetDatabase.ExportPackage(prefabPath, vccFilePath, ExportPackageOptions.Default);

        AssetDatabase.Refresh();

        Debug.Log($"VCC package exported: {vccFilePath}");
    }
}
