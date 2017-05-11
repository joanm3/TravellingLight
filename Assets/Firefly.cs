using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firefly : MonoBehaviour
{

    [SerializeField]
    private bool isEquipped = false;
    public int index = 0;
    [Range(0, 1)]
    public float cloak = 0f;
    public bool useGlobalDistance = true;
    public float changeDistance = 10f;
    public SetColorMaterial colorSetter;
    public Vector3 startPosition;
    public Transform targetTransform;
    public ParticleSystem particles;

    public bool isInSphere = false;

    //[SerializeField]
    //private float selectedScale = 0.25f;
    //[SerializeField]
    //private float velocityToChange = 1f;
    public float startingScale;

    [SerializeField]
    private float followSpeed = 1f;
    public float distanceFromTarget = 2f;
    [SerializeField]
    private bool isFollowingCharacter = false;
    [SerializeField]
    private float rotationAngleForce = 5f;
    [SerializeField]
    private bool desactivateWhenFollowing = true;
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

    void Update()
    {
        if (useGlobalDistance) changeDistance = WorldMaskManager.Instance.worldMaskGlobalVariables.GlobalChangeDistance;
    }

    public bool IsFollowingCharacter
    {
        get { return isEquipped; }
        set
        {
            isEquipped = value;
            OnIsEquippedChanged(isEquipped);
        }
    }

    void OnIsEquippedChanged(bool following)
    {
        if (following)
        {
            lightIntegrator.AssignedFireflies.Add(this);
            //SinusMovement[] sinMoves = GetComponentsInChildren<SinusMovement>(); 
            //if(sinMoves.Length > 0)
            //{
            //    foreach (SinusMovement item in sinMoves)
            //    {
            //        item.move = false; 
            //    }
            //}
            colorMat.IsActive = false;
            isInSphere = false;
        }
        else
        {
            //SinusMovement[] sinMoves = GetComponentsInChildren<SinusMovement>();
            //if (sinMoves.Length > 0)
            //{
            //    foreach (SinusMovement item in sinMoves)
            //    {
            //        item.move = false;
            //    }
            //}
            lightIntegrator.AssignedFireflies.Remove(this);
            colorMat.IsActive = true;
            isInSphere = true;
        }
        colorMat.IsLightEnabled = !following;
    }

}
