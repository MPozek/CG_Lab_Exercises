using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EmissionParameter
{
    public Vector3 Position;
    public Vector3 Normal;
}

public abstract class BaseEmissionShape : ScriptableObject
{
    public abstract void Reset();

    public abstract EmissionParameter Next();
}