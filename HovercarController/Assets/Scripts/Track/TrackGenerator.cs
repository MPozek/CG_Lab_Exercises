using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TrackGenerator : MonoBehaviour
{
    private MeshFilter m_filter;
    private MeshRenderer m_renderer;

    private MeshFilter _filter
    {
        get
        {
            if (m_filter == null)
                m_filter = GetComponent<MeshFilter>();
            return m_filter;
        }
    }

    private MeshRenderer _renderer
    {
        get
        {
            if (m_renderer == null)
                m_renderer = GetComponent<MeshRenderer>();
            return m_renderer;
        }
    }

    [SerializeField] private CatmulCurve _curveReference;

    [SerializeField] private float _width = 1f;

    [SerializeField] private float _crossSectionYScale = 1f;
    [SerializeField] private int _trackCrossSectionResolution;
    [SerializeField] private AnimationCurve _trackCrossSectionCurve;

    private Vector2[] _rightOffsets = new[]
    {
        Vector2.right * 0.5f, Vector2.right * 0f, Vector2.right * -0.5f
    };

    private void GenerateRightOffsets()
    {
        _rightOffsets = new Vector2[_trackCrossSectionResolution];

        for (int i = 0; i < _rightOffsets.Length; i++)
        {
            float t = -i * 2f / (_rightOffsets.Length - 1) + 1f;

            _rightOffsets[i] = new Vector2(_width * t, _trackCrossSectionCurve.Evaluate(t) * _crossSectionYScale);
        }
    }

    private void AddPiece(List<Vector3> vertices, List<int> triangles, List<Vector2> uv, Vector3 position, Vector3 right, Vector3 up, float length)
    {
        bool hasSegments = vertices.Count > 0;
        for (int i = 0; i < _rightOffsets.Length; i++)
        {
            vertices.Add(position + right * _rightOffsets[i].x + up * _rightOffsets[i].y);
            uv.Add(new Vector2(_rightOffsets[i].y, length));

            if (i > 0 && hasSegments)
            {
                int end = vertices.Count - 1;
                triangles.Add(end);
                triangles.Add(end - 1);
                triangles.Add(end - _rightOffsets.Length);

                triangles.Add(end - 1);
                triangles.Add(end - 1 - _rightOffsets.Length);
                triangles.Add(end - _rightOffsets.Length);
            }
        }
    }

    private void Generate()
    {
        var mc = GetComponent<MeshCollider>();
        if (mc != null)
            DestroyImmediate(mc);

        GenerateRightOffsets();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        float len = 0f;

        var p0 = _curveReference.Points[0];

        AddPiece(vertices, triangles, uv, p0.Position, p0.Right, p0.Up, len);
        // vertices.Add(p0.Position + p0.Right * _width * 0.5f);
        // vertices.Add(p0.Position - p0.Right * _width * 0.5f);

        for (int i = 1; i < _curveReference.Points.Count; i++)
        {
            var p = _curveReference.Points[i];

            len += (p.Position - _curveReference.Points[i-1].Position).magnitude;

            AddPiece(vertices, triangles, uv, p.Position, p.Right, p.Up, len);

            /*
            vertices.Add(p.Position + p.Right * _width * 0.5f);
            vertices.Add(p.Position - p.Right * _width * 0.5f);

            // construct the triangles
            triangles.Add(vertices.Count - 1);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 3);

            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 4);
            triangles.Add(vertices.Count - 3);
            */
        }

        var m = new Mesh();

        m.SetVertices(vertices);
        m.SetTriangles(triangles, 0);
        m.SetUVs(0, uv);

        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();

        _filter.mesh = m;

        gameObject.AddComponent<MeshCollider>();
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(TrackGenerator))]
    public class TrackGeneratorEditor : Editor
    {
        private TrackGenerator _t;

        private void OnEnable()
        {
            _t = target as TrackGenerator;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                _t.Generate();
            }
        }
    }
#endif

}
