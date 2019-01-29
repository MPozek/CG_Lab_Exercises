using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SphereEmissionShape : BaseEmissionShape
{
    [SerializeField] private float _radius;
    [SerializeField] private float _innerRadius;

    public override void Reset()
    {
    }

    public override EmissionParameter Next()
    {
        var ep = new EmissionParameter();
        ep.Position = Random.insideUnitSphere * _radius;
        ep.Normal = ep.Position.normalized;

        ep.Position += ep.Normal * _innerRadius;

        return ep;
    }
}
