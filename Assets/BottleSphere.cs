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


    public bool isFull = false;

    public List<Firefly> assignedFireflies = new List<Firefly>();
    public List<Firefly> firefliesInZone = new List<Firefly>();

    private float timer = 0f;
    private int lastIndex;
    private MeshRenderer[][] renderers;
    private float[][] startingTransparencies;
    private float[][] currentTransparencies;
    private bool transparencyChangedDone = false;

    void Start()
    {

        // get the renderers of each object that appears. 
        renderers = new MeshRenderer[objectsThatAppearWithEachFirefly.Length][];
        startingTransparencies = new float[objectsThatAppearWithEachFirefly.Length][];
        currentTransparencies = new float[objectsThatAppearWithEachFirefly.Length][];

        lastIndex = assignedFireflies.Count;
        for (int i = 0; i < objectsThatAppearWithEachFirefly.Length; i++)
        {
            renderers[i] = objectsThatAppearWithEachFirefly[i].GetComponentsInChildren<MeshRenderer>(true);
            startingTransparencies[i] = new float[renderers[i].Length];
            currentTransparencies[i] = new float[renderers[i].Length];

            for (int j = 0; j < renderers[i].Length; j++)
            {
                startingTransparencies[i][j] = renderers[i][j].material.GetFloat("_Transparency");
                currentTransparencies[i][j] = startingTransparencies[i][j];
                Debug.Log(startingTransparencies[i][j]);
            }
            if (i == 0) { objectsThatAppearWithEachFirefly[i].SetActive(true); continue; }
            objectsThatAppearWithEachFirefly[i].SetActive(false);
        }

    }

    void Update()
    {








        if (lastIndex != assignedFireflies.Count)
        {
            //improve later
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].Length; j++)
                {
                    if (!transparencyChangedDone)
                    {
                        if (i == assignedFireflies.Count)
                        {
                            objectsThatAppearWithEachFirefly[i].SetActive(true);
                            float transparencyStep = Time.deltaTime * transparencyChangeSpeed;
                            currentTransparencies[i][j] += Mathf.Max(0f, transparencyStep);
                            renderers[i][j].material.SetFloat("_Transparency", currentTransparencies[i][j]);
                        }
                        else
                        {

                            float transparencyStep = Time.deltaTime * transparencyChangeSpeed;
                            currentTransparencies[i][j] -= Mathf.Max(0f, transparencyStep);
                            renderers[i][j].material.SetFloat("_Transparency", currentTransparencies[i][j]);
                            if (currentTransparencies[i][j] < 0f) transparencyChangedDone = true;
                        }
                    }
                    else
                    {
                        if (i == assignedFireflies.Count)
                        {
                            renderers[i][j].material.SetFloat("_Transparency", startingTransparencies[i][j]);
                        }
                        else
                        {
                            renderers[i][j].material.SetFloat("_Transparency", 0f);
                            objectsThatAppearWithEachFirefly[i].SetActive(true);
                        }
                    }

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

            if (transparencyChangedDone)
            {
                lastIndex = assignedFireflies.Count;
                transparencyChangedDone = false;
            }
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
