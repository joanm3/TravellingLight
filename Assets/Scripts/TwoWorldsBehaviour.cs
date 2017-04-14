using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWorldsBehaviour : GetMinDistanceFromTargets
{

    public Collider inSphereCollider;
    public Collider outsideSphereCollider;



    sealed override internal void Start()
    {
        base.Start();
        OnStart();
    }

    internal virtual void OnStart()
    {

    }

    sealed override internal void Update()
    {
        base.Update();
        OnUpdate();

        if (IsInsideSphere != lastInsideSphere)
        {
            if (IsInsideSphere)
            {
                OnSphereEnter();
            }
            else
            {
                OnSphereExit();
            }
            lastInsideSphere = IsInsideSphere;
        }


        if (IsInsideSphere)
        {
            InSphereUpdate();
        }
        else
        {
            OutsideSphereUpdate();
        }
    }

    /// <summary>
    /// Called at every Update before:
    /// -OnSphereEnter
    /// -OnSphereExit
    /// -InSphereUpdate
    /// -OutsideSphereUpdate
    /// </summary>
    internal virtual void OnUpdate()
    {

    }

    /// <summary>
    /// Called when the object enters a sphere
    /// </summary>
    internal virtual void OnSphereEnter()
    {

    }

    /// <summary>
    /// Called when the object exits a sphere
    /// </summary>
    internal virtual void OnSphereExit()
    {

    }

    /// <summary>
    /// called every Update while the object is inside a sphere
    /// </summary>
    internal virtual void InSphereUpdate()
    {

    }

    /// <summary>
    /// called every Update while the object is outside a sphere
    /// </summary>
    internal virtual void OutsideSphereUpdate()
    {

    }

}
