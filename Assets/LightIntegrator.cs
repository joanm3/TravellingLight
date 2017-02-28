using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntegrator : MonoBehaviour
{

    public AssignToCharacter assignToCharacter;
    public List<AssignToCharacter> LightFollowers = new List<AssignToCharacter>();
    public float minDistanceFromCharacter = 5f;
    public float distanceMultiplier = 2f;
    public float positioningSpeed = 5f;
    public float positioningYHeight = 4f;

    private AssignToCharacter placingObject;
    private Vector3 playerPlacePosition;

    void Update()
    {
        if (LightFollowers.Count > 0)
        {
            if (InputManager.Instance.placeLight.IsCurrentEvent() && placingObject == null)
            {
                placingObject = LightFollowers[0];
                placingObject.IsFollowingCharacter = false;
                playerPlacePosition = transform.position;
                playerPlacePosition.y = transform.position.y + positioningYHeight;
            }
        }

        if (placingObject)
        {
            //attention cause you can reposition a object when you should not like this. improve later. 
            float step = positioningSpeed * Time.deltaTime;
            if (Vector3.Distance(placingObject.transform.position, playerPlacePosition) > 0.1f)
            {
                placingObject.transform.position = Vector3.MoveTowards(placingObject.transform.position, playerPlacePosition, step);
            }
            else
            {
                placingObject = null;
            }
        }

    }


    void OnTriggerEnter(Collider other)
    {
        AssignToCharacter atc = other.gameObject.GetComponent<AssignToCharacter>();
        if (atc == null)
            atc = other.gameObject.GetComponentInParent<AssignToCharacter>();

        if (atc != null)
        {
            if (!atc.IsFollowingCharacter)
            {
                assignToCharacter = atc;
                atc.isInSphere = true;
                atc.distanceFromTarget = minDistanceFromCharacter + ((LightFollowers.Count - 1f) * distanceMultiplier);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        AssignToCharacter atc = other.gameObject.GetComponent<AssignToCharacter>();
        if (atc == null)
            atc = other.gameObject.GetComponentInParent<AssignToCharacter>();

        if (atc != null)
        {
            atc.isInSphere = false;
        }
    }
}
