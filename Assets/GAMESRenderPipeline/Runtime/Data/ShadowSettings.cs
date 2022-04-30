using UnityEngine;

[System.Serializable]
public class ShadowSettings 
{
    public enum ShadowType
    {
        SM,
        CSM,
    }

    public enum ShadowResolution
    {
        _256x256 = 256,
        _512x512 = 512,
        _1024x1024 = 1024,
        _2048x2048 = 2048,
        _4096x4096 = 4096
    }

    public ShadowType shadowType;
    public ShadowResolution shadowResolution = ShadowResolution._1024x1024;
    [Min(0f)] public float maxDistance = 100f;

}
