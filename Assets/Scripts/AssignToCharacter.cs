using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AssignToCharacter : MonoBehaviour
{

    public bool isInSphere = false;

    //[SerializeField]
    //private float selectedScale = 0.25f;
    //[SerializeField]
    //private float velocityToChange = 1f;
    [SerializeField]
    private float followSpeed = 1f;
    public float distanceFromTarget = 2f;
    [SerializeField]
    private bool isFollowingCharacter = false;
    [SerializeField]
    private float rotationAngleForce = 5f;
    [SerializeField]
    private bool desactivateWhenFollowing = true;

    public float startingScale;

    public bool IsFollowingCharacter
    {
        get { return isFollowingCharacter; }
        set
        {
            isFollowingCharacter = value;
            OnIsFollowingCharacterChanged(isFollowingCharacter);
        }
    }

    private SetColorMaterial colorMat;
    private GameObject player;
    private LightIntegrator lightIntegrator;

    void Start()
    {
        startingScale = transform.localScale.x;
        colorMat = GetComponentInChildren<SetColorMaterial>();
        if (colorMat == null)
        {
            Debug.LogError("color material null for " + this.name);
            Destroy(this);
        }
        player = GameObject.FindGameObjectWithTag("Player");
        lightIntegrator = player.GetComponentInChildren<LightIntegrator>();
    }


    void OnIsFollowingCharacterChanged(bool following)
    {
        if (following)
        {
            lightIntegrator.AssignedFireflies.Add(this);
            colorMat.IsActive = false;
            isInSphere = false;
        }
        else
        {
            lightIntegrator.AssignedFireflies.Remove(this);
            colorMat.IsActive = true;
            isInSphere = true;
        }
        colorMat.IsLightEnabled = !following;
    }


}
