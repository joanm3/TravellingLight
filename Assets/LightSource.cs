using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LightSource : MonoBehaviour
{

    public GameObject lightSourcePrefab;
    public float maxDistance = 100f;
    public LayerMask layerMask;
    [Header("Gizmos")]
    public float sphereRadius = 0.5f;
    [Header("Read Only, dont modify:")]
    public bool isReflecting = false;
    public Ray rayLight;
    public RaycastHit hitInfo;
    public Vector3 contactPoint;
    public Vector3 contactNormal;
    public float distance;
    public Transform otherCollider = null;
    public LightSource reflectedLight = null;


    void Update()
    {
        rayLight.origin = transform.position;
        rayLight.direction = transform.forward;

        isReflecting = Physics.Raycast(rayLight, out hitInfo, maxDistance, layerMask);
        if (isReflecting)
        {
            contactPoint = hitInfo.point;
            contactNormal = hitInfo.normal;
            distance = hitInfo.distance;
            otherCollider = hitInfo.collider.transform;
            if (reflectedLight == null)
            {
                reflectedLight = Instantiate(lightSourcePrefab, contactPoint, Quaternion.identity, null).GetComponent<LightSource>();
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
        Gizmos.DrawSphere(transform.position, sphereRadius);

        if (!Application.isPlaying)
            return;

        if (isReflecting)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(transform.position, contactPoint);
            Gizmos.DrawSphere(contactPoint, _sphereRadius);
        }
        else
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rayLight.origin, rayLight.direction * maxDistance);
        }



    }


}
