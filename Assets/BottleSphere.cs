using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleSphere : MonoBehaviour
{
    public int numberRequired = 3;
    public float activatedChangeDistance = 30f;
    public float transparencyChangeSpeed = 1f;
    public float TickUpdateForFireflies = 0.2f;


    public GameObject[] objectsThatAppearWithEachFirefly = new GameObject[3];
    private FadeWhenEnabling[] faders;

    public bool isFull = false;

    public List<Firefly> assignedFireflies = new List<Firefly>();
    public List<Firefly> firefliesInZone = new List<Firefly>();

    private float timer = 0f;
    private int lastIndex;


    void Start()
    {
        lastIndex = assignedFireflies.Count;
        faders = new FadeWhenEnabling[objectsThatAppearWithEachFirefly.Length];
        for (int i = 0; i < objectsThatAppearWithEachFirefly.Length; i++)
        {
            faders[i] = objectsThatAppearWithEachFirefly[i].GetComponent<FadeWhenEnabling>();
            if (i == 0) { objectsThatAppearWithEachFirefly[i].SetActive(true); continue; }

            for (int j = 0; j < faders[i].currentTransparencies.Length; j++)
            {
                faders[i].currentTransparencies[j] = 0f;
            }
            objectsThatAppearWithEachFirefly[i].SetActive(false);
        }

    }

    void Update()
    {


        if (lastIndex != assignedFireflies.Count)
        {
            //fade In/out the child objects
            for (int i = 0; i < faders.Length; i++)
            {

                if (i == assignedFireflies.Count)
                {
                    faders[i].gameObject.SetActive(true);
                    faders[i].finishedFading = false;
                    StartCoroutine(faders[i].FadeIn(transparencyChangeSpeed));
                }
                else
                {
                    faders[i].finishedFading = false;
                    StartCoroutine(faders[i].FadeOut(transparencyChangeSpeed));
                }
            }

            for (int i = 0; i < assignedFireflies.Count; i++)
            {
                assignedFireflies[i].cloak = 1f; 
            }

            if (assignedFireflies.Count >= numberRequired)
            {
                for (int i = 0; i < assignedFireflies.Count; i++)
                {
                    assignedFireflies[i].canActivate = false;
                    if (i != 0)
                    {
                        assignedFireflies[i].waterRenderer.gameObject.SetActive(false);
                    }
                    else
                    {
                        assignedFireflies[i].changeDistance = activatedChangeDistance;
                    }
                }
                isFull = true;
            }

            lastIndex = assignedFireflies.Count;
        }



        //ifFullBehaviour
        if (isFull)
        {

            return;
        }




        timer += Time.deltaTime;
        if (timer < TickUpdateForFireflies) return;



        for (int i = 0; i < firefliesInZone.Count; i++)
        {
            if (!firefliesInZone[i].IsEquipped && !assignedFireflies.Contains(firefliesInZone[i]))
            {
                firefliesInZone[i].integrateToBottle = true;
                firefliesInZone[i].currentBottleSphere = this;
                assignedFireflies.Add(firefliesInZone[i]);
                firefliesInZone.Remove(firefliesInZone[i]);
            }

        }
        timer = 0f;
    }



    void OnTriggerEnter(Collider other)
    {
        Firefly firefly = other.GetComponentInParent<Firefly>();
        if (firefly != null && !firefliesInZone.Contains(firefly))
        {
            firefliesInZone.Add(firefly);
        }
    }


    void OnTriggerExit(Collider other)
    {
        Firefly firefly = other.GetComponentInParent<Firefly>();
        if (firefly != null && firefliesInZone.Contains(firefly))
        {
            firefliesInZone.Remove(firefly);
        }
    }

}
