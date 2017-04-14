using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoWorldsCubeTestHeritage : TwoWorldsBehaviour
{

    //dont use Start or Update in TwoWorldsBehaviour child classes to avoid overriding it. 
    //also it is usable the bool IsInsideSphere. 

    override internal void OnStart()
    {
        Debug.Log("IsInsideSphere = " + IsInsideSphere.ToString());
        Debug.Log("OnStart");
    }

    override internal void OnUpdate()
    {
        Debug.Log("OnUpdate");

    }

    internal override void OnSphereEnter()
    {
        Debug.Log("OnSphereEnter");
        Debug.Log("IsInsideSphere = " + IsInsideSphere.ToString());
    }

    internal override void OnSphereExit()
    {
        Debug.Log("OnSphereExit");
        Debug.Log("IsInsideSphere = " + IsInsideSphere.ToString());
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
