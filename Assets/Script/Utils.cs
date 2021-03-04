using UnityEngine;
using UnityEngine.Jobs;

namespace DxrGlobe {

static class Utils
{
    public static GameObject CreateMeshRendererGameObject
      (Mesh mesh, Material material, Transform parent)
    {
        // We have to insert an empty game object to avoid an issue where
        // prevents game objects with HideFlags from getting ray-traced.
        var go1 = new GameObject("Element");
        var go2 = new GameObject
          ("Renderer", typeof(MeshFilter), typeof(MeshRenderer));

        go1.hideFlags = HideFlags.HideAndDontSave;
        go1.transform.parent = parent;
        go2.transform.parent = go1.transform;

        go2.GetComponent<MeshFilter>().sharedMesh = mesh;
        go2.GetComponent<MeshRenderer>().sharedMaterial = material;

        return go1;
    }

    public static void DestroyAllGameObjects(TransformAccessArray taa)
    {
        for (var i = 0; i < taa.length; i++)
        {
            var xform = taa[i];
            if (xform == null) continue;

            if (Application.isPlaying)
                Object.Destroy(xform.gameObject);
            else
                Object.DestroyImmediate(xform.gameObject);
        }
    }
}

} // namespace DxrGlobe
