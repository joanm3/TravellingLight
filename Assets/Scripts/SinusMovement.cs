using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectLight.Types;

public class SinusMovement : MonoBehaviour
{

    public Axis movementAxis = Axis.y;
    public bool move = true;
    public float velocity = 1f;
    public float intensity = 1f;

    private float actualPosition;
    private float startPosition;
    private float moveTime;

    void Start()
    {
        //this will kidnap the movement of the object. change it to a localscale 
        //if we need to move it, and add a parent.  
        switch (movementAxis)
        {
            case Axis.x:
                actualPosition = transform.localPosition.x;
                break;
            case Axis.y:
                actualPosition = transform.localPosition.y;
                break;
            case Axis.z:
                actualPosition = transform.localPosition.z;
                break;
        }

    }

    void Update()
    {
        if (move)
        {
            moveTime += Time.deltaTime;

            //find the equivalence to not go out of scope
            if (moveTime >= 100000f)
                moveTime = 0f;

            actualPosition = startPosition + Mathf.Sin(moveTime * velocity) * intensity;

            switch (movementAxis)
            {
                case Axis.x:
                    transform.localPosition =
                        new Vector3(actualPosition, transform.localPosition.y, transform.localPosition.z);
                    break;
                case Axis.y:
                    transform.localPosition =
                        new Vector3(transform.localPosition.x, actualPosition, transform.localPosition.z);
                    break;
                case Axis.z:
                    transform.localPosition =
                        new Vector3(transform.localPosition.x, transform.localPosition.y, actualPosition);
                    break;
            }
        }
    }

}
