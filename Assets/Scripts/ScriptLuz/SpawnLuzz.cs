using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLuzz : MonoBehaviour {

    public GameObject Luzz;
    public float nbLuzz;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == ("LightSource"))
        {
            for (int i = 0; i< nbLuzz; i++)
            {
                GameObject.Instantiate(Luzz, gameObject.transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
