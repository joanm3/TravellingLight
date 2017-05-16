using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectLight.Functions;


[RequireComponent(typeof(Collider))]
public class ChangeCameraDistanceOnTriggerEnter : MonoBehaviour
{


    public CameraController playerCamera;
    public float targetCameraDistance = 10f;
    public float speedToChange = 1f;
    public AnimationCurve speedCurve;


    public float startingCameraDistance;
    private float currentDistance;
    private float realCurrentDistance;
    private float mappedDistance;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main.GetComponent<CameraController>();
        }
        if (playerCamera == null) Debug.LogError("Please assign camera to changecameradistancecomponent");
        startingCameraDistance = playerCamera.cameraDistance;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //playerCamera.cameraDistance = targetCameraDistance;
            StopAllCoroutines();
            StartCoroutine(ChangeCameraDistance(targetCameraDistance));
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            //playerCamera.cameraDistance = startingCameraDistance;

            StopAllCoroutines();
            StartCoroutine(ChangeCameraDistance(startingCameraDistance));
        }
    }

    private IEnumerator ChangeCameraDistance(float newDistance)
    {

        float firstDistance = playerCamera.cameraDistance;

        if (newDistance > currentDistance)
        {
            while (currentDistance < newDistance)
            {
                currentDistance += Time.deltaTime * speedToChange;
                mappedDistance = Functions.MapRange(currentDistance, firstDistance, newDistance, 0f, 1f);
                speedCurve.Evaluate(mappedDistance);
                realCurrentDistance = Functions.MapRange(mappedDistance, 0f, 1f, firstDistance, newDistance);
                playerCamera.cameraDistance = realCurrentDistance;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (currentDistance > newDistance)
            {
                currentDistance -= Time.deltaTime * speedToChange;
                mappedDistance = Functions.MapRange(currentDistance, firstDistance, newDistance, 0f, 1f);
                speedCurve.Evaluate(mappedDistance);
                realCurrentDistance = Functions.MapRange(mappedDistance, 0f, 1f, firstDistance, newDistance);
                playerCamera.cameraDistance = realCurrentDistance;
                yield return new WaitForEndOfFrame();
            }
        }
        playerCamera.cameraDistance = newDistance;
    }




}
