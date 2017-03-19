using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldMaskController : MonoBehaviour
{

    public enum WorldType { Forest, City };
    public WorldType worldType = WorldType.Forest;
    public Shader maskShader;
    public float changeDistance = 10f;
    public bool waitTimeToChange = true;
    public float timeToCheck = 1f;
    public bool updateTarget = true;



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
    public Transform target5;
    [Range(0, 1)]
    public float cloak5;
    public Transform target6;
    [Range(0, 1)]
    public float cloak6;

    private Material maskMaterial;
    private float timeLeft = 1f;


    void Start()
    {

        //if (!maskShader)
        //{
        //    maskShader = Shader.Find("Custom/WorldSpaceNoiseMask");
        //    if (!maskShader)
        //    {
        //        Debug.LogError("no maskShader assigned for " + name);
        //        Destroy(this);
        //    }
        //}

        maskMaterial = (Application.isPlaying) ? GetComponent<MeshRenderer>().material : GetComponent<MeshRenderer>().sharedMaterial;
        //if (!maskMaterial.shader.Equals(maskShader))
        //{
        //    Debug.LogError("shader is not correct for " + name + ", please assign " + maskShader.name);
        //    Destroy(this);
        //}

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

        if (target == null || target2 == null
            || target3 == null || target4 == null
            || target5 == null || target6 == null)
            return;

        maskMaterial.SetVector("_TargetPosition", target.position);
        maskMaterial.SetVector("_TargetPos2", target2.position);
        maskMaterial.SetVector("_TargetPos3", target3.position);
        maskMaterial.SetVector("_TargetPos4", target4.position);
        maskMaterial.SetVector("_TargetPos5", target5.position);
        maskMaterial.SetVector("_TargetPos6", target6.position);

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

        switch (worldType)
        {
            case WorldType.Forest:
                cloak1 = WorldMaskManager.Instance.cloak1;
                cloak2 = WorldMaskManager.Instance.cloak2;
                cloak3 = WorldMaskManager.Instance.cloak3;
                cloak4 = WorldMaskManager.Instance.cloak4;
                cloak5 = WorldMaskManager.Instance.cloak5;
                cloak6 = WorldMaskManager.Instance.cloak6;
                break;
            case WorldType.City:
                cloak1 = WorldMaskManager.Instance.cloakA;
                cloak2 = WorldMaskManager.Instance.cloakB;
                cloak3 = WorldMaskManager.Instance.cloakC;
                cloak4 = WorldMaskManager.Instance.cloakD;
                cloak3 = WorldMaskManager.Instance.cloakE;
                cloak4 = WorldMaskManager.Instance.cloakF;
                break;
        }

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

        switch (worldType)
        {
            case WorldType.Forest:
                target = WorldMaskManager.Instance.target1;
                target2 = WorldMaskManager.Instance.target2;
                target3 = WorldMaskManager.Instance.target3;
                target4 = WorldMaskManager.Instance.target4;
                target5 = WorldMaskManager.Instance.target5;
                target6 = WorldMaskManager.Instance.target6;
                break;
            case WorldType.City:
                target = WorldMaskManager.Instance.targetA;
                target2 = WorldMaskManager.Instance.targetB;
                target3 = WorldMaskManager.Instance.targetC;
                target4 = WorldMaskManager.Instance.targetD;
                target5 = WorldMaskManager.Instance.targetE;
                target6 = WorldMaskManager.Instance.targetF;
                break;
        }

    }

}
