using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectLight.Types;


public class ConstantRotation : MonoBehaviour
{

    public Axis rotationAxis = Axis.y;
    public bool rotate = true;
    public float velocity = 1f;

    float actualRotation;


    void Update()
    {
        if (rotate)
        {
            actualRotation += velocity * Time.deltaTime;
            if (actualRotation > 359f)
                actualRotation = 0f;

            switch (rotationAxis)
            {
                case Axis.x:
                    transform.eulerAngles =
                        new Vector3(actualRotation, transform.eulerAngles.y, transform.eulerAngles.z);
                    break;
                case Axis.y:
                    transform.eulerAngles =
                        new Vector3(transform.eulerAngles.x, actualRotation, transform.eulerAngles.z);
                    break;
                case Axis.z:
                    transform.eulerAngles =
                        new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, actualRotation);
                    break;
            }
        }

    }


}
