using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightIntegrator : MonoBehaviour
{
    public Transform equippedFirefliesTransform;
    public List<Firefly> AssignedFireflies = new List<Firefly>();
    public float positioningSpeed = 5f;
    public float positioningYHeight = 4f;
    public float scalingVelocity = 0.5f;
    public float transparencySpeed = 0.3f;

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
    private bool isInGoodTransparency = false;
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
            //you could look the closest one
            //HERE HERE HERE RECUPERATE ACCORDING TO ANGLE TO PLAYER

            for (int i = 0; i < inZoneFireflies.Count; i++)
            {
                if (inZoneFireflies[i].canActivate)
                {
                    assigningFirefly = inZoneFireflies[i];
                    inZoneFireflies.Remove(inZoneFireflies[i]);
                    isAssigningFirefly = true;
                    break;
                }
            }
            if (isAssigningFirefly)
            {
                assigningFirefly.IsEquipped = true;
                //check if it is in a bottle but can be used and take it out of the box. 
                if (assigningFirefly.consumedByBottleSphere && assigningFirefly.currentBottleSphere != null)
                {
                    if (assigningFirefly.currentBottleSphere.assignedFireflies.Contains(assigningFirefly))
                    {
                        assigningFirefly.currentBottleSphere.assignedFireflies.Remove(assigningFirefly);
                        assigningFirefly.currentBottleSphere.firefliesInZone.Add(assigningFirefly);
                    }
                    assigningFirefly.currentBottleSphere = null;
                }
            }
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

            if (assigningFirefly.currentTransparency < assigningFirefly.startingTransparency)
            {
                float transparencyStep = Time.deltaTime * transparencySpeed;
                assigningFirefly.currentTransparency += Mathf.Min(assigningFirefly.startingTransparency, transparencyStep);
                var emission = assigningFirefly.particles.emission;
                emission.enabled = true;
                assigningFirefly.waterMaterial.SetFloat("_Transparency", assigningFirefly.currentTransparency);
            }
            else if (!isInGoodTransparency)
            {
                assigningFirefly.waterMaterial.SetFloat("_Transparency", assigningFirefly.startingTransparency);
                isInGoodTransparency = true;
            }




            if (isInAssignedScale && isInAssignedPosition && isInGoodTransparency)
            {
                assigningFirefly.transform.localScale = Vector3.zero;
                assigningFirefly.transform.position = equippedFirefliesTransform.transform.position;
                assigningFirefly = null;
                isInAssignedScale = false;
                isInAssignedPosition = false;
                isAssigningFirefly = false;
                isInGoodTransparency = true;
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
                playerPlacePosition = equippedFirefliesTransform.position;
                playerPlacePosition.y = playerPlacePosition.y + positioningYHeight;
                placingFirefly.IsEquipped = false;
                if (!inZoneFireflies.Contains(placingFirefly)) inZoneFireflies.Add(placingFirefly);
                placingFirefly.canActivate = true;
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

        if (atc != null && !atc.IsEquipped)
        {
            if (!inZoneFireflies.Contains(atc)) inZoneFireflies.Add(atc);
            //assigningFirefly = atc;
            atc.isInLightIntegratorZone = true;
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
