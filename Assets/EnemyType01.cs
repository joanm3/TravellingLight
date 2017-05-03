using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyType01 : TwoWorldsBehaviour
{
    public PathPoints Path;
    public float walkingSpeed = 3.5f;
    public float chasingSpeed = 4f;
    public float accelerationFactor = 0.2f;
    public float deccelerationFactor = 0.2f;

    public float checkDistanceToDest = 0.5f;

    public float currSpeed;

    NavMeshAgent agent;
    FieldOfView fov;
    Vector3 destination;
    public int destIndex = 0;

    float timer = 0f;
    public bool waiting = false;

    internal override void OnStart()
    {
        agent = GetComponent<NavMeshAgent>();
        fov = GetComponent<FieldOfView>();
        destination = Path.points[destIndex].transform.position;
        currSpeed = walkingSpeed;
        agent.SetDestination(destination);
    }

    internal override void OnUpdate()
    {

        agent.speed = currSpeed;

        if (Vector3.Distance(destination, transform.position) < checkDistanceToDest)
        {
            timer = 0f;
            destination = Path.points[Path.GetNextIndex(destIndex)].transform.position;
            StartCoroutine(WaitAndGoToNextPoint());
        }
    }

    internal override void OnSphereEnter()
    {
        StartCoroutine(ReduceSpeedToZero());
    }

    internal override void OnSphereExit()
    {
        StartCoroutine(RetrieveSpeedAndPath());
    }

    IEnumerator ReduceSpeedToZero()
    {

        while (agent.speed > 0f)
        {
            currSpeed -= deccelerationFactor * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        currSpeed = 0f;
    }

    IEnumerator RetrieveSpeedAndPath()
    {

        while (agent.speed < walkingSpeed)
        {
            currSpeed += accelerationFactor * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        currSpeed = walkingSpeed;
    }

    IEnumerator WaitAndGoToNextPoint()
    {
        PathPoint pp = Path.points[destIndex].GetComponent<PathPoint>();
        Debug.Log(pp.name);
        if (pp != null)
        {
            while (timer < pp.waitTime)
            {
                yield return new WaitForEndOfFrame();
                timer += Time.deltaTime;
            }
        }
        destIndex = Path.GetNextIndex(destIndex);
        agent.SetDestination(destination);
    }

    internal override void OutsideSphereUpdate()
    {

    }


}
