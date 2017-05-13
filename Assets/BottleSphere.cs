using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierCurves;

public class BottleSphere : MonoBehaviour
{

    [Header("Bottle")]
    public int numberRequired = 3;
    public float activatedChangeDistance = 30f;
    public float activatedCloakSpeed = 1f;
    public float transparencyChangeSpeed = 1f;
    public float TickUpdateForFireflies = 0.2f;


    public GameObject[] objectsThatAppearWithEachFirefly = new GameObject[3];
    public FadeWhenEnabling[] faders;
    public ParticleSystem particles;

    public bool isFull = false;
    [Header("Bezier")]
    public FollowCurve followCurve;
    public float goToPositionSpeed = 1f;
    public bool isInEndPosition = false;

    [Header("Read Only")]
    public List<Firefly> assignedFireflies = new List<Firefly>();
    public List<Firefly> firefliesInZone = new List<Firefly>();

    private float timer = 0f;
    private int lastIndex;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (!particles) particles = GetComponentInChildren<ParticleSystem>();
        var emission = particles.emission;
        emission.enabled = false;
        isInEndPosition = false;
        if (!followCurve) followCurve = GetComponent<FollowCurve>();
        lastIndex = assignedFireflies.Count;
        if (faders.Length <= 0) faders = new FadeWhenEnabling[objectsThatAppearWithEachFirefly.Length];
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

        //update when assigned fireflies number change
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
                else if (faders[i].gameObject.activeInHierarchy)
                {
                    faders[i].finishedFading = false;
                    StartCoroutine(faders[i].FadeOut(transparencyChangeSpeed));
                }
            }

            for (int i = 0; i < assignedFireflies.Count; i++)
            {
                WorldMaskManager.Instance.forestTargets[assignedFireflies[i].index].cloak = 1f;
            }

            if (assignedFireflies.Count >= numberRequired)
            {
                for (int i = 0; i < assignedFireflies.Count; i++)
                {
                    assignedFireflies[i].canActivate = false;
                    assignedFireflies[i].transform.parent = this.transform;
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
                var emission = particles.emission;
                emission.enabled = true;
                particles.Play();
            }

            lastIndex = assignedFireflies.Count;
        }



        //ifFullBehaviour
        if (isFull)
        {

            if (player) particles.transform.LookAt(player, Vector3.up);

            for (int i = 0; i < faders.Length - 1; i++)
            {
                if (faders[i].gameObject.activeInHierarchy && !faders[i].finishedFading)
                {
                    StartCoroutine(faders[i].FadeOut(transparencyChangeSpeed));
                }
            }


            if (!isInEndPosition)
            {
                WorldMaskManager.Instance.forestTargets[assignedFireflies[0].index].cloak = 1f;
                followCurve.speed = goToPositionSpeed;

                if (followCurve.pos >= 1)
                {
                    followCurve.pos = 1f;
                    isInEndPosition = true;
                }
            }
            else
            {
                assignedFireflies[0].useGlobalDistance = false;
                assignedFireflies[0].changeDistance = activatedChangeDistance;
                WorldMaskManager.Instance.forestTargets[assignedFireflies[0].index].changeDistance = assignedFireflies[0].changeDistance;
                if (WorldMaskManager.Instance.forestTargets[assignedFireflies[0].index].cloak > 0f)
                {
                    WorldMaskManager.Instance.forestTargets[assignedFireflies[0].index].cloak -= (activatedCloakSpeed) * Time.deltaTime;
                }
                else
                {
                    WorldMaskManager.Instance.forestTargets[assignedFireflies[0].index].cloak = 0f;
                }
            }
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
