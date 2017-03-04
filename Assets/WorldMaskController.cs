using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaskController : MonoBehaviour
{
    public Shader maskShader;
    public Transform target;
    public float changeDistance = 10f;
    [Range(0, 1)]
    public float cloak;
    public bool waitTimeToChange = true;
    public float timeToCheck = 1f;
    public bool updateTarget = true;

    private Material maskMaterial;
    private float timeLeft = 1f;

    // Use this for initialization
    void Start()
    {
        if (!maskShader)
        {
            Debug.LogError("no maskShader assigned for " + name);
            Destroy(this);
        }

        maskMaterial = GetComponent<MeshRenderer>().material;
        if (!maskMaterial.shader.Equals(maskShader))
        {
            Debug.LogError("shader is not correct for " + name + ", please assign " + maskShader.name);
            Destroy(this);
        }
        if (target == null)
            target = GameObject.FindGameObjectWithTag("WorldTarget").transform;

        UpdateUniforms();
        UpdateTarget();
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
                UpdateTarget();
                timeLeft = Mathf.Abs(timeToCheck);
            }
        }
        else
        {
            UpdateTarget();
        }
    }

    // Update is called once per frame
    void UpdateUniforms()
    {
        maskMaterial.SetFloat("_Cloak", cloak);
        maskMaterial.SetFloat("_ChangePoint", changeDistance);
        maskMaterial.SetVector("_TargetPosition", target.position);
    }
    void UpdateTarget()
    {
        //try to find a way where i dont have to loop at every frame (start a coroutine or wait for time better at least). 
        if (WorldMaskManager.Instance != null)
        {
            if (WorldMaskManager.Instance.targets.Count > 0)
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
}
