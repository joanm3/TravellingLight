using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : Singleton<PlayerState>
{

    public bool IsInsideSphere { get { return isInsideSphere; } }

    public float minDistance;
    public float updateTick = 0.2f;

    [SerializeField]
    private bool isInsideSphere;
    private bool lastIsInsideSphere;
    private List<TwoWorldsBehaviour> twoWorldsObjects = new List<TwoWorldsBehaviour>();

    private float timer;

    void Start()
    {
        isInsideSphere = false;
        minDistance = 100f;
        lastIsInsideSphere = isInsideSphere;
        ReloadTwoWorldsObjectsList();
        SetWorldColliders();
    }

    // Update is called once per frame
    void Update()
    {
        //get where the player is. 
        if (ColliderManager.Instance == null)
            return;


        timer += Time.deltaTime;
        if (timer < updateTick) return;

        //minDistance = GetNearestDistance();
        isInsideSphere = InsideSphere();

        if (isInsideSphere != lastIsInsideSphere)
        {
            ColliderManager.Instance.SetWorldAndColliders(isInsideSphere);
            //new method not yet integrated. 
            //SetWorldColliders();
            lastIsInsideSphere = isInsideSphere;
        }

        timer = 0f;
    }

    private void SetWorldColliders()
    {

        //you should do in a way that a collider is still a trigger if the player is inside it when exiting a sphere
        //do it later if you have the time. 
        //and make them once again not triggers when the player exits them. 
        if (isInsideSphere)
        {
            for (int i = 0; i < twoWorldsObjects.Count; i++)
            {
                if (twoWorldsObjects[i].inSphereCollider != null)
                {
                    twoWorldsObjects[i].inSphereCollider.isTrigger = false;
                }
                if (twoWorldsObjects[i].outsideSphereCollider != null)
                {
                    twoWorldsObjects[i].outsideSphereCollider.isTrigger = true;
                }
            }
        }
        else
        {
            for (int i = 0; i < twoWorldsObjects.Count; i++)
            {
                if (twoWorldsObjects[i].inSphereCollider != null)
                {
                    twoWorldsObjects[i].inSphereCollider.isTrigger = true;
                }
                if (twoWorldsObjects[i].outsideSphereCollider != null)
                {
                    twoWorldsObjects[i].outsideSphereCollider.isTrigger = false;
                }
            }
        }
    }

    private float GetNearestDistance()
    {
        float minDist = 1000;
        int nearestIndex = -1;
        if (WorldMaskManager.Instance == null)
        {
            isInsideSphere = false;
            return 1000f;
        }

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

    private bool InsideSphere()
    {
        if (WorldMaskManager.Instance == null)
        {
            return false;
        }

        for (int i = 0; i < WorldMaskManager.Instance.forestTargets.Count; i++)
        {
            if (WorldMaskManager.Instance.forestTargets[i].colorSetter.IsActive ||
                (WorldMaskManager.Instance.forestTargets[i].firefly.consumedByBottleSphere && WorldMaskManager.Instance.forestTargets[i].firefly.currentBottleSphere.isFull))
            {
                Vector3 posWithCloak = Mathf.Abs(1 - WorldMaskManager.Instance.forestTargets[i].cloak) * WorldMaskManager.Instance.forestTargets[i].target.transform.position;
                var dist = Vector3.Distance(transform.position, posWithCloak);
                if (dist < WorldMaskManager.Instance.forestTargets[i].changeDistance)
                {
                    //Debug.Log()
                    return true;
                }
            }
        }

        return false;
    }

    private void ReloadTwoWorldsObjectsList()
    {
        if (twoWorldsObjects != null || twoWorldsObjects.Count > 0)
            twoWorldsObjects.Clear();
        else
            twoWorldsObjects = new List<TwoWorldsBehaviour>();

        TwoWorldsBehaviour[] _twb = FindObjectsOfType<TwoWorldsBehaviour>();
        for (int i = 0; i < _twb.Length; i++)
        {
            twoWorldsObjects.Add(_twb[i]);
        }
    }
}
