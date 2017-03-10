using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldMaskController : MonoBehaviour
{
    public Shader maskShader;
    public float changeDistance = 10f;
    public bool waitTimeToChange = true;
    public float timeToCheck = 1f;
    public bool updateTarget = true;
    [Header("Forest")]
    public Transform target;
    [Range(0, 1)]
    public float cloak1;
    public Transform target2;
    [Range(0, 1)]
    public float cloak2;
    public Transform target3;
    [Range(0, 1)]
    public float cloak3;
    public Transform target4;
    [Range(0, 1)]
    public float cloak4;
    private Material maskMaterial;
    private float timeLeft = 1f;


    void Start()
    {

        if (!maskShader)
        {
            maskShader = Shader.Find("Custom/WorldSpaceNoiseMask");
            if (!maskShader)
            {
                Debug.LogError("no maskShader assigned for " + name);
                Destroy(this);
            }
        }

        maskMaterial = (Application.isPlaying) ? GetComponent<MeshRenderer>().material : GetComponent<MeshRenderer>().sharedMaterial;
        if (!maskMaterial.shader.Equals(maskShader))
        {
            Debug.LogError("shader is not correct for " + name + ", please assign " + maskShader.name);
            Destroy(this);
        }

        if (Application.isPlaying)
        {
            maskMaterial.SetFloat("_Clip", 1f);
        }

        UpdateGlobalVariables();
        UpdateCloaks();
        UpdateTargetsDone();
        UpdateTargetUniforms();

    }


    void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateGlobalVariables();
            if (WorldMaskManager.Instance.showHidden)
            {
                maskMaterial.SetFloat("_Clip", 0f);
            }
            else
            {
                maskMaterial.SetFloat("_Clip", 1f);

            }
        }

        UpdateCloaks();
        UpdateTargetsDone();
        UpdateTargetUniforms();



        if (!updateTarget)
            return;

        if (waitTimeToChange)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                UpdateTargetsDone();
                timeLeft = Mathf.Abs(timeToCheck);
            }
        }
        else
        {
            UpdateTargetsDone();
        }
    }

    // Update is called once per frame
    void UpdateTargetUniforms()
    {
        if (maskMaterial == null)
            return;

        maskMaterial.SetFloat("_ChangePoint", changeDistance);
        maskMaterial.SetVector("_TargetPosition", target.position);
        maskMaterial.SetVector("_TargetPos2", target2.position);
        maskMaterial.SetVector("_TargetPos3", target3.position);
        maskMaterial.SetVector("_TargetPos4", target4.position);

    }

    void UpdateGlobalVariables()
    {
        maskMaterial.SetVector("_NoiseSettings2", WorldMaskManager.Instance.worldMaskGlobalVariables.NoiseSettings);
        maskMaterial.SetVector("_LineColor", WorldMaskManager.Instance.worldMaskGlobalVariables.LineColor);
        maskMaterial.SetFloat("_CircleForce", WorldMaskManager.Instance.worldMaskGlobalVariables.CircleForce);
        maskMaterial.SetFloat("_Expand", WorldMaskManager.Instance.worldMaskGlobalVariables.InnerExpand);
    }

    void UpdateCloaks()
    {
        if (WorldMaskManager.Instance == null)
            return;

        cloak1 = WorldMaskManager.Instance.cloak1;
        cloak2 = WorldMaskManager.Instance.cloak2;
        cloak3 = WorldMaskManager.Instance.cloak3;
        cloak4 = WorldMaskManager.Instance.cloak4;

        if (maskMaterial == null)
            return;

        maskMaterial.SetFloat("_Cloak", cloak1);
        maskMaterial.SetFloat("_Cloak2", cloak2);
        maskMaterial.SetFloat("_Cloak3", cloak3);
        maskMaterial.SetFloat("_Cloak4", cloak4);
    }

    void UpdateTargetsDone()
    {
        if (WorldMaskManager.Instance == null)
            return;

        target = WorldMaskManager.Instance.target1;
        target2 = WorldMaskManager.Instance.target2;
        target3 = WorldMaskManager.Instance.target3;
        target4 = WorldMaskManager.Instance.target4;

    }

    //void UpdateTargets()
    //{
    //    //try to find a way where i dont have to loop at every frame (start a coroutine or wait for time better at least). 
    //    if (WorldMaskManager.Instance != null)
    //    {
    //        UpdateTarget(ref target, 1);
    //        UpdateTarget(ref target2, 2);
    //        UpdateTarget(ref target3, 3);
    //        UpdateTarget(ref target4, 4);
    //    }
    //}


    //void UpdateTarget(ref Transform target, int index)
    //{
    //    if (WorldMaskManager.Instance.targets.Count > index - 1)
    //    {
    //        float distance = Vector3.Distance(transform.position, target.position);
    //        for (int i = 0; i < WorldMaskManager.Instance.targets.Count; i++)
    //        {
    //            if (Vector3.Distance(transform.position, WorldMaskManager.Instance.targets[i].transform.position) < distance)
    //            {
    //                target = WorldMaskManager.Instance.targets[i].transform;
    //                distance = Vector3.Distance(transform.position, WorldMaskManager.Instance.targets[i].transform.position);
    //            }
    //        }
    //    }
    //}

}
