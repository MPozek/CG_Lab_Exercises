using UnityEngine;

[System.Serializable]
public struct CubicBSpline
{
    private static readonly Matrix4x4 BASE_MATRIX = new Matrix4x4(
        new Vector4(-1, 3, -3, 1),
        new Vector4(3, -6, 0, 4),
        new Vector4(-3, 3, 3, 1),
        new Vector4(1, 0, 0, 0)
        );

    private const float BASE_MATRIX_FACTOR = 1f / 6f;

    [SerializeField] private Vector4[] _anchors;

    private int _segmentCount => _anchors.Length - 3;

    public CubicBSpline(params Vector3[] anchors)
    {
        // TODO:: add assert message
        Debug.Assert(anchors.Length >= 4); // have to have atleast 4 points
        
        // copy the array
        _anchors = new Vector4[anchors.Length];
        for (int i = 0; i < _anchors.Length; i++)
        {
            _anchors[i] = new Vector4(0f,0f,0f,1f) + (Vector4)anchors[i];
        }
    }

    public CubicBSpline(params Vector4[] anchors)
    {
        // TODO:: add assert message
        Debug.Assert(anchors.Length >= 4); // have to have atleast 4 points

        // copy the array
        _anchors = new Vector4[anchors.Length];
        for (int i = 0; i < _anchors.Length; i++)
        {
            _anchors[i] = anchors[i];
        }
    }

    public CubicBSpline(CubicBSpline toCopy) : this(toCopy._anchors) { }
    
    public Vector3 Evaluate(float t)
    {
        var segment = SegmentizeParameter(ref t);

        var tCubic = new Vector4(t * t * t, t * t, t, 1f);

        var p = EvaluateInternal(tCubic, segment);

        return p / p.w;
    }

    public Vector3 EvaluateTangent(float t)
    {
        var segment = SegmentizeParameter(ref t);

        var tCubic = new Vector4(3f * t * t, 2f * t, 1f, 0f);

        var p = EvaluateInternal(tCubic, segment);

        return p;
    }

    public Vector3 EvaluateSecondDerivative(float t)
    {
        var segment = SegmentizeParameter(ref t);

        var tCubic = new Vector4(6f * t, 2f, 0f, 0f);

        var p = EvaluateInternal(tCubic, segment);

        return p;
    }

    private int SegmentizeParameter(ref float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        t *= _segmentCount;

        var segment = Mathf.Clamp(Mathf.FloorToInt(t), 0, _segmentCount - 1); // clamp in case t = 1 -> 5
        t -= segment; // now t is between 0 and 1 in the given segment

        return segment;
    }

    private Vector4 EvaluateInternal(Vector4 tCubic, int segment)
    {
        var segmentMatrix = BASE_MATRIX *
                            new Matrix4x4(
                                _anchors[segment],
                                _anchors[segment + 1],
                                _anchors[segment + 2],
                                _anchors[segment + 3]
                            ).transpose;

        var p = (tCubic * BASE_MATRIX_FACTOR).Multiply(segmentMatrix);
        return p;
    }
}

public static class Matrix4x4Extensions
{
    public static Vector4 Multiply(this Matrix4x4 m, Vector4 v)
    {
        return new Vector4
        {
            x = Vector4.Dot(m.GetRow(0), v),
            y = Vector4.Dot(m.GetRow(1), v),
            z = Vector4.Dot(m.GetRow(2), v),
            w = Vector4.Dot(m.GetRow(3), v)
        };
    }

    public static Vector4 Multiply(this Vector4 v, Matrix4x4 m)
    {
        return new Vector4
        {
            x = Vector4.Dot(m.GetColumn(0), v),
            y = Vector4.Dot(m.GetColumn(1), v),
            z = Vector4.Dot(m.GetColumn(2), v),
            w = Vector4.Dot(m.GetColumn(3), v)
        };
    }
}
