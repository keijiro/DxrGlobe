using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

namespace DxrGlobe {

[ExecuteInEditMode]
public sealed partial class LoopController :
  MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [SerializeField] Config _config;
    [SerializeField] int _instanceCount = 100;
    [SerializeField] Mesh[] _meshes;
    [SerializeField] Material _material;

    #endregion

    #region Private fields

    TransformAccessArray _taa;
    MotionVectorLimitter _limitter;
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
        if (_meshes == null || _meshes.Length == 0) return false;
        if (_material == null) return false;
        var e = Enumerable.Range(0, _instanceCount).
          Select(i => Utils.CreateMeshRendererGameObject
                        (_meshes[i % _meshes.Length], _material, transform));
        _taa = new TransformAccessArray(e.Select(go => go.transform).ToArray());
        _limitter = new MotionVectorLimitter(transform);
        return true;
    }

    void Cleanup()
    {
        if (_taa.isCreated)
        {
            Utils.DestroyAllGameObjects(_taa);
            _taa.Dispose();
        }
        _limitter = null;
        _wantsCleanup = false;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnValidate()
      => _wantsCleanup = true;

    void LateUpdate()
    {
        if (_wantsCleanup) Cleanup();
        if (Prepare()) new UpdateJob(_config, _time).Schedule(_taa).Complete();
        _limitter?.CheckLimit(_config.extent.z / 2);
    }

    void OnDisable()
      => Cleanup();

    #endregion
}

} // namespace DxrGlobe
