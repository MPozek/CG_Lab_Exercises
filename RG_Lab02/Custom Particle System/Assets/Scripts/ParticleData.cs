using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ParticleData
{
    public float Lifetime;
    public float MaxLifetime;
    public float LifetimeFactor => Mathf.Clamp01(Lifetime / MaxLifetime);

    public Vector3 Position;
    public Vector3 Velocity;

    public float Size;
    public Color Color;
}
