using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DxrGlobe {

[ExecuteInEditMode]
sealed class FragmentGroup : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [SerializeField] FragmentConfig _config;
    [SerializeField] Mesh _mesh;
    [SerializeField] Material _material;

    #endregion

    #region Private fields

    TransformAccessArray _taa;
    float _time;
    bool _wantsCleanup;

    #endregion

    #region ITimeControl implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => _time = (float)time;

    #endregion

    #region IPropertyPreview implementation

    public void GatherProperties
      (PlayableDirector director, IPropertyCollector driver) {}

    #endregion

    #region Fragment object population/destruction


    bool Prepare()
    {
        if (_taa.isCreated) return true;
        if (_mesh == null || _material == null) return false;

        var icount = _config.TotalInstanceCount;
        var xforms = new Transform[icount];

        for (var i = 0u; i < icount; i++)
        {
            // We have to insert an empty game object to avoid an issue where
            // prevents game objects with HideFlags from getting ray-traced.
            var go1 = new GameObject("Fragment");
            var go2 = new GameObject("Renderer", typeof(MeshFilter), typeof(MeshRenderer));

            go1.hideFlags = HideFlags.HideAndDontSave;
            go1.transform.parent = transform;
            go2.transform.parent = go1.transform;

            go2.GetComponent<MeshFilter>().sharedMesh = _mesh;
            go2.GetComponent<MeshRenderer>().sharedMaterial = _material;

            xforms[i] = go1.transform;
        }

        _taa = new TransformAccessArray(xforms);
        return true;
    }

    void Cleanup()
    {
        if (_taa.isCreated)
        {
            Utils.DestroyAllGameObjects(_taa);
            _taa.Dispose();
        }
        _wantsCleanup = false;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnValidate()
      => _wantsCleanup = true;

    void LateUpdate()
    {
        if (_wantsCleanup) Cleanup();
        if (Prepare())
            new FragmentUpdateJob(_config, _time).Schedule(_taa).Complete();
    }

    void OnDisable()
      => Cleanup();

    #endregion
}

} // namespace DxrGlobe
