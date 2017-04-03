﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMinDistanceFromTargets : MonoBehaviour
{

    public bool changeWorldAccordingToDistance = false;
    //this should change if distances are according to different types
    //for the moment all distances are the same so we leave it like that. to see in the future
    public float minDistance;
    public bool IsInsideSpheres { get { return minDistance < WorldMaskManager.Instance.worldMaskGlobalVariables.GlobalChangeDistance; } }

    // Use this for initialization
    void Start()
    {
        minDistance = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        if (ColliderManager.Instance == null)
            return;
        minDistance = GetNearestDistance();
        if (changeWorldAccordingToDistance)
        {
            ColliderManager.Instance.SetWorldAndColliders(minDistance < WorldMaskManager.Instance.worldMaskGlobalVariables.GlobalChangeDistance);
        }
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
