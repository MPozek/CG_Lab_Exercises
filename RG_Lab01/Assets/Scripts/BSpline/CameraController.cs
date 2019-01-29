using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Vector2 _rotationSpeed;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void LateUpdate()
    {
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        input = Vector2.Scale(_rotationSpeed, input);

        var rot = _transform.rotation.eulerAngles;
        rot.x += input.y * Time.deltaTime;
        rot.y += input.x * Time.deltaTime;
        _transform.rotation = Quaternion.Euler(rot);
    }
}
