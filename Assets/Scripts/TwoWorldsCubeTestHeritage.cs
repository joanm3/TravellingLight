using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWorldsCubeTestHeritage : TwoWorldsBehaviour
{

    override internal void OnStart()
    {
        Debug.Log("OnStart");
    }

    override internal void OnUpdate()
    {
        Debug.Log("OnUpdate");

    }

    internal override void OnSphereEnter()
    {
        Debug.Log("OnSphereEnter");

    }

    internal override void OnSphereExit()
    {
        Debug.Log("OnSphereExit");

    }

    internal override void InSphereUpdate()
    {
        Debug.Log("InSphereUpdate");

    }

    internal override void OutsideSphereUpdate()
    {
        Debug.Log("OutsideSphereUpdate");
    }


}
