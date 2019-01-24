using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GravityNode : MonoBehaviour
{
    [SerializeField] private Collider _collider;

    public Vector3 GetDown(Vector3 position)
    {
        var d = (_collider.ClosestPoint(position) - position);
        return d / d.sqrMagnitude;
    }
}