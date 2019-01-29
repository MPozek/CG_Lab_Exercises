using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [SerializeField] private float _panSpeed = 1f;
    [SerializeField] private float _rotationSpeed = 10f;

    // Update is called once per frame
    private void LateUpdate()
    {
        float horiz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        float rotate = (Input.GetKey(KeyCode.E) ? 1f : 0f) + (Input.GetKey(KeyCode.Q) ? -1f : 0f);

        var delta = horiz * transform.right + transform.forward * vert;

        transform.position += Time.deltaTime * delta * _panSpeed;

        transform.eulerAngles = transform.eulerAngles + Vector3.up * Time.deltaTime * -rotate * _rotationSpeed;
    }
}
