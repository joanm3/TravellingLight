using UnityEngine;
using System.Collections;

public class ClicAndDrag : MonoBehaviour
{

    private float _sensitivity;
    private Vector3 _mouseReference;
    private Vector3 _mouseOffset;
    private Vector3 _position;
    private bool _isRotating;

    void Start()
    {
        _sensitivity = 0.005f;
        _position = Vector3.zero;
    }

    void Update()
    {
        if (_isRotating)
        {
            // offset
            _mouseOffset = (Input.mousePosition - _mouseReference);
            // apply rotation
            //_position.y = -(_mouseOffset.x + _mouseOffset.y) * _sensitivity;
            _position.y = (_mouseOffset.y) * _sensitivity;
            _position.x = (_mouseOffset.x) * _sensitivity;
            //_position.z = -((_mouseOffset.y + _mouseOffset.x) / 2) * _sensitivity;
            // rotate
            //transform.Rotate(_position);
            transform.position += _position;
            // store mouse
            _mouseReference = Input.mousePosition;
        }
    }

    void OnMouseDown()
    {
        // rotating flag
        _isRotating = true;

        // store mouse
        _mouseReference = Input.mousePosition;
    }

    void OnMouseUp()
    {
        // rotating flag
        _isRotating = false;
    }

}