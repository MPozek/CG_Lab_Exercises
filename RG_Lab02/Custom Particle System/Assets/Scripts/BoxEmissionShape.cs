using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BoxEmissionShape : BaseEmissionShape
{
    [SerializeField] private Vector3 _size;

    public override void Reset()
    {
        
    }

    public override EmissionParameter Next()
    {
        return new EmissionParameter
        {
            Position = Vector3.Scale(_size, new Vector3(Random.value, Random.value, Random.value)) - _size * 0.5f,
            Normal = Random.onUnitSphere
        };
    }
}
