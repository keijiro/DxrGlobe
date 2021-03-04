using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

namespace DxrGlobe {

[ExecuteInEditMode]
public sealed partial class AtomController :
  MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable attributes

    [SerializeField] Config _config;
    [SerializeField] int _instanceCount = 10;
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
        var e = Enumerable.Range(0, _instanceCount).
                Select(_ => Utils.CreateMeshRendererGameObject
                              (_mesh, _material, transform));
        _taa = new TransformAccessArray(e.Select(go => go.transform).ToArray());
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
        if (Prepare()) new UpdateJob(_config, _time).Schedule(_taa).Complete();
    }

    void OnDisable()
      => Cleanup();

    #endregion
}

} // namespace DxrGlobe
