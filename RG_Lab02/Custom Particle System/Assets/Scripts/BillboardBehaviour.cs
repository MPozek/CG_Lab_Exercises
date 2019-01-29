using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _target;
    
    private void OnDrawGizmos()
    {
        if (_target == null)
            return;
        
        var localToWorld = _target.localToWorldMatrix;

        Vector3 right = localToWorld.GetColumn(0) / localToWorld[3,3];
        Vector3 up = localToWorld.GetColumn(1) / localToWorld[3,3];
        right.Normalize();
        up.Normalize();

        Vector3 forward = Vector3.Cross(right, up);

        //Vector3 forward = -_target.position + transform.position;
        //forward.Normalize();
        //Vector3 up = Vector3.Cross(forward, right);
        //right = Vector3.Cross(up, forward);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + 2 * right);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + 2 * up);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + 2 * forward);

        var dcm = new Matrix4x4(
            right,
            up,
            forward,
            new Vector4(0, 0, 0, 1)
        ).inverse;


        transform.rotation = dcm.rotation;
    }
}
