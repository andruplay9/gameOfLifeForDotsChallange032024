using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    private InputMap _map;
    private Camera _camera;
    
    void Start()
    {
        _map = new InputMap();
        _map.Enable();
        _camera = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_map.Basic.Move.inProgress)
        {
            this.transform.Translate(_map.Basic.Move.ReadValue<Vector2>()*8 * Time.deltaTime);
        }
        if (_map.Basic.Zoom.inProgress)
        {
            _camera.orthographicSize+= _map.Basic.Zoom.ReadValue<float>() * -Time.deltaTime;
        }
    }
}
