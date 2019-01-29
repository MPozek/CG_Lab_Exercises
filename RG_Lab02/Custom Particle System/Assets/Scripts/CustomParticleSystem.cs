using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomParticleSystem : MonoBehaviour
{
    private void OnValidate()
    {
        _period = _frequency <= 0f ? float.PositiveInfinity : 1f / _frequency;
    }
    private float _period;


    [Header("Rendering")]
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private bool _doBillboard = true;

    [Header("Base")]
    [SerializeField] private Vector2 _startLifetime;
    [SerializeField] private Gradient _startColor;
    [SerializeField] private Vector2 _startSize;
    [SerializeField] private Vector2 _startSpeed;

    [Header("Emission")]
    [SerializeField] private float _frequency;

    [SerializeField] private BaseEmissionShape _shape;

    [Header("Lifetime Variables")]
    [SerializeField] private AnimationCurve _speedOverLifetime = AnimationCurve.Linear(1, 1, 1, 1);
    [SerializeField] private AnimationCurve _sizeOverLifetime = AnimationCurve.Linear(1, 1, 1, 1);
    [SerializeField] private Gradient _colorOverLifetime;

    [Header("Forces")]
    [SerializeField] private Vector3 _gravity = Vector3.zero;
    [SerializeField] private bool _useNoise = false;
    [SerializeField] private NoiseSettings _noiseSettings = new NoiseSettings();

    #region INNER_VARIABLES

    private int _maxParticles;

    private CyclicArray<ParticleData> _particles;
    private Matrix4x4[] _matrices;
    private Vector4[] _colors;
    private MaterialPropertyBlock _propertyBlock;

    private Transform _transform;

    private AbstractNoise _noiseX, _noiseY, _noiseZ;

    #endregion

    #region STATE

    private float _startTime;

    private float _aggregatedDeltaTime = 0f;

    #endregion

    private void Awake()
    {
        _transform = transform;

        _maxParticles = Mathf.CeilToInt(_startLifetime.y * _frequency+50);

        _particles = new CyclicArray<ParticleData>(_maxParticles);
        _matrices = new Matrix4x4[_maxParticles];
        _colors = new Vector4[_maxParticles];
        _propertyBlock = new MaterialPropertyBlock();

        _material.enableInstancing = true;
        _startTime = Time.time;

        if (_useNoise)
        {
            _noiseX = AbstractNoise.GetNoise(_noiseSettings, 13);
            _noiseY = AbstractNoise.GetNoise(_noiseSettings, 28);
            _noiseZ = AbstractNoise.GetNoise(_noiseSettings, 43);
        }
    }

    private void Update()
    {
        var dt = Time.deltaTime;
        _aggregatedDeltaTime += dt;

        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i];
            p.Lifetime -= dt;
            p.Velocity += _gravity * dt;
            p.Position += p.Velocity * _speedOverLifetime.Evaluate(1f - p.LifetimeFactor) * dt;
            _particles[i] = p;
        }

        // clear old particles
        while (_particles.Count > 0f)
        {
            var front = _particles[0];
            if (front.Lifetime <= 0f)
                _particles.Pop();
            else
                break;
        }

        // emit new particles
        while (_aggregatedDeltaTime > _period)
        {
            _aggregatedDeltaTime -= _period;
            var par = _shape.Next();
            var lt = Random.Range(_startLifetime.x, _startLifetime.y);

            var pos = par.Position + _transform.position;

            _particles.Add(new ParticleData
            {
                MaxLifetime = lt,
                Lifetime = lt,
                Position = pos,
                Velocity = par.Normal * Random.Range(_startSpeed.x, _startSpeed.y),
                Size = Random.Range(_startSize.x, _startSize.y),
                Color = _startColor.Evaluate(Random.value)
            });
        }

        // fill the matrices / color array for the surviving particles
        var cameraToWorld = Camera.main.cameraToWorldMatrix;
        var rotation = _doBillboard ? Billboard(cameraToWorld) : Quaternion.identity;

        for (int i = 0; i < _particles.Count; i++)
        {
            var p = _particles[i];
            var pos = p.Position;
            if (_useNoise)
                pos += new Vector3(_noiseX.Evaluate(pos), _noiseY.Evaluate(pos), _noiseZ.Evaluate(pos));

            _matrices[i].SetTRS(pos, rotation, Vector3.one * p.Size * _sizeOverLifetime.Evaluate(1f - p.LifetimeFactor));
            _colors[i] = Vector4.Scale(p.Color, _colorOverLifetime.Evaluate(1f-p.Lifetime));
        }
        
        _propertyBlock.SetVectorArray("_Color", _colors);
        Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, _particles.Count, _propertyBlock,
            UnityEngine.Rendering.ShadowCastingMode.Off, false, 0, null, UnityEngine.Rendering.LightProbeUsage.Off);
    }

    private Quaternion Billboard(Matrix4x4 localToWorld)
    {
        Vector3 right = localToWorld.GetColumn(0) / localToWorld[3, 3];
        Vector3 up = localToWorld.GetColumn(1) / localToWorld[3, 3];
        right.Normalize();
        up.Normalize();
        Vector3 forward = Vector3.Cross(right, up);

        return Quaternion.LookRotation(forward, up);
    }
}
