using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaskController : MonoBehaviour
{
    public Shader maskShader;
    public float changeDistance = 10f;
    [Range(0, 1)]
    public float cloak;
    public bool waitTimeToChange = true;
    public float timeToCheck = 1f;
    public bool updateTarget = true;

    private Material maskMaterial;
    private float timeLeft = 1f;
    public Transform target;
    public Transform target2;
    public Transform target3;
    public Transform target4;

    // Use this for initialization
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

        maskMaterial = GetComponent<MeshRenderer>().material;
        if (!maskMaterial.shader.Equals(maskShader))
        {
            Debug.LogError("shader is not correct for " + name + ", please assign " + maskShader.name);
            Destroy(this);
        }
        //target = GameObject.FindGameObjectWithTag("WorldTarget").transform;
        //target2 = target;
        //target3 = target;
        //target4 = target;

        UpdateUniforms();
        //UpdateTargets();
        // Debug.Log(target);
    }


    void Update()
    {
        UpdateUniforms();

        if (!updateTarget)
            return;

        if (waitTimeToChange)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                UpdateTargets();
                timeLeft = Mathf.Abs(timeToCheck);
            }
        }
        else
        {
            UpdateTargets();
        }
    }

    // Update is called once per frame
    void UpdateUniforms()
    {
        maskMaterial.SetFloat("_Cloak", cloak);
        maskMaterial.SetFloat("_ChangePoint", changeDistance);
        maskMaterial.SetVector("_TargetPosition", target.position);
        maskMaterial.SetVector("_TargetPos2", target2.position);
        maskMaterial.SetVector("_TargetPos3", target3.position);
        maskMaterial.SetVector("_TargetPos4", target4.position);

    }
    void UpdateTargets()
    {
        //try to find a way where i dont have to loop at every frame (start a coroutine or wait for time better at least). 
        if (WorldMaskManager.Instance != null)
        {
            UpdateTarget(ref target, 1);
            UpdateTarget(ref target2, 2);
            UpdateTarget(ref target3, 3);
            UpdateTarget(ref target4, 4);

        }
    }


    void UpdateTarget(ref Transform target, int index)
    {
        if (WorldMaskManager.Instance.targets.Count > index - 1)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            for (int i = 0; i < WorldMaskManager.Instance.targets.Count; i++)
            {
                if (Vector3.Distance(transform.position, WorldMaskManager.Instance.targets[i].transform.position) < distance)
                {
                    target = WorldMaskManager.Instance.targets[i].transform;
                    distance = Vector3.Distance(transform.position, WorldMaskManager.Instance.targets[i].transform.position);
                }
            }
        }
    }

}
