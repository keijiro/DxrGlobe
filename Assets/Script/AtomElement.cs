using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Klak.Math;

namespace DxrGlobe {
partial class AtomController {

[System.Serializable]
public struct Config
{
    public float frequency;
    public float radius;
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
        var hash = new XXHash(_config.seed);
        var seed = (uint)(index * 10);

        var n1 = hash.Float3((float3)100, seed);
        var n2 = _config.frequency * _time;

        var x = noise.snoise(math.float2(n1.x, n2));
        var y = noise.snoise(math.float2(n1.y, n2));
        var z = noise.snoise(math.float2(n1.z, n2));

        xform.localPosition = math.float3(x, y, z) * _config.radius;
        xform.localScale = (float3)_config.scale;
    }
}

} // partial class AtomController
} // namespace DxrGlobe
