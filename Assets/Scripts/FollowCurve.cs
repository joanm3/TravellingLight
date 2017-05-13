using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierCurves;

[ExecuteInEditMode]
public class FollowCurve : MonoBehaviour
{

    public BezierCurve3D curve;
    [Range(0, 1)]
    public float pos;
    public bool changeRotation = true;
    public float speed;
    const float speedConst = 3;
    float startSpeed;
    public bool loop = false;
    public bool stopMove = false;

    public enum MoveType
    {
        lineaire, sinusoidale, loop
    }
    public MoveType movementType;

    public float waitTimeForSinMove;
    float startWaitTimeForSinMove;
    bool sinusDir = false;

    public AnimationCurve speedCurve;

    void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        startWaitTimeForSinMove = waitTimeForSinMove;
        startSpeed = speed;
        speed = 0;
    }

    void Update()
    {
        //if (speed < 0) speed = 0;
        //if (speed > startSpeed) speed = startSpeed;

        if (curve == null)
        {
            Debug.LogError("Assign Curve To " + gameObject.name);
            return;
        }

        transform.position = curve.GetPoint(pos);

        if (changeRotation) transform.rotation = curve.GetRotation(pos, Vector3.up);

        if (!Application.isPlaying)
        {
            return;
        }

        switch (movementType)
        {
            //LINEAIRE
            case MoveType.lineaire:
                //if (speed < startSpeed) speed += Time.deltaTime;
                //if (pos >= 0.8f && speed > 0) speed -= Time.deltaTime;

                if (pos < 1) pos += Time.deltaTime * speed;
                //STOP MOVE
                if (stopMove && speed > 0)
                {
                    speed -= Time.deltaTime * 0.5f;
                    if (speed < 0) speed = 0;
                }

                else if (!stopMove && speed < startSpeed)
                {
                    speed += Time.deltaTime;
                }
                //END STOP MOVE
                break;
            //SINUSOIDALE
            case MoveType.sinusoidale:
                //if (speed < startSpeed) speed += Time.deltaTime * 0.5f;
                //if (pos >= 0.8f && speed > 0) speed -= Time.deltaTime * 0.5f;
                //STOP MOVE
                if (stopMove && speed > 0)
                {
                    speed -= Time.deltaTime;
                    if (speed < 0) speed = 0;
                }

                else if (!stopMove && speed < startSpeed)
                {
                    speed += Time.deltaTime;
                }
                //END STOP MOVE
                if (!sinusDir && pos <= 1)
                {
                    pos += Time.deltaTime * speed;
                }
                else if (sinusDir && pos >= 0)
                {
                    pos -= Time.deltaTime * speed;
                }
                if ((pos >= 1 && !sinusDir) || (pos <= 0 && sinusDir))
                {
                    if (waitTimeForSinMove > 0)
                    {
                        waitTimeForSinMove -= Time.deltaTime;
                    }
                    else
                    {
                        sinusDir = !sinusDir;
                        waitTimeForSinMove = startWaitTimeForSinMove;
                    }
                }
                break;
            //LOOP
            case MoveType.loop:
                speed = 0.1f;
                pos += Time.deltaTime * speed;
                if (pos >= 1 && loop)
                {
                    pos = 0;
                }

                //STOP MOVE
                /*if (stopMove && speed > 0)
                {
                    speed -= Time.deltaTime;
                    if (speed < 0) speed = 0;
                }

                else if (!stopMove && speed < startSpeed)
                {
                    speed += Time.deltaTime;
                }*/
                //END STOP MOVE
                break;
            default:
                Debug.LogError("Asign MoveType to " + gameObject.name);
                return;
        }
    }
}
