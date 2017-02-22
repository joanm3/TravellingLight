using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveLight : MonoBehaviour
{

    public bool desactivateOnExit = false;
    public Material white;
    public Material black;
    public GameObject luz;
    public GameObject player;
    public float dist;
    bool actived = false;
    GameObject luzInstance = null;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, gameObject.transform.position) < dist && !actived)
        {

        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (!actived && col.gameObject.tag == ("LightSourceCollider"))
        {
            if (luz != null)
            {
                actived = !actived;
                gameObject.GetComponent<Renderer>().material = white;
                luzInstance = Instantiate(luz, gameObject.transform.position, Quaternion.identity);
            }
        }
    }


    void OnTriggerExit(Collider col)
    {
        if (!desactivateOnExit)
            return;

        if (actived && col.gameObject.tag == ("LightSourceCollider"))
        {
            if (luzInstance != null)
            {
                actived = !actived;
                gameObject.GetComponent<Renderer>().material = black;
                Destroy(luzInstance);
            }
        }
    }
}
