using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateLightRay : MonoBehaviour {

    [HideInInspector]
    public GameObject currentMirror;
    public float dist;
    Vector3 reflectedVector = Vector3.zero;

    void Update()
    {
        Vector3 up = transform.TransformDirection(Vector3.up);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, up, out hit, dist))
        {
            Debug.DrawRay(transform.position, up * hit.distance);

            if (hit.transform.gameObject.tag == ("Mirror"))
            {
                reflectedVector = Vector3.Reflect(up, hit.normal);
                Debug.DrawRay(hit.point, reflectedVector * dist);

                if (Physics.Raycast(hit.point, reflectedVector * dist, out hit, dist))
                {
                    if (hit.transform.gameObject.tag == ("Catalyst"))
                    {
                        hit.transform.gameObject.GetComponent<CatalystLightRay>().onLight = true;
                        if (currentMirror != null && currentMirror != hit.transform.gameObject)
                        {
                            currentMirror.GetComponent<CatalystLightRay>().DisableRay();
                        }
                        currentMirror = hit.transform.gameObject;
                    }
                }
            }

                else if (hit.transform.gameObject.tag == ("Catalyst"))
                {
                    hit.transform.gameObject.GetComponent<CatalystLightRay>().onLight = true;
                    if (currentMirror != null && currentMirror != hit.transform.gameObject)
                    {
                        currentMirror.GetComponent<CatalystLightRay>().DisableRay();
                    }
                    currentMirror = hit.transform.gameObject;
                }
            }
            else
            {
                Debug.DrawRay(transform.position, up * dist);
                if (currentMirror != null && currentMirror.GetComponent<CatalystLightRay>().onLight)
                {
                    currentMirror.GetComponent<CatalystLightRay>().DisableRay();
                }
            }
        }
}
