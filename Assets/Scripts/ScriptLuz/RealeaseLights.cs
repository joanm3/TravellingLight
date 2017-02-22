using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealeaseLights : MonoBehaviour {

    public GameObject sphereContainer;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == ("Player") || col.gameObject.tag == (" LightSourceCollider"))
        {
            sphereContainer.GetComponent<Rigidbody>().useGravity = true;
        }
    }

}
