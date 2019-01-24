using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CatmulCurve : MonoBehaviour
{
    //Use the transforms of GameObjects in 3d space as your points or define array with desired points
    [SerializeField] private List<Transform> _anchors = new List<Transform>();

    [System.Serializable]
    public struct CurvePoint
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Up;
        public Vector3 Right;
    }

    //Store points on the Catmull curve so we can visualize them
    public List<CurvePoint> Points = new List<CurvePoint>();
    
    [SerializeField] private int _pointsPerSegment = 10;

    private int _numberOfPoints => _pointsPerSegment * (_anchors.Count - 1) + 1;

    //set from 0-1
    [SerializeField] private float _alpha = 0.5f;

    public void RefreshPoints()
    {
        if (_anchors.Count < 2)
        {
            Debug.LogError("To construct a catmul, you need atleast 2 anchors!");
            return;
        }

        // resize
        if (Points.Count > _numberOfPoints)
            Points.Clear();

        if (Points.Count < _numberOfPoints)
        {
            for (int i = Points.Count; i < _numberOfPoints; i++)
            {
                Points.Add(new CurvePoint());
            }
        }

        int idx = 0;

        for (int i = -1; i < _anchors.Count - 2; i++)
        {
            var p0 = i < 0? 2 * _anchors[0].position - _anchors[1].position : _anchors[i].position;
            var p1 = _anchors[i+1].position;
            var p2 = _anchors[i+2].position;
            var p3 = i+3 >= _anchors.Count ? 2 * _anchors[_anchors.Count - 1].position - _anchors[_anchors.Count - 2].position : _anchors[i+3].position;

            float t0 = 0.0f;
            float t1 = GetNextKnot(t0, p0, p1);
            float t2 = GetNextKnot(t1, p1, p2);
            float t3 = GetNextKnot(t2, p2, p3);

            for (int j = 0; j < _pointsPerSegment; j++)
            {
                float t = Mathf.Lerp(t1, t2, j * 1f / _pointsPerSegment);

                var p = GetPointOnSegment(idx, t, t0, t1, t2, t3, p0, p1, p2, p3);

                p.Right = Vector3.Slerp(_anchors[i + 1].right, _anchors[i + 2].right, (t-t1) / (t2-t1)).normalized;
                p.Up = Vector3.Slerp(_anchors[i + 1].up, _anchors[i + 2].up, (t - t1) / (t2 - t1)).normalized;

                Points[idx] = p;
                

                idx++;
            }

            if (i == _anchors.Count - 3)
            {
                var p = GetPointOnSegment(idx, t2, t0, t1, t2, t3, p0, p1, p2, p3);

                p.Right = _anchors[_anchors.Count - 1].right;
                p.Up = _anchors[_anchors.Count - 1].up;

                Points[idx] = p;
            }
        }

        for (int i = 0; i < Points.Count-1; i++)
        {
            var p = Points[i];
            p.Direction = (Points[i+1].Position - p.Position).normalized;
            Points[i] = p;
        }
        var pl = Points[Points.Count - 1];
        pl.Direction = Points[Points.Count - 2].Direction;
        Points[Points.Count - 1] = pl;

    }

    private CurvePoint GetPointOnSegment(int idx, float t, float t0, float t1, float t2, float t3, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
        var A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
        var A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

        var B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
        var B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;

        var C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;

        return new CurvePoint { Position = C };
    }

    private float GetNextKnot(float previous, Vector3 p0, Vector3 p1)
    {
        float a = (p1-p0).magnitude; // Mathf.Pow((p1.x - p0.x), 2.0f) + Mathf.Pow((p1.y - p0.y), 2.0f) + Mathf.Pow((p1.z - p0.z), 2.0f)
        float c = Mathf.Pow(a, _alpha);
        
        return (c + previous);
    }

    //Visualize the points
    private void OnDrawGizmos()
    {
        if (Points.Count < _numberOfPoints)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < _numberOfPoints - 1; i++)
        {
            Gizmos.DrawLine(Points[i].Position, Points[i + 1].Position);
        }

        Gizmos.color = Color.green;

        for (int i = 0; i < _numberOfPoints; i++)
        {
            Gizmos.DrawLine(Points[i].Position, Points[i].Position + Points[i].Up);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CatmulCurve))]
    public class CatmulCurveEditor : Editor
    {
        private CatmulCurve _t;
        private bool _drawHandles;

        private void OnEnable()
        {
            _t = target as CatmulCurve;
        }

        private void OnSceneGUI()
        {
            if (_drawHandles)
            {
                foreach (var a in _t._anchors)
                {
                    a.position = Handles.PositionHandle(a.position, a.rotation);
                    a.rotation = Handles.RotationHandle(a.rotation, a.position);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            if (GUILayout.Button("Refresh"))
            {
                _t.RefreshPoints();
            }

            if (GUILayout.Button("Add point"))
            {
                var refTransform = _t._anchors.Count > 0 ? _t._anchors[_t._anchors.Count - 1] : _t.transform;

                var t = new GameObject("Anchor " + _t._anchors.Count).transform;
                t.SetParent(_t.transform);
                t.position = refTransform.position + refTransform.forward;
                t.rotation = refTransform.rotation;

                _t._anchors.Add(t);
            }

            if (GUILayout.Button("Align anchors to direction"))
            {
                for (int i = 0; i < _t._anchors.Count; i++)
                {
                    _t._anchors[i].forward = _t.Points[i * _t._pointsPerSegment].Direction;
                }
            }

            _drawHandles = EditorGUILayout.Toggle("Draw handles?", _drawHandles);
        }
    }
#endif
}