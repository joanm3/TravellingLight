using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightManager : MonoBehaviour {

    public float lightStart;
    float currentLight;
    public float speedLightEnd;
    //public GameObject lightDisplay;
    bool onLightSource;
    public GameObject playerLight;
    float lightRange;

    void Start()
    {
        currentLight = lightStart;
    }

    void Update()
    {
        if (!onLightSource && currentLight > 0)
        {
            currentLight -= speedLightEnd * Time.deltaTime;
            lightRange = currentLight / 10;
        }
        //playerLight.GetComponent<Light>().range = lightRange;
        //gameObject.transform.localScale = new Vector3(lightRange, lightRange, lightRange);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == ("LightSource"))
        {
            currentLight = lightStart;
            onLightSource = true;
        }
    }
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == ("LightSource"))
        {
            onLightSource = false;
        }
    }
}
