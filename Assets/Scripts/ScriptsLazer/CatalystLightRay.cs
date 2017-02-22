using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalystLightRay : MonoBehaviour {

    public float dist;
    public bool onLight = false;
    public GameObject lightCanon;
    [HideInInspector]
    public GameObject currentMirror;
    GameObject currentMirrorReflect;
    //[HideInInspector]
    public float nbRayTouched;
    public Material illuminatedMaterial;
    Material startMaterial;
    Vector3 reflectedVector = Vector3.zero;

    void Start()
    {
        startMaterial = GetComponent<Renderer>().material;
    }
    void Update()
    {
        if (onLight)
        {
            GetComponent<Renderer>().material = illuminatedMaterial;
            Vector3 up = lightCanon.transform.TransformDirection(Vector3.up);
            RaycastHit hit;

            if (Physics.Raycast(lightCanon.transform.position, up, out hit, dist))
            {
                Debug.DrawRay(lightCanon.transform.position, up * hit.distance);
                if (hit.transform.gameObject.tag == ("Mirror"))
                {
                    reflectedVector = Vector3.Reflect(up, hit.normal);

                    Vector3 firstHit = hit.point;

                    if (Physics.Raycast(hit.point, reflectedVector * dist, out hit, dist))
                    {
                        Vector3 secondHit = hit.point;
                        currentMirrorReflect = hit.transform.gameObject;
                        if (hit.transform.gameObject.tag == ("Catalyst"))
                        {
                            Debug.DrawRay(firstHit, -hit.normal * hit.distance);
                            hit.transform.gameObject.GetComponent<CatalystLightRay>().onLight = true;
                            if (currentMirror != null && currentMirror != hit.transform.gameObject)
                            {
                                currentMirror.GetComponent<CatalystLightRay>().DisableRay();
                            }
                            currentMirror = hit.transform.gameObject;
                        }
                        else
                        {
                            Debug.DrawRay(firstHit, reflectedVector * dist);
                        }
                    }
                    else
                    {
                        Debug.DrawRay(firstHit, reflectedVector * dist);
                    }
                }
                else if (hit.transform.gameObject.tag == ("Catalyst"))
                {
                    currentMirror = hit.transform.gameObject;
                    currentMirror.GetComponent<CatalystLightRay>().onLight = true;
                    if (currentMirror != null && currentMirror != hit.transform.gameObject)
                    {
                        currentMirror.GetComponent<CatalystLightRay>().nbRayTouched += 1;
                        currentMirror.GetComponent<CatalystLightRay>().DisableRay();
                    }
                }
                if (hit.transform.gameObject.tag == ("Wall"))
                {

                }
            }
            else 
            {
                Debug.DrawRay(lightCanon.transform.position, up * dist);
                if (currentMirror != null) currentMirror.GetComponent<CatalystLightRay>().DisableRay();
                if (currentMirrorReflect != null) currentMirrorReflect.GetComponent<CatalystLightRay>().DisableRay();
            }
        }
    }

    public void DisableRay()
    {
        onLight = false;
        GetComponent<Renderer>().material = startMaterial;        
            if (currentMirrorReflect.tag == ("Catalyst") && currentMirrorReflect != null)
        {
            currentMirrorReflect.GetComponent<CatalystLightRay>().nbRayTouched -= 1;
        }
        if (currentMirrorReflect != null)
        {
            if (currentMirrorReflect.GetComponent<CatalystLightRay>().onLight && currentMirrorReflect.GetComponent<CatalystLightRay>().nbRayTouched <= 0)
            {
                currentMirrorReflect.GetComponent<CatalystLightRay>().DisableRay();
            }
        }
        if (currentMirror.tag == ("Catalyst") && currentMirror != null)
        {
            currentMirror.GetComponent<CatalystLightRay>().nbRayTouched -= 1;
        }
        if (currentMirror != null)
        {
            if (currentMirror.GetComponent<CatalystLightRay>().onLight && currentMirror.GetComponent<CatalystLightRay>().nbRayTouched <= 0)
            {
                currentMirror.GetComponent<CatalystLightRay>().DisableRay();
            }
        }
    }
}
