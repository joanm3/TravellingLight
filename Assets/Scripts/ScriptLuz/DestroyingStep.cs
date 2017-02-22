using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyingStep : MonoBehaviour {


    public float timer;
    public float timerStartDestroy;
    public float sizeSpeed;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= timerStartDestroy)
        {
            gameObject.transform.localScale = new Vector3 
                (Mathf.Lerp(gameObject.transform.localScale.x, 0, sizeSpeed * Time.deltaTime)
                ,gameObject.transform.localScale.y
                , Mathf.Lerp(gameObject.transform.localScale.z, 0, sizeSpeed * Time.deltaTime));
        }
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
