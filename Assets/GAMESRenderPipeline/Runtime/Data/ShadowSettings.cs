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

    public enum ShadowFilterMode
    {
        PCF3x3, 
        PCF5x5, 
        PCF7x7
    }

    public ShadowType shadowType;
    public ShadowResolution shadowResolution = ShadowResolution._1024x1024;
    public ShadowFilterMode shadowFilterMode = ShadowFilterMode.PCF5x5;

    [Min(0f)] public float maxDistance = 100f;

}
