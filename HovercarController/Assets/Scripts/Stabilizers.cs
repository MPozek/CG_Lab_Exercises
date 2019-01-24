using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabilizers : MonoBehaviour
{
    private Transform m_transform;
    private Transform _transform
    {
        get
        {
            if (m_transform == null)
                m_transform = transform;
            return m_transform;
        }
    }

    [SerializeField] private float _maxGroundDist = 5f;
    [SerializeField] private LayerMask _groundMask;

    [SerializeField] private List<Transform> _hoverPoints = new List<Transform>();
    
    private Rigidbody _rigidbody;
    private GravityController _gravityController;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityController = GetComponent<GravityController>();
    }

    public bool Raycast(out StabilizerInfo info)
    {
        info = new StabilizerInfo();

        float hits = 0f;
        var rbDown = _rigidbody.rotation * Vector3.down;
        var castDist = Mathf.Max(Time.deltaTime * Vector3.Dot(rbDown, _rigidbody.velocity),
            _maxGroundDist);

        for (int i = 0; i < _hoverPoints.Count; i++)
        {
            var pos = _hoverPoints[i].position;
            var rot = _hoverPoints[i].up * -1f;

            RaycastHit hit;
            Ray ray = new Ray(pos, rot);
            var didHit = Physics.Raycast(ray, out hit, castDist, _groundMask);

            if (didHit)
            {
                hits++;
                info.Distance += hit.distance;
                info.Normal += hit.normal;
            }
            else
            {
                info.Normal += -_gravityController.GetDown();
            }
        }

        if (hits > 0)
        {
            info.Distance /= hits;
        }

        info.Normal.Normalize();

        return hits > 0;
    }

    public struct StabilizerInfo
    {
        public Vector3 Normal;
        public float Distance;
    }
}
