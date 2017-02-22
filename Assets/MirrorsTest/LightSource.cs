using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LightSource : MonoBehaviour
{

    public GameObject lightSourcePrefab;
    public float maxDistance = 100f;
    public LayerMask mirrorMask;
    public LayerMask mecanismMask;
    [Header("Gizmos")]
    public float sphereRadius = 0.5f;
    [Header("Read Only, dont modify:")]
    public bool isReflecting = false;
    public bool isActivatingItem = false;
    public Ray rayLight;
    public RaycastHit hitInfo;
    public Vector3 contactPoint;
    public Vector3 contactNormal;
    public float distance;
    public Transform otherCollider = null;
    public GameObject contactGameObject = null;
    public LightSource reflectedLight = null;
    public ActivateOnLightContact lightMecanism;


    void Update()
    {
        rayLight.origin = transform.position;
        rayLight.direction = transform.forward;

        isActivatingItem = Physics.Raycast(rayLight, out hitInfo, maxDistance, mecanismMask);
        if (isActivatingItem)
        {
            contactPoint = hitInfo.point;
            distance = hitInfo.distance;
            contactGameObject = hitInfo.collider.gameObject;
            lightMecanism = contactGameObject.GetComponent<ActivateOnLightContact>();
            lightMecanism.IsActive = true;
            return;
        }

        if (lightMecanism != null)
        {
            lightMecanism.IsActive = false;
            contactGameObject = null;
            lightMecanism = null;
        }


        isReflecting = Physics.Raycast(rayLight, out hitInfo, maxDistance, mirrorMask);
        if (isReflecting)
        {
            contactPoint = hitInfo.point;
            contactNormal = hitInfo.normal;
            distance = hitInfo.distance;
            otherCollider = hitInfo.collider.transform;
            if (reflectedLight == null)
            {
                contactGameObject = Instantiate(lightSourcePrefab, contactPoint, Quaternion.identity, null);
                reflectedLight = contactGameObject.GetComponent<LightSource>();
            }
            else
            {
                reflectedLight.transform.forward = Vector3.Reflect(rayLight.direction, contactNormal);
                reflectedLight.transform.position = contactPoint;
            }
        }
        else
        {
            if (reflectedLight != null)
            {
                if (reflectedLight.reflectedLight != null)
                    Destroy(reflectedLight.reflectedLight.gameObject);
                Destroy(reflectedLight.gameObject);
            }
            distance = maxDistance;
            otherCollider = null;
        }



    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

#if UNITY_EDITOR
        float _sphereRadius = sphereRadius * HandleUtility.GetHandleSize(transform.position);
#else
        float _sphereRadius = sphereRadius; 
#endif
        Gizmos.DrawSphere(transform.position, _sphereRadius);

        if (!Application.isPlaying)
            return;

        if (isActivatingItem)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, contactPoint);
            Gizmos.DrawSphere(contactPoint, _sphereRadius);
            return;
        }

        if (isReflecting)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(transform.position, contactPoint);
            //Gizmos.DrawSphere(contactPoint, _sphereRadius);
        }
        else
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rayLight.origin, rayLight.direction * maxDistance);
        }



    }


}
