using System.Linq;
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

sealed class MotionVectorLimitter
{
    (Transform xform, Renderer render, Vector3 prev) [] _objects;

    public MotionVectorLimitter(Transform root)
      => _objects =
           root.GetComponentsInChildren<Renderer>().
           Select(r => (r.GetComponent<Transform>(), r, Vector3.zero)).
           ToArray();

    public void CheckLimit(float threshold)
    {
        threshold *= threshold;

        for (var i = 0; i < _objects.Length; i++)
        {
            var pos = _objects[i].xform.position;

            _objects[i].render.motionVectorGenerationMode =
              (pos - _objects[i].prev).sqrMagnitude > threshold
                ? MotionVectorGenerationMode.ForceNoMotion :
                  MotionVectorGenerationMode.Object;

            _objects[i].prev = pos;
        }
    }
}

} // namespace DxrGlobe
