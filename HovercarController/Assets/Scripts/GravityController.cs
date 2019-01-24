using UnityEngine;
using System.Collections.Generic;

public class GravityController : MonoBehaviour
{
    private readonly List<GravityNode> _activeNodes = new List<GravityNode>();

    public Vector3 GetDown()
    {
        if (_activeNodes.Count == 0)
            return Vector3.down;

        var sum = Vector3.zero;
        var pos = transform.position;

        for (int i = 0; i < _activeNodes.Count; i++)
        {
            sum += _activeNodes[i].GetDown(pos);
        }

        return sum.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        var gn = other.GetComponent<GravityNode>();

        if (gn != null)
            _activeNodes.Add(gn);
    }

    private void OnTriggerExit(Collider other)
    {
        var gn = other.GetComponent<GravityNode>();

        if (gn != null)
            _activeNodes.Remove(gn);
    }
}