using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Mecanism : MonoBehaviour
{

    public enum ActivationType { WorldInSphere, WorldOutsideSphere, TwoWorlds }
    public ActivationType activationType;
    public bool activateOnCollision = false;
    public bool stayActive = false;
    public GameObject door;
    public Color activeColor;
    public Color inactiveColor;

    private bool isActive;
    //for the moment only player, 
    //we should also add other objects that can activate; 
    private bool playerInTrigger = false;
    private GetMinDistanceFromTargets distanceFromTargets;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        distanceFromTargets = GetComponent<GetMinDistanceFromTargets>();
        if (distanceFromTargets == null)
            distanceFromTargets = gameObject.AddComponent<GetMinDistanceFromTargets>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive && stayActive)
            return;

        switch (activationType)
        {
            case ActivationType.WorldInSphere:
                isActive = (distanceFromTargets.IsInsideSphere) ? playerInTrigger : false;
                break;
            case ActivationType.WorldOutsideSphere:
                isActive = (!distanceFromTargets.IsInsideSphere) ? playerInTrigger : false;
                break;
            case ActivationType.TwoWorlds:
                isActive = playerInTrigger;
                break;
        }

        //TODO: make sure the shader has a color param. 
        rend.material.color = isActive ? activeColor : inactiveColor;
        //this could be called only when it changes maybe. 
        if (activateOnCollision)
            door.SetActive(isActive);
        else
            door.SetActive(!isActive);

    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInTrigger = false;
        }
    }



}
