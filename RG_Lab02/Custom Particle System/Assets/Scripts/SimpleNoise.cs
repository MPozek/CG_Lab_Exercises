using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface INoiseSampler
{
    float Evaluate(Vector3 position);
}

public enum NoiseType { Simple, Ridge }

[System.Serializable]
public class NoiseSettings
{
    public bool Enabled = true;
    public int MaskLayer = -1;

    public NoiseType Type;
    public float Strength;
    public Vector3 Offset;

    public float Roughness = 2f;

    public int NumLayers = 1;
    public float BaseRoughness = 1f;
    public float Persistence = .5f;

    public bool RestrictAmplitude = false;
    public Vector2 AmplitudeRestriction = Vector2.zero;
}

public abstract class AbstractNoise : INoiseSampler
{
    protected NoiseSettings _settings;
    protected Noise _noise;

    protected AbstractNoise(NoiseSettings settings, int seed = -1)
    {
        if (seed == -1)
            seed = Time.renderedFrameCount;
        _noise = new Noise(seed);
        _settings = settings;
    }

    public abstract float Evaluate(Vector3 position);

    public static AbstractNoise GetNoise(NoiseSettings settings, int seed = -1)
    {
        switch (settings.Type)
        {
            case NoiseType.Simple:
                return new SimpleNoise(settings, seed);
            case NoiseType.Ridge:
                return new RidgeNoise(settings, seed);
                // TODO ridge
        }
        return null;
    }
}

public class SimpleNoise : AbstractNoise
{
    public SimpleNoise(NoiseSettings settings, int seed = -1) : base(settings, seed) { }

    public override float Evaluate(Vector3 position)
    {
        if (!_settings.Enabled)
            return 0f;

        float noiseValue = 0f; // _noise.Evaluate(position * _settings.Roughness + _settings.Offset);
        float frequuency = _settings.BaseRoughness;
        float amplitude = 1f;

        for (int i = 0; i < _settings.NumLayers; i++)
        {
            float v = _noise.Evaluate(position * frequuency + _settings.Offset);
            noiseValue += v * amplitude;
            frequuency *= _settings.Roughness;
            amplitude *= _settings.Persistence;
        }

        if (_settings.RestrictAmplitude)
        {
            noiseValue = Mathf.Clamp(noiseValue, _settings.AmplitudeRestriction.x, _settings.AmplitudeRestriction.y);
        }

        return noiseValue * _settings.Strength;
    }
}

public class RidgeNoise : AbstractNoise
{
    public RidgeNoise(NoiseSettings settings, int seed = -1) : base(settings, seed) { }

    public override float Evaluate(Vector3 position)
    {
        if (!_settings.Enabled)
            return 0f;

        float noiseValue = 0f; // _noise.Evaluate(position * _settings.Roughness + _settings.Offset);
        float frequuency = _settings.BaseRoughness;
        float amplitude = 1f;

        for (int i = 0; i < _settings.NumLayers; i++)
        {
            float v = _noise.Evaluate(position * frequuency + _settings.Offset);
            noiseValue += v * amplitude;
            frequuency *= _settings.Roughness;
            amplitude *= _settings.Persistence;
        }

        noiseValue = Mathf.Sign(noiseValue) * Mathf.Pow(noiseValue, 4f);

        if (_settings.RestrictAmplitude)
        {
            noiseValue = Mathf.Clamp(noiseValue, _settings.AmplitudeRestriction.x, _settings.AmplitudeRestriction.y);
        }

        return noiseValue * _settings.Strength;
    }
}
