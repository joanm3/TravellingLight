using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AssignToCharacter : MonoBehaviour
{

    public bool isInSphere = false;

    [SerializeField]
    private float selectedScale = 0.25f;
    [SerializeField]
    private float velocityToChange = 1f;
    [SerializeField]
    private float followSpeed = 1f;
    public float distanceFromTarget = 2f;
    [SerializeField]
    private bool isFollowingCharacter = false;
    [SerializeField]
    private float rotationAngleForce = 5f;
    [SerializeField]
    private bool desactivateWhenFollowing = true;

    public bool IsFollowingCharacter
    {
        get { return isFollowingCharacter; }
        set { isFollowingCharacter = value; OnIsFollowingCharacterChanged(isFollowingCharacter); }
    }

    private SetColorMaterial colorMat;
    private GameObject player;
    private LightIntegrator lightIntegrator;

    void Start()
    {
        colorMat = GetComponentInChildren<SetColorMaterial>();
        if (colorMat == null)
        {
            Debug.LogError("color material null for " + this.name);
            Destroy(this);
        }
        player = GameObject.FindGameObjectWithTag("Player");
        lightIntegrator = player.GetComponentInChildren<LightIntegrator>();
    }


    void Update()
    {
        if (IsFollowingCharacter)
        {
            if (DistantFromTarget())
            {
                float step = followSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
            }
            else
            {
                transform.RotateAround(player.transform.position, player.transform.up, rotationAngleForce * Time.deltaTime);
            }
            //make scale small
            if (transform.localScale.x > selectedScale)
            {
                transform.localScale = new Vector3(
                    transform.localScale.x - (velocityToChange * Time.deltaTime),
                    transform.localScale.y - (velocityToChange * Time.deltaTime),
                    transform.localScale.z - (velocityToChange * Time.deltaTime));
            }
            else if (transform.localScale.x < selectedScale)
            {
                transform.localScale = new Vector3(selectedScale, selectedScale, selectedScale);
            }
        }
        else
        {
            //make scale 1
            if (transform.localScale.x < 1f)
            {
                transform.localScale = new Vector3(
                    transform.localScale.x + (velocityToChange * Time.deltaTime),
                    transform.localScale.y + (velocityToChange * Time.deltaTime),
                    transform.localScale.z + (velocityToChange * Time.deltaTime));
            }
            else if (transform.localScale.x < selectedScale)
            {
                transform.localScale = Vector3.one;
            }
        }

        if (isInSphere && InputManager.Instance.select.IsCurrentEvent())
        {
            //Debug.Log("integrating the cube");
            IsFollowingCharacter = true;

        }
    }


    void OnIsFollowingCharacterChanged(bool following)
    {
        if (following)
        {
            lightIntegrator.LightFollowers.Add(this);
            colorMat.IsActive = false;
            isInSphere = false;
        }
        else
        {
            colorMat.IsActive = true;
            lightIntegrator.LightFollowers.Remove(this);
            isInSphere = true;
        }
        colorMat.IsLightEnabled = !following;
    }

    bool DistantFromTarget()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > distanceFromTarget)
            return true;
        return false;
    }
}
