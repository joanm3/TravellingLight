using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderController : MonoBehaviour {


    public enum WorldAppartenance
    {
        worldInSphere,worldOutSphere
    }

    public WorldAppartenance gameObjectWorld;

    public void SetCollider(bool inThisWorld)
    {
        gameObject.GetComponent<Collider>().enabled = inThisWorld;
        //gameObject.GetComponent<MeshRenderer>().enabled = inThisWorld;
    }
}
