using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrGlobe {

[System.Serializable]
public struct FragmentConfig
{
    public int2 repeats;
    public float2 dimensions;

    [Space, Range(0.0001f, 1)]
    public float curvature;
    public float scale;

    [Space]
    public float noiseFrequency;
    public float noiseSpeed;
    public float noiseAmount;

    [Space]
    public float waveFrequency;
    public float waveSpeed;
    public float waveAmount;

    public int TotalInstanceCount
      => math.max(0, repeats.x * repeats.y);
}

[BurstCompile]
struct FragmentUpdateJob : IJobParallelForTransform
{
    FragmentConfig _config;
    float _time;

    public FragmentUpdateJob(in FragmentConfig config, float time)
    {
        _config = config;
        _time = time;
    }

    public void Execute(int index, TransformAccess xform)
    {
        // Base position
        var col = _config.repeats.x;
        var p0 = math.float2(index % col, index / col);
        p0 -= (float2)(_config.repeats - 1) * 0.5f;
        p0 *= _config.dimensions.xy / _config.repeats;

        // Depth for curvature
        var d = math.length(_config.dimensions) / _config.curvature;

        // Curved position and rotation
        var pc = math.normalize(math.float3(p0, d)) * d;
        var r0 = quaternion.LookRotation(-pc, math.float3(0, 1, 0));
        pc.z -= d;

        // Rotation by noise
        var np = _config.noiseFrequency * pc.xy;
        var n1 = noise.snoise(math.float3(np, _config.noiseSpeed * +_time));
        var n2 = noise.snoise(math.float3(np, _config.noiseSpeed * -_time));
        var euler = math.float3(n1, n2, 0) * _config.noiseAmount;

        // Scale animation
        var wp = math.length(pc) * _config.waveFrequency;
        wp -= _time * _config.waveSpeed;
        var sc = _config.scale + (math.sin(wp) - 1) / 2 * _config.waveAmount;

        // Transform output
        xform.localPosition = pc;
        xform.localRotation = math.mul(r0, quaternion.EulerXYZ(euler));
        xform.localScale = (float3)sc;
    }
}

} // namespace DxrGlobe
