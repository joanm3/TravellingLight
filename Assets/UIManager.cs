using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{


    public GameObject[] waterSpheres;
    public LightIntegrator lightIntegrator;
    public ParticleSystem particles;
    public FollowCurve followCurve;
    public float curveVelocity = 1f;
    public Transform bezierEnding;
    public Vector3 positionOfSphere;

    public int lastNumberOfSpheres;
    public int currentNumberOfSpheres;
    private ParticleSystem.EmissionModule emission;

    void Start()
    {
        emission = particles.emission;
        emission.enabled = false;
        currentNumberOfSpheres = 0;
        lastNumberOfSpheres = currentNumberOfSpheres;
        for (int i = 0; i < waterSpheres.Length; i++)
        {
            waterSpheres[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentNumberOfSpheres = lightIntegrator.AssignedFireflies.Count;
        if (currentNumberOfSpheres != lastNumberOfSpheres)
        {
            if (currentNumberOfSpheres > lastNumberOfSpheres)
            {
                bezierEnding.position = waterSpheres[lightIntegrator.AssignedFireflies.Count - 1].transform.position;
                followCurve.pos = 0f;
                followCurve.speed = curveVelocity;
                emission.enabled = true;
            }

            for (int i = 0; i < waterSpheres.Length; i++)
            {
                if (i < currentNumberOfSpheres)
                    waterSpheres[i].SetActive(true);
                else
                    waterSpheres[i].SetActive(false);
            }
            lastNumberOfSpheres = currentNumberOfSpheres;
        }

        if (followCurve.pos >= 1f)
        {
            emission = particles.emission;
            emission.enabled = false;
        }


    }




}
