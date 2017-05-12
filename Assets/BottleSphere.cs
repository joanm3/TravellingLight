using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleSphere : MonoBehaviour
{
    public int numberRequired = 3;
    public float activatedChangeDistance = 30f;
    public float TickUpdateForFireflies = 0.2f;

    public GameObject[] objectsThatAppearWithEachFirefly = new GameObject[3];

    public Color whiteColor = Color.white;
    public Color blueColor = Color.cyan;

    public bool isFull = false;

    public List<Firefly> assignedFireflies = new List<Firefly>();
    public List<Firefly> firefliesInZone = new List<Firefly>();

    private float timer = 0f;
    private Material mat;

    private int lastIndex;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        lastIndex = assignedFireflies.Count;
        for (int i = 0; i < objectsThatAppearWithEachFirefly.Length; i++)
        {
            objectsThatAppearWithEachFirefly[i].SetActive(false);
        }
    }

    void Update()
    {
        if (lastIndex != assignedFireflies.Count)
        {
            //improve later
            for (int i = 0; i < objectsThatAppearWithEachFirefly.Length; i++)
            {
                if (objectsThatAppearWithEachFirefly[i] != null)
                {
                    if (i < assignedFireflies.Count) objectsThatAppearWithEachFirefly[i].SetActive(true);
                    else objectsThatAppearWithEachFirefly[i].SetActive(false);
                }
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


        if (isFull) return;

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
