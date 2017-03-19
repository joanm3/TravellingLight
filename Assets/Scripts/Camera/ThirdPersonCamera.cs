using ProjectLight.Functions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct CameraPosition
{
    //position to align camera to, probable somewhere behind the character
    //or position to point camera at, probable somewhere along characters axis
    private Vector3 position;
    //transform used for any rotation
    private Transform xForm;
    public Vector3 Position { get { return position; } set { position = value; } }
    public Transform XForm { get { return xForm; } set { xForm = value; } }


    public void Init(string camName, Vector3 pos, Transform transform, Transform parent)
    {
        position = pos;
        xForm = transform;
        xForm.name = camName;
        xForm.parent = parent;
        xForm.localPosition = Vector3.zero;
        xForm.localPosition = position;
    }
}


//[RequireComponent(typeof(BarsEffect))]
public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField]
    private CamMode cameraMode = CamMode.ThirdPersonOrbit;
    [SerializeField]
    private Transform character;
    [SerializeField]
    private CharacterMotion characterMotion;
    [SerializeField]
    private Transform secondPersonCameraPosition;
    [SerializeField]
    private Transform parentRig;
    [SerializeField]
    private bool inverseCameraX = false;
    [SerializeField]
    private bool inverseCameraY = false;
    [SerializeField]
    [Range(0.1f, 1.5f)]
    private float mouseSensibility = 0.5f;
    [SerializeField]
    private float distanceAwayDefault;
    [SerializeField]
    private float distanceUpDefault;
    [SerializeField]
    private Vector2 distanceAwayMinMax;
    [SerializeField]
    private Vector2 distanceUpMinMax;
    [SerializeField]
    private AnimationCurve distanceUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float distanceAway;
    private float distanceUp;


    [SerializeField]
    private float camSmoothDampTime = 0.1f;
    [SerializeField]
    private Transform firstPersonCameraPosition;
    [SerializeField]
    [Range(0, 1)]
    private float firstPersonThreshold = 0.3f;
    [SerializeField]
    float fpLookSpeed = 1f;
    [SerializeField]
    [Tooltip("x=xmin, y=xmax, z=ymin, w=ymax")]
    private Vector4 fpsXYminAndMaxClampAngles;


    [SerializeField]
    private float movementThreshold = 1f;
    [SerializeField]
    private float lookDirDampTime = 0.2f;
    [SerializeField]
    private float lookDirFactorRotation = 1f;
    [SerializeField]
    private float lookAtSmoothFactor = 3f;

    [SerializeField]
    [Range(0, 1)]
    private float distanceSensibility = 0.5f;
    [SerializeField]
    private float freeThreshold = 0.1f;
    [SerializeField]
    private Vector2 camMinDistFromChar = new Vector2(1f, -0.5f);
    [SerializeField]
    private float rightStickThreshold = 0.1f;
    [SerializeField]
    private const float freeRotationDegreePerSecond = -5f;

    //private global only
    private Vector3 lookDir;
    private Vector3 targetPosition;
    private Vector3 velocityCamSmooth = Vector3.zero;
    private CameraPosition firstPersonCamPos = new CameraPosition();

    float fpXRot = 0f;
    float fpYRot = 0f;
    private float fpStartingXRot = 0f;
    private Vector3 gizmoPosition;
    private Vector3 characterForward;
    private Vector3 characterUp;
    private float distanceStartWhenGoingToFPS;
    private float xAxisRot;
    private float lookWeight;
    private Vector3 curLookDir;
    private Vector3 velocityLookDir;
    private Vector3 savedRigToGoal;
    private Vector2 rightStickPrevFrame = Vector2.zero;
    float firstDistanceUp;
    float firstDistanceAway;
    float distanceUpNormalized;
    float distanceUpNormalizedCurve;
    float distanceAwayNormalized;
    Vector3 lookAt;
    float smoothLookAtPosition;


    private const float TARGETING_THRESHOLD = 0.1f;

    public float angleTest = 10f;

    #region Properites (Public)
    public enum CamMode
    {
        ThirdPersonOrbit, ThirdPersonTarget, ThirdPersonFree, FirstPerson, SecondPerson
    };

    public CamMode CameraMode { get { return cameraMode; } }

    #endregion


    #region Unity Methods
    void Start()
    {
        character = GameObject.FindGameObjectWithTag("Player").transform;
        lookDir = character.forward;
        curLookDir = character.forward;
        characterMotion = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterMotion>();
        firstPersonCamPos = new CameraPosition();
        firstPersonCamPos.Init("First Person Camera", firstPersonCameraPosition.localPosition, firstPersonCameraPosition, character);
        distanceAway = distanceAwayDefault;
        distanceUp = distanceUpDefault;
        firstDistanceUp = distanceUpDefault;
        firstDistanceAway = distanceAwayDefault;
        distanceUpNormalized = JFunctions.MapRange(firstDistanceUp, distanceUpMinMax.x, distanceUpMinMax.y, 0f, 1f);
        distanceAwayNormalized = JFunctions.MapRange(firstDistanceAway, distanceAwayMinMax.x, distanceAwayMinMax.y, 0f, 1f);
        smoothLookAtPosition = distanceUp;

        Vector3 _offset = new Vector3(0f, distanceUp, 0f);
        Vector3 characterOffset = character.position + _offset;
        lookAt = characterOffset;
    }




    void LateUpdate()
    {

        float leftArrowY = Input.GetAxis("FirstPerson");
        float rightX = (!inverseCameraX) ? -Input.GetAxis("Mouse X") * mouseSensibility : Input.GetAxis("Mouse X") * mouseSensibility;
        float rightY = (!inverseCameraY) ? Input.GetAxis("Mouse Y") * mouseSensibility : -Input.GetAxis("Mouse Y") * mouseSensibility;

        //IT IS DOING DISGUSTING STUFF SMOOOTH IT!!!! 
        smoothLookAtPosition = Mathf.Lerp(smoothLookAtPosition, distanceUp, Time.deltaTime * lookAtSmoothFactor);

        Vector3 _offset = new Vector3(0f, smoothLookAtPosition, 0f);
        //Vector3 _offset = new Vector3(0f, distanceUpDefault, 0f);
        Vector3 characterOffset = character.position + _offset;
        lookAt = characterOffset;
        //lookAt = SmoothPosition(lookAt, characterOffset);
        gizmoPosition = characterOffset;
        Vector3 targetPosition = Vector3.zero;

        bool test = false;
        if (!test)
        {
            #region Assign Camera
            if (Input.GetAxis("Target") > TARGETING_THRESHOLD && cameraMode != CamMode.FirstPerson)
            {
                cameraMode = CamMode.ThirdPersonTarget;
            }
            else
            {
                if (cameraMode == CamMode.ThirdPersonTarget)
                    cameraMode = CamMode.ThirdPersonOrbit;

                //first person case
                if (Input.GetButtonDown("FirstPerson")) //&& cameraMode != CamMode.ThirdPersonFree)// && characterMotion.Speed < 0.2f)
                {
                    if (cameraMode != CamMode.FirstPerson)
                    {
                        fpStartingXRot = UpdateAngleInDeg(firstPersonCameraPosition.forward, Vector3.forward);
                        //fpXRot = fpStartingXRot;
                        fpXRot = 0f;
                        distanceStartWhenGoingToFPS = Vector3.Distance(this.transform.position, firstPersonCamPos.XForm.position);
                        fpYRot = 0f;
                        cameraMode = CamMode.FirstPerson;
                    }
                    else
                    {
                        cameraMode = CamMode.ThirdPersonOrbit;
                    }
                }

                if (Input.GetButtonDown("SecondPerson") && cameraMode != CamMode.FirstPerson) //&& cameraMode != CamMode.ThirdPersonFree)// && characterMotion.Speed < 0.2f)
                {
                    if (cameraMode != CamMode.SecondPerson)
                    {
                        cameraMode = CamMode.SecondPerson;
                    }
                    else
                    {
                        cameraMode = CamMode.ThirdPersonOrbit;
                    }
                }

                //free camera case
                if ((Mathf.Abs(rightY) > freeThreshold || Mathf.Abs(rightX) > freeThreshold) && cameraMode == CamMode.ThirdPersonOrbit) // && System.Math.Round(characterMotion.Speed, 2) == 0)
                {
                    cameraMode = CamMode.ThirdPersonFree;
                    savedRigToGoal = Vector3.zero;
                }

            }
            #endregion
        }

        Vector3 rigToGoalDirection = Vector3.Normalize(characterOffset - this.transform.position);
        rigToGoalDirection.y = 0f;


        //distance camera
        //Up
        distanceUpNormalized += rightY * distanceSensibility;
        distanceUpNormalized = Mathf.Clamp(distanceUpNormalized, 0f, 1f);
        distanceUpNormalizedCurve = distanceUpCurve.Evaluate(distanceUpNormalized);
        firstDistanceUp = JFunctions.MapRange(distanceUpNormalizedCurve, 0f, 1f, distanceUpMinMax.x, distanceUpMinMax.y);
        distanceUp = firstDistanceUp;
        //Away
        distanceAwayNormalized += rightY * distanceSensibility;
        distanceAwayNormalized = Mathf.Clamp(distanceAwayNormalized, 0f, 1f);
        firstDistanceAway = JFunctions.MapRange(distanceAwayNormalized, 0f, 1f, distanceAwayMinMax.x, distanceAwayMinMax.y);
        distanceAway = firstDistanceAway;
        //Debug.LogFormat("distance: {0}, {1}", distanceAway, distanceUp);
        //Debug.LogFormat("distance normalized: {0}, {1}", distanceAwayNormalized, distanceUpNormalizedCurve);

        switch (cameraMode)
        {
            #region Orbit
            case CamMode.ThirdPersonOrbit:
                ResetCamera();
                //characterMotion.characterMovementType = CharacterMotion.CharacterMovementType.Relative;

                if (characterMotion.Speed > movementThreshold)
                {
                    //all this does that the character tends to look to the side we are facing. 
                    lookDir = Vector3.Lerp(character.right * (rightX < 0 ? 1f : -1f) * lookDirFactorRotation, character.forward * (rightY < 0 ? -1f : 1f) * lookDirFactorRotation,
                        Mathf.Abs(Vector3.Dot(this.transform.forward, character.forward)));
                    curLookDir = Vector3.Normalize(characterOffset - this.transform.position);
                    curLookDir.y = 0f;
                    curLookDir = Vector3.SmoothDamp(curLookDir, lookDir, ref velocityLookDir, lookDirDampTime);

                }
                targetPosition = characterOffset + character.up * distanceUp - Vector3.Normalize(curLookDir) * distanceAway;

                //not yet used!
                Vector2 rotatedAngle = RotateVectorWithAngle(characterForward.x, characterForward.z, angleTest);
                Vector3 finalAngle = new Vector3(rotatedAngle.x, 0f, rotatedAngle.y);
                Debug.DrawRay(characterMotion.transform.position, curLookDir * 10f, Color.red);
                // Debug.DrawRay(characterMotion.transform.position, finalAngle * 10f, Color.magenta);
                //Debug.Log("targetPosition: " + targetPosition.ToString());

                //targetPosition = characterOffset + character.up * distanceUp - Vector3.Normalize(finalAngle) * distanceAway;
                characterMotion.characterMovementType = CharacterMotion.CharacterMovementType.Relative;

                characterForward = character.forward;
                characterUp = character.up;
                //Debug.DrawLine(character.position, targetPosition, Color.magenta);
                break;
            #endregion

            #region Free
            case CamMode.ThirdPersonFree:
                Vector3 rigToGoal = characterOffset - parentRig.position;
                rigToGoal.y = 0f;
                Debug.DrawRay(parentRig.transform.position, rigToGoal, Color.red);

                parentRig.RotateAround(characterOffset, character.up, freeRotationDegreePerSecond * (Mathf.Abs(rightX) > rightStickThreshold ? rightX : 0f));

                if (rightX != 0 || rightY != 0)
                {
                    savedRigToGoal = rigToGoalDirection;
                }

                //if (targetPosition == Vector3.zero)
                //{
                //    targetPosition = characterOffset + character.up * distanceUpFree - savedRigToGoal * distanceAwayFree;
                //}


                targetPosition = characterOffset + character.up * distanceUp - savedRigToGoal * distanceAway;
                //Debug.Log("targetPosition: " + targetPosition.ToString());
                characterMotion.characterMovementType = CharacterMotion.CharacterMovementType.Relative;

                characterForward = character.forward;
                characterUp = character.up;

                this.transform.position = SmoothPosition(this.transform.position, targetPosition);
                transform.LookAt(lookAt);
                break;
            #endregion

            #region Target
            case CamMode.ThirdPersonTarget:
                //ResetCamera();
                characterMotion.characterMovementType = CharacterMotion.CharacterMovementType.NoInput;
                lookDir = character.forward;
                curLookDir = character.forward;

                // targetPosition = characterOffset + characterUp * distanceUp - characterForward * distanceAway;
                distanceAwayNormalized = JFunctions.MapRange(distanceAwayDefault, distanceAwayMinMax.x, distanceAwayMinMax.y, 0f, 1f);
                distanceUpNormalized = JFunctions.MapRange(distanceUpDefault, distanceUpMinMax.x, distanceUpMinMax.y, 0f, 1f);

                //just to test.
                //you should not lose it till the character positions itself. 
                //only update when entering in this state
                //LOOK THIS! LOOK THIS!
                //if (Vector3.Distance(characterForward, character.forward) > 0.1f)
                //{
                //    characterForward = character.forward;
                //}


                //targetPosition = characterOffset + characterUp * distanceUp - rigToGoalDirection * distanceAway;
                targetPosition = characterOffset + characterUp * distanceUp - characterForward * distanceAway;
                characterMotion.characterMovementType = CharacterMotion.CharacterMovementType.Relative;
                break;
            #endregion

            #region First Person
            case CamMode.FirstPerson:
                //clamp this to a max.
                //ResetCamera();


                fpXRot += (!inverseCameraX) ? rightX * -fpLookSpeed : rightX * fpLookSpeed;
                fpYRot += rightY * -fpLookSpeed;
                fpXRot = Mathf.Clamp(fpXRot, fpsXYminAndMaxClampAngles.x, fpsXYminAndMaxClampAngles.y);
                fpYRot = Mathf.Clamp(fpYRot, -fpsXYminAndMaxClampAngles.z, -fpsXYminAndMaxClampAngles.w);
                firstPersonCamPos.XForm.localRotation = Quaternion.Euler(fpYRot, fpXRot, 0f);
                Quaternion rotationShift = Quaternion.FromToRotation(this.transform.forward, firstPersonCamPos.XForm.forward);
                this.transform.rotation = rotationShift * this.transform.rotation;
                targetPosition = firstPersonCamPos.XForm.position;
                float _distance = Vector3.Distance(this.transform.position, firstPersonCamPos.XForm.position);
                float _goodDistance = (_distance > 0.1) ? Functions.NormalizeRange(_distance, 0f, distanceStartWhenGoingToFPS) : 0f;
                lookAt = (Vector3.Lerp(this.transform.position + this.transform.forward, lookAt, _goodDistance));

                //later we can do that the character rotates with this, but not important now. 

                characterMotion.characterMovementType = CharacterMotion.CharacterMovementType.NoInput;
                break;
            #endregion

            #region SecondPerson
            case CamMode.SecondPerson:
                if (secondPersonCameraPosition != null)
                {
                    //change the time to go to the point or add also a bool to go automatically to that point without lerp. 
                    targetPosition = secondPersonCameraPosition.position;
                }
                else
                {
                    Debug.LogError("No staticCameraPosition assigned", this);
                    cameraMode = CamMode.ThirdPersonOrbit;
                }
                break;
                #endregion

        }

        if (cameraMode != CamMode.ThirdPersonFree)
        {
            RaycastHit wallHit = new RaycastHit();
            CompensateForWalls(characterOffset, ref targetPosition, out wallHit);
            this.transform.position = SmoothPosition(this.transform.position, targetPosition);
            transform.LookAt(lookAt);
        }

        rightStickPrevFrame = new Vector2(rightX, rightY);

    }

    #endregion

    #region Methods

    private Vector3 SmoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        //improve the damptime to be able to change between cameras differently
        return Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget, out RaycastHit wallHit)
    {
        //correct this to not see outside. or add your old code. 
        Debug.DrawLine(fromObject, toTarget, Color.cyan);

        if (Physics.Linecast(fromObject, toTarget, out wallHit))
        {
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
            toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);
        }
    }

    private float UpdateAngleInDeg(Vector3 direction, Vector3 worldVector)
    {
        float _angle =
            Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(worldVector, direction)),
            Vector3.Dot(worldVector, direction)) * Mathf.Rad2Deg;

        //Debug.Log(_angle);

        return _angle;
    }

    private void ResetCamera()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
    }

    private static Vector2 RotateVectorWithAngle(float x, float y, float degrees)
    {
        Vector2 result = new Vector3();
        result.x = x * Mathf.Cos(degrees * Mathf.Deg2Rad) - y * Mathf.Sin(degrees * Mathf.Deg2Rad);
        result.y = x * Mathf.Sin(degrees * Mathf.Deg2Rad) + y * Mathf.Cos(degrees * Mathf.Deg2Rad);
        return result;

    }

    #endregion
}


public static class JFunctions
{
    //public static float MappedRangeValue(float value, float oldMin, float oldMax, float newMin, float newMax)
    //{
    //    float oldRange = oldMax - oldMin;
    //    float newRange = newMax - newMin;
    //    return value - oldMin * newRange / oldRange + newMin;
    //}

    //public static float NormalizeRangeValue(float value, float oldMin, float oldMax)
    //{
    //    float oldRange = oldMax - oldMin;
    //    return value - oldMin / oldRange;
    //}

    public static float MapRange(float x, float inMin, float inMax, float outMin, float outMax)
    {
        return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }


}
