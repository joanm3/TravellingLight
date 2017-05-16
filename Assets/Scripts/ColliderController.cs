using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderController : MonoBehaviour
{


    public Collider[] colliders;


    public enum WorldAppartenance
    {
        worldInSphere, worldOutSphere
    }

    public WorldAppartenance gameObjectWorld;

    public void SetCollider(bool inThisWorld)
    {
        if (colliders == null || colliders.Length <= 0) gameObject.GetComponent<Collider>().enabled = inThisWorld;
        else
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = inThisWorld;
            }
        }
        //gameObject.GetComponent<MeshRenderer>().enabled = inThisWorld;
    }
}
