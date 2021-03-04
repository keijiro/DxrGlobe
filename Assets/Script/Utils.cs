using UnityEngine;
using UnityEngine.Jobs;

namespace DxrGlobe {

static class Utils
{
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
