using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMinDistanceFromTargets : MonoBehaviour
{
    //this should change if distances are according to different types
    //for the moment all distances are the same so we leave it like that. to see in the future
    public bool IsInsideSphere { get { return isInsideSphere; } }
    internal bool isInsideSphere;
    internal bool lastInsideSphere;

    internal float minDistance;

    // Use this for initialization
    internal virtual void Start()
    {
        isInsideSphere = minDistance < WorldMaskManager.Instance.worldMaskGlobalVariables.GlobalChangeDistance;
        lastInsideSphere = isInsideSphere;
        minDistance = 100f;
    }

    // Update is called once per frame
    internal virtual void Update()
    {
        if (ColliderManager.Instance == null)
        {
            Debug.LogError("ColliderManager.Instance not found.");
            return;
        }
        minDistance = GetNearestDistance();
        isInsideSphere = minDistance < WorldMaskManager.Instance.worldMaskGlobalVariables.GlobalChangeDistance;

    }


    private float GetNearestDistance()
    {
        float minDist = 1000;
        int nearestIndex = -1;
        if (WorldMaskManager.Instance == null)
            return 1000f;

        for (int i = 0; i < WorldMaskManager.Instance.forestTargets.Count; i++)
        {
            if (WorldMaskManager.Instance.forestTargets[i].colorSetter.IsActive)
            {
                var dist = Vector3.Distance(transform.position, WorldMaskManager.Instance.forestTargets[i].target.transform.position);
                if (dist < minDist)
                {
                    nearestIndex = i;
                    minDist = dist;
                }
            }
        }
        return nearestIndex == -1 ? 1000 : minDist;
    }
}
