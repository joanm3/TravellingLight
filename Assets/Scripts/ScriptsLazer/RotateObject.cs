using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour {

    public float speed;

    void OnMouseOver()
    {
        transform.localEulerAngles += new Vector3 (0,0,Input.GetAxis("Mouse ScrollWheel") * speed);
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.localEulerAngles += new Vector3(0, 0, -1) * Time.deltaTime * speed;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q))
        {
            transform.localEulerAngles += new Vector3(0, 0, 1) * Time.deltaTime * speed;
        }
    }

}
