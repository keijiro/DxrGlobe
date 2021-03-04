using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrGlobe {
partial class LoopController {

[System.Serializable]
public struct Config
{
    public float frequency;
    public float spin;
    [Range(0, 1)] public float distribution;
    public float3 extent;
    public float scale;
    public uint seed;
}

[BurstCompile]
struct UpdateJob : IJobParallelForTransform
{
    Config _config;
    float _time;

    public UpdateJob(in Config config, float time)
    {
        _config = config;
        _time = time;
    }

    public void Execute(int index, TransformAccess xform)
    {
        // System shared hash
        var hash0 = new XXHash(_config.seed);

        // Per-element hash
        var hash = new XXHash(hash0.UInt((uint)index));

        // Total accumulated time parameter
        var param = _time * _config.frequency;
        param *= hash.Float(1 - _config.distribution, 1, 1);

        // Base rotation
        var rot0 = _config.spin > 0 ? hash.Rotation(2) : quaternion.identity;

        // Spin
        var axis = hash.Direction(3);
        var spin = quaternion.AxisAngle(axis, _config.spin * param);

        // Per-stride random vector
        var rand = hash.Float2((uint)param + 4u);

        // Position
        var pos = math.float3(rand.xy, math.frac(param));
        pos = (pos - 0.5f) * _config.extent;

        // Transform output
        xform.localPosition = pos;
        xform.localRotation = math.mul(rot0, spin);
        xform.localScale = (float3)_config.scale;
    }
}

} // partial class LoopController
} // namespace DxrGlobe
