using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntegrator : MonoBehaviour
{
    public Transform equippedFirefliesTransform;
    public List<Firefly> AssignedFireflies = new List<Firefly>();
    public float minDistanceFromCharacter = 5f;
    public float distanceMultiplier = 2f;
    public float positioningSpeed = 5f;
    public float positioningYHeight = 4f;
    public float scalingVelocity = 0.5f;

    private Firefly assigningFirefly;
    private Firefly placingFirefly;
    private Vector3 playerPlacePosition;
    private bool currentlyAssigning = false;

    private bool isInAssignedPosition = false;
    private bool isInAssignedScale = false;

    private bool isInPlacedPosition = false;
    private bool isInPlacedScale = false;
    private bool isAssigningFirefly = false;
    private bool isPlacingFirefly = false;

    private List<Firefly> inZoneFireflies = new List<Firefly>();


    void Start()
    {
        if (equippedFirefliesTransform == null)
        {
            Debug.LogError("Please assign the " + equippedFirefliesTransform.ToString());
        }
    }


    void Update()
    {

        #region TAKE FIREFLY

        if (InputManager.Instance.takeFirefly.IsCurrentEvent() && inZoneFireflies.Count > 0 && !isAssigningFirefly && !isPlacingFirefly)
        {
            isAssigningFirefly = true;
            //you could look the closest one
            //HERE HERE HERE RECUPERATE ACCORDING TO ANGLE
            assigningFirefly = inZoneFireflies[0];
            inZoneFireflies.Remove(inZoneFireflies[0]);
            assigningFirefly.IsFollowingCharacter = true;
            //if (!AssignedFireflies.Contains(assigningFirefly)) AssignedFireflies.Add(assigningFirefly);
        }

        if (isAssigningFirefly && assigningFirefly != null && !isPlacingFirefly)
        {
            //position
            float step = positioningSpeed * Time.deltaTime;
            if (Vector3.Distance(assigningFirefly.transform.position, equippedFirefliesTransform.transform.position) > 0.4f)
            {
                assigningFirefly.transform.position = Vector3.MoveTowards(assigningFirefly.transform.position, equippedFirefliesTransform.transform.position, step);
            }
            else
            {
                isInAssignedPosition = true;
            }

            //scale
            if (assigningFirefly.transform.localScale.x > 0f)
            {
                assigningFirefly.transform.localScale = new Vector3(
                    assigningFirefly.transform.localScale.x - (scalingVelocity * Time.deltaTime),
                    assigningFirefly.transform.localScale.y - (scalingVelocity * Time.deltaTime),
                    assigningFirefly.transform.localScale.z - (scalingVelocity * Time.deltaTime));
            }
            else if (!isInAssignedScale)
            {
                assigningFirefly.transform.localScale = Vector3.zero;
                assigningFirefly.transform.parent = this.transform;
                assigningFirefly.transform.position = this.transform.position;
                isInAssignedScale = true;
            }


            if (isInAssignedScale && isInAssignedPosition)
            {

                assigningFirefly.transform.localScale = Vector3.zero;
                assigningFirefly.transform.position = equippedFirefliesTransform.transform.position;
                assigningFirefly = null;
                isInAssignedScale = false;
                isInAssignedPosition = false;
                isAssigningFirefly = false;
            }
        }

        #endregion


        #region PLACE FIREFLY
        if (isAssigningFirefly) return;

        if (AssignedFireflies.Count > 0)
        {
            if (InputManager.Instance.placeFirefly.IsCurrentEvent() && placingFirefly == null)
            {
                isPlacingFirefly = true;
                placingFirefly = AssignedFireflies[0];
                placingFirefly.transform.parent = null;
                playerPlacePosition = transform.position;
                playerPlacePosition.y = transform.position.y + positioningYHeight;
                placingFirefly.IsFollowingCharacter = false;
                if (!inZoneFireflies.Contains(placingFirefly)) inZoneFireflies.Add(placingFirefly);
            }
        }

        if (placingFirefly != null)
        {
            //position 
            float step = positioningSpeed * Time.deltaTime;
            if (Vector3.Distance(placingFirefly.transform.position, playerPlacePosition) > 0.1f)
            {
                placingFirefly.transform.position = Vector3.MoveTowards(placingFirefly.transform.position, playerPlacePosition, step);
            }
            else
            {
                //placingFirefly.IsFollowingCharacter = false;
                isInPlacedPosition = true;
            }

            //scale
            if (placingFirefly.transform.localScale.x < placingFirefly.startingScale)
            {
                placingFirefly.transform.localScale = new Vector3(
                    placingFirefly.transform.localScale.x + (scalingVelocity * Time.deltaTime),
                    placingFirefly.transform.localScale.y + (scalingVelocity * Time.deltaTime),
                    placingFirefly.transform.localScale.z + (scalingVelocity * Time.deltaTime));
            }
            else if (!isInAssignedScale)
            {
                placingFirefly.transform.localScale = new Vector3(placingFirefly.startingScale, placingFirefly.startingScale, placingFirefly.startingScale);
                isInPlacedScale = true;
            }


            if (isInPlacedPosition && isInPlacedScale)
            {
                placingFirefly = null;
                isInPlacedPosition = false;
                isInPlacedScale = false;
                isPlacingFirefly = false;
            }

        }
        #endregion



    }


    void OnTriggerEnter(Collider other)
    {
        Firefly atc = other.gameObject.GetComponent<Firefly>();
        if (atc == null)
            atc = other.gameObject.GetComponentInParent<Firefly>();

        if (atc != null && !atc.IsFollowingCharacter)
        {
            if (!inZoneFireflies.Contains(atc)) inZoneFireflies.Add(atc);
            //assigningFirefly = atc;
            atc.isInSphere = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Firefly atc = other.gameObject.GetComponent<Firefly>();
        if (atc == null)
            atc = other.gameObject.GetComponentInParent<Firefly>();

        //if (assigningFirefly != null && atc != null && assigningFirefly.Equals(atc))
        //{
        //    //assigningFirefly = null;
        //}

        if (inZoneFireflies.Contains(atc)) inZoneFireflies.Remove(atc);

    }
}
