using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuzzImpulse : MonoBehaviour {

    public float timer;
    float timerStart;

    void Start()
    {
        timerStart = timer;
    }
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            gameObject.GetComponent<Rigidbody>().AddForce(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            timer = timerStart;
        }
    }
}
