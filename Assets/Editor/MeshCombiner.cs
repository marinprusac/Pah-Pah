using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshCombiner : MonoBehaviour
{
    [ContextMenu("Combine Meshes")]
    void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh == null) continue;

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        MeshFilter parentMeshFilter = gameObject.GetComponent<MeshFilter>();
        if (parentMeshFilter == null)
            parentMeshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer parentMeshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (parentMeshRenderer == null)
            parentMeshRenderer = gameObject.AddComponent<MeshRenderer>();

        parentMeshFilter.sharedMesh = combinedMesh;
        parentMeshRenderer.sharedMaterial = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        Debug.Log("Meshes combined. Use 'Save Mesh' to save it.");
    }

    [ContextMenu("Save Mesh")]
    void SaveMesh()
    {
        MeshFilter parentMeshFilter = gameObject.GetComponent<MeshFilter>();
        if (parentMeshFilter == null || parentMeshFilter.sharedMesh == null)
        {
            Debug.LogWarning("No combined mesh found. Please combine meshes first.");
            return;
        }

        SaveMeshAsAsset(parentMeshFilter.sharedMesh, "CombinedMesh");
    }

    void SaveMeshAsAsset(Mesh mesh, string name)
    {
        string path = "Assets/" + name + ".asset";

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Mesh saved to {path}");
    }
}