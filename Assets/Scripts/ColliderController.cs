using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderController : MonoBehaviour {


    public enum WorldAppartenance
    {
        world1,world2
    }

    public WorldAppartenance gameObjectWorld;
    public Collider col; 

    public void DoSomething()
    {
        gameObject.GetComponent<Collider>().enabled = !gameObject.GetComponent<Collider>().enabled;
    }

    //CurrentWorld = true : World 1
    //CurrentWorld = false : World 2
    void OnChangeWorld(WorldAppartenance w)
    {
        if (w == WorldAppartenance.world1 && gameObjectWorld == WorldAppartenance.world1)
        {
            //gameObject.GetComponent<Collider>().
        }
    }
}
