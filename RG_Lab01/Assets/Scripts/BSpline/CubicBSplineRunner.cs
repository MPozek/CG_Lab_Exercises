using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CubicBSplineRunner : MonoBehaviour
{
    public enum RotationMode
    {
        AngleAxis, DCM
    }

    public bool Loop = false;
    public bool ReadFromTransformAnchors = true;

    public bool ReadObjFile = false;
    public string OBJ_FILE_PATH = "";

    public Transform[] Anchors = new Transform[0];


    [SerializeField] private RotationMode _rotationMode;
    private CubicBSpline _spline;

    [Header("Spline follow")]
    [SerializeField] private Transform _followingTransform;
    [SerializeField] private float _followTime = 5f;
    [SerializeField] private Vector3[] _anchors;

    [Header("Gizmos")]
    [SerializeField] private bool GIZMOS_doDraw = true;

    [SerializeField] private float TANGENT_scale = 0.1f;
    [SerializeField] private float GIZMOS_step = 0.05f;
    [SerializeField] private Color GIZMOS_controlPointColor = Color.white;
    [SerializeField] private Color GIZMOS_curveColor = Color.white;
    [SerializeField] private Color GIZMOS_tangentColor = Color.cyan;
    [SerializeField] private Color GIZMOS_2ndDColor = Color.Lerp(Color.red, Color.blue, 0.5f);

    // STATE
    private float _startFollowTime;

    private Mesh ReadSimpleOBJ(string filePath)
    {
        Mesh m = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();

        using (StreamReader fs = File.OpenText(filePath))
        {
            while (fs.EndOfStream == false)
            {
                var line = fs.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    continue;
                if (line.StartsWith("#"))
                    continue;

                var split = line.Split(' ');

                if (line[0] == 'v')
                {
                    Vector3 p;
                    p.x = float.Parse(split[1]);
                    p.y = float.Parse(split[2]);
                    p.z = float.Parse(split[3]);

                    vertices.Add(p);

                } else if (line[0] == 'f')
                {
                    tris.Add(int.Parse(split[1])-1);
                    tris.Add(int.Parse(split[2]) - 1);
                    tris.Add(int.Parse(split[3]) - 1);
                }
            }
        }

        m.SetVertices(vertices);
        m.SetTriangles(tris, 0);
        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();

        return m;
    }
    
    private void RefreshSpline()
    {
        if (ReadObjFile && Application.isPlaying && string.IsNullOrWhiteSpace(OBJ_FILE_PATH) == false)
        {
            _followingTransform.GetComponentInChildren<MeshFilter>().mesh = ReadSimpleOBJ(OBJ_FILE_PATH);
        }

        if (ReadFromTransformAnchors)
        {
            _anchors = new Vector3[Loop ? Anchors.Length + 3 : Anchors.Length];

            for (int i = 0; i < _anchors.Length; i++)
                _anchors[i] = Anchors[i % Anchors.Length].transform.position;
        }
        else
        {
            string fileName = @"spline.txt";
            
            using (StreamReader fs = File.OpenText(fileName))
            {
                List<Vector3> points = new List<Vector3>();
                Vector3 p;
                string s;
                float x = 0f, y = 0f, z = 0f;
                while (fs.EndOfStream == false) {
                    s = fs.ReadLine();
                    var split = s.Split(' ');

                    p.x = float.Parse(split[0]);
                    p.y = float.Parse(split[1]);
                    p.z = float.Parse(split[2]);
                    points.Add(p);

                    x += p.x;
                    y += p.y;
                    z += p.z;
                }

                _anchors = points.ToArray();

                // center the transform
                transform.position = -Vector3.right * x / points.Count - Vector3.up * y / points.Count - Vector3.forward * z / points.Count + Vector3.forward;
            }

        }

        _spline = new CubicBSpline(_anchors);
    }

    private void LineRender()
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(GIZMOS_curveColor);
        Vector3 p0 = transform.position + _spline.Evaluate(0f);
        for (float t = 0f; t <= 1f; t += GIZMOS_step)
        {
            var p = transform.position + _spline.Evaluate(t);

            GL.Vertex(p0);
            p0 = p;
        }
        GL.Vertex(p0);

        GL.End();
        
        GL.Begin(GL.LINES);

        GL.Color(GIZMOS_tangentColor);
        for (float t = 0f; t <= 1f; t += GIZMOS_step)
        {
            var p = transform.position + _spline.Evaluate(t);
            var tg = _spline.EvaluateTangent(t);
            
            GL.Vertex(p);
            GL.Vertex(p + tg * TANGENT_scale);
        }
        GL.Vertex(p0);

        GL.End();
    }

    private void ResetState()
    {
        _startFollowTime = Time.time;
        RefreshSpline();
    }

    private void Start()
    {
        ResetState();

        FindObjectOfType<GLDrawTest>().ToDraw = LineRender;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ResetState();
        }

        var t = (Time.time - _startFollowTime) / _followTime;

        if (Loop)
        {
            t = t - Mathf.Floor(t);
        }

        var p = transform.position + _spline.Evaluate(t);
        var tg = _spline.EvaluateTangent(t);

        _followingTransform.position = p;

        // rotation
        var from = Vector3.forward; // (0, 0, 1)
        var to = tg.normalized;

        if (_rotationMode == RotationMode.AngleAxis)
        {
            Debug.DrawRay(_followingTransform.position, to, Color.green);

            var axis = Vector3.Cross(from, to);

            var cosAngle = Vector3.Dot(from, to); // already normalized so no division here
                                                  // the sign is already stored in the axis
            var angle = Mathf.Acos(cosAngle) * Mathf.Rad2Deg;

            _followingTransform.rotation = Quaternion.AngleAxis(angle, axis);
        }
        else if (_rotationMode == RotationMode.DCM)
        {
            Vector3 w, u, v;
            v = to;
            u = Vector3.Cross(v, _spline.EvaluateSecondDerivative(t).normalized);
            w = Vector3.Cross(v, u);

            var dcm = new Matrix4x4(
                w,
                u, 
                v,
                new Vector4(1, 0, 0, 0)
            ).inverse;

            _followingTransform.rotation = dcm.rotation;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!GIZMOS_doDraw)
            return;

        RefreshSpline();

        Gizmos.color = GIZMOS_controlPointColor;
        foreach (var a in _anchors)
        {
            Gizmos.DrawWireSphere(transform.position + a, 0.1f);
        }

        for (int i = 1; i < _anchors.Length; i++)
        {
            Gizmos.DrawLine(transform.position + _anchors[i - 1], transform.position +_anchors[i]);
        }

        Vector3 p0 = transform.position + _spline.Evaluate(0f);
        for (float t = 0f; t <= 1f; t += GIZMOS_step)
        {
            var p = transform.position + _spline.Evaluate(t);

            var tg = _spline.EvaluateTangent(t);

            Gizmos.color = GIZMOS_curveColor;
            Gizmos.DrawLine(p0, p);
            Gizmos.color = GIZMOS_tangentColor;
            Gizmos.DrawLine(p, p + tg * TANGENT_scale);

            Gizmos.color = GIZMOS_2ndDColor;
            Gizmos.DrawLine(p, p + _spline.EvaluateSecondDerivative(t).normalized * TANGENT_scale);

            p0 = p;
        }
    }

}
