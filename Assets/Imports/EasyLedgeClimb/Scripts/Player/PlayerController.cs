/// Easy Ledge Climb Character System
/// PlayerController.cs
///
/// As long as the player has a CharacterController or Rigidbody component, this script allows the player to:
/// 1. Move and rotate (Movement).
/// 2. Slide down slopes (Movement).
/// 3. Perform any amount of jumps with different heights and animations (Jumping).
/// 4. Perform a double jump (Jumping).
/// 5. Wall jump (Wall Jumping).
/// 6. Perform any amount of attacks with different strengths and animations (Attacking).
/// 7. Climb ladders and walls (Climbing).
/// 8. Ride moving and rotating platforms (Moving Platforms).
///
/// NOTE: *You should always set a layer for your player so that you can disable collisions with that layer (by unchecking it in the script's Collision Layers).
///	If you do not, the raycasts and linecasts will collide with the player himself and keep the script from working properly!*
///
/// (C) 2015-2016 Grant Marrs

using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	
	public Transform playerCamera; //the camera set to follow the player
	public float gravity = 20.00f; //the amount of downward force, or "gravity," that is constantly being applied to the player
	public float slopeLimit = 25.00f; //the maximum angle of a slope you can stand on without sliding down
	
	//Grounded
	[System.Serializable]
	public class Grounded {
		public bool showGroundDetectionRays; //shows the rays that detect whether the player is grounded or not
		public float maxGroundedHeight = 0.2f; //the maximum height of the ground the ground detectors can hit to be considered grounded
		public float maxGroundedRadius = 0.2f; //the maximum radius of the area ground detectors can hit to be considered grounded
		public float maxGroundedDistance = 0.2f; //the maximum distance you can be from the ground to be considered grounded
		public bool currentlyGrounded; //determines if player is currently grounded/on the ground
	}
	
	//Movement
	[System.Serializable]
	public class Movement {
		public float forwardSpeed = 6.0f; //player's speed when running forward
		public float sideSpeed = 4.0f; //player's speed when running sideways
		public float backSpeed = 5.0f; //player's speed when running backwards
		[System.Serializable]
		public class Running {
			public bool useRunningButton = false; //allows the player to multiply his movement speed when the run button is pressed
			public string runInputButton = "Fire3"; //the button (found in "Edit > Project Settings > Input") that is used to make the player run
			public float runSpeedMultiple = 1.3f; //player's movement speed while the player is running/the run button is held down (multiplied by move speed)
		}
		public Running running = new Running(); //variables that determine whether or not the player uses a running button to run
		[System.Serializable]
		public class Crouching {
			public bool allowCrouching = true; //determines whether or not the player is allowed to crouch
			public float crouchMovementSpeedMultiple = 0.4f; //player's movement speed while crouching (multiplied by move speed)
			public float crouchColliderHeightMultiple = 0.7f; //what to multiply the player's collider height while crouching by
		}
		public Crouching crouching = new Crouching(); //variables that determine whether or not the player can crouch
		public float midAirMovementSpeedMultiple = 1.1f; //player's movement speed in mid-air (multiplied by move speed)
		public float acceleration = 50; //how fast the player will reach their maximum speed
		public float movementFriction = 0; //the amount of friction applied to the player's movement
		public float rotationSpeed = 8; //player's rotation speed
		public float midAirRotationSpeedMultiple = 1; //player's rotation speed in mid-air (multiplied by rotationSpeed)
		public float slopeSlideSpeed = 1; //how quickly you slide down slopes
		public float slideFriction = 4; //the amount of friction applied to the player from sliding down a slope
		public bool hardStickToGround = false; //by using a raycast, this option sets the position of the player to the position of the ground under him
		
		[System.Serializable]
		public class SideScrolling {
			public float movementSpeedIfAxisLocked = 6.0f; //the move speed of the player if one of the axis are locked
			public bool lockMovementOnZAxis = false; //locks the movement of the player on the z-axis
			public float zValue = 0; //the permanent z-value of the player if his movement on the z-axis is locked
			public bool lockMovementOnXAxis = false; //locks the movement of the player on the x-axis
			public float xValue = 0; //the permanent x-value of the player if his movement on the x-axis is locked
			public bool flipAxisRotation = false; //flips the player's rotation on the non-locked axis (it adds 180 degrees to the player's rotation)
			public bool rotateInwards = true; //when the player rotates from side to side, he rotates inward (so that you see his front side while he is rotating)
		}
		public SideScrolling sideScrolling = new SideScrolling(); //variables that determine whether or not the player uses 2.5D side-scrolling
		
		[System.Serializable]
		public class FirstPerson {
			public bool useCameraControllerSettingsIfPossible = true; //if the player camera has the script: "CameraController.cs" attached to it, the player will use the same first person settings as the camera
			public bool alwaysUseFirstPerson = false; //allows the player to always stay in first person mode
			public bool switchToFirstPersonIfInputButtonPressed = false; //switches to first person mode and back when the "firstPersonInputButton" is pressed
			public string firstPersonInputButton = "Fire3"; //the button (found in "Edit > Project Settings > Input") that is used to enter first person mode
			public bool startOffInFirstPersonModeForSwitching = false; //if the player is allowed to switch to first person mode, start off in first person mode instead of having to switch to it first
			public bool walkBackwardsWhenDownKeyIsPressed = true; //allows the player to walk backwards (instead of turn around) when the down key is pressed
			public bool onlyRotateWithCamera = true; //does not allow the arrow keys to change the player's direction; only allows the player to rotate to the direction that the camera is facing
		}
		public FirstPerson firstPerson = new FirstPerson(); //variables that determine whether or not the player uses first person mode
		
	}
	
	//Jumping
	[System.Serializable]
	public class Jumping {
		public float [] numberAndHeightOfJumps = {6, 8, 12}; //the number of jumps the player can perform and the height of the jumps (the elements)
		public float timeLimitBetweenJumps = 1; //the amount of time you have between each jump to continue the jump combo
		public bool allowJumpWhenSlidingFacingUphill = false; //determines whether or not you are allowed to jump when you are facing uphill and sliding down a slope
		public bool allowJumpWhenSlidingFacingDownhill = true; //determines whether or not you are allowed to jump when you are facing downhill and sliding down a slope
		public bool doNotIncreaseJumpNumberWhenSliding = true; //only allows the player to perform their first jump when sliding down a slope
		public GameObject jumpLandingEffect; //optional dust effect to appear after landing jump
		public bool allowDoubleJump = true; //determines whether or not you are allowed to double jump
		public bool doubleJumpPerformableOutOfWallJump = true; //(if allowDoubleJump is true) determines whether or not the player can perform their double jump if they are in mid-air as a result of wall jumping
		public bool doubleJumpPerformableIfInMidAirInGeneral = true; //(if allowDoubleJump is true) determines whether or not the player can perform their double jump simply because they are in mid-air (instead of having to be in mid-air as a result of jumping)
		public float doubleJumpHeight = 6; //height of double jump
		public GameObject doubleJumpEffect; //optional effect to appear when performing a double jump
		public float maxFallingSpeed = 90; //the maximum speed you can fall
	}
	
	//Wall Jumping
	[System.Serializable]
	public class WallJumping {
		public bool allowWallJumping = true; //determines whether or not you are allowed to wall jump
		public float minimumWallAngle = 80; //the minimum angle a wall can be to wall jump off of it
		public float wallJumpDistance = 6; //distance of wall jump
		public float wallJumpHeight = 10; //height of wall jump
		public float wallJumpDecelerationRate = 2; //how quickly the momentum from the wall jump stops
		public float overallMovementSpeed = 2; //player's movement speed in mid-air
		public float forwardMovementSpeedMultiple = 0.85f; //player's speed when moving forward in mid air (multiplied by overallMovementSpeed)
		public float sideMovementSpeedMultiple = 0.85f; //player's speed when moving sideways in mid air (multiplied by overallMovementSpeed)
		public float backMovementSpeedMultiple = 0.75f; //player's speed when moving backwards in mid air (multiplied by overallMovementSpeed)
		public float rotationSpeedMultiple = 0; //player's rotation speed in mid-air (multiplied by rotationSpeed)
		public float distanceToKeepFromWallWhenOnWall = 1; //the distance the player keeps from the wall he is currently stuck to
		public bool useWallJumpTimeLimit = true; //allows the use of a time limit to wall jump when on walls
		public float wallJumpTimeLimit = 2; //the amount of time you can stay on a wall before falling
		public bool slideDownWalls = true; //allows player to slide down if on a wall
		public float slideDownSpeed = 8; //the speed at which the player slides down walls
		public float rotationToWallSpeed = 6; //how quickly the player rotates onto a wall for a wall jump
		public float inputPercentageNeededToWallJump = 50; //the amount of input needed to be applied to the joystick or key in order to stick to a wall for a wall jump
		public bool showWallJumpDetectors = false; //determines whether to show or hide the detectors that allow wall jumping
		public float spaceOnWallNeededToWallJumpUpAmount = 0.0f; //moves the rays that detect the amount of open space on a wall up and down
		public float spaceOnWallNeededToWallJumpHeight = 0.0f; //changes the height of the rays that detect the amount of open space on a wall
		public float spaceOnWallNeededToWallJumpLength = 0.0f; //changes the length of the rays that detect the amount of open space on a wall
		public float spaceOnWallNeededToWallJumpWidth = 0.0f; //changes the width of the rays that detect the amount of open space on a wall
		public float spaceBelowNeededToWallJump = 0.0f; //changes the minimum distance from the ground you must be in order to wall jump
	}
	
	//Attacking
	[System.Serializable]
	public class Attacking {
		public float [] numberAndStrengthOfAttacks = {1, 1, 2}; //the number of attacks the player can perform and the strength of each one (the elements)
		public float waitingTimeBetweenAttacks = 0.2f; //the amount of time you have to wait (between each attack) before you can continue the attack combo
		public float timeLimitBetweenAttacks = 0.5f; //the amount of time you have to wait between each attack to continue the attack combo
		public float [] numberAndStrengthOfMidAirAttacks = {1}; //the number of maximum attacks the player can perform and the strength of each one (the elements)
		public bool onlyAllowMidAirAttackOnceInMidAir = true; //only allows the player to use his mid-air attack once while in the air
		public float waitingTimeBetweenMidAirAttacks = 0.2f; //the amount of time you have to wait (between each attack) before you can continue the mid-air attack combo
		public float timeLimitBetweenMidAirAttacks = 0.5f; //the amount of time you have to wait between each attack to continue the mid air attack combo
		public bool allowCrouchAttack = true; //allows the player to attack while crouching
		public float crouchAttackStrength = 1; //the strength of the player's crouch attack
		public float timeLimitBetweenCrouchAttacks = 0.5f; //the amount of time you have to wait between each attack to crouch attack again
		public string attackInputButton = "Fire1"; //the button (found in "Edit > Project Settings > Input") that is used to attack
	}
	
	//Climbing
	[System.Serializable]
	public class Climbing {
		
		public string climbableTag = "Ladder"; //the tag of a climbable object
		public bool climbVertically = true; //determines whether or not the player is allowed to climb vertically
		public bool climbHorizontally = false; //determines whether or not the player is allowed to climb horizontally
		public float climbMovementSpeed = 4; //how quickly the player climbs on walls
		public float climbRotationSpeed = 10; //how quickly the player rotates on walls
		public bool snapToCenterOfObject = true; //snaps the player to the middle (along the x and z-axis) of the climbable object (most useful for ladders)
		public bool moveInBursts = true; //move in bursts (while on a climbable object)
		public float burstLength = 1; //the amount of time a movement burst lasts
		public bool stayUpright = false; //determines whether or not the player can rotate up and down
		public float distanceToPushOffAfterLettingGo = 0.5f; //the distance the player pushes off of a ladder/wall after letting go
		public float rotationToClimbableObjectSpeed = 6; //how quickly the player rotates onto a wall to climb
		public bool showClimbingDetectors = false; //determines whether to show or hide the detectors that allow climbing
		public float climbingSurfaceDetectorsUpAmount = 0.0f; //moves the rays that detect the surface of a wall up and down
		public float climbingSurfaceDetectorsHeight = 0.0f; //changes the height of the rays that detect the surface of a wall
		public float climbingSurfaceDetectorsLength = 0.0f; //changes the length of the rays that detect the surface of a wall
		public bool showEdgeOfObjectDetctors = false; //determines whether or not to show the detectors that determine where the top and bottom of a climbable object is
		public float topNoSurfaceDetectorHeight = 0.0f; //the height of the detector that determines if there is no surface detected at the top of the climbable object, so that the player can pull up or stop before climbing any higher
		public float bottomNoSurfaceDetectorHeight = 0.0f; //the height of the detector that determines if there is no surface detected at the bottom of the climbable object, so that the player can drop off or stop before climbing any lower
		public float topAndBottomNoSurfaceDetectorsWidth = 0.0f; //the width of the detectors that determines if there is no surface detected at the top and bottom of the climbable object
		public float sideNoSurfaceDetectorsHeight = 0.0f; //the height of the detectors that determines if there is no surface detected at the sides of the climbable object
		public float sideNoSurfaceDetectorsWidth = 0.0f; //the width of the detectors that determines if there is no surface detected at the sides of the climbable object
		public bool stopAtSides = true; //keeps player from climbing any further sideways once he has reached the side
		public bool dropOffAtBottom = false; //allows player to drop off of a climbable object once he has reached the bottom
		public bool dropOffAtFloor = true; //allows player to drop off of a climbable object once he has reached the floor
		public bool pullUpAtTop = true; //allows player to pull up and over a climbable object once he has reached the top
		public float pullUpSpeed = 4; //the speed the player pulls up and over ledges once he has reached the top of a climbable object
		public bool showPullUpDetector = false; //determines whether or not to show the detector that determines where the player pulls up to
		public float pullUpLocationForward = 0.0f; //the forward distance of the detector that determines where the player pulls up to
		
		[System.Serializable]
		public class WalkingOffOfClimbableSurface {
			public bool allowGrabbingOnAfterWalkingOffLedge = true; //allows the player to grab on to a climbable surface under the ledge that he is walking off of
			public bool showWalkingOffLedgeRays = false; //shows the rays that detect if the player is walking off of a ledge
			public float spaceInFrontNeededToGrabBackOn = 0.0f; //the amount of space in front of the player needed to grab on to a climbable object under a ledge
			public float spaceBelowNeededToGrabBackOnHeight = 0.0f; //the height of the detectors that determine the amount of space below the player needed to grab on to a climbable object under a ledge
			public float spaceBelowNeededToGrabBackOnForward = 0.0f; //the forward distance of the detectors that determine the amount of space below the player needed to grab on to a climbable object under a ledge
			public float firstSideLedgeDetectorsHeight = 0.0f; //the height of the first set of detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float secondSideLedgeDetectorsHeight = 0.0f; //the height of the second set of detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float thirdSideLedgeDetectorsHeight = 0.0f; //the height of the third set of detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float sideLedgeDetectorsWidth = 0.0f; //the width of the detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float sideLedgeDetectorsLength = 0.0f; //the length of the detectors that determine if there are ledges to the side of the player keeping him from grabbing on
			public float grabBackOnLocationHeight = 0.0f; //the height of the detectors that determine where the player will grab on to
			public float grabBackOnLocationWidth = 0.0f; //the height of the detectors that determine where the player will grab on to
			public float grabBackOnLocationForward = 0.0f; //the forward distance of the detectors that determine where the player will grab on to
		}
		public WalkingOffOfClimbableSurface walkingOffOfClimbableSurface = new WalkingOffOfClimbableSurface(); //variables that detect whether the player has walked off a ledge and can grab on to a ladder
		
		public string[] scriptsToEnableOnGrab; //scripts to enable when the player grabs on to a wall (scripts disable when the player lets go of a wall)
		public string[] scriptsToDisableOnGrab; //scripts to disable when the player grabs on to a wall (scripts enable when the player lets go of a wall)
		
		public bool pushAgainstWallIfPlayerIsStuck = true; //if the script considers the player stuck, the player pushes himself away from the wall until he is free
		
	}
	
	[System.Serializable]
	public class MovingPlatforms {
		public bool allowMovingPlatformSupport = true; //determines whether or not the player can move with moving platforms
		public string movingPlatformTag = "Platform"; //the tag of the moving platform objects
	}
	
	public Grounded grounded = new Grounded(); //variables that detect whether the player is grounded or not
	public Movement movement = new Movement(); //variables that control the player's movement
	public Jumping jumping = new Jumping(); //variables that control the player's jumping
	public WallJumping wallJumping = new WallJumping(); //variables that control the player's wall jumping
	public Attacking attacking = new Attacking(); //variables that control the player's attacks
	public Climbing[] climbing = new Climbing[1]; //variables that control the player's ladder and wall climbing
	public MovingPlatforms movingPlatforms = new MovingPlatforms(); //variables that determine whether the player moves with moving platforms or not
	
	//Grounded variables without class name
	private Vector3 maxGroundedHeight2;
	private float maxGroundedRadius2;
	private float maxGroundedDistance2;
	private Vector3 maxGroundedDistanceDown;
	
	//Movement variables without class name
	private float forwardSpeed2;
	private float sideSpeed2;
	private float backSpeed2;
	private float midAirMovementSpeedMultiple2;
	private float acceleration2;
	private float rotationSpeed2;
	private float slopeSlideSpeed2;
	
	//Jumping variables without class name
	private float [] jumpsToPerform;
	private bool allowDoubleJump2;
	private bool doubleJumpPerformableIfInMidAirInGeneral2;
	private float doubleJumpHeight2;
	private float timeLimitBetweenJumps2;
	private float maxFallingSpeed2;
	private GameObject jumpLandingEffect2;
	private GameObject doubleJumpEffect2;
	
	//WallJumping variables without class name
	private Vector3 spaceOnWallNeededToWallJumpUpAmount2;
	private float spaceOnWallNeededToWallJumpHeight2;
	private float spaceOnWallNeededToWallJumpLength2;
	private float spaceOnWallNeededToWallJumpWidth2;
	private Vector3 spaceBelowNeededToWallJump2;
	
	//Attacking variables without class name
	[System.NonSerialized]
	public float [] attacksToPerform;
	private float timeLimitBetweenAttacks2;
	
	//Climbing variables without class name
	private Vector3 climbingSurfaceDetectorsUpAmount2;
	private float climbingSurfaceDetectorsHeight2;
	private float climbingSurfaceDetectorsLength2;
	private float distanceToPushOffAfterLettingGo2;
	private float rotationToClimbableObjectSpeed2;
	private bool climbHorizontally2;
	private bool climbVertically2;
	private float climbMovementSpeed2;
	private float climbRotationSpeed2;
	private bool moveInBursts;
	private float burstLength;
	[HideInInspector]
	public string climbableTag2;
	private bool stayUpright2;
	private bool snapToCenterOfObject2;
	private Vector3 bottomNoSurfaceDetectorHeight2;
	private Vector3 topNoSurfaceDetectorHeight2;
	private Vector3 topAndBottomNoSurfaceDetectorsWidth2;
	private float sideNoSurfaceDetectorsHeight2;
	private Vector3 sideNoSurfaceDetectorsWidth2;
	private float sideNoSurfaceDetectorsWidthTurnBack2;
	private bool stopAtSides2;
	private bool dropOffAtBottom2;
	private bool dropOffAtFloor2;
	private bool pullUpAtTop2;
	private float pullUpSpeed;
	private Vector3 pullUpLocationForward2;
	private bool pushAgainstWallIfPlayerIsStuck2;
	//walk off ledge detectors
	private bool allowGrabbingOnAfterWalkingOffLedge2;
	private Vector3 spaceInFrontNeededToGrabBackOn2;
	private Vector3 spaceBelowNeededToGrabBackOnHeight2;
	private Vector3 spaceBelowNeededToGrabBackOnForward2;
	private Vector3 firstSideLedgeDetectorsHeight2;
	private Vector3 secondSideLedgeDetectorsHeight2;
	private Vector3 thirdSideLedgeDetectorsHeight2;
	private Vector3 sideLedgeDetectorsWidth2;
	private Vector3 sideLedgeDetectorsLength2;
	//climbing variables used for drawing
	private bool showClimbingDetectors3;
	private Vector3 climbingSurfaceDetectorsUpAmount3;
	private float climbingSurfaceDetectorsHeight3;
	private float climbingSurfaceDetectorsLength3;
	private bool showEdgeOfObjectDetctors3;
	private Vector3 bottomNoSurfaceDetectorHeight3;
	private Vector3 topNoSurfaceDetectorHeight3;
	private Vector3 topAndBottomNoSurfaceDetectorsWidth3;
	private float sideNoSurfaceDetectorsHeight3;
	private Vector3 sideNoSurfaceDetectorsWidth3;
	private bool showPullUpDetector3;
	private Vector3 pullUpLocationForward3;
	//walk off ledge then transition to climb variables
	private bool showWalkingOffLedgeRays3;
	private Vector3 spaceInFrontNeededToGrabBackOn3;
	private Vector3 firstSideLedgeDetectorsHeight3;
	private Vector3 secondSideLedgeDetectorsHeight3;
	private Vector3 thirdSideLedgeDetectorsHeight3;
	private Vector3 sideLedgeDetectorsWidth3;
	private Vector3 sideLedgeDetectorsLength3;
	private Vector3 spaceBelowNeededToGrabBackOnHeight3;
	private Vector3 spaceBelowNeededToGrabBackOnForward3;
	private Vector3 grabBackOnLocationHeight3;
	private Vector3 grabBackOnLocationWidth3;
	private Vector3 grabBackOnLocationForward3;
	
	//Moving platform variables without class name
	private bool allowMovingPlatformSupport; //determines whether or not the player can move with moving platforms
	private string movingPlatformTag; //the tag of the moving platform objects
	
	//private movement variables
	private Vector3 moveDirection; //the direction that the player moves in
	private float moveSpeed; //the current speed of the player
	private float moveSpeedAndFriction; //the current speed of the player with friction applied
	private float runSpeedMultiplier; //what to multiply the player's move speed by if we are running/the run button is held down
	private float accelerationRate; //how fast the player is accelerating
	private float deceleration = 1; //how fast the player will reach the speed of 0
	private float decelerationRate; //how fast the player is decelerating
	private float h; //the absolute value of the "Horizontal" axis minus the absolute value of the "Vertical" axis
	private float v; //the absolute value of the "Vertical" axis minus the absolute value of the "Horizontal" axis
	private Vector3 directionVector; //the direction that the joystick is being pushed in
	private bool inBetweenSlidableSurfaces; //determines whether you are in between two slidable surfaces or not
	private bool uphill; //determines whether you are going uphill on a slope or not
	private bool angHit; //determines whether or not a raycast going straight down (with a distance of 1) is hitting
	private float collisionSlopeAngle; //the angle of the surface you are currently standing on
	private float raycastSlopeAngle; //the angle of the surface being raycasted on
	private float slidingAngle; //the angle of the last slidable surface you collided with or are currently colliding with
	private bool slidePossible; //determines whether you can slide down a slope or not
	private bool sliding; //determines whether you are sliding down a slope or not
	private float slideSpeed = 6; //player's downward speed on slopes
	private Vector3 slidingVector; //the normal of the object you are colliding with
	private Vector3 slideMovement; //Vector3 that slerps to the normal of the object you are colliding with (slidingVector)
	[HideInInspector]
	public float bodyRotation; //the rotation that the player lerps to
	//crouching
	private bool canCrouch; //determines if the player can crouch (or if he will be able to crouch after landing on the ground)
	[HideInInspector]
	public bool crouching; //determines whether the player is currently crouching or not
	[HideInInspector]
	public bool crouchCancelsAttack; //if the player's attack key is the same as his crouch key, crouch instead of attacking
	private bool finishedCrouching; //determines whether the player has finished crouching/uncrouching
	private float originalColliderY;
	private float originalColliderHeight;
	private bool colliderAdjusted;
	private float oldYPos;
	private Vector3 feetPosition;
	private Vector3 headPosition;
	private float feetPositionNew;
	private float headPositionNew;
	[HideInInspector]
	public bool canCrouchToAction;
	//first person mode
	private bool firstPersonButtonPressed;
	private bool firstPersonStart;
	private bool inFirstPersonMode;
	private bool enabledLastUpdate;
	
	//private jumping variables
	private int currentJumpNumber; //the number of the most current jump performed
	private int totalJumpNumber; //the total amount of jumps set
	private float airSpeed; //player's movement speed in mid-air
	private float jumpTimer; //time since last jump was performed
	private float jumpPerformedTime; //time since last jump was first performed
	private bool inMidAirFromJump; //player is in mid-air as a result of jumping
	private bool jumpEnabled; //enables jumping while the script is enabled and disables jumping when the script is disabled
	private bool jumpPossible; //determines whether a jump is possible or not
	private bool doubleJumpPossible = true; //determines whether a double jump is possible or not
	private bool jumpPressed; //"Jump" button was pressed
	private float jumpPressedTimer; //time since "Jump" button was last pressed
	private bool jumpPerformed; //determines whether a jump was just performed
	private bool headHit; //determines if player's head hit the ceiling
	private float yPos; //player's position on the y-axis
	private float yVel; //player's y velocity
	private Vector3 pos; //position and collider bounds of the player
	private Vector3 contactPoint; //the specific point where the player and another object are colliding
	private float noCollisionTimer; //time since last collision
	
	//private attacking variables
	[System.NonSerialized]
	public int attackState; //the current attacking state of the player (on the ground or in the air)
	[System.NonSerialized]
	public int currentAttackNumber; //the number of the most current jump performed
	[System.NonSerialized]
	public int totalAttackNumber; //the total amount of jumps set
	private float attackPressedTimer; //time since attack button was last pressed
	private bool attackPressed;
	private bool attackButtonPressed;
	[System.NonSerialized]
	public float attackTimer; //time since last attack was performed
	private bool attackFinished; //the attack has finished
	[HideInInspector]
	public bool attackFinishedLastUpdate; //the attack finished in the last update
	private float waitingBetweenAttacksTimer; //time since last attack was performed or the attack state was switched
	private float attackPerformedTime; //time since last jump was first performed
	private bool canAttack; //determines whether or not the player can currently attack
	private bool attackedInMidAir; //determines if the player already attacked while in mid-air
	
	//private wall jumping variables
	[HideInInspector]
	public bool currentlyOnWall;
	private bool onWallLastUpdate;
	[HideInInspector]
	public bool turningToWall;
	private bool middleWallJumpable;
	private bool leftWallJumpable;
	private bool rightWallJumpable;
	private Vector3 wallNormal;
	private Vector3 wallHitPoint;
	private float forwardDir;
	private float rightDir;
	private float originalForwardDir;
	private float originalRightDir;
	private bool jumpedOffWallForWallJump;
	private Vector3 originalWallJumpDirection;
	private Vector3 wallJumpDirection;
	private float angleBetweenPlayerAndWall;
	private bool wallBackHit;
	private float distFromWall;
	private float firstDistFromWall;
	private bool inMidAirFromWallJump;
	private float wallJumpTimer;
	private float slideDownSpeed2;
	private bool onWallAnimation;
	private bool rbUsesGravity;
	private bool canChangeRbGravity;
	
	//private ladder and wall climbing variables
	[HideInInspector]
	public bool currentlyClimbingWall = false;
	[HideInInspector]
	public bool wallIsClimbable;
	[HideInInspector]
	public bool climbableWallInFront;
	private bool wallInFront;
	[HideInInspector]
	public bool finishedRotatingToWall;
	private float jumpedOffClimbableObjectTimer = 1.0f;
	private Vector3 jumpedOffClimbableObjectDirection;
	private Vector3 climbDirection;
	private bool downCanBePressed;
	private bool climbedUpAlready;
	private bool aboveTop;
	private bool reachedTopPoint = false;
	private bool reachedBottomPoint = false;
	private bool reachedRightPoint = false;
	private bool reachedLeftPoint = false;
	private float climbingMovement;
	private bool beginClimbBurst;
	private bool switchedDirection;
	private float lastAxis;
	private float horizontalClimbSpeed;
	private float verticalClimbSpeed;
	private float arm;
	private Vector3 centerPoint;
	private float snapTimer = 1;
	private bool snappingToCenter;
	private bool startedClimbing;
	private bool firstTest;
	private bool secondTest;
	private bool thirdTest;
	private bool fourthTest;
	[HideInInspector]
	public bool fifthTest;
	private int i = 0;
	private int tagNum;
	//walking off ledge variables
	[HideInInspector]
	public bool turnBack = false;
	private float turnBackTimer = 0.0f;
	private bool turnBackMiddle = true;
	private bool turnBackLeft;
	private bool turnBackRight;
	[HideInInspector]
	public bool back2 = false;
	private Vector3 backPoint;
	private Vector3 turnBackPoint;
	private Quaternion backRotation;
	private Quaternion normalRotation;
	private Vector3 playerPosXZ;
	private float playerPosY;
	//pulling up variables
	private bool pullingUp;
	private float pullingUpTimer;
	private bool pullUpLocationAcquired;
	private bool movingToPullUpLocation;
	private Vector3 movementVector;
	private Vector3 hitPoint;
	private bool animatePullingUp;
	//climbing rotation variables
	[HideInInspector]
	public Vector3 rotationHit;
	private Quaternion rotationNormal;
	private float rotationState;
	private bool hasNotMovedOnWallYet;
	private Quaternion lastRot3;
	private bool axisChanged;
	private float horizontalAxis;
	private float climbingHeight;
	private float lastYPosOnWall;
	[HideInInspector]
	public float horizontalValue = 1;
	//avoiding getting stuck to wall variables
	private Vector3 lastPos;
	private Quaternion lastRot2;
	private float distFromWallWhenStuck;
	private float firstDistFromWallWhenStuck;
	private bool stuckInSamePos;
	private bool stuckInSamePosNoCol;
	//enabling and disabling script variables
	private string[] scriptsToEnableOnGrab; //scripts to enable when the player grabs on to a wall (scripts disable when the player lets go of a wall)
	private MonoBehaviour scriptToEnable; //the current script being enabled (or disabled when the player lets go of a wall) in the scriptsToEnableOnGrab array
	private string[] scriptsToDisableOnGrab; //scripts to disable when the player grabs on to a wall (scripts enable when the player lets go of a wall)
	private MonoBehaviour scriptToDisable; //the current script being disabled (or enabled when the player lets go of a wall) in the scriptsToDisableOnGrab array
	private bool currentlyEnablingAndDisablingScripts = false; //determines whether the scripts in scriptsToEnableOnGrab or scriptsToDisableOnGrab are currently being enabled/disabled or not
	private bool scriptWarning = false; //determines whether or not the user entered any script names that do not exist on the player
	private bool onWallScriptsFinished = false; //determines whether the scripts in scriptsToEnableOnGrab or scriptsToDisableOnGrab have finished being enabled/disabled or not
	
	//private moving platform variables
	private float noCollisionWithPlatformTimer; //time since last collision with platform tag
	[HideInInspector]
	public GameObject oldParent; //the player's parent before coming in contact with a platform
	[HideInInspector]
	public GameObject emptyObject; //empty object that undoes the platform's properties that affect the player's scale
	private GameObject emptyObjectParent; //parent of the empty object (the platform itself)
	
	private RaycastHit hit = new RaycastHit(); //information on the hit point of a raycast
	private RaycastHit frontHit = new RaycastHit(); //information on the hit point of a raycast in front of a player
	private RaycastHit backHit = new RaycastHit(); //information on the hit point of a raycast behind the player
	private Animator animator; //the "Animator" component of the script holder
	public LayerMask collisionLayers = -1; //the layers that the detectors (raycasts/linecasts) will collide with
	
	// Use this for initialization
	void Start () {
		StartUp();
		waitingBetweenAttacksTimer = attacking.waitingTimeBetweenAttacks;
		//starting off in first person mode
		if (movement.firstPerson.switchToFirstPersonIfInputButtonPressed && movement.firstPerson.startOffInFirstPersonModeForSwitching
		&& (!playerCamera.GetComponent<CameraController>() || playerCamera.GetComponent<CameraController>() && !playerCamera.GetComponent<CameraController>().mouseOrbit.startOffMouseOrbitingForSwitching)){
			firstPersonStart = true;
			firstPersonButtonPressed = true;
		}
	}
	
	void StartUp () {
		//resetting script to make sure that everything initializes
		enabled = false;
		enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		
		//if the player is currently on a wall, set jumpedOffWallForWallJump to false
		if (currentlyOnWall){
			jumpedOffWallForWallJump = false;
		}
		//if the player has jumped off of a wall, set jumpedOffWallForWallJump to true
		else if (inMidAirFromWallJump && noCollisionTimer >= 5){
			jumpedOffWallForWallJump = true;
		}
		if (jumpedOffWallForWallJump && noCollisionTimer < 5 && inMidAirFromWallJump){
			transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
			inMidAirFromWallJump = false;
		}
		
		//storing values to variables
		//Grounded variables
		maxGroundedHeight2 = transform.up * grounded.maxGroundedHeight;
		maxGroundedRadius2 = grounded.maxGroundedRadius - 0.0075f;
		maxGroundedDistance2 = grounded.maxGroundedDistance;
		maxGroundedDistanceDown = Vector3.down*grounded.maxGroundedDistance;
		//Movement variables
		forwardSpeed2 = movement.forwardSpeed;
		sideSpeed2 = movement.sideSpeed;
		backSpeed2 = movement.backSpeed;
		if (!inMidAirFromWallJump){
			midAirMovementSpeedMultiple2 = movement.midAirMovementSpeedMultiple;
		}
		//wall jumps have their own mid-air speed and dampening, so during a wall jump, we set midAirMovementSpeedMultiple2 to 0 to avoid affecting it
		else {
			midAirMovementSpeedMultiple2 = 0;
		}
		acceleration2 = movement.acceleration;
		slopeSlideSpeed2 = movement.slopeSlideSpeed;
		//Jumping variables
		jumpsToPerform = jumping.numberAndHeightOfJumps;
		timeLimitBetweenJumps2 = jumping.timeLimitBetweenJumps;
		jumpLandingEffect2 = jumping.jumpLandingEffect;
		allowDoubleJump2 = jumping.allowDoubleJump;
		doubleJumpPerformableIfInMidAirInGeneral2 = jumping.doubleJumpPerformableIfInMidAirInGeneral;
		doubleJumpHeight2 = jumping.doubleJumpHeight;
		doubleJumpEffect2 = jumping.doubleJumpEffect;
		maxFallingSpeed2 = jumping.maxFallingSpeed;
		//Wall Jumping variables
		spaceOnWallNeededToWallJumpUpAmount2 = transform.up * wallJumping.spaceOnWallNeededToWallJumpUpAmount;
		spaceOnWallNeededToWallJumpHeight2 = wallJumping.spaceOnWallNeededToWallJumpHeight;
		spaceOnWallNeededToWallJumpLength2 = wallJumping.spaceOnWallNeededToWallJumpLength - 0.5f;
		spaceOnWallNeededToWallJumpWidth2 = wallJumping.spaceOnWallNeededToWallJumpWidth;
		spaceBelowNeededToWallJump2 = transform.up * wallJumping.spaceBelowNeededToWallJump;
		//Attacking variables
		if (attackState == 0){
			attacksToPerform = attacking.numberAndStrengthOfAttacks;
		}
		else {
			attacksToPerform = attacking.numberAndStrengthOfMidAirAttacks;
		}
		timeLimitBetweenAttacks2 = attacking.timeLimitBetweenAttacks;
		//MovingPlatform variables
		allowMovingPlatformSupport = movingPlatforms.allowMovingPlatformSupport;
		movingPlatformTag = movingPlatforms.movingPlatformTag;
		//

		//getting position and collider information for raycasts
		pos = transform.position;
        pos.y = GetComponent<Collider>().bounds.min.y + 0.1f;
		
		
		//showing wall jump detectors
		if (wallJumping.showWallJumpDetectors){
			//middle
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, Color.yellow);
			//left
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			//right
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			Debug.DrawLine(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, Color.yellow);
			//below
			Debug.DrawLine(transform.position, transform.position - transform.up*0.3f - spaceBelowNeededToWallJump2, Color.cyan);
		}
		
		//showing climbing detectors
		DrawClimbingDetectors();
		
		//setting the player's "Animator" component (if player has one) to animator
		if (GetComponent<Animator>()){
			animator = GetComponent<Animator>();
		}
		
		if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
			//checking to see if player's head hit the ceiling
			if (GetComponent<CharacterController>().velocity.y < 0.5 && moveDirection.y > 0.5 && !grounded.currentlyGrounded){
				headHit = true;
			}
			else {
				headHit = false;
			}
			yVel = GetComponent<CharacterController>().velocity.y;
		}
		else if (GetComponent<Rigidbody>()){
			//checking to see if player's head hit the ceiling
			if (yPos == transform.position.y && GetComponent<Rigidbody>().velocity.y + 0.1f > yVel && !jumpPerformed && noCollisionTimer == 0 && !sliding && moveDirection.y > 0 && !grounded.currentlyGrounded){
				if (collisionSlopeAngle < slopeLimit || collisionSlopeAngle > 91){
					headHit = true;
				}
				else {
					headHit = false;
				}
			}
			else {
				headHit = false;
			}
			yPos = transform.position.y;
			yVel = GetComponent<Rigidbody>().velocity.y;
		}
		
		//if user set jumps, totalJumpNumber equals the number set
		if (jumpsToPerform.Length > 0){
			totalJumpNumber = jumpsToPerform.Length;
		}
		//if user did not set jumps, totalJumpNumber equals 0
		else {
			totalJumpNumber = 0;
		}
		
		//if the "Jump" button was pressed, jumpPressed equals true
		if (Input.GetButtonDown("Jump")){
			jumpPressedTimer = 0.0f;
			jumpPressed = true;
		}
		else{
			jumpPressedTimer += 0.02f;
		}
		
		//wait 0.2 seconds for jumpPressed to become false
		//this allows the player to press the "Jump" button slightly early and still jump once they have landed
		if (jumpPressedTimer > 0.2f){
			jumpPressed = false;
		}

		//jump
		if (grounded.currentlyGrounded){
			if (jumpPressed && jumpPossible && !jumpPerformed && totalJumpNumber > 0 && !onWallLastUpdate && jumpEnabled && (raycastSlopeAngle > slopeLimit && (uphill && jumping.allowJumpWhenSlidingFacingUphill || !uphill && jumping.allowJumpWhenSlidingFacingDownhill || inBetweenSlidableSurfaces) || raycastSlopeAngle <= slopeLimit) && canCrouchToAction){
				Jump();
			}
			doubleJumpPossible = true;
		}
		
		//double jump
		if (Input.GetButtonDown("Jump") && doubleJumpPossible && !grounded.currentlyGrounded && allowDoubleJump2 && jumpEnabled && (doubleJumpPerformableIfInMidAirInGeneral2 || !doubleJumpPerformableIfInMidAirInGeneral2 && inMidAirFromJump) && (jumping.doubleJumpPerformableOutOfWallJump || !inMidAirFromWallJump) && !onWallLastUpdate && canCrouchToAction){
			if (!Physics.Raycast(pos, Vector3.down, out hit, 0.5f, collisionLayers) && moveDirection.y < 0 || !grounded.currentlyGrounded && moveDirection.y >= 0){
				DoubleJump();
				jumpPressed = false;
				doubleJumpPossible = false;
			}
		}
		
		//enabling jumping while the script is enabled
		jumpEnabled = true;
		
		//checking to see if player was on the wall in the last update
		if (currentlyOnWall || currentlyClimbingWall || turnBack || back2){
			onWallLastUpdate = true;
		}
		else {
			onWallLastUpdate = false;
		}
		
		//counting how long it has been since last jump was first performed
		if (jumpPerformed){
			jumpPerformedTime += 0.02f;
		}
		else {
			jumpPerformedTime = 0;
		}
		
		//if in mid air as a result of jumping
		if (inMidAirFromJump){
			
			if (grounded.currentlyGrounded){
				
				if (!jumpPerformed){
					//creating the optional dust effect after landing a jump
					if (jumpLandingEffect2 != null){
						Instantiate(jumpLandingEffect2, transform.position + new Vector3(0, 0.05f, 0), jumpLandingEffect2.transform.rotation);
					}
				
					//once player has landed jump, stop jumping animation and return to movement
					if (animator != null && animator.runtimeAnimatorController != null){
						animator.CrossFade("Movement", 0.2f);
					}
				
					//once player has landed jump, set inMidAirFromJump to false
					inMidAirFromJump = false;
				}
				
				if (jumpTimer == 0 && jumpPerformedTime > 0.1f){
					//creating the optional dust effect after landing a jump
					if (jumpLandingEffect2 != null){
						Instantiate(jumpLandingEffect2, transform.position + new Vector3(0, 0.05f, 0), jumpLandingEffect2.transform.rotation);
					}
					jumpPerformed = false;
					inMidAirFromJump = false;
				}
				
			}
			
		}
		
		
		//if player is not in mid-air as a result of jumping, increase the jump timer
		if (!inMidAirFromJump) {
			jumpTimer += 0.02f;
		}
		
		//if the jump timer is greater than the jump time limit, reset current jump number
		if (jumpTimer > timeLimitBetweenJumps2 && timeLimitBetweenJumps2 > 0) {
			currentJumpNumber = totalJumpNumber;
		}

		//set animator's float parameter, "jumpNumber," to currentJumpNumber
		if (animator != null && animator.runtimeAnimatorController != null){
			animator.SetFloat("jumpNumber", currentJumpNumber);
		}

		
		//after jump is performed and jumpPerformed is true, set jumpPerformed to false
		if (jumpPerformed){
			if (!grounded.currentlyGrounded || headHit){
				jumpPerformed = false;
			}
		}
		
		//crouching
		if (movement.crouching.allowCrouching){
			//when the crouching button is pressed, determine if the player can crouch (or if he will be able to crouch after landing on the ground)
			if (!canCrouch && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Joystick1Button8)) && finishedCrouching){
				finishedCrouching = false;
				crouchCancelsAttack = true;
				canCrouch = true;
			}
			//when the crouching button is pressed and the player is already crouched: uncrouch (if there is enough space above the player's head)
			else if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.Joystick1Button8)) && !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, collisionLayers) && finishedCrouching){
				finishedCrouching = false;
				crouchCancelsAttack = true;
				crouching = false;
				canCrouch = false;
			}
			//if the player is grounded and canCrouch is true, crouch
			if (grounded.currentlyGrounded && canCrouch && !crouching){
				crouchCancelsAttack = true;
				crouching = true;
			}
			//if the player performs a jump, wall climb, etc.: uncrouch
			if (crouching && (currentlyClimbingWall || currentlyOnWall) && !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, collisionLayers)){
				finishedCrouching = false;
				canCrouch = false;
				crouching = false;
			}
			
		}
		else {
			canCrouch = false;
			crouching = false;
		}
		
		//telling animator to crouch when the player is crouching
		if (animator != null){
			animator.SetBool("crouch", crouching);
		}
		finishedCrouching = true;
		
		//determining if player can uncrouch then jump, attack, etc.
		if (!crouching || !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, collisionLayers)){
			canCrouchToAction = true;
		}
		else {
			canCrouchToAction = false;
		}
		
		//if the player is not colliding with a platform (and he is not on the ledge of the platform), set the player's parent to what it was before
		if (allowMovingPlatformSupport && noCollisionWithPlatformTimer >= 5 && emptyObject != null){
			
			//unparenting player from platform
			if (transform.parent == emptyObject.transform){
				if (oldParent != null){
					transform.parent = oldParent.transform;
				}
				else {
					transform.parent = null;
				}
			}
			
			//deleting empty object once the player is no longer a child of it
			if (transform.parent != emptyObject.transform && emptyObject.transform.childCount == 0 && (transform.parent == oldParent || transform.parent == null)){
				//making sure we are no longer attached to the empty object (so that we don't delete ourself)
				transform.parent = null;
				//deleting the emptyObject
				if (transform.parent != emptyObject.transform){
					transform.parent = null;
					Destroy(emptyObject);
				}
				//setting parent back to normal
				if (oldParent != null){
					transform.parent = oldParent.transform;
				}
				else {
					transform.parent = null;
				}
			}
			
		}
		
	}
	
	void FixedUpdate () {
		
		ChangeColliderHeightForCrouch();
		
		if (climbing.Length > 0){
			GettingClimbingValues();
		}
		
		//increase the noCollisionTimer (if there is a collision, the noCollisionTimer is later set to 0)
		noCollisionTimer++;
		
		AvoidSlidingWhileClimbing();
		
		MovingPlatformParenting();
		
		Attacks();
		
		GettingRotationDirection();
		
		CheckRBForUseGravity();
		
		CheckIfWallJumpIsPossible();
		
		DirectionOfWallJump();
		
		PerformWallJump();
		
		//wall and ladder climbing
		if (climbing.Length > 0){
			CheckIfPlayerCanClimb();
			
			ClimbingWall();
			
			JumpOffOfClimb();
			
			ClimbableObjectEdgeDetection();
			
			PullingUpClimbableObject();
			
			AnimatingClimbing();
		}
		
		RotatePlayer();
		
		SettingPlayerSpeed();
		
		DetermineGroundedState();
		
		GettingGroundAndSlopeAngles();
		
		GettingMovementDirection();
		
		LockAxisForSideScrolling();
		
		SlopeSliding();
		
		PreventBouncing();
		
		ApplyGravity();
		
		MovePlayer();
		
		AvoidFallingWhileClimbing();
		
		SwitchingToFirstPersonMode();
		
		CrouchAttack();
	
	}
	
	void ChangeColliderHeightForCrouch () {
		
		//changing height of collider while crouching
		//determining if player can uncrouch then jump, attack, etc.
		if (!crouching || !Physics.Linecast(new Vector3(transform.position.x, feetPositionNew, transform.position.z), new Vector3(transform.position.x, headPositionNew, transform.position.z), out hit, collisionLayers)){
			canCrouchToAction = true;
		}
		else {
			canCrouchToAction = false;
		}
		//if player has a character controller collider
		if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
			if (crouching){
				if (!colliderAdjusted){
					Vector3 colliderCenter = GetComponent<CharacterController>().center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<CharacterController>().center = colliderCenter;
					GetComponent<CharacterController>().height = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					colliderAdjusted = true;
				}
			}
			else {
				if (colliderAdjusted){
					Vector3 colliderCenter2 = GetComponent<CharacterController>().center;
					colliderCenter2.y = originalColliderY;
					GetComponent<CharacterController>().center = colliderCenter2;
					GetComponent<CharacterController>().height = originalColliderHeight;
				}
				colliderAdjusted = false;
				originalColliderY = GetComponent<CharacterController>().center.y;
				originalColliderHeight = GetComponent<CharacterController>().height;
				feetPosition = GetComponent<CharacterController>().bounds.min;
				headPosition = GetComponent<CharacterController>().bounds.max;
				oldYPos = transform.position.y;
			}
		}
		else if (GetComponent<Rigidbody>()){
			//if player has a capsule collider
			if (GetComponent<CapsuleCollider>()){
				
				if (crouching){
					Vector3 colliderCenter = GetComponent<CapsuleCollider>().center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<CapsuleCollider>().center = colliderCenter;
					GetComponent<CapsuleCollider>().height = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					colliderAdjusted = true;
				}
				else {
					if (colliderAdjusted){
						Vector3 colliderCenter2 = GetComponent<CapsuleCollider>().center;
						colliderCenter2.y = originalColliderY;
						GetComponent<CapsuleCollider>().center = colliderCenter2;
						GetComponent<CapsuleCollider>().height = originalColliderHeight;
					}
					colliderAdjusted = false;
					originalColliderY = GetComponent<CapsuleCollider>().center.y;
					originalColliderHeight = GetComponent<CapsuleCollider>().height;
					feetPosition = GetComponent<CapsuleCollider>().bounds.min;
					headPosition = GetComponent<CapsuleCollider>().bounds.max;
					oldYPos = transform.position.y;
				}
				
			}
			//if player has a sphere collider
			else if (GetComponent<SphereCollider>()){
				
				if (crouching){
					Vector3 colliderCenter = GetComponent<SphereCollider>().center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<SphereCollider>().center = colliderCenter;
					GetComponent<SphereCollider>().radius = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					colliderAdjusted = true;
				}
				else {
					if (colliderAdjusted){
						Vector3 colliderCenter2 = GetComponent<SphereCollider>().center;
						colliderCenter2.y = originalColliderY;
						GetComponent<SphereCollider>().center = colliderCenter2;
						GetComponent<SphereCollider>().radius = originalColliderHeight;
					}
					colliderAdjusted = false;
					originalColliderY = GetComponent<SphereCollider>().center.y;
					originalColliderHeight = GetComponent<SphereCollider>().radius;
					feetPosition = GetComponent<SphereCollider>().bounds.min;
					headPosition = GetComponent<SphereCollider>().bounds.max;
					oldYPos = transform.position.y;
				}
				
			}
			//if player has a box collider
			else if (GetComponent<BoxCollider>()){
				
				if (crouching){
					//position
					Vector3 colliderCenter = GetComponent<BoxCollider>().center;
					colliderCenter.y = originalColliderY*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<BoxCollider>().center = colliderCenter;
					//height
					Vector3 colliderSize = GetComponent<BoxCollider>().size;
					colliderSize.y = originalColliderHeight*movement.crouching.crouchColliderHeightMultiple;
					GetComponent<BoxCollider>().size = colliderSize;
					colliderAdjusted = true;
				}
				else {
					if (colliderAdjusted){
						//position
						Vector3 colliderCenter2 = GetComponent<BoxCollider>().center;
						colliderCenter2.y = originalColliderY;
						GetComponent<BoxCollider>().center = colliderCenter2;
						//height
						Vector3 colliderSize2 = GetComponent<BoxCollider>().size;
						colliderSize2.y = originalColliderHeight;
						GetComponent<BoxCollider>().size = colliderSize2;
					}
					colliderAdjusted = false;
					originalColliderY = GetComponent<BoxCollider>().center.y;
					originalColliderHeight = GetComponent<BoxCollider>().size.y;
					feetPosition = GetComponent<BoxCollider>().bounds.min;
					headPosition = GetComponent<BoxCollider>().bounds.max;
					oldYPos = transform.position.y;
				}
				
			}
		}
		//getting the new positions of the top and bottom of the player's collider (his collider from when he is not crouching)
		feetPositionNew = feetPosition.y + (transform.position.y - oldYPos);
		headPositionNew = headPosition.y + (transform.position.y - oldYPos);
		
	}
	
	void GettingClimbingValues () {
		
		if (!wallIsClimbable && !currentlyClimbingWall && !turnBack && !back2 && !pullingUp){
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				tagNum = i;
			}
		}
		//Climbing variables
		climbingSurfaceDetectorsUpAmount2 = transform.up * (climbing[tagNum].climbingSurfaceDetectorsUpAmount + 0.2f);
		climbingSurfaceDetectorsHeight2 = climbing[tagNum].climbingSurfaceDetectorsHeight - 0.18f;
		climbingSurfaceDetectorsLength2 = climbing[tagNum].climbingSurfaceDetectorsLength - 0.5f;
		distanceToPushOffAfterLettingGo2 = climbing[tagNum].distanceToPushOffAfterLettingGo;
		rotationToClimbableObjectSpeed2 = climbing[tagNum].rotationToClimbableObjectSpeed;
		climbHorizontally2 = climbing[tagNum].climbHorizontally;
		climbVertically2 = climbing[tagNum].climbVertically;
		climbMovementSpeed2 = climbing[tagNum].climbMovementSpeed;
		climbRotationSpeed2 = climbing[tagNum].climbRotationSpeed;
		moveInBursts = climbing[tagNum].moveInBursts;
		burstLength = climbing[tagNum].burstLength;
		climbableTag2 = climbing[tagNum].climbableTag;
		stayUpright2 = climbing[tagNum].stayUpright;
		snapToCenterOfObject2 = climbing[tagNum].snapToCenterOfObject;
		bottomNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].bottomNoSurfaceDetectorHeight + 0.15f);
		topNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].topNoSurfaceDetectorHeight + 0.15f);
		topAndBottomNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].topAndBottomNoSurfaceDetectorsWidth);
		sideNoSurfaceDetectorsHeight2 = climbing[tagNum].sideNoSurfaceDetectorsHeight - 0.15f;
		sideNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f);
		sideNoSurfaceDetectorsWidthTurnBack2 = climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f;
		stopAtSides2 = climbing[tagNum].stopAtSides;
		dropOffAtBottom2 = climbing[tagNum].dropOffAtBottom;
		dropOffAtFloor2 = climbing[tagNum].dropOffAtFloor;
		pullUpAtTop2 = climbing[tagNum].pullUpAtTop;
		pullUpSpeed = climbing[tagNum].pullUpSpeed;
		pullUpLocationForward2 = transform.forward*(climbing[tagNum].pullUpLocationForward);
		pushAgainstWallIfPlayerIsStuck2 = climbing[tagNum].pushAgainstWallIfPlayerIsStuck;
		//walk off ledge then transition to climb variables
		allowGrabbingOnAfterWalkingOffLedge2 = climbing[tagNum].walkingOffOfClimbableSurface.allowGrabbingOnAfterWalkingOffLedge;
		spaceInFrontNeededToGrabBackOn2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceInFrontNeededToGrabBackOn + 0.03f);
		firstSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.firstSideLedgeDetectorsHeight);
		secondSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.secondSideLedgeDetectorsHeight);
		thirdSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.thirdSideLedgeDetectorsHeight);
		sideLedgeDetectorsWidth2 = transform.right*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsWidth);
		sideLedgeDetectorsLength2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsLength);
		spaceBelowNeededToGrabBackOnHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnHeight + 0.07f);
		spaceBelowNeededToGrabBackOnForward2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnForward + 0.06f);
		//enabling and disabling script variables
		scriptsToEnableOnGrab = climbing[tagNum].scriptsToEnableOnGrab;
		scriptsToDisableOnGrab = climbing[tagNum].scriptsToDisableOnGrab;
		
	}
	
	void AvoidSlidingWhileClimbing () {
		
		//keeping player from randomly dropping while on wall (at the beginning of the function)
		if (currentlyClimbingWall && !pullingUp){
			if (Mathf.Abs(transform.position.y - lastYPosOnWall) >= 0.2f * (climbMovementSpeed2/4) && lastYPosOnWall != 0){
				transform.position = new Vector3(transform.position.x, climbingHeight, transform.position.z);
			}
			else {
				climbingHeight = transform.position.y;
			}
			lastYPosOnWall = transform.position.y;
		}
		else {
			lastYPosOnWall = 0;
		}
		
		//keeping player from randomly sliding while climbing wall
		if (currentlyClimbingWall || pullingUp || turnBack || back2){
			if (GetComponent<Rigidbody>() && !startedClimbing){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			startedClimbing = true;
		}
		else {
			startedClimbing = false;
		}
		
	}
	
	void MovingPlatformParenting () {
		
		//moving with moving platforms
		//increase the noCollisionWithPlatformTimer (if there is a collision, the noCollisionWithPlatformTimer is later set to 0)
		noCollisionWithPlatformTimer++;
		
		//undoing parent's properties that affect the player's scale 
		if (emptyObject != null){
			emptyObjectParent = emptyObject.transform.parent.gameObject;
			emptyObject.transform.localScale = new Vector3(1/emptyObjectParent.transform.localScale.x, 1/emptyObjectParent.transform.localScale.y, 1/emptyObjectParent.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-emptyObjectParent.transform.localRotation.x, -emptyObjectParent.transform.localRotation.y, -emptyObjectParent.transform.localRotation.z);
		}
		else {
			emptyObjectParent = null;
		}
		
		//getting what the player's parent was before coming in contact with a platform
		if (transform.parent == null){
			oldParent = null;
		}
		else if (emptyObject == null || transform.parent != emptyObject.transform){
			oldParent = transform.parent.gameObject;
		}
		else {
			oldParent = null;
		}
		
	}
	
	void Attacks () {
		
		//if user set attacks, totalAttackNumber equals the number set
		if (attacking.numberAndStrengthOfAttacks.Length > 0){
			if (attackState == 0){
				totalAttackNumber = attacking.numberAndStrengthOfAttacks.Length;
				if (attackedInMidAir){
					currentAttackNumber = totalAttackNumber;
				}
				attackedInMidAir = false;
			}
			else {
				totalAttackNumber = attacking.numberAndStrengthOfMidAirAttacks.Length;
			}
		}
		//if user did not set attacks, totalAttackNumber equals 0
		else {
			totalAttackNumber = 0;
		}
		
		//if player is on ground or wall
		if (!crouching){
			if (grounded.currentlyGrounded || currentlyOnWall || currentlyClimbingWall || turnBack || back2){
				if (attackState == 1){
					waitingBetweenAttacksTimer = attacking.waitingTimeBetweenAttacks;
				}
				attackState = 0;
			}
			//if player is in the air
			else {
				if (attackState == 0){
					waitingBetweenAttacksTimer = attacking.waitingTimeBetweenMidAirAttacks;
				}
				attackState = 1;
			}
		}
		else {
			attackState = 2;
		}
		
		//attack
		//if the "Attack" button was pressed (and player is not going in to a crouch), attackPressed equals true
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.Joystick1Button8) || !movement.crouching.allowCrouching){
			if (Input.GetButton(attacking.attackInputButton) && !attackButtonPressed){
				attackPressedTimer = 0.0f;
				attackPressed = true;
				canAttack = true;
				attackButtonPressed = true;
			}
			else {
				attackPressedTimer += 0.02f;
				if (!Input.GetButton(attacking.attackInputButton)){
					attackButtonPressed = false;
				}
			}
			
			//wait 0.2 seconds for attackPressed to become false
			//this allows the player to press the "Attack" button slightly early and still attack even if waitingBetweenAttacksTimer was not high enough yet
			if (attackPressedTimer > 0.2f){
				attackPressed = false;
			}
			
			if (attackPressed && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2 && !crouchCancelsAttack && canCrouchToAction && !crouching){
				if (canAttack && (attackState == 0 && waitingBetweenAttacksTimer > attacking.waitingTimeBetweenAttacks || attackState == 1 && waitingBetweenAttacksTimer > attacking.waitingTimeBetweenMidAirAttacks && (!attackedInMidAir || !attacking.onlyAllowMidAirAttackOnceInMidAir))){
					Attack();
					if (attackState == 1){
						attackedInMidAir = true;
					}
					waitingBetweenAttacksTimer = 0;
					canAttack = false;
				}
			}
			else if (attackState == 0){
				canAttack = true;
			}
			
			//increase the attack timers
			attackTimer += 0.02f;
			waitingBetweenAttacksTimer += 0.02f;
			
			//if the attack timer is greater than the attack time limit, reset current attack number
			if (attackTimer > timeLimitBetweenAttacks2 && timeLimitBetweenAttacks2 > 0) {
				currentAttackNumber = totalAttackNumber;
			}
			
			//set animator's float parameter, "attackNumber," to currentAttackNumber
			if (animator != null && animator.runtimeAnimatorController != null){
				animator.SetFloat("attackState", attackState);
				animator.SetFloat("attackNumber", currentAttackNumber);
			}
		}
		
	}
	
	void GettingRotationDirection () {
		
		//getting the direction to rotate towards
		directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (directionVector != Vector3.zero) {
			
            //getting the length of the direction vector and normalizing it
            float directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;

            //setting the maximum direction length to 1
            directionLength = Mathf.Min(1, directionLength);

            directionLength *= directionLength;

            //multiply the normalized direction vector by the modified direction length
            directionVector *= directionLength;
			
        }
		
	}
	
	void CheckRBForUseGravity () {
		
		//checking to see if player (if using a Rigidbody) is using "Use Gravity"
		if (GetComponent<Rigidbody>()){
			if (GetComponent<Rigidbody>().useGravity && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2){
				rbUsesGravity = true;
			}
			
			if (currentlyOnWall || currentlyClimbingWall || turnBack || back2){
				canChangeRbGravity = true;
				GetComponent<Rigidbody>().useGravity = false;
			}
			else if (rbUsesGravity && canChangeRbGravity){
				GetComponent<Rigidbody>().useGravity = true;
				canChangeRbGravity = false;
			}
			
			if (!GetComponent<Rigidbody>().useGravity && !currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2){
				rbUsesGravity = false;
			}
		}
		
	}
	
	void CheckIfWallJumpIsPossible () {
		
		//checking to see if wall jumping is possible
		//middle
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  noCollisionTimer < 5 && !grounded.currentlyGrounded && directionVector.magnitude >= wallJumping.inputPercentageNeededToWallJump/100 && hit.transform.tag != climbableTag2){
			middleWallJumpable = true;
		}
		else {
			middleWallJumpable = false;
		}
		//left
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f - (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  noCollisionTimer < 5 && !grounded.currentlyGrounded && directionVector.magnitude >= wallJumping.inputPercentageNeededToWallJump/100 && hit.transform.tag != climbableTag2){
			leftWallJumpable = true;
		}
		else {
			leftWallJumpable = false;
		}
		//right
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f + (transform.right * (spaceOnWallNeededToWallJumpWidth2 + 1))/3, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle
		&&  noCollisionTimer < 5 && !grounded.currentlyGrounded && directionVector.magnitude >= wallJumping.inputPercentageNeededToWallJump/100 && hit.transform.tag != climbableTag2){
			rightWallJumpable = true;
		}
		else {
			rightWallJumpable = false;
		}
		
		if (!headHit && wallJumping.allowWallJumping && !grounded.currentlyGrounded && !wallIsClimbable && !turnBack && !back2){
			if ((middleWallJumpable && leftWallJumpable || middleWallJumpable && rightWallJumpable) && !Physics.Linecast(transform.position, transform.position - transform.up*0.3f - spaceBelowNeededToWallJump2, out hit, collisionLayers)){
				if (inMidAirFromWallJump){
					transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
					inMidAirFromWallJump = false;
				}
				wallJumpDirection = Vector3.zero;
				if (!currentlyOnWall){
					wallJumpTimer = 0.0f;
					slideDownSpeed2 = 0;
					if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					else if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, collisionLayers) && 90 - Mathf.Abs(90 - Vector3.Angle(Vector3.up, hit.normal)) >= wallJumping.minimumWallAngle){
						wallNormal = hit.normal;
						wallHitPoint = hit.point + hit.normal*(wallJumping.distanceToKeepFromWallWhenOnWall/3.75f);
					}
					
					angleBetweenPlayerAndWall = Vector3.Angle(transform.right, hit.normal);
				}
				if (!sliding){
					turningToWall = true;
					currentlyOnWall = true;
				}
			}
		}
		
	}
	
	void DirectionOfWallJump () {
		
		//getting directions of wall jump
		//new directions (using wallJumpDirection, which is modified by the joystick)
		//forward
		if (Vector3.Dot(transform.forward, wallJumpDirection) + 1 > 0){
			forwardDir = Vector3.Dot(transform.forward, wallJumpDirection) + 1;
		}
		//back
		else if (Mathf.Abs(Vector3.Dot(transform.forward, wallJumpDirection)) != 0){
			forwardDir = 1 / Mathf.Abs(Vector3.Dot(transform.forward, wallJumpDirection));
		}
		//right
		if (Vector3.Dot(transform.right, wallJumpDirection) + 1 > 0){
			rightDir = Vector3.Dot(transform.right, wallJumpDirection) + 1;
		}
		//left
		else if (Mathf.Abs(Vector3.Dot(transform.right, wallJumpDirection)) != 0){
			rightDir = 1 / Mathf.Abs(Vector3.Dot(transform.right, wallJumpDirection));
		}
		
		//original directions (using originalWallJumpDirection, which is not modified by the joystick)
		//original forward
		if (Vector3.Dot(transform.forward, originalWallJumpDirection) + 1 >= 0){
			originalForwardDir = Vector3.Dot(transform.forward, originalWallJumpDirection) + 1;
		}
		//original back
		else if (Mathf.Abs(Vector3.Dot(transform.forward, originalWallJumpDirection)) != 0){
			originalForwardDir = 1 / Mathf.Abs(Vector3.Dot(transform.forward, originalWallJumpDirection));
		}
		//original right
		if (Vector3.Dot(transform.right, originalWallJumpDirection) + 1 >= 0){
			originalRightDir = Vector3.Dot(transform.right, originalWallJumpDirection) + 1;
		}
		//original left
		else if (Mathf.Abs(Vector3.Dot(transform.right, originalWallJumpDirection)) != 0){
			originalRightDir = 1 / Mathf.Abs(Vector3.Dot(transform.right, originalWallJumpDirection));
		}
		
		//checking to make sure the player did not slide off of the wall
		if (Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)), out hit, collisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.1875f, out hit, collisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.375f, out hit, collisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.5625f, out hit, collisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.75f, out hit, collisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*0.9375f, out hit, collisionLayers)
		||  Physics.Linecast(transform.position + spaceOnWallNeededToWallJumpUpAmount2 + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, transform.position + spaceOnWallNeededToWallJumpUpAmount2 - (transform.forward*(spaceOnWallNeededToWallJumpLength2 + 1)) + (transform.up*(spaceOnWallNeededToWallJumpHeight2 + 1))*1.125f, out hit, collisionLayers)){
			wallBackHit = true;
			distFromWall = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(transform.position.x, 0, transform.position.z));
		}
		else {
			wallBackHit = false;
		}
		//if player did slide off wall, set currentlyOnWall to false
		if (wallNormal != Vector3.zero
		&&  Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 0.1f && !wallBackHit){
			if (animator != null && currentlyOnWall){
				onWallAnimation = false;
			}
			currentlyOnWall = false;
		}
		
		if (wallJumping.allowWallJumping && currentlyOnWall){
			if (animator != null && !onWallAnimation && !sliding){
				
				//if the angle between the player and wall is greater than or equal to 90, turn to the left
				if (angleBetweenPlayerAndWall >= 90){
					animator.SetFloat("wallState", 4);
				}
				//if the angle between the player and wall is less than 90, turn to the right
				else {
					animator.SetFloat("wallState", 3);
				}
				animator.CrossFade("WallJump", 0f, -1, 0f);
				onWallAnimation = true;

			}
			
			wallJumpTimer += 0.02f;
			moveDirection = Vector3.zero;
			if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 0.1f && wallJumping.slideDownWalls && noCollisionTimer < 5){
				wallHitPoint += transform.forward/125;
			}
			transform.position = Vector3.Lerp(transform.position, new Vector3(wallHitPoint.x, transform.position.y, wallHitPoint.z), 10 * Time.deltaTime);
			if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) >= 0.01f){
				firstDistFromWall = distFromWall;
			}
			//if the wall becomes further away from the player (most likely because it is sloped), push the player backwards towards the wall
			else if (distFromWall - firstDistFromWall > 0.01f) {
				transform.position -= transform.forward/125;
			}
			if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 1){
				turningToWall = false;
			}
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(wallNormal), (wallJumping.rotationToWallSpeed*2) * Time.deltaTime);
			originalWallJumpDirection = (transform.forward * wallJumping.wallJumpDistance) + (transform.up * wallJumping.wallJumpHeight);
			wallJumpDirection = (transform.forward * wallJumping.wallJumpDistance) + (transform.up * wallJumping.wallJumpHeight);
			
			//sliding down walls
			if (wallJumping.slideDownWalls && wallJumping.slideDownSpeed != 0){
				
				if (slideDownSpeed2 < gravity){
					slideDownSpeed2 += (wallJumping.slideDownSpeed/100) * wallJumpTimer;
				}
				else {
					slideDownSpeed2 = gravity;
				}
				
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					GetComponent<CharacterController>().Move(new Vector3(0, -slideDownSpeed2, 0) * Time.deltaTime);
				}
				else if (GetComponent<Rigidbody>()){
					GetComponent<Rigidbody>().MovePosition(transform.position + new Vector3(0, -slideDownSpeed2, 0) * Time.deltaTime);
				}
				
				//if the player has finished rotating and is colliding with the wall, push the player slightly forward to avoid becoming stuck
				if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(wallNormal)) < 0.1f && noCollisionTimer < 5){
					wallHitPoint = transform.position + transform.forward/125;
				}
				
			}
			
			if (Input.GetButtonDown("Jump") && canCrouchToAction){
				inMidAirFromWallJump = true;
				doubleJumpPossible = true;
				WallJump();
				currentlyOnWall = false;
			}
			if (wallJumping.useWallJumpTimeLimit && wallJumpTimer > wallJumping.wallJumpTimeLimit){
				currentlyOnWall = false;
			}
			
		}
		else if (animator != null){
			onWallAnimation = false;
		}
		if (!currentlyOnWall){
			turningToWall = false;
		}
		
		if (grounded.currentlyGrounded){
			currentlyOnWall = false;
			if (inMidAirFromWallJump){
				transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
				inMidAirFromWallJump = false;
			}
			if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("WallJump") && animator.GetFloat("wallState") == 0){
				animator.CrossFade("Movement", 0f, -1, 0f);
			}
		}
		
	}
	
	void PerformWallJump () {
		
		if (inMidAirFromWallJump){
			//animating jumping on to wall
			if (animator != null && onWallAnimation){
				
				//if the angle between the player and wall is greater than or equal to 90, turn to the left
				if (angleBetweenPlayerAndWall >= 90){
					animator.SetFloat("wallState", 2);
				}
				//if the angle between the player and wall is less than 90, turn to the right
				else {
					animator.SetFloat("wallState", 1);
				}
				animator.CrossFade("WallJump", 0f, -1, 0f);
				onWallAnimation = false;
			}
			
			if (jumpedOffWallForWallJump){
				originalWallJumpDirection = Vector3.Lerp(originalWallJumpDirection, new Vector3(0, originalWallJumpDirection.y, 0), wallJumping.wallJumpDecelerationRate * Time.deltaTime);
				wallJumpDirection = Vector3.Lerp(wallJumpDirection, new Vector3(0, wallJumpDirection.y, 0), wallJumping.wallJumpDecelerationRate * Time.deltaTime);
				if (noCollisionTimer >= 5 && !currentlyOnWall){
					//if neither axis is locked
					if (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
						wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * playerCamera.transform.right);
						wallJumpDirection += (Input.GetAxis("Vertical")*(wallJumping.overallMovementSpeed/8) * playerCamera.transform.forward);
					}
					else {
						if (movement.sideScrolling.lockMovementOnXAxis){
							//if the rotation on the axis is not flipped
							if (!movement.sideScrolling.flipAxisRotation){
								if (transform.eulerAngles.y >= 270 && transform.eulerAngles.y <= 360 || transform.eulerAngles.y <= 90 && transform.eulerAngles.y >= -90){
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
							//if the rotation on the axis is flipped
							else {
								if (transform.eulerAngles.y >= 270 || transform.eulerAngles.y <= 90 && transform.eulerAngles.y >= -90){
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
						}
						if (movement.sideScrolling.lockMovementOnZAxis){
							//if the rotation on the axis is not flipped
							if (!movement.sideScrolling.flipAxisRotation){
								if (transform.eulerAngles.y >= 0 && transform.eulerAngles.y <= 180){
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
							//if the rotation on the axis is flipped
							else {
								if (transform.eulerAngles.y >= 0 && transform.eulerAngles.y <= 180){
									wallJumpDirection += (-Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
								else {
									wallJumpDirection += (Input.GetAxis("Horizontal")*(wallJumping.overallMovementSpeed/8) * transform.forward);
								}
							}
						}
					}
					
					//moving forward
					if (forwardDir > originalForwardDir){
						
						wallJumpDirection -= (transform.forward*(forwardDir/originalForwardDir))*(-(wallJumping.forwardMovementSpeedMultiple*0.1f + 0.9f) + 1);
						
					}
					//moving backwards
					else if (forwardDir < originalForwardDir){
						
						wallJumpDirection += (transform.forward/(forwardDir/originalForwardDir))*(-(wallJumping.backMovementSpeedMultiple*0.1f + 0.9f) + 1);
						
					}
					
					//moving right
					if (rightDir > originalRightDir){
						
						wallJumpDirection -= (transform.right*((rightDir - originalRightDir)*originalRightDir))*(-(wallJumping.sideMovementSpeedMultiple/2 + 0.5f) + 1);
						
					}
					//moving left
					else if (rightDir < originalRightDir){
						
						wallJumpDirection += (transform.right/((rightDir/originalRightDir)*2))*(-(wallJumping.sideMovementSpeedMultiple*0.075f + 0.925f) + 1);
						
					}
					
				}
			}
			
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				GetComponent<CharacterController>().Move(new Vector3(wallJumpDirection.x, 0, wallJumpDirection.z) * Time.deltaTime);
			}
			else if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().MovePosition(transform.position + new Vector3(wallJumpDirection.x, 0, wallJumpDirection.z) * Time.deltaTime);
			}
			
		}
		
		//if player is no longer on wall or is on the ground, set wallState to 0
		if (animator != null){
			if (!currentlyOnWall && (animator.GetFloat("wallState") == 3 || animator.GetFloat("wallState") == 4) || grounded.currentlyGrounded){
				animator.SetFloat("wallState", 0);
			}
		}
		
	}
	
	void CheckIfPlayerCanClimb () {
		
		//enabling and disabling scripts (and warning the user if any script names they entered do not exist on the player) when climbing on wall
		ScriptEnablingDisabling();
		
		//Climbing variables
		i++;
		if (i == climbing.Length){
			i = 0;
		}
		if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
			SetClimbingVariables();
		}
		//detecting whether the player can climb or not
		if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			//setting initial rotation
			if (!currentlyClimbingWall){
				rotationNormal = Quaternion.LookRotation(-hit.normal);
				rotationState = 1;
			}
			
			firstTest = true;
			//if we are not over an edge of a climbable object, the object can be climbed
			if (currentlyClimbingWall || !currentlyClimbingWall && !reachedTopPoint && !reachedBottomPoint && (!reachedLeftPoint && !reachedRightPoint || !stopAtSides2)){
				wallIsClimbable = true;
			}
			//if we are over an edge of a climbable object, the object cannot be climbed
			else if (!currentlyClimbingWall){
				wallIsClimbable = false;
			}
			climbableWallInFront = true;
			
			//if we are currently climbing the wall and are no longer rotating, set finishedRotatingToWall to true
			if (currentlyClimbingWall && transform.rotation == lastRot2){
				finishedRotatingToWall = true;
			}
			
		}
		else if (!pullingUp){
			firstTest = false;
			wallIsClimbable = false;
			climbableWallInFront = false;
			
			//if there is a wall in front of the player, but we are not actually on the wall, set currentlyClimbingWall to false
			if (transform.rotation == lastRot2 && wallInFront){
				if (animator != null && animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing") && !currentlyClimbingWall){
					animator.CrossFade("Movement", 0f, -1, 0f);
				}
				currentlyClimbingWall = false;
			}
			
			//if we are not climbing the wall yet
			if (!currentlyClimbingWall){
				finishedRotatingToWall = false;
			}
		}
		
		//checking if there is a wall in front of the player
		if ((Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers))
		
		&& (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, out hit, collisionLayers))
		
		&& (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.25f, out hit, collisionLayers)
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.25f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.25f, out hit, collisionLayers))){
			
			wallInFront = true;
			
		}
		else {
			wallInFront = false;
		}
		
		//Climbing variables
		i++;
		if (i == climbing.Length){
			i = 0;
		}
		if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
			SetClimbingVariables();
		}
		//getting center of climbable object
		if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*2*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			secondTest = true;
			centerPoint = new Vector3(hit.transform.position.x - transform.position.x, 0, hit.transform.position.z - transform.position.z);
			
		}
		else {
			secondTest = false;
		}
		
	}
	
	void ClimbingWall () {
		
		if (currentlyClimbingWall && !pullingUp){
			
			//if the horizontal axis is switched, set switchedDirection to true
			if (Input.GetAxisRaw("Horizontal") > 0 && lastAxis <= 0 || Input.GetAxisRaw("Horizontal") < 0 && lastAxis >= 0){
				switchedDirection = true;
			}
			lastAxis = Input.GetAxisRaw("Horizontal");
			
			//climbing down/off of ladder or wall
			if (Input.GetAxisRaw("Vertical") >= 0 || switchedDirection && !downCanBePressed || !grounded.currentlyGrounded){
				downCanBePressed = true;
				if (Input.GetAxisRaw("Vertical") > 0 || !grounded.currentlyGrounded){
					climbedUpAlready = true;
				}
			}
			else if (downCanBePressed && grounded.currentlyGrounded){
				if (dropOffAtFloor2){
					moveDirection = Vector3.zero;
					jumpedOffClimbableObjectTimer = 0;
					jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
					inMidAirFromJump = false;
					inMidAirFromWallJump = false;
					currentJumpNumber = totalJumpNumber;
					moveSpeed = 0;
					currentlyClimbingWall = false;
				}
				else {
					downCanBePressed = false;
					climbedUpAlready = false;
					reachedBottomPoint = true;
				}
			}
			switchedDirection = false;
			
			//if player's movement is stopped at the bottom, set the speed and direction of the player to 0
			if (Input.GetAxisRaw("Vertical") < 0 && Input.GetAxisRaw("Horizontal") == 0 && (!downCanBePressed || reachedBottomPoint && (!dropOffAtBottom2 || grounded.currentlyGrounded && !dropOffAtFloor2))){
				climbDirection = Vector3.zero;
				moveDirection = Vector3.zero;
				moveSpeed = 0;
			}
			
			//rotating player to face the wall
			WallClimbingRotation();
			
			//checking to see if player is stuck on wall
			CheckIfStuck();
			
			//moving in bursts
			if (moveInBursts){
				
				if (beginClimbBurst){
					climbingMovement = Mathf.Lerp(climbingMovement, climbMovementSpeed2, ((2 + climbMovementSpeed2)*(climbingMovement/2 + 1))/burstLength * Time.deltaTime);
				}
				else {
					climbingMovement = Mathf.Lerp(climbingMovement, 0, ((2 + climbMovementSpeed2)*(climbingMovement/2 + 1))/burstLength * Time.deltaTime);
				}
				if (climbMovementSpeed2 - climbingMovement < 0.1f){
					beginClimbBurst = false;
				}
				else if (climbingMovement < 0.1f){
					beginClimbBurst = true;
				}
				
			}
			else {
				climbingMovement = climbMovementSpeed2;
			}
			
			//getting direction to climb towards
			if (directionVector.magnitude != 0){
				
				//climbing horizontally and vertically
				if (climbHorizontally2 && climbVertically2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
					
					//if we have reached the top, bottom, right, or left point, do not allow movement in any direction
					if (stopAtSides2 && (reachedRightPoint && Input.GetAxis("Horizontal") > 0 || reachedLeftPoint && Input.GetAxis("Horizontal") < 0)
					&& (!downCanBePressed || reachedTopPoint && Input.GetAxis("Vertical") > 0 || reachedBottomPoint && Input.GetAxis("Vertical") < 0)){
						climbDirection = Vector3.zero;
					}
					//if we have reached the left or right point, do not allow horizontal movement in that direction
					else if (downCanBePressed && stopAtSides2 && (reachedRightPoint && Input.GetAxis("Horizontal") > 0 || reachedLeftPoint && Input.GetAxis("Horizontal") < 0)
					&& ((!reachedTopPoint || Input.GetAxis("Vertical") <= 0) && (!reachedBottomPoint || Input.GetAxis("Vertical") >= 0))){
						climbDirection = (Input.GetAxis("Vertical")*transform.up) * climbingMovement;
					}
					//if down cannot be pressed, or we have reached the top or bottom point, do not allow vertical movement in that direction
					else if (!downCanBePressed || reachedTopPoint && Input.GetAxis("Vertical") > 0 || reachedBottomPoint && Input.GetAxis("Vertical") < 0){
						climbDirection = (Input.GetAxis("Horizontal")*transform.right) * climbingMovement;
					}
					//if down can be pressed, and we have not reached the top, bottom, right, or left point, allow movement in every direction
					else if (downCanBePressed){
						climbDirection = (Input.GetAxis("Horizontal")*transform.right + Input.GetAxis("Vertical")*transform.up) * climbingMovement;
					}
					
				}
				//climbing horizontally
				else if (climbHorizontally2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
					
					//if we have not reached the sides of the climbable object
					if (!stopAtSides2 || (!reachedRightPoint || Input.GetAxis("Horizontal") < 0) && (!reachedLeftPoint || Input.GetAxis("Horizontal") > 0)){
						climbDirection = (Input.GetAxis("Horizontal")*transform.right) * climbingMovement;
					}
					else {
						climbDirection = Vector3.zero;
					}
					
				}
				//climbing vertically
				else if (climbVertically2){
					
					//if down cannot be pressed or we have reached the top point, do not allow vertical movement in that direction
					if (!downCanBePressed || reachedTopPoint && Input.GetAxis("Vertical") > 0 || reachedBottomPoint && Input.GetAxis("Vertical") < 0){
						climbDirection = Vector3.zero;
					}
					//if not, allow vertical movement
					else if (downCanBePressed){
						climbDirection = (Input.GetAxis("Vertical")*transform.up) * climbingMovement;
					}
				}
				else {
					climbDirection = Vector3.zero;
				}
				
			}
			else {
				climbDirection = Vector3.Slerp(climbDirection, Vector3.zero, 15 * Time.deltaTime);
			}
			
			//moving player on wall
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				GetComponent<CharacterController>().Move(climbDirection * Time.deltaTime);
			}
			else if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().MovePosition(transform.position + climbDirection * Time.deltaTime);
			}
			
			
			//setting player's side-scrolling rotation to what it is currently closest to (if we are side scrolling / an axis is locked)
			float yRotationValue;
			if (transform.eulerAngles.y > 180){
				yRotationValue = transform.eulerAngles.y - 360;
			}
			else {
				yRotationValue = transform.eulerAngles.y;
			}
			//getting rotation on z-axis (x-axis is locked)
			if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (yRotationValue >= 90){
					horizontalValue = -1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = 180.001f;
					}
					else {
						bodyRotation = 179.999f;
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					horizontalValue = 1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = -0.001f;
					}
					else {
						bodyRotation = 0.001f;
					}
				}
			}
			//getting rotation on x-axis (z-axis is locked)
			else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
				//if our rotation is closer to the right, set the bodyRotation to the right
				if (yRotationValue >= 0){
					horizontalValue = 1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = 90.001f;
					}
					else {
						bodyRotation = 89.999f;
					}
				}
				//if our rotation is closer to the left, set the bodyRotation to the left
				else {
					horizontalValue = -1;
					if (movement.sideScrolling.rotateInwards){
						bodyRotation = -90.001f;
					}
					else {
						bodyRotation = -89.999f;
					}
				}
			}
			
		}
		else {
			climbDirection = Vector3.zero;
			downCanBePressed = false;
			climbedUpAlready = false;
			climbingMovement = 0;
			beginClimbBurst = true;
			switchedDirection = false;
		}
		lastAxis = Input.GetAxisRaw("Horizontal");
		
	}
	
	void JumpOffOfClimb () {
		
		//jumping off of ladder/wall
		if ((currentlyClimbingWall || turnBack || back2) && Input.GetButtonDown("Jump")){
			moveDirection = Vector3.zero;
			jumpedOffClimbableObjectTimer = 0;
			if (turnBack || back2){
				transform.rotation = backRotation;
			}
			jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
			inMidAirFromJump = false;
			inMidAirFromWallJump = false;
			currentJumpNumber = totalJumpNumber;
			moveSpeed = 0;
			turnBack = false;
			back2 = false;
			grounded.currentlyGrounded = true;
			jumpPressed = false;
			jumpPossible = true;
			doubleJumpPossible = true;
			Jump();
			currentlyClimbingWall = false;
		}
		PushOffWall();
		
	}
	
	void ClimbableObjectEdgeDetection () {
		
		//snapping to the center of the ladder
		SnapToCenter();
		
		//grabbing on to a ladder after walking off of a ledge
		TurnToGrabLadder();
		
		//determining if player has reached any of the edges of the climbable object
		CheckClimbableEdges();
		
		//set values to 0 and false if not climbing wall
		if (!currentlyClimbingWall){
			climbDirection = Vector3.zero;
			pullUpLocationAcquired = false;
			movingToPullUpLocation = false;
			pullingUp = false;
		}
		
	}
	
	void PullingUpClimbableObject () {
		
		//pulling up once player has reached the top of ladder
		if (pullingUp){
			pullingUpTimer += 0.02f;
			
			if (pullUpLocationAcquired){
				if (Physics.Linecast(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward2), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward2), out hit, collisionLayers)) {
					hitPoint = hit.point;
				}
				pullUpLocationAcquired = false;
			}
			
			if (movingToPullUpLocation){
				movementVector = (new Vector3(transform.position.x, hitPoint.y + transform.up.y/10, transform.position.z) - transform.position).normalized * pullUpSpeed;
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					GetComponent<CharacterController>().Move(movementVector * Time.deltaTime);
				}
				else if (GetComponent<Rigidbody>()){
					GetComponent<Rigidbody>().MovePosition(transform.position + movementVector * Time.deltaTime);
				}
			}
			if (Vector3.Distance(transform.position, new Vector3(transform.position.x, hitPoint.y + transform.up.y/10, transform.position.z)) <= 0.2f || pullingUpTimer > Mathf.Abs((pullUpSpeed/4)) && onWallLastUpdate){
				pullUpLocationAcquired = false;
				movingToPullUpLocation = false;
			}
			
			if (!movingToPullUpLocation){
				transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
				movementVector = ((hitPoint + transform.forward/10) - transform.position).normalized * pullUpSpeed;
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					GetComponent<CharacterController>().Move(movementVector * Time.deltaTime);
				}
				else if (GetComponent<Rigidbody>()){
					GetComponent<Rigidbody>().MovePosition(transform.position + movementVector * Time.deltaTime);
				}
			}
			if (Vector3.Distance(transform.position, (hitPoint + transform.forward/10)) <= 0.2f || pullingUpTimer > Mathf.Abs((pullUpSpeed/4)) && onWallLastUpdate){
				movingToPullUpLocation = false;
				wallIsClimbable = false;
				currentlyClimbingWall = false;
				pullingUp = false;
			}
		}
		else {
			pullingUpTimer = 0;
		}
		
	}
	
	void AnimatingClimbing () {
		
		//animating player climbing wall
		if (animator != null){
			
			if (!pullingUp){
				
				//animating player when he turns on to wall
				if (currentlyClimbingWall || turnBack || back2){
					if (animator.GetFloat("climbState") < 1){
						animator.CrossFade("Climbing", 0f, -1, 0f);
						animator.SetFloat("climbState", 1);
					}
				}
				else {
					if ((grounded.currentlyGrounded && animator.GetFloat("climbState") != 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")) && !inMidAirFromJump){
						animator.CrossFade("Movement", 0f, -1, 0f);
					}
					animator.SetFloat("climbState", 0);
					
				}
				
				//animating the player's climbing motions while he is on a wall
				if (currentlyClimbingWall){
					if (climbHorizontally2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
						//if we have not reached the sides of the climbable object
						if (!stopAtSides2 || (!reachedRightPoint || Input.GetAxis("Horizontal") < 0) && (!reachedLeftPoint || Input.GetAxis("Horizontal") > 0)){
							animator.SetFloat("climbState", Mathf.Abs(Input.GetAxis("Horizontal")) + 1);
						}
						else {
							animator.SetFloat("climbState", Mathf.Lerp(animator.GetFloat("climbState"), 1, 8 * Time.deltaTime));
						}
					}
					if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")){
						animator.CrossFade("Climbing", 0f, -1, 0f);
					}
				}
				
				//horizontal movement
				if (Input.GetAxis("Horizontal") > 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && (!reachedRightPoint || !stopAtSides2)){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the right while grabbed on to a ledge
					if (horizontalClimbSpeed <= 0 || climbingMovement < 0.1f){
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
					}
					horizontalClimbSpeed = 1;
					
				}
				else if (Input.GetAxis("Horizontal") < 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && (!reachedLeftPoint || !stopAtSides2)){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the left while grabbed on to a ledge
					if (horizontalClimbSpeed >= 0 || climbingMovement < 0.1f){
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
					}
					horizontalClimbSpeed = -1;
					
				}
				else {
					animator.SetFloat ("climbSpeedHorizontal", Mathf.Lerp(animator.GetFloat("climbSpeedHorizontal"), 0, 15 * Time.deltaTime) );
				}
				//vertical movement
				if (Input.GetAxis("Vertical") > 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && !reachedTopPoint){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the right while grabbed on to a ledge
					if (verticalClimbSpeed <= 0 || climbingMovement < 0.1f){
						//immediately transitioning to climbing animation
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
						
						//switching climbing arms
						if (arm == 1){
							arm = 2;
						}
						else {
							arm = 1;
						}
					}
					verticalClimbSpeed = 1;
					
				}
				else if (Input.GetAxis("Vertical") < 0 && currentlyClimbingWall && climbMovementSpeed2 > 0 && downCanBePressed && !reachedBottomPoint){
					
					animator.speed = ((climbMovementSpeed2/3)/burstLength)/((climbMovementSpeed2*2)/(3 + climbMovementSpeed2));
					
					//animating moving to the left while grabbed on to a ledge
					if (verticalClimbSpeed >= 0 || climbingMovement < 0.1f){
						//immediately transitioning to climbing animation
						animator.CrossFade("Climbing", 0.5f, -1, 0f);
						
						//switching climbing arms
						if (arm == 1){
							arm = 2;
						}
						else {
							arm = 1;
						}
					}
					verticalClimbSpeed = -1;
					
				}
				else {
					animator.SetFloat ("climbSpeedVertical", Mathf.Lerp(animator.GetFloat("climbSpeedVertical"), 0, 15 * Time.deltaTime) );
				}
				//switching arm to climb with in animator
				animator.SetFloat ("armToClimbWith", Mathf.Lerp(animator.GetFloat("armToClimbWith"), arm, 15 * Time.deltaTime) );
				
				//setting climbing speeds in animator
				if (currentlyClimbingWall && climbMovementSpeed2 > 0){
					
					//setting vertical climb speed in animator
					if (Input.GetAxis("Vertical") != 0 && climbVertically2 && climbedUpAlready){
						if ((!reachedTopPoint || Input.GetAxis("Vertical") < 0) && (!reachedBottomPoint || Input.GetAxis("Vertical") > 0)){
							animator.SetFloat ("climbSpeedVertical", Mathf.Lerp(animator.GetFloat("climbSpeedVertical"), verticalClimbSpeed, 15 * Time.deltaTime) );
						}
					}
					//setting horizontal climb speed in animator
					if (Input.GetAxis("Horizontal") != 0 && climbHorizontally2 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)){
						//if we have not reached sides or are not moving toward them
						if (!stopAtSides2 || (!reachedRightPoint || Input.GetAxis("Horizontal") < 0) && (!reachedLeftPoint || Input.GetAxis("Horizontal") > 0)){
							animator.SetFloat ("climbSpeedHorizontal", Mathf.Lerp(animator.GetFloat("climbSpeedHorizontal"), horizontalClimbSpeed, 15 * Time.deltaTime) );
						}
					}
				}
				else {
					animator.SetFloat ("climbSpeedVertical", 0);
					animator.SetFloat ("climbSpeedHorizontal", 0);
				}
				
			}
			
			//animating climbing over a ledge
			if (pullingUp){
				if (animator.GetFloat("climbState") != 0 || !animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")){
					if (!animatePullingUp){
						if (onWallLastUpdate){
							animator.speed = Mathf.Abs(pullUpSpeed/4);
						}
						else {
							animator.speed = pullUpSpeed/4;
						}
						animator.SetFloat ("climbState", 0);
						animator.CrossFade("Climbing", 0f, -1, 0f);
						animatePullingUp = true;
					}
				}
			}
			else if (animatePullingUp){
				animator.speed = 1;
				animatePullingUp = false;
			}
		}
		
	}
	
	void RotatePlayer () {
		
		//if joystick/arrow keys are being pushed
		if ((directionVector.magnitude > 0 || (movement.sideScrolling.lockMovementOnXAxis || movement.sideScrolling.lockMovementOnZAxis)) && !currentlyClimbingWall && !turnBack && !back2){
			
			float myAngle = Mathf.Atan2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")) * Mathf.Rad2Deg;
			
			//normal rotation from side to side (detected by which way joystick was pushed last)
			if (!inMidAirFromWallJump && !currentlyOnWall){
				//getting which direction we are turning to (only used if axis is locked)
				if (Input.GetAxis("Horizontal") > 0){
					horizontalValue = 1;
				}
				else if (Input.GetAxis("Horizontal") < 0){
					horizontalValue = -1;
				}
			}
			//normal rotation from side to side (detected by which way the player is closest to facing)
			else {
				float yRotationValue;
				if (transform.eulerAngles.y > 180){
					yRotationValue = transform.eulerAngles.y - 360;
				}
				else {
					yRotationValue = transform.eulerAngles.y;
				}
				//getting rotation on z-axis (x-axis is locked)
				if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					//if our rotation is closer to the right, set the bodyRotation to the right
					if (yRotationValue >= 90){
						horizontalValue = -1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = 180.001f;
						}
						else {
							bodyRotation = 179.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						horizontalValue = 1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = -0.001f;
						}
						else {
							bodyRotation = 0.001f;
						}
					}
				}
				//getting rotation on x-axis (z-axis is locked)
				else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
					//if our rotation is closer to the right, set the bodyRotation to the right
					if (yRotationValue >= 0){
						horizontalValue = 1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = 90.001f;
						}
						else {
							bodyRotation = 89.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						horizontalValue = -1;
						if (movement.sideScrolling.rotateInwards){
							bodyRotation = -90.001f;
						}
						else {
							bodyRotation = -89.999f;
						}
					}
				}
			}
			
			if (!currentlyOnWall){
				//getting rotation on z-axis (x-axis is locked)
				if (movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					//moving to right
					if (horizontalValue == 1){
						//if the rotation on the axis is not flipped
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 180.001f;
							}
							else {
								bodyRotation = 179.999f;
							}
						}
						//if the rotation on the axis is flipped
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 180.001f + 180;
							}
							else {
								bodyRotation = 179.999f + 180;
							}
						}
					}
					//moving to left
					else {
						//if the rotation on the axis is not flipped
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -0.001f;
							}
							else {
								bodyRotation = 0.001f;
							}
						}
						//if the rotation on the axis is flipped
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -0.001f + 180;
							}
							else {
								bodyRotation = 0.001f + 180;
							}
						}
					}
				}
				//getting rotation on x-axis (z-axis is locked)
				else if (movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
					//moving to right
					if (horizontalValue == 1){
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 90.001f;
							}
							else {
								bodyRotation = 89.999f;
							}
						}
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = 90.001f + 180;
							}
							else {
								bodyRotation = 89.999f + 180;
							}
						}
					}
					//moving to left
					else {
						if (!movement.sideScrolling.flipAxisRotation){
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -90.001f;
							}
							else {
								bodyRotation = -89.999f;
							}
						}
						else {
							if (movement.sideScrolling.rotateInwards){
								bodyRotation = -90.001f + 180;
							}
							else {
								bodyRotation = -89.999f + 180;
							}
						}
					}
				}
				//getting rotation if neither or both axis are locked)
				else {
					bodyRotation = myAngle + playerCamera.eulerAngles.y;
				}
				
				//setting player's rotation
				//if in mid-air from wall jumping, rotate using the rotationSpeedMultiple
				if (inMidAirFromWallJump){
					//rotating in air when not side-scrolling
					if (!movement.sideScrolling.lockMovementOnZAxis && !movement.sideScrolling.lockMovementOnXAxis){
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0, bodyRotation, 0), (rotationSpeed2*wallJumping.rotationSpeedMultiple) * Time.deltaTime);
					}
					//rotating (from side to side) in air when side-scrolling
					else {
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0, bodyRotation, 0), rotationSpeed2 * Time.deltaTime);
					}
				}
				//if we are not allowed to move straight backwards or we are simply not going that direction, continue to rotate
				else if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler(0, bodyRotation, 0), rotationSpeed2 * Time.deltaTime);
					}
				}
				
				//increase the player's acceleration until accelerationRate has reached 1
				if (accelerationRate < 1){
					accelerationRate += 0.01f*acceleration2;
				}
				else {
					accelerationRate = 1;
				}
				
			}
			
		}
		else {
			accelerationRate = 0;
		}
		//if the player only rotates with the camera
		if (enabledLastUpdate){
			if (movement.firstPerson.onlyRotateWithCamera && (!currentlyClimbingWall && !turnBack && !back2 && !currentlyOnWall) && inFirstPersonMode){
				transform.eulerAngles = new Vector3(0, playerCamera.transform.eulerAngles.y, 0);
			}
		}
		enabledLastUpdate = true;
		
	}
	
	void SettingPlayerSpeed () {
		
		//getting the running speed of the player (if we are using a run button)
		if (movement.running.useRunningButton){
			if (Input.GetButton(movement.running.runInputButton) || Input.GetAxis(movement.running.runInputButton) != 0){
				runSpeedMultiplier = movement.running.runSpeedMultiple;
			}
			else {
				runSpeedMultiplier = 1;
			}
		}
		else {
			runSpeedMultiplier = 1;
		}
		
		//setting the speed of the player
		h = Mathf.Lerp(h, (Mathf.Abs(Input.GetAxisRaw ("Horizontal")) - Mathf.Abs(Input.GetAxisRaw ("Vertical")) + 1)/2, 8 * Time.deltaTime);
		v = Mathf.Lerp(v, (Mathf.Abs(Input.GetAxisRaw ("Vertical")) - Mathf.Abs(Input.GetAxisRaw ("Horizontal")) + 1)/2, 8 * Time.deltaTime);
		if (directionVector.magnitude != 0){
			if (Input.GetAxis("Vertical") >= 0){
				
				//if not side-scrolling (neither axis is locked)
				if (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (h*sideSpeed2 + v*forwardSpeed2)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((h*sideSpeed2 + v*forwardSpeed2)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
				//if side-scrolling (either axis is locked)
				else {
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
			
			}
			else {
				
				//if not side-scrolling (neither axis is locked)
				if (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis){
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (h*sideSpeed2 + v*backSpeed2)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((h*sideSpeed2 + v*backSpeed2)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
				//if side-scrolling (either axis is locked)
				else {
					if (!crouching){
						moveSpeed = (Mathf.Lerp(moveSpeed, (Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
					//if player is crouching
					else {
						moveSpeed = (Mathf.Lerp(moveSpeed, ((Mathf.Abs(Input.GetAxisRaw ("Horizontal"))*movement.sideScrolling.movementSpeedIfAxisLocked)*movement.crouching.crouchMovementSpeedMultiple)*runSpeedMultiplier, 8 * Time.deltaTime)*directionVector.magnitude)*accelerationRate;
					}
				}
				
			}
			
		}
		if (animator != null && animator.runtimeAnimatorController != null){
			animator.SetFloat ("speed", moveSpeed);
		}
		
		decelerationRate += deceleration/10;
		airSpeed = moveSpeed * midAirMovementSpeedMultiple2;
		
		//applying friction to the player's movement
		if (movement.movementFriction > 0){
			moveSpeedAndFriction = Mathf.Lerp(moveSpeedAndFriction, moveSpeed, (24/movement.movementFriction) * Time.deltaTime);
		}
		else {
			moveSpeedAndFriction = moveSpeed;
		}
		
	}
	
	void DetermineGroundedState () {
		
		//determining whether the player is grounded or not
		//drawing ground detection rays
		if (grounded.showGroundDetectionRays){
			Debug.DrawLine(pos + maxGroundedHeight2, pos + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
			Debug.DrawLine(pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, Color.yellow);
		}
		//determining if grounded
		if (Physics.Linecast(pos + maxGroundedHeight2, pos + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.right*(maxGroundedRadius2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2*0.75f) + transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)
		||	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2*0.75f) - transform.right*(maxGroundedRadius2*0.75f) + maxGroundedDistanceDown, out hit, collisionLayers)){
			if (!angHit){
				raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
			}
			grounded.currentlyGrounded = true;
		}
		else {
			grounded.currentlyGrounded = false;
		}
		
	}
	
	void GettingGroundAndSlopeAngles () {

		//determining the slope of the surface you are currently standing on
		float myAng2 = 0.0f;
		if (Physics.Raycast(pos, Vector3.down, out hit, 1f, collisionLayers)){
			angHit = true;
			myAng2 = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
		}
		else {
			angHit = false;
		}
		
		//raycasting to determine whether sliding is possible or not
		RaycastHit altHit = new RaycastHit();
		if (Physics.Raycast(pos, Vector3.down, out hit, maxGroundedDistance2, collisionLayers)){
			slidePossible = true;
			if (Physics.Raycast(pos + transform.forward/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.forward/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos + transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos + transform.forward/10 + transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos + transform.forward/10 - transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.forward/10 + transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)
			||  Physics.Raycast(pos - transform.forward/10 - transform.right/10, Vector3.down, out altHit, maxGroundedDistance2, collisionLayers)
			&& ((Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f) > ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f)){
				raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(altHit.normal.y, -1f, 1f))) * 57.2958f;
			}
			else {
				raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
			}
		}
		else if (Physics.Raycast(contactPoint + Vector3.up, Vector3.down, out hit, 5f, collisionLayers) && collisionSlopeAngle < 90 && collisionSlopeAngle > slopeLimit){
			if (angHit && myAng2 > slopeLimit || !angHit){
				slidePossible = true;
				if (angHit){
					raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
				}
			}

		}
		else if (Physics.Raycast(pos, Vector3.down, out hit, 1f, collisionLayers)){
			slidePossible = true;
			if (angHit){
				raycastSlopeAngle = (Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f;
			}
		}
		else {
			slidePossible = false;
		}
		
		
		//checking to see if player is stuck between two slopes
		CheckIfInBetweenSlopes();
		
		
		//checking to see if player is facing uphill or downhill on a slope
		if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, collisionLayers) && Physics.Raycast(pos - transform.forward/2 + transform.up, Vector3.down, out backHit, 5f, collisionLayers)){
			if (frontHit.point.y >= backHit.point.y){
				uphill = true;
			}
			else{
				uphill = false;
			}
		}
		else if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, collisionLayers)){
			uphill = true;
		}
		else {
			uphill = false;
		}
		
	}
	
	void GettingMovementDirection () {
		
		if (grounded.currentlyGrounded && (noCollisionTimer < 5 || Physics.Raycast(pos, Vector3.down, maxGroundedDistance2, collisionLayers))) {
			//since we are grounded, recalculate move direction directly from axes
			if (!jumpPerformed){
				moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			}
			else {
				moveDirection = new Vector3(Input.GetAxis("Horizontal"), moveDirection.y, Input.GetAxis("Vertical"));
			}
			if (directionVector.magnitude != 0){
				//if we are not allowed to move straight backwards or we are simply not going that direction, do not move straight backwards
				if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(transform.forward.x, moveDirection.y, transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				//if we are allowed to move straight backwards and we are going that direction, move straight backwards
				else {
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(-transform.forward.x, moveDirection.y, -transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				decelerationRate = 0;
			}
			
			
			if (directionVector.magnitude == 0 ){
				
				if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(transform.forward.x, moveDirection.y, transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				//if we are allowed to move straight backwards and we are going that direction, move straight backwards
				else {
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(-transform.forward.x, moveDirection.y, -transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				
				if(moveSpeed > 0){
					moveSpeed -= decelerationRate * moveSpeed;
				}
				if (moveSpeed <= 0){
					moveSpeed = 0;
				}
			}
			moveDirection.x *= moveSpeedAndFriction;
			moveDirection.z *= moveSpeedAndFriction;
			
			rotationSpeed2 = movement.rotationSpeed;
		}
		else {
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), moveDirection.y, Input.GetAxis("Vertical"));
			if (directionVector.magnitude != 0) {
				//if we are not allowed to move straight backwards or we are simply not going that direction, do not move straight backwards
				if ((!movement.firstPerson.walkBackwardsWhenDownKeyIsPressed || Input.GetAxis("Vertical") >= 0 || Input.GetAxis("Horizontal") != 0 || !inFirstPersonMode) && playerCamera.transform.parent != transform){
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(transform.forward.x, moveDirection.y, transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
				//if we are allowed to move straight backwards and we are going that direction, move straight backwards
				else {
					//if the player rotates by pressing the arrow keys/moving the joystick
					if (!movement.firstPerson.onlyRotateWithCamera || !inFirstPersonMode){
						moveDirection = new Vector3(-transform.forward.x, moveDirection.y, -transform.forward.z);
					}
					//if the player only rotates with the camera
					else {
						moveDirection = new Vector3((Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).x, moveDirection.y, (Quaternion.Euler(0, bodyRotation, 0) * Vector3.forward).z);
					}
				}
			}
			else {
				moveSpeed = 0;
			}
			moveDirection.x *= airSpeed;
			moveDirection.z *= airSpeed;
			
			rotationSpeed2 = movement.rotationSpeed * movement.midAirRotationSpeedMultiple;
		}
		
	}
	
	void LockAxisForSideScrolling () {
		
		//locking axis for side-scrolling
		if (movement.sideScrolling.lockMovementOnXAxis){
			moveDirection.x = 0;
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled || !currentlyOnWall && !currentlyClimbingWall && !pullingUp && !inMidAirFromWallJump){
				transform.position = new Vector3(movement.sideScrolling.xValue, transform.position.y, transform.position.z);
			}
		}
		if (movement.sideScrolling.lockMovementOnZAxis){
			moveDirection.z = 0;
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled || !currentlyOnWall && !currentlyClimbingWall && !pullingUp && !inMidAirFromWallJump){
				transform.position = new Vector3(transform.position.x, transform.position.y, movement.sideScrolling.zValue);
			}
		}
		
		if (GetComponent<Rigidbody>() && currentlyClimbingWall){
			moveDirection = Vector3.zero;
			moveSpeed = 0;
			slidingVector = Vector3.zero;
			slideMovement = Vector3.zero;
		}
		
		if (noCollisionTimer >= 5 && !grounded.currentlyGrounded || inMidAirFromJump || jumpPerformed || !sliding){
			slidingVector = Vector3.zero;
		}
		if (!angHit && noCollisionTimer < 5 && slidingVector != Vector3.zero && moveDirection.y <= -gravity){
			moveDirection.y = -gravity;
		}
		
	}
	
	void SlopeSliding () {
		
		//sliding
		if (raycastSlopeAngle > slopeLimit && collisionSlopeAngle < 89 && !jumpPerformed && !inMidAirFromJump && slidePossible && !inBetweenSlidableSurfaces){
			if (!inBetweenSlidableSurfaces && (uphill && !jumping.allowJumpWhenSlidingFacingUphill || !uphill && !jumping.allowJumpWhenSlidingFacingDownhill)){
				jumpPossible = false;
			}
			else {
				jumpPossible = true;
			}

			if (noCollisionTimer < 5 || grounded.currentlyGrounded){
				if (!sliding){
					slideSpeed = 1.0f;
				}
				sliding = true;
				if (jumping.doNotIncreaseJumpNumberWhenSliding){
					currentJumpNumber = 0;
				}
				slideMovement = Vector3.Slerp(slideMovement, new Vector3(slidingVector.x, -slidingVector.y, slidingVector.z), 6 * Time.deltaTime);
				moveDirection.x += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).x;
				moveDirection.z += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).z;
				if (noCollisionTimer < 2 || !jumpPossible){
					
					if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
						moveDirection.y += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).y;
					}
					else if (GetComponent<Rigidbody>()){
						
						if (yVel < -0.01f * gravity && Physics.Raycast(pos, Vector3.down, out hit, 1f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > slopeLimit){
							if (transform.position.y - hit.point.y < 0.2f){
								transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
							}
							else {
								moveDirection.y += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).y;
							}
						}
						
					}
					
				}
				slideSpeed += -slideMovement.y * Time.deltaTime * gravity;
				if (noCollisionTimer > 2 && !grounded.currentlyGrounded || moveDirection.y <= -gravity){
					if (!inMidAirFromJump && GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
						moveDirection.y = -gravity;
					}
				}
			}
			else if (!inMidAirFromJump && GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				if (sliding){
					moveDirection.y = -gravity;
				}
				sliding = false;
			}
			else {
				sliding = false;
			}
			
		}
		else {
			jumpPossible = true;
			sliding = false;
		}
		
		//applying friction after sliding
		if (!sliding){
			if (movement.slideFriction > 0){
				if (!inMidAirFromJump){
					slideMovement = Vector3.Slerp(slideMovement, Vector3.zero, (24/movement.slideFriction) * Time.deltaTime);
				}
				else {
					slideMovement = Vector3.Slerp(slideMovement, Vector3.zero, (24/(movement.slideFriction*1.5f)) * Time.deltaTime);
				}
				if (slideMovement != Vector3.zero){
					if (GetComponent<Rigidbody>() && !jumpPerformed && !inMidAirFromJump && grounded.currentlyGrounded && noCollisionTimer < 5 && Physics.Raycast(pos, Vector3.down, out hit, 1f, collisionLayers)){
						transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
					}
					moveDirection.x += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).x;
					moveDirection.z += (slideMovement*(slideSpeed*slopeSlideSpeed2) + new Vector3(0, -8, 0)).z;
					slideSpeed += -slideMovement.y * Time.deltaTime * gravity;
				}
			}
			else {
				slideSpeed = 1.0f;
			}
		}
		
	}
	
	void PreventBouncing () {
		
		//if we are grounded, and are no longer jumping, set jumpPerformed to false
		if (grounded.currentlyGrounded || angHit){
			if (jumpPerformed && noCollisionTimer < 5 && !inMidAirFromJump){
				jumpPerformed = false;
			}
		}
		
		//keeping player from bouncing down slopes
		if (Physics.Raycast(pos, Vector3.down, out hit, 1f, collisionLayers) && !GetComponent<Rigidbody>() || Physics.Raycast(pos + transform.forward/10, Vector3.down, out hit, 1f, collisionLayers) && GetComponent<Rigidbody>()){
			if (grounded.currentlyGrounded && !jumpPerformed){
				
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled && (raycastSlopeAngle > 1 || raycastSlopeAngle < -1)){
					//applying a downward force to keep the player from bouncing down slopes
					moveDirection.y -= hit.point.y;
					if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, collisionLayers) && Physics.Raycast(pos - transform.forward/2 + transform.up, Vector3.down, out backHit, 5f, collisionLayers)){
						
						if (frontHit.point.y < backHit.point.y){
							moveDirection.y -= hit.normal.y;
						}
						
					}
					
				}
				else if (GetComponent<Rigidbody>() && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) <= slopeLimit){
					//applying a downward force to keep the player from bouncing down slopes
					if (grounded.currentlyGrounded && (noCollisionTimer < 2 || -(moveDirection.y - GetComponent<Rigidbody>().velocity.y) < transform.position.y && moveDirection.y <= GetComponent<Rigidbody>().velocity.y + 5 - transform.position.y) && !sliding && !inMidAirFromJump && !inMidAirFromWallJump){
						moveDirection.y -= hit.point.y;
						if (Physics.Raycast(pos + transform.forward/2 + transform.up, Vector3.down, out frontHit, 5f, collisionLayers) && Physics.Raycast(pos - transform.forward/2 + transform.up, Vector3.down, out backHit, 5f, collisionLayers)){
							
							if (frontHit.point.y < backHit.point.y){
								moveDirection.y -= hit.normal.y;
							}
							
						}
					}
					
				}

			}
		}
		
	}
	
	void ApplyGravity () {
		
		//apply gravity
		if (!jumpPressed || !grounded.currentlyGrounded){
			if (!currentlyOnWall && !currentlyClimbingWall && !turnBack && !back2){
				moveDirection.y -= gravity * Time.deltaTime;
			}
		}
		
		//telling the player to not fall faster than the maximum falling speed
		if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
			if (moveDirection.y <= -maxFallingSpeed2){
				moveDirection.y = -maxFallingSpeed2;
			}
		}
		else if (GetComponent<Rigidbody>()) {
			if (GetComponent<Rigidbody>().velocity.y <= -maxFallingSpeed2){
				moveDirection.y = -maxFallingSpeed2;
			}
		}

		//if head is blocked/hits the ceiling, stop going up
		if (headHit){
			moveDirection.y = 0;
		}
		
	}
	
	void MovePlayer () {
		
		if (!currentlyOnWall && !currentlyClimbingWall){
			//if player is using a CharacterController
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				
				//applying a downward force to keep the player falling instead of slowly floating to the ground
				if (grounded.currentlyGrounded && moveDirection.y >= 0 && (noCollisionTimer > 5 || uphill) && !sliding && !jumpPerformed){
					if (Physics.Raycast(pos, Vector3.down, out hit, 1f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) <= slopeLimit){
						moveDirection.y -= -transform.position.y + hit.normal.y;
					}
				}

				// move the player
				//checking for grounded (using the position variable pos)
				if (noCollisionTimer < 5 && grounded.currentlyGrounded
				&&  Physics.Linecast(pos + maxGroundedHeight2, pos + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				//checking for grounded in a different way (using transform.position, instead of the position variable pos), and checking for sliding
				||  (raycastSlopeAngle > slopeLimit && collisionSlopeAngle < 89 && !jumpPerformed && slidePossible || inBetweenSlidableSurfaces)
				&&  (Physics.Linecast(transform.position + maxGroundedHeight2, transform.position + maxGroundedDistanceDown, out hit, collisionLayers)
				||	Physics.Linecast(transform.position - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position - transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
				||	Physics.Linecast(transform.position + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position + transform.forward*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
				||	Physics.Linecast(transform.position - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position - transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
				||	Physics.Linecast(transform.position + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, transform.position + transform.right*(maxGroundedRadius2/2) + maxGroundedDistanceDown, out hit, collisionLayers)
				||  noCollisionTimer < 5)){
					
					//moving player, and avoiding bouncing if on a sloped surface
					if ((grounded.currentlyGrounded && !inMidAirFromJump && !inMidAirFromWallJump || raycastSlopeAngle > slopeLimit && collisionSlopeAngle < 89 && !jumpPerformed && slidePossible || inBetweenSlidableSurfaces)
					&& (moveDirection.y > transform.position.y)){
						GetComponent<CharacterController>().Move((moveDirection + new Vector3(0, transform.position.y, 0)) * Time.deltaTime);
					}
					//moving player
					else {
						GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);
					}
					
				}
				else {
					//moving player
					GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);
				}
			
			}
			//if player is using a Rigidbody
			else if (GetComponent<Rigidbody>()){
			
				//applying a downward force to keep the player falling instead of slowly floating to the ground
				if (grounded.currentlyGrounded && Mathf.Abs(GetComponent<Rigidbody>().velocity.y) > 1f && moveDirection.y >= 0 && (noCollisionTimer > 5 || uphill) && !sliding && !jumpPerformed){
					if (Physics.Raycast(pos, Vector3.down, out hit, 1f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) <= slopeLimit){
						if (grounded.currentlyGrounded && (noCollisionTimer < 2 || -(moveDirection.y - GetComponent<Rigidbody>().velocity.y) < transform.position.y && moveDirection.y <= GetComponent<Rigidbody>().velocity.y + 5 - transform.position.y) && !sliding && !inMidAirFromJump && !inMidAirFromWallJump){
							moveDirection.y -= -transform.position.y + hit.normal.y;
						}
					}
				}
			
				// move the player
				//checking for grounded (using the position variable pos)
				if (grounded.currentlyGrounded && !sliding && noCollisionTimer < 5
				&&  Physics.Linecast(pos + maxGroundedHeight2, pos + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos - transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos + transform.forward*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.forward*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos - transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos - transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)
				&&	Physics.Linecast(pos + transform.right*(maxGroundedRadius2/2) + maxGroundedHeight2, pos + transform.right*(maxGroundedRadius2/2) + (maxGroundedDistanceDown * ((raycastSlopeAngle/90) + 1)), out hit, collisionLayers)){
					
					//moving player, and avoiding bouncing if on a sloped surface
					if (raycastSlopeAngle <= slopeLimit && grounded.currentlyGrounded && (noCollisionTimer < 2 || -(moveDirection.y - GetComponent<Rigidbody>().velocity.y) < transform.position.y && moveDirection.y <= GetComponent<Rigidbody>().velocity.y + 5 - transform.position.y) && !sliding && !inMidAirFromJump && !inMidAirFromWallJump){
						GetComponent<Rigidbody>().velocity = moveDirection + new Vector3(0, transform.position.y, 0);
					}
					//moving player
					else {
						GetComponent<Rigidbody>().velocity = moveDirection;
					}
					
				}
				else {
					//moving player
					GetComponent<Rigidbody>().velocity = moveDirection;
				}
				
			}
			
			//hard sticking to the ground
			if (movement.hardStickToGround){
				if (Physics.Linecast(transform.position, transform.position - transform.up/10, out hit, collisionLayers) && !jumpPressed && !jumpPerformed && !inMidAirFromJump && !inMidAirFromWallJump){
					transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
				}
			}
		}
		
	}
	
	void AvoidFallingWhileClimbing () {
		
		//keeping player from randomly dropping while on wall (at the end of the function)
		if (currentlyClimbingWall && !pullingUp){
			if (Mathf.Abs(transform.position.y - lastYPosOnWall) >= 0.2f * (climbMovementSpeed2/4) && lastYPosOnWall != 0){
				transform.position = new Vector3(transform.position.x, climbingHeight, transform.position.z);
			}
			else {
				climbingHeight = transform.position.y;
			}
			lastYPosOnWall = transform.position.y;
		}
		else {
			lastYPosOnWall = 0;
		}
		
	}
	
	void SwitchingToFirstPersonMode () {
		
		//getting first person settings from camera (if possible)
		if (movement.firstPerson.useCameraControllerSettingsIfPossible && playerCamera.GetComponent<CameraController>()){
			movement.firstPerson.alwaysUseFirstPerson = playerCamera.GetComponent<CameraController>().firstPerson.alwaysUseFirstPerson;
			movement.firstPerson.switchToFirstPersonIfInputButtonPressed = playerCamera.GetComponent<CameraController>().firstPerson.switchToFirstPersonIfInputButtonPressed;
			movement.firstPerson.firstPersonInputButton = playerCamera.GetComponent<CameraController>().firstPerson.firstPersonInputButton;
			movement.firstPerson.startOffInFirstPersonModeForSwitching = playerCamera.GetComponent<CameraController>().firstPerson.startOffInFirstPersonModeForSwitching;
			movement.firstPerson.onlyRotateWithCamera = playerCamera.GetComponent<CameraController>().inFirstPersonMode;
			movement.firstPerson.onlyRotateWithCamera = playerCamera.GetComponent<CameraController>().firstPerson.mouseOrbiting.mouseOrbitInFirstPersonMode;
		}
		//if the firstPersonInputButton has been pressed and we are entering first person mode, set firstPersonButtonPressed to true
		if (movement.firstPerson.switchToFirstPersonIfInputButtonPressed){
			if (firstPersonStart){
				firstPersonButtonPressed = true;
			}
			else {
				firstPersonButtonPressed = false;
			}
		}
		else {
			firstPersonButtonPressed = false;
		}
		//determining if we are using first person mode or not
		if (movement.firstPerson.alwaysUseFirstPerson || movement.firstPerson.switchToFirstPersonIfInputButtonPressed && firstPersonButtonPressed){
			inFirstPersonMode = true;
		}
		else {
			inFirstPersonMode = false;
		}
		
	}
	
	void CrouchAttack () {
		
		//crouch attacking
		if (movement.crouching.allowCrouching){
			//if the player attacks while crouching: perform attack
			if (crouching && attackPressed && finishedCrouching && attacking.allowCrouchAttack && attackTimer > attacking.timeLimitBetweenCrouchAttacks){
				crouchCancelsAttack = false;
				attackFinished = false;
				attackFinishedLastUpdate = false;
				if (animator != null){
					animator.SetFloat("attackState", 2);
				}
				Attack();
			}
		}
		//if the crouch attack has finished, set crouchCancelsAttack to true
		if ((crouching || canCrouch) && !Input.GetButtonDown(attacking.attackInputButton) && attackFinishedLastUpdate){
			crouchCancelsAttack = true;
		}
		if (!crouching && !canCrouch || !movement.crouching.allowCrouching){
			crouchCancelsAttack = false;
		}
		//if the crouch attack finished in the last update
		if (attackFinished){
			attackFinishedLastUpdate = true;
		}
		else {
			attackFinishedLastUpdate = false;
		}
		attackFinished = true;
		
	}
	
	void LateUpdate () {
		
		//checking to see if the first person button has been pressed
		if (Input.GetButtonDown(movement.firstPerson.firstPersonInputButton) && movement.firstPerson.switchToFirstPersonIfInputButtonPressed){
			if (firstPersonStart){
				firstPersonStart = false;
			}
			else {
				firstPersonStart = true;
			}
		}
		else if (!movement.firstPerson.switchToFirstPersonIfInputButtonPressed){
			firstPersonStart = false;
		}
		
		if (enabledLastUpdate){
			//if the player only rotates with the camera
			if (movement.firstPerson.onlyRotateWithCamera && (!currentlyClimbingWall && !turnBack && !back2 && !currentlyOnWall) && inFirstPersonMode){
				transform.eulerAngles = new Vector3(0, playerCamera.transform.eulerAngles.y, 0);
			}
		}
		enabledLastUpdate = true;
		
	}
	
	void DrawClimbingDetectors () {
		
		//showing climbing detectors
		for (int i = 0; i < climbing.Length; i++) {
			
			//setting the climbing variables that we will be using to draw
			showClimbingDetectors3 = climbing[i].showClimbingDetectors;
			climbingSurfaceDetectorsUpAmount3 = transform.up * (climbing[i].climbingSurfaceDetectorsUpAmount + 0.2f);
			climbingSurfaceDetectorsHeight3 = climbing[i].climbingSurfaceDetectorsHeight - 0.18f;
			climbingSurfaceDetectorsLength3 = climbing[i].climbingSurfaceDetectorsLength - 0.5f;
			showEdgeOfObjectDetctors3 = climbing[i].showEdgeOfObjectDetctors;
			bottomNoSurfaceDetectorHeight3 = transform.up*(climbing[i].bottomNoSurfaceDetectorHeight + 0.15f);
			topNoSurfaceDetectorHeight3 = transform.up*(climbing[i].topNoSurfaceDetectorHeight + 0.15f);
			topAndBottomNoSurfaceDetectorsWidth3 = transform.right*(climbing[i].topAndBottomNoSurfaceDetectorsWidth);
			sideNoSurfaceDetectorsHeight3 = climbing[i].sideNoSurfaceDetectorsHeight - 0.15f;
			sideNoSurfaceDetectorsWidth3 = transform.right*(climbing[i].sideNoSurfaceDetectorsWidth - 0.25f);
			showPullUpDetector3 = climbing[i].showPullUpDetector;
			pullUpLocationForward3 = transform.forward*(climbing[i].pullUpLocationForward);
			//walk off ledge then transition to climb variables
			showWalkingOffLedgeRays3 = climbing[i].walkingOffOfClimbableSurface.showWalkingOffLedgeRays;
			spaceInFrontNeededToGrabBackOn3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.spaceInFrontNeededToGrabBackOn + 0.03f);
			firstSideLedgeDetectorsHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.firstSideLedgeDetectorsHeight);
			secondSideLedgeDetectorsHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.secondSideLedgeDetectorsHeight);
			thirdSideLedgeDetectorsHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.thirdSideLedgeDetectorsHeight);
			sideLedgeDetectorsWidth3 = transform.right*(climbing[i].walkingOffOfClimbableSurface.sideLedgeDetectorsWidth);
			sideLedgeDetectorsLength3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.sideLedgeDetectorsLength);
			spaceBelowNeededToGrabBackOnHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnHeight + 0.07f);
			spaceBelowNeededToGrabBackOnForward3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnForward + 0.06f);
			grabBackOnLocationHeight3 = transform.up*(climbing[i].walkingOffOfClimbableSurface.grabBackOnLocationHeight);
			grabBackOnLocationWidth3 = transform.right*(climbing[i].walkingOffOfClimbableSurface.grabBackOnLocationWidth);
			grabBackOnLocationForward3 = transform.forward*(climbing[i].walkingOffOfClimbableSurface.grabBackOnLocationForward);
			
			if (showClimbingDetectors3){
				//middle
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.1875f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.375f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.5625f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.75f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*0.9375f, Color.red);
				Debug.DrawLine(transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight3 + 1))*1.125f, Color.red);
			}
			//drawing the rays that detect the top and bottom of a climbable object
			if (showEdgeOfObjectDetctors3){
				//bottom
				Debug.DrawLine(transform.position + bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*0.1875f, Color.cyan);
				Debug.DrawLine(transform.position + bottomNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*0.1875f, Color.cyan);
				Debug.DrawLine(transform.position + bottomNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*0.1875f, Color.cyan);
				//top
				Debug.DrawLine(transform.position + topNoSurfaceDetectorHeight3 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*1.125f, Color.cyan);
				Debug.DrawLine(transform.position + topNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight3 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*1.125f, Color.cyan);
				Debug.DrawLine(transform.position + topNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight3 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + transform.up*1.125f, Color.cyan);
				//right
				Debug.DrawLine(transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				//left
				Debug.DrawLine(transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + (((topNoSurfaceDetectorHeight3 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight3 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight3 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
				Debug.DrawLine(transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), transform.position + ((bottomNoSurfaceDetectorHeight3 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength3 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth3), Color.cyan);
			}
			//show grabbing on to climbable object under ledge detectors
			if (showWalkingOffLedgeRays3){
				//in front
				Debug.DrawLine(transform.position + transform.up*0.5f, transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn3, Color.red);
				//below
				Debug.DrawLine(transform.position + transform.forward/4 + transform.up*0.5f + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/4 - transform.up*1.5f - spaceBelowNeededToGrabBackOnHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn3, transform.position + transform.forward/1.5f - transform.up*1.5f + spaceInFrontNeededToGrabBackOn3 - spaceBelowNeededToGrabBackOnHeight3, Color.red);
				
				//grab back on location
				//middle
				Debug.DrawLine(transform.position + transform.up - transform.forward/3 + grabBackOnLocationForward3 + grabBackOnLocationHeight3, transform.position - transform.up - transform.forward/3 + grabBackOnLocationForward3, Color.yellow);
				//left
				Debug.DrawLine(transform.position + transform.up - transform.forward/3 - transform.right*0.4f + grabBackOnLocationForward3 - grabBackOnLocationWidth3 + grabBackOnLocationHeight3, transform.position - transform.up - transform.forward/3 - transform.right*0.4f + grabBackOnLocationForward3 - grabBackOnLocationWidth3, Color.yellow);
				//right
				Debug.DrawLine(transform.position + transform.up - transform.forward/3 + transform.right*0.4f + grabBackOnLocationForward3 + grabBackOnLocationWidth3 + grabBackOnLocationHeight3, transform.position - transform.up - transform.forward/3 + transform.right*0.4f + grabBackOnLocationForward3 + grabBackOnLocationWidth3, Color.yellow);
				
				//side blocking ledge detectors
				//left
				//first
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 - sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.1f + transform.right/4 - sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//second
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 - sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.5f + transform.right/4 - sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//third
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.right/4 + sideLedgeDetectorsLength3 - transform.up + thirdSideLedgeDetectorsHeight3 - sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up + transform.right/4 - sideLedgeDetectorsLength3 + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				
				//right
				//first
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 + sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.1f - transform.right/4 - sideLedgeDetectorsLength3 + firstSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//second
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 + sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up*0.5f - transform.right/4 - sideLedgeDetectorsLength3 + secondSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				//third
				Debug.DrawLine(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position + transform.forward/3 + transform.right/4 + sideLedgeDetectorsLength3 - transform.up + thirdSideLedgeDetectorsHeight3 + sideLedgeDetectorsWidth3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
				Debug.DrawLine(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, transform.position - transform.forward/4 - transform.up - transform.right/4 - sideLedgeDetectorsLength3 + thirdSideLedgeDetectorsHeight3 + spaceBelowNeededToGrabBackOnForward3, Color.red);
			}
			//showing the linecast that detects where the player pulls up to
			if (showPullUpDetector3){
				Debug.DrawLine(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward3), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward3), Color.red);
			}
		}
		
	}
	
	void SetClimbingVariables () {
		
		if (!wallIsClimbable && !currentlyClimbingWall && !turnBack && !back2 && !pullingUp){
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				tagNum = i;
			}
		}
		//Climbing variables
		climbingSurfaceDetectorsUpAmount2 = transform.up * (climbing[tagNum].climbingSurfaceDetectorsUpAmount + 0.2f);
		climbingSurfaceDetectorsHeight2 = climbing[tagNum].climbingSurfaceDetectorsHeight - 0.18f;
		climbingSurfaceDetectorsLength2 = climbing[tagNum].climbingSurfaceDetectorsLength - 0.5f;
		distanceToPushOffAfterLettingGo2 = climbing[tagNum].distanceToPushOffAfterLettingGo;
		rotationToClimbableObjectSpeed2 = climbing[tagNum].rotationToClimbableObjectSpeed;
		climbHorizontally2 = climbing[tagNum].climbHorizontally;
		climbVertically2 = climbing[tagNum].climbVertically;
		climbMovementSpeed2 = climbing[tagNum].climbMovementSpeed;
		climbRotationSpeed2 = climbing[tagNum].climbRotationSpeed;
		moveInBursts = climbing[tagNum].moveInBursts;
		burstLength = climbing[tagNum].burstLength;
		climbableTag2 = climbing[tagNum].climbableTag;
		stayUpright2 = climbing[tagNum].stayUpright;
		snapToCenterOfObject2 = climbing[tagNum].snapToCenterOfObject;
		bottomNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].bottomNoSurfaceDetectorHeight + 0.15f);
		topNoSurfaceDetectorHeight2 = transform.up*(climbing[tagNum].topNoSurfaceDetectorHeight + 0.15f);
		topAndBottomNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].topAndBottomNoSurfaceDetectorsWidth);
		sideNoSurfaceDetectorsHeight2 = climbing[tagNum].sideNoSurfaceDetectorsHeight - 0.15f;
		sideNoSurfaceDetectorsWidth2 = transform.right*(climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f);
		sideNoSurfaceDetectorsWidthTurnBack2 = climbing[tagNum].sideNoSurfaceDetectorsWidth - 0.25f;
		stopAtSides2 = climbing[tagNum].stopAtSides;
		dropOffAtBottom2 = climbing[tagNum].dropOffAtBottom;
		dropOffAtFloor2 = climbing[tagNum].dropOffAtFloor;
		pullUpAtTop2 = climbing[tagNum].pullUpAtTop;
		pullUpSpeed = climbing[tagNum].pullUpSpeed;
		pullUpLocationForward2 = transform.forward*(climbing[tagNum].pullUpLocationForward);
		pushAgainstWallIfPlayerIsStuck2 = climbing[tagNum].pushAgainstWallIfPlayerIsStuck;
		//walk off ledge then transition to climb variables
		allowGrabbingOnAfterWalkingOffLedge2 = climbing[tagNum].walkingOffOfClimbableSurface.allowGrabbingOnAfterWalkingOffLedge;
		spaceInFrontNeededToGrabBackOn2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceInFrontNeededToGrabBackOn + 0.03f);
		firstSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.firstSideLedgeDetectorsHeight);
		secondSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.secondSideLedgeDetectorsHeight);
		thirdSideLedgeDetectorsHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.thirdSideLedgeDetectorsHeight);
		sideLedgeDetectorsWidth2 = transform.right*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsWidth);
		sideLedgeDetectorsLength2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.sideLedgeDetectorsLength);
		spaceBelowNeededToGrabBackOnHeight2 = transform.up*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnHeight + 0.07f);
		spaceBelowNeededToGrabBackOnForward2 = transform.forward*(climbing[tagNum].walkingOffOfClimbableSurface.spaceBelowNeededToGrabBackOnForward + 0.06f);
		//enabling and disabling script variables
		scriptsToEnableOnGrab = climbing[tagNum].scriptsToEnableOnGrab;
		scriptsToDisableOnGrab = climbing[tagNum].scriptsToDisableOnGrab;
		
	}
	
	void CheckIfInBetweenSlopes () {
		
		//checking to see if player is stuck between two slopes
		
		RaycastHit hit2 = new RaycastHit();
		
		//checking left and right
		if (Physics.Raycast(pos - transform.right/5, Vector3.down, out hit, maxGroundedDistance2, collisionLayers) && Physics.Raycast(pos + transform.right/5, Vector3.down, out hit2, maxGroundedDistance2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position - transform.right/5 + transform.up/4, transform.position - transform.right/5 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position + transform.right/5 + transform.up/4, transform.position + transform.right/5 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)	
		|| Physics.Linecast(transform.position - transform.right/25 + transform.up/4, transform.position - transform.right/25 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position + transform.right/25 + transform.up/4, transform.position + transform.right/25 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward left and back right
		|| Physics.Raycast(pos + transform.forward/25 - transform.right/5, Vector3.down, out hit, maxGroundedDistance2, collisionLayers) && Physics.Raycast(pos - transform.forward/25 + transform.right/5, Vector3.down, out hit2, maxGroundedDistance2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 - transform.right/5 + transform.up/4, transform.position + transform.forward/50 - transform.right/5 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/50 - transform.right/5 + transform.up/4, transform.position - transform.forward/50 - transform.right/5 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 - transform.right/5 + transform.up/4, transform.position + transform.forward/25 - transform.right/5 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.right/5 + transform.up/4, transform.position - transform.forward/25 + transform.right/5 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 - transform.right/25 + transform.up/4, transform.position + transform.forward/25 - transform.right/25 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.right/25 + transform.up/4, transform.position - transform.forward/25 + transform.right/25 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward right and back left
		|| Physics.Raycast(pos + transform.forward/25 + transform.right/5, Vector3.down, out hit, maxGroundedDistance2, collisionLayers) && Physics.Raycast(pos - transform.forward/25 - transform.right/5, Vector3.down, out hit2, maxGroundedDistance2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 + transform.right/5 + transform.up/4, transform.position + transform.forward/50 + transform.right/5 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/50 + transform.right/5 + transform.up/4, transform.position - transform.forward/50 + transform.right/5 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.right/5 + transform.up/4, transform.position + transform.forward/25 + transform.right/5 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/25 - transform.right/5 + transform.up/4, transform.position - transform.forward/25 - transform.right/5 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.right/25 + transform.up/4, transform.position + transform.forward/25 + transform.right/25 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/25 - transform.right/25 + transform.up/4, transform.position - transform.forward/25 - transform.right/25 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward and back
		|| Physics.Linecast(transform.position + transform.forward/10 + transform.up/4, transform.position + transform.forward/10 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/10 + transform.up/4, transform.position - transform.forward/10 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.up/4, transform.position + transform.forward/25 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.up/4, transform.position - transform.forward/25 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward left and back left
		|| Physics.Linecast(transform.position + transform.forward/25 - transform.right/3 + transform.up/4, transform.position + transform.forward/25 - transform.right/3 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/25 - transform.right/3 + transform.up/4, transform.position - transform.forward/25 - transform.right/3 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 - transform.right/3 + transform.up/4, transform.position + transform.forward/50 - transform.right/3 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/50 - transform.right/3 + transform.up/4, transform.position - transform.forward/50 - transform.right/3 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		
		//checking forward right and back right
		|| Physics.Linecast(transform.position + transform.forward/25 + transform.right/3 + transform.up/4, transform.position + transform.forward/25 + transform.right/3 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/25 + transform.right/3 + transform.up/4, transform.position - transform.forward/25 + transform.right/3 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)
		|| Physics.Linecast(transform.position + transform.forward/50 + transform.right/3 + transform.up/4, transform.position + transform.forward/50 + transform.right/3 - transform.up*maxGroundedDistance2, out hit, collisionLayers) && Physics.Linecast(transform.position - transform.forward/50 + transform.right/3 + transform.up/4, transform.position - transform.forward/50 + transform.right/3 - transform.up*maxGroundedDistance2, out hit2, collisionLayers) && (hit.normal.z < 0 && hit2.normal.z > 0 || hit.normal.z > 0 && hit2.normal.z < 0)){
			
			if (((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > slopeLimit && ((Mathf.Acos(Mathf.Clamp(hit2.normal.y, -1f, 1f))) * 57.2958f) > slopeLimit){
			
				inBetweenSlidableSurfaces = true;
				uphill = false;
				
			}
			else {
				inBetweenSlidableSurfaces = false;
			}
			
		}
		else if (!inMidAirFromJump){
			inBetweenSlidableSurfaces = false;
		}
		
	}

	void Jump () {
		canCrouch = false;
		crouching = false;
		jumpPerformed = true;
		if (currentJumpNumber == totalJumpNumber || timeLimitBetweenJumps2 <= 0 || jumping.doNotIncreaseJumpNumberWhenSliding && sliding){
			currentJumpNumber = 0;
		}
		currentJumpNumber++;
		if (animator != null && animator.runtimeAnimatorController != null){
			animator.CrossFade("Jump", 0f, -1, 0f);
		}
		jumpTimer = 0.0f;
		moveDirection.y = jumpsToPerform[currentJumpNumber - 1];
		inMidAirFromJump = true;
		jumpPressed = false;
		return;
		
	}
	
	void DoubleJump () {
		inMidAirFromJump = true;
		if (inMidAirFromWallJump){
			transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
			inMidAirFromWallJump = false;
		}
		if (moveDirection.y > 0){
			moveDirection.y += doubleJumpHeight2;
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);
			}
			if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().velocity = moveDirection;
			}
		}
		else {
			moveDirection.y = doubleJumpHeight2;
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				GetComponent<CharacterController>().Move(moveDirection * Time.deltaTime);
			}
			if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().velocity = moveDirection;
			}
		}
		if (animator != null && animator.runtimeAnimatorController != null){
			animator.CrossFade("DoubleJump", 0f, -1, 0f);
		}
		if (doubleJumpEffect2 != null){
			Instantiate(doubleJumpEffect2, transform.position + new Vector3(0, 0.2f, 0), doubleJumpEffect2.transform.rotation);
		}
		return;
	}
	
	void WallJump () {
		canCrouch = false;
		crouching = false;
		inMidAirFromJump = true;
		moveDirection = wallJumpDirection;
		return;
	}
	
	void Attack () {
		if (!crouching){
			if (currentAttackNumber == totalAttackNumber || timeLimitBetweenAttacks2 <= 0){
				currentAttackNumber = 0;
			}
			currentAttackNumber++;
		}
		if (animator != null && animator.runtimeAnimatorController != null){
			animator.CrossFade("Attack", 0f, -1, 0f);
		}
		attackTimer = 0.0f;
		return;
	}
	
	void PushOffWall () {
		
		//pushing off of wall after letting go
		jumpedOffClimbableObjectTimer += 0.02f;
		if (jumpedOffClimbableObjectTimer < 0.3f){
			currentlyClimbingWall = false;
			jumpedOffClimbableObjectDirection = Vector3.Slerp(jumpedOffClimbableObjectDirection, Vector3.zero, 8 * Time.deltaTime);
			if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
				GetComponent<CharacterController>().Move(jumpedOffClimbableObjectDirection * Time.deltaTime);
			}
			else if (GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().MovePosition(transform.position + jumpedOffClimbableObjectDirection * Time.deltaTime);
			}
		}
		
	}
	
	void SnapToCenter () {
		
		if (snapToCenterOfObject2 && (turnBack || back2)){
			snappingToCenter = true;
		}
		if (currentlyClimbingWall && !pullingUp || turnBack || back2){
			
			//increasing snapTimer so the player knows when to let go
			if (!turnBack || wallIsClimbable || currentlyClimbingWall){
				snapTimer += 0.02f;
			}
			
			//snapping player to center of climbable object
			if (snappingToCenter && snapTimer < 0.6f && (!turnBack || turnBackTimer >= 0.1f)){
				if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
					//if climbing on to wall from the floor
					if (!turnBack && !back2){
						GetComponent<CharacterController>().Move(centerPoint * 15 * Time.deltaTime);
					}
					//if turning back on to wall from walking off ledge
					else {
						GetComponent<CharacterController>().Move(centerPoint * 13 * Time.deltaTime);
					}
				}
				else if (GetComponent<Rigidbody>()){
					//if climbing on to wall from the floor
					if (!turnBack && !back2){
						GetComponent<Rigidbody>().MovePosition(transform.position + centerPoint * 15 * Time.deltaTime);
					}
					//if turning back on to wall from walking off ledge
					else {
						GetComponent<Rigidbody>().MovePosition(transform.position + centerPoint * 13 * Time.deltaTime);
					}
				}
			}
			
		}
		else {
			snapTimer = 0;
			snappingToCenter = false;
		}
		
	}
	
	void TurnToGrabLadder () {
		
		//Climbing variables
		i++;
		if (i == climbing.Length){
			i = 0;
		}
		if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
			SetClimbingVariables();
		}
		//checking to see if we are about to turn on to a climbable wall
		//if left is blocked, go to back right
		if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f - transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&	Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f - transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f - transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.5f + transform.right/2 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f - transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up + transform.right/2 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.1f + transform.right/2 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			fifthTest = true;
			
		}
		//if right is blocked, go to back left
		else if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + transform.right/2 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
	
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.5f - transform.right/2 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + transform.right/2 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 + transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up - transform.right/2 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
		&&  Physics.Linecast(transform.position + transform.forward/4 + transform.right/2 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/2 - transform.up*0.1f - transform.right/2 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
			
			fifthTest = true;
			
		}
		//if neither side is blocked, go directly back
		else if ((Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
		||  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45)
		&& !Physics.Linecast(new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y + transform.up.y*0.5f, (hit.point + hit.normal/(3.5f/0.77f)).z), new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y - transform.up.y*1.5f - spaceBelowNeededToGrabBackOnHeight2.y, (hit.point + hit.normal/(3.5f/0.77f)).z), out hit, collisionLayers)){
			
			fifthTest = true;
			
		}
		else {
			fifthTest = false;
		}
		
		
		if (!turnBack){
			//checking to see if either side of a ledge is blocked
			if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0){
				if (!wallIsClimbable && !Physics.Linecast(transform.position + transform.up*0.5f, transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn2, out hit, collisionLayers) && !back2 && !currentlyClimbingWall){
					if (!Physics.Linecast(transform.position + transform.forward/4 + transform.up*0.5f + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*1.5f - spaceBelowNeededToGrabBackOnHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && !Physics.Linecast(transform.position + transform.forward/1.5f + transform.up*0.5f + spaceInFrontNeededToGrabBackOn2, transform.position + transform.forward/1.5f - transform.up*1.5f + spaceInFrontNeededToGrabBackOn2 - spaceBelowNeededToGrabBackOnHeight2, out hit, collisionLayers)){
						
						//if left is blocked, go to back right
						if ((Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&	Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f - transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.5f + transform.right/4 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f - transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up + transform.right/4 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 - sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.1f + transform.right/4 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else {
								rotationHit = Vector3.zero;
								backRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
								normalRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
							}
							
							//getting center of wall we are turning back on to
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
									if (snapToCenterOfObject2){
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.4f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.4f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y;
									}
									else {
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.2f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.2f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y + 0.13f;
									}
									turnBackPoint = backPoint;
									if (!snappingToCenter && hit.transform != null && !wallIsClimbable && !currentlyClimbingWall){
										centerPoint = new Vector3(hit.transform.position.x - transform.position.x + (transform.position.x - hit.point.x), 0, hit.transform.position.z - transform.position.z + (transform.position.z - hit.point.z));
									}
									if (snapToCenterOfObject2){
										snappingToCenter = true;
									}
									turnBackRight = true;
									turnBackLeft = false;
									turnBackMiddle = false;
									turnBack = true;
								}
							}
						}
						//if right is blocked, go to back left
						else if ((Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.1f + transform.right/4 + sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
					
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.5f - transform.right/4 - sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 - transform.up*0.5f + transform.right/4 + sideLedgeDetectorsLength2 + secondSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up*0.5f + secondSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						
						||  Physics.Linecast(transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/2 + transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && Physics.Linecast(transform.position + transform.forward/3 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up - transform.right/4 - sideLedgeDetectorsLength2 + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2
						&&  Physics.Linecast(transform.position + transform.forward/2 + transform.right/4 + sideLedgeDetectorsLength2 - transform.up + thirdSideLedgeDetectorsHeight2 + sideLedgeDetectorsWidth2 + spaceBelowNeededToGrabBackOnForward2, transform.position + transform.forward/4 - transform.up + thirdSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2) && Physics.Linecast(transform.position + transform.forward/3 - transform.up*0.1f + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward/4 - transform.up*0.1f - transform.right/4 - sideLedgeDetectorsLength2 + firstSideLedgeDetectorsHeight2 + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else {
								rotationHit = Vector3.zero;
								backRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
								normalRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
							}
							
							//getting center of wall we are turning back on to
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
									if (snapToCenterOfObject2){
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.4f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.4f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y;
									}
									else {
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.2f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.2f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y + 0.13f;
									}
									turnBackPoint = backPoint;
									if (!snappingToCenter && hit.transform != null && !wallIsClimbable && !currentlyClimbingWall){
										centerPoint = new Vector3(hit.transform.position.x - transform.position.x + (transform.position.x - hit.point.x), 0, hit.transform.position.z - transform.position.z + (transform.position.z - hit.point.z));
									}
									if (snapToCenterOfObject2){
										snappingToCenter = true;
									}
									turnBackLeft = true;
									turnBackRight = false;
									turnBackMiddle = false;
									turnBack = true;
								}
							}
						}
						//if neither side is blocked, go directly back
						else if ((Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.25f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.05f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.15f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45
						||  Physics.Linecast(transform.position + transform.forward/3.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, transform.position - transform.forward*0.5f - transform.up*0.25f + spaceBelowNeededToGrabBackOnForward2, out hit, collisionLayers) && hit.transform.tag == climbableTag2 && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) > 45)
						&& !Physics.Linecast(new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y + transform.up.y*0.5f, (hit.point + hit.normal/(3.5f/0.77f)).z), new Vector3( (hit.point + hit.normal/(3.5f/0.77f)).x, transform.position.y - transform.up.y*1.5f - spaceBelowNeededToGrabBackOnHeight2.y, (hit.point + hit.normal/(3.5f/0.77f)).z), out hit, collisionLayers)){
							
							fifthTest = true;
							if (Physics.Raycast(transform.position - transform.up/9 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else if (Physics.Raycast(transform.position - transform.up/20 + transform.forward, -transform.forward/4, out hit, 3f, collisionLayers) && ((Mathf.Acos(Mathf.Clamp(hit.normal.y, -1f, 1f))) * 57.2958f) >= 45){
								rotationHit = -hit.normal;
								backRotation = Quaternion.LookRotation(-hit.normal);
								normalRotation = Quaternion.LookRotation(hit.normal);
							}
							else {
								rotationHit = Vector3.zero;
								backRotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
								normalRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
							}
							
							//getting center of wall we are turning back on to
							if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * Vector3.right)/3 + (normalRotation * Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
							&&  Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward) + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), transform.position - transform.up*0.1f + (normalRotation * -Vector3.right)/3 + (normalRotation * -Vector3.right*sideNoSurfaceDetectorsWidthTurnBack2), out hit, collisionLayers) && hit.transform.tag == climbableTag2){
								if (Physics.Linecast(transform.position - transform.up*0.1f + (normalRotation * Vector3.forward), transform.position - transform.up*0.1f, out hit, collisionLayers) && hit.transform.tag == climbableTag2){
									if (snapToCenterOfObject2){
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.4f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.4f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y;
									}
									else {
										backPoint = new Vector3(hit.point.x + (normalRotation * Vector3.forward).x*1.2f - hit.normal.x, hit.point.y - 1.15f, hit.point.z + (normalRotation * Vector3.forward).z*1.2f - hit.normal.z);
										playerPosY = backPoint.y - topNoSurfaceDetectorHeight2.y + 0.13f;
									}
									turnBackPoint = backPoint;
									if (!snappingToCenter && hit.transform != null && !wallIsClimbable && !currentlyClimbingWall){
										centerPoint = new Vector3(hit.transform.position.x - transform.position.x + (transform.position.x - hit.point.x), 0, hit.transform.position.z - transform.position.z + (transform.position.z - hit.point.z));
									}
									if (snapToCenterOfObject2){
										snappingToCenter = true;
									}
									turnBackMiddle = true;
									turnBackLeft = false;
									turnBackRight = false;
									turnBack = true;
								}
							}
						}
						else {
							turnBackMiddle = false;
							turnBackLeft = false;
							turnBackRight = false;
							turnBack = false;
						}
						
					}
				}
			}
			
		}
		
		//turning around when you walk off a ledge
		if (turnBack){
			
			if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("Climbing")){
				animator.CrossFade("Climbing", 0f, -1, 0f);
			}
			
			if (allowGrabbingOnAfterWalkingOffLedge2 && (turnBackMiddle || turnBackLeft || turnBackRight)){
				if (!stayUpright2){
					if ((Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z)) > 0.3f && !snapToCenterOfObject2 || Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z)) > 0.3f && snapToCenterOfObject2
					|| Quaternion.Angle(transform.rotation, backRotation) > 0.1f) && (!snapToCenterOfObject2 && turnBackTimer < 0.5f || turnBackTimer < 0.2f)){
						turnBackTimer += 0.02f;
						back2 = false;
						currentlyClimbingWall = false;
						//movement
						if (!snapToCenterOfObject2){
							if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
								transform.position = Vector3.Lerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
							else if (GetComponent<Rigidbody>()){
								transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
						}
						else {
							transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z), 10 * Time.deltaTime);
							
						}
						//moving over if player is past side
						if (!snapToCenterOfObject2 && Quaternion.Angle(transform.rotation, backRotation) < 45){
							if (reachedLeftPoint){
								turnBackPoint += transform.right/30;
							}
							if (reachedRightPoint){
								turnBackPoint -= transform.right/30;
							}
						}
						//rotation
						rotationNormal = backRotation;
						transform.rotation = Quaternion.Slerp(transform.rotation, backRotation, 12 * Time.deltaTime);
						playerPosXZ = transform.position;
					}
					else {
						turnBackTimer = 0.0f;
						back2 = true;
						turnBack = false;
					}
				}
				else {
					if ((Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z)) > 0.3f && !snapToCenterOfObject2 || Vector3.Distance(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z)) > 0.3f && snapToCenterOfObject2
					|| Quaternion.Angle(transform.rotation, backRotation) > 0.1f) && (!snapToCenterOfObject2 && turnBackTimer < 0.5f || turnBackTimer < 0.2f)){
						turnBackTimer += 0.02f;
						back2 = false;
						currentlyClimbingWall = false;
						//movement
						if (!snapToCenterOfObject2){
							if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
								transform.position = Vector3.Lerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
							else if (GetComponent<Rigidbody>()){
								transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, playerPosY, turnBackPoint.z), 10 * Time.deltaTime);
							}
						}
						else {
							transform.position = Vector3.Slerp(transform.position, new Vector3(turnBackPoint.x, (playerPosY + 0.06f/5), turnBackPoint.z), 10 * Time.deltaTime);
							
						}
						//moving over if player is past side
						if (!snapToCenterOfObject2 && Quaternion.Angle(transform.rotation, backRotation) < 45){
							if (reachedLeftPoint){
								turnBackPoint += transform.right/30;
							}
							if (reachedRightPoint){
								turnBackPoint -= transform.right/30;
							}
						}
						//rotation
						rotationNormal = backRotation;
						transform.rotation = Quaternion.Slerp(transform.rotation, backRotation, 12 * Time.deltaTime);
						playerPosXZ = transform.position;
					}
					else {
						turnBackTimer = 0.0f;
						back2 = true;
						turnBack = false;
					}
				}
			}
			else {
				back2 = false;
				turnBack = false;
			}
			
		}
		
		
		if (allowGrabbingOnAfterWalkingOffLedge2){
			if (turnBack || back2){
				climbDirection = Vector3.zero;
				moveDirection = Vector3.zero;
				moveSpeed = 0;
			}
			if (back2){
				turnBack = false;
				if (!currentlyClimbingWall){
					if (!snapToCenterOfObject2){
						transform.position = new Vector3(playerPosXZ.x, transform.position.y, playerPosXZ.z);
					}
					rotationNormal = backRotation;
					transform.rotation = backRotation;
					currentlyClimbingWall = true;
				}
				back2 = false;
			}
		}
		
	}
	
	void CheckClimbableEdges () {
		
		//determining if player has reached any of the edges of the climbable object
		if (!pullingUp){
			
			//BOTTOM POINT
			//checking to see if player has reached the bottom of the ladder (first: if not touching anything at bottom; next: if touching something at bottom, but does not have climbable tag)
			if (!Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers)
			||
			Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*0.1875f, transform.position + bottomNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*0.1875f, out hit, collisionLayers) && hit.transform.tag != climbableTag2){
				
				//bottom of climbable object has been reached
				reachedBottomPoint = true;
				
				//dropping off at bottom
				if (currentlyClimbingWall && dropOffAtBottom2 && transform.rotation == lastRot2 && wallIsClimbable && Input.GetAxisRaw("Vertical") < 0){
					moveDirection = Vector3.zero;
					jumpedOffClimbableObjectTimer = 0;
					jumpedOffClimbableObjectDirection = -transform.forward*distanceToPushOffAfterLettingGo2;
					inMidAirFromJump = false;
					inMidAirFromWallJump = false;
					currentJumpNumber = totalJumpNumber;
					moveSpeed = 0;
					if (animator != null){
						animator.CrossFade("Movement", 0f, -1, 0f);
					}
					animator.SetFloat("climbState", 0);
					currentlyClimbingWall = false;
				}
				
			}
			else if (!currentlyClimbingWall || currentlyClimbingWall && (!grounded.currentlyGrounded || dropOffAtBottom2)){
				//bottom of climbable object has not been reached
				reachedBottomPoint = false;
			}
			
			
			//TOP POINT
			//if our front, right and left side are above the top
			if (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers)
			&& !Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers)
			||
			Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2
			&& Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2){
				aboveTop = true;
			}
			else {
				aboveTop = false;
			}
			
			//checking to see if player has reached the top of the ladder
			if (aboveTop
			||
			//if top and left is blocked
			((!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2)
			&& (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2)
			||
			//if top and right is blocked
			(!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2)
			&& (!Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) || Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag != climbableTag2))){
				
				//top of climbable object has been reached
				reachedTopPoint = true;
				
				//checking to see if surface can be pulled up to
				if (aboveTop){
					if (currentlyClimbingWall && pullUpAtTop2 && transform.rotation == lastRot2 && wallIsClimbable && Input.GetAxisRaw("Vertical") > 0
					&& Physics.Linecast(transform.position + transform.up + transform.forward/1.25f + transform.up*1.5f + (pullUpLocationForward2), transform.position + transform.up*0.8f + transform.forward/1.75f + (pullUpLocationForward2), out hit, collisionLayers)){
						pullUpLocationAcquired = true;
						movingToPullUpLocation = true;
						pullingUp = true;
					}
				}
				
			}
			else {
				//top of climbable object has not been reached
				reachedTopPoint = false;
			}
			
			
			//RIGHT POINT
			//Climbing variables
			i++;
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				SetClimbingVariables();
			}
			
			//making sure we are not above the top point, and not moving to the right
			if (Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + transform.right/3 + topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			//if there is a wall on the top right to turn in to
			|| (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)
			//if we are above the top, and not just stuck on the right side
			|| aboveTop){
				//if we have room to move to the right, we have not reached the farthest right point
				if ((Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//checking over to right more (using topAndBottomNoSurfaceDetectorsWidth instead of sideNoSurfaceDetectorsWidth)
				(Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//if there is a wall on the right to turn in to
				(Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2 + transform.forward/4, out hit, collisionLayers)&& hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)){
					
					//right edge of climbable object has not been reached
					thirdTest = true;
					reachedRightPoint = false;
					
				}
				//checking to see if player has reached the right edge (first: if not touching anything to right; next: if touching something to right, but does not have climbable tag)
				else if (!Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				||
				Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2){
					
					//right edge of climbable object has been reached
					thirdTest = false;
					reachedRightPoint = true;
					
				}
				else {
					//right edge of climbable object has not been reached
					reachedRightPoint = false;
				}
			}
			else {
				//right edge of climbable object has not been reached
				reachedRightPoint = true;
			}
			
			
			//LEFT POINT
			//Climbing variables
			i++;
			if (i == climbing.Length){
				i = 0;
			}
			if (!firstTest && !secondTest && !thirdTest && !fourthTest && !fifthTest){
				SetClimbingVariables();
			}
			
			//making sure that we are not above the top point and moving to the left
			if (Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + transform.up*1.125f, transform.position + topNoSurfaceDetectorHeight2 - transform.right/3 - topAndBottomNoSurfaceDetectorsWidth2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + transform.up*1.125f, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			//if there is a wall on the top left to turn in to
			|| (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
			|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)
			//if we are above the top, and not just stuck on the left side
			|| aboveTop){
				//if we have room to move to the left, we have not reached the farthest left point
				if ((Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//checking over to left more (using topAndBottomNoSurfaceDetectorsWidth instead of sideNoSurfaceDetectorsWidth)
				(Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/3 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + topAndBottomNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag == climbableTag2)
				||
				//if there is a wall on the left to turn in to
				(Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2
				|| Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2 + transform.forward/4, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2 + transform.forward/4, out hit, collisionLayers) && hit.transform.tag == climbableTag2)){
					
					//left edge of climbable object has not been reached
					fourthTest = true;
					reachedLeftPoint = false;
					
				}
				//checking to see if player has reached the left edge (first: if not touching anything to left; next: if touching something to left, but does not have climbable tag)
				else if (!Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				&& !Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers)
				||
				Physics.Linecast(transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f)*3 + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f))/2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + (((topNoSurfaceDetectorHeight2 + (transform.up*1.125f)*(sideNoSurfaceDetectorsHeight2 + 1)) * 0.99f) + ((bottomNoSurfaceDetectorHeight2 + transform.up*0.1875f) * 0.99f)*3)/4 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2
				&& Physics.Linecast(transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), transform.position + ((bottomNoSurfaceDetectorHeight2 + (transform.up*0.1875f)) * 0.99f) + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) - ((transform.right/3) + sideNoSurfaceDetectorsWidth2), out hit, collisionLayers) && hit.transform.tag != climbableTag2){
					
					//left edge of climbable object has been reached
					fourthTest = false;
					reachedLeftPoint = true;
					
				}
				else {
					//left edge of climbable object has not been reached
					reachedLeftPoint = false;
				}
			}
			else {
				//left edge of climbable object has not been reached
				reachedLeftPoint = true;
			}
			
		}
		
	}
	
	void WallClimbingRotation () {
		
		//rotation
		if (currentlyClimbingWall && !pullingUp){
			
			//only change the rotation normal if player is moving
			if ((transform.rotation == lastRot3 || axisChanged) && (climbingMovement > 0 || Input.GetAxis("Horizontal") != 0) || hasNotMovedOnWallYet){
				
				//to the right of player
				if ((Input.GetAxis("Horizontal") > 0 || transform.rotation != lastRot2) && (wallIsClimbable)){
					if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") > 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
				}
				//to the left of player
				else if ((Input.GetAxis("Horizontal") < 0 || transform.rotation != lastRot2) && (wallIsClimbable)){
					if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
					else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 - transform.right/3 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
						
						if (Input.GetAxis("Horizontal") < 0){
							rotationNormal = Quaternion.LookRotation(-hit.normal);
							rotationState = 4;
						}
					}
				}
				
				//in front of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				//in front of player, slightly to the right
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				//in front of player, slightly to the left
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.5f, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/4.5f, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 1;
				}
				
				//inward turn, front and to the right of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 2;
				}
				
				//inward turn, front and to the left of player
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.5625f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.375f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.1875f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.75f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*0.9375f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (Physics.Linecast(transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f - transform.right/2, transform.position + climbingSurfaceDetectorsUpAmount2 + (transform.forward*(climbingSurfaceDetectorsLength2 + 1)) + (transform.up*(climbingSurfaceDetectorsHeight2 + 1))*1.125f + transform.right/2, out hit, collisionLayers) && hit.transform != null && hit.transform.tag == climbableTag2){
					
					rotationNormal = Quaternion.LookRotation(-hit.normal);
					rotationState = 3;
				}
				else if (noCollisionTimer >= 5 && !wallIsClimbable){
					currentlyClimbingWall = false;
				}
				
				
				if (climbingMovement > 0 || Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0){
					hasNotMovedOnWallYet = false;
				}
			}
			
			//checking to see if player changed their direction from left to right/right to left
			if ((Input.GetAxis("Horizontal") > 0 && horizontalAxis <= 0 || Input.GetAxis("Horizontal") < 0 && horizontalAxis >= 0)){
				axisChanged = true;
			}
			else {
				axisChanged = false;
			}
			horizontalAxis = Input.GetAxis("Horizontal");
			lastRot3 = transform.rotation;
			
			//rotating the player
			if (rotationState != 0){
				//if we are just getting on the wall
				if (!finishedRotatingToWall){
					transform.rotation = Quaternion.Slerp(transform.rotation, rotationNormal, (rotationToClimbableObjectSpeed2*2) * Time.deltaTime);
				}
				//if we are already on the wall, and have finished rotating to it
				else {
					transform.rotation = Quaternion.Slerp(transform.rotation, rotationNormal, climbRotationSpeed2 * Time.deltaTime);
				}
			}
			if (stayUpright2){
				transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
			}
			
		}
		else {
			lastYPosOnWall = 0;
			climbingHeight = transform.position.y;
			hasNotMovedOnWallYet = true;
		}
		
	}
	
	void CheckIfStuck () {
		
		if (pushAgainstWallIfPlayerIsStuck2){
			//if player is off of the surface of the wall
			if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward + transform.up, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward + transform.up*1.1f, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward + transform.up*1.2f, out hit, collisionLayers)){
				distFromWallWhenStuck = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(transform.position.x, 0, transform.position.z));
				//push player forward to wall
				if (currentlyClimbingWall && !pullingUp
				&& (distFromWallWhenStuck >= 0.35f || firstDistFromWallWhenStuck != 0 && distFromWallWhenStuck >= firstDistFromWallWhenStuck + 0.05f)
				&& noCollisionTimer >= 5){
					transform.position += transform.forward/30;
				}
				
				//getting the player's first distance from the wall
				if (currentlyClimbingWall){
					if (firstDistFromWallWhenStuck == 0){
						firstDistFromWallWhenStuck = distFromWallWhenStuck;
					}
				}
				else {
					firstDistFromWallWhenStuck = 0;
				}
			}
			
			if (climbedUpAlready){
				//checking to see if the player is stuckInSamePos and not colliding
				if (!stuckInSamePos || pullingUp){
					stuckInSamePosNoCol = false;
				}
				else if (noCollisionTimer > 25){
					stuckInSamePosNoCol = true;
				}
				//checking to see if player is stuck on a collider
				if (currentlyClimbingWall && climbingMovement > 0 && (Input.GetAxisRaw("Horizontal") > 0 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Horizontal") < 0 && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Vertical") > 0 || Input.GetAxisRaw("Vertical") < 0) || pullingUp){
					
					//getting distance from the wall we are colliding with
					if (transform.position == lastPos){
						
						if (noCollisionTimer < 5 ){
							if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward/2 + transform.up, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward/2 + transform.up*1.1f, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward/2 + transform.up*1.2f, out hit, collisionLayers)){
								distFromWallWhenStuck = Vector3.Distance(new Vector3(hit.point.x, 0, hit.point.z), new Vector3(transform.position.x, 0, transform.position.z));
							}
							if (!hasNotMovedOnWallYet && (Input.GetAxisRaw("Horizontal") > 0.1f && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis) || Input.GetAxisRaw("Horizontal") < -0.1f && (!movement.sideScrolling.lockMovementOnXAxis && !movement.sideScrolling.lockMovementOnZAxis)
							|| Input.GetAxisRaw("Vertical") > 0.1f || Input.GetAxisRaw("Vertical") < -0.1f)){
								stuckInSamePos = true;
							}
						}
					}
					if (transform.rotation != lastRot2 || Mathf.Abs(transform.position.y - lastPos.y) > 0.001f || stuckInSamePosNoCol && noCollisionTimer < 2){
						stuckInSamePos = false;
						stuckInSamePosNoCol = false;
					}
					
					if (GetComponent<CharacterController>() && GetComponent<CharacterController>().enabled){
						
						//if player is stuck
						if (!pullingUp && stuckInSamePos){
							
							//move the player slightly back to avoid collision
							if (Physics.Linecast(transform.position + transform.up, transform.position + transform.forward/2 + transform.up, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.1f, transform.position + transform.forward/2 + transform.up*1.1f, out hit, collisionLayers) || Physics.Linecast(transform.position + transform.up*1.2f, transform.position + transform.forward/2 + transform.up*1.2f, out hit, collisionLayers)){
								if (distFromWallWhenStuck != 0){
									transform.position = new Vector3((hit.point + (hit.normal / (distFromWallWhenStuck/(0.07f * (distFromWallWhenStuck/0.2601f))))*(distFromWallWhenStuck/0.2601f)).x, transform.position.y, (hit.point + (hit.normal / (distFromWallWhenStuck/(0.07f * (distFromWallWhenStuck/0.2601f))))*(distFromWallWhenStuck/0.2601f)).z);
								}
								else {
									transform.position = new Vector3((hit.point + (hit.normal/3.5f)).x, transform.position.y, (hit.point + (hit.normal/3.5f)).z);
								}
							}
							else if (transform.position == lastPos && transform.rotation == lastRot2 || noCollisionTimer < 2){
								transform.position -= transform.forward/100;
							}
							
						}
						
						//if player is stuck while climbing over a ledge, move the player slightly back and up to avoid collision
						if (pullingUp && noCollisionTimer < 5 && transform.position == lastPos){
							transform.position -= transform.forward/25;
							transform.position += transform.up/15;
						}
						
					}
					
				}
			}
			lastPos = transform.position;
		}
		lastRot2 = transform.rotation;
		
	}
	
	void ScriptEnablingDisabling () {
		
		//enabling and disabling scripts while player is on wall
		if (currentlyClimbingWall || turnBack || back2 || pullingUp){
			//if scripts have not been disabled/enabled yet
			if (!onWallScriptsFinished){
				if (scriptsToDisableOnGrab != null){
					foreach (string script in scriptsToDisableOnGrab)
					{
						scriptToDisable = GetComponent(script) as MonoBehaviour;
						if (scriptToDisable != null){
							scriptToDisable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingScripts){
							scriptWarning = true;
						}
					}
				}
				if (scriptsToEnableOnGrab != null){
					foreach (string script in scriptsToEnableOnGrab)
					{
						scriptToEnable = GetComponent(script) as MonoBehaviour;
						if (scriptToEnable != null){
							scriptToEnable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingScripts){
							scriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingScripts = true;
			}
			onWallScriptsFinished = true;
			
		}
		//undoing enabling and disabling scripts when player lets go of wall
		else {
			//if scripts have not been un-disabled/enabled yet
			if (onWallScriptsFinished){
				if (scriptsToDisableOnGrab != null){
					foreach (string script in scriptsToDisableOnGrab)
					{
						scriptToDisable = GetComponent(script) as MonoBehaviour;
						if (scriptToDisable != null){
							scriptToDisable.enabled = true;
						}
						else if (!currentlyEnablingAndDisablingScripts || currentlyClimbingWall || turnBack || back2){
							scriptWarning = true;
						}
					}
				}
				if (scriptsToEnableOnGrab != null){
					foreach (string script in scriptsToEnableOnGrab)
					{
						scriptToEnable = GetComponent(script) as MonoBehaviour;
						if (scriptToEnable != null){
							scriptToEnable.enabled = false;
						}
						else if (!currentlyEnablingAndDisablingScripts || currentlyClimbingWall || turnBack || back2){
							scriptWarning = true;
						}
					}
				}
				currentlyEnablingAndDisablingScripts = true;
			}
			onWallScriptsFinished = false;
			
		}
		
		//all loops that enable or disable scripts have finished, so we set currentlyEnablingAndDisablingScripts to false
		if (!currentlyClimbingWall && !turnBack && !back2 && !pullingUp){
			currentlyEnablingAndDisablingScripts = false;
		}
		//warns the user if any script names they entered do not exist on the player
		if (scriptWarning){
			if (scriptsToDisableOnGrab != null){
				foreach (string script in scriptsToDisableOnGrab)
				{
					scriptToDisable = GetComponent(script) as MonoBehaviour;
					if (scriptToDisable == null){
						Debug.Log("<color=red>The script to disable on grab named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			if (scriptsToEnableOnGrab != null){
				foreach (string script in scriptsToEnableOnGrab)
				{
					scriptToEnable = GetComponent(script) as MonoBehaviour;
					if (scriptToEnable == null){
						Debug.Log("<color=red>The script to enable on grab named: </color>\"" + script + "\"<color=red> was not found on the player</color>");
					}
				}
			}
			scriptWarning = false;
		}
		
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		
		contactPoint = hit.point;
		collisionSlopeAngle = Vector3.Angle(Vector3.up, hit.normal);
		noCollisionTimer = 0;
		
		//determining slope angles
		slidingAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slidingAngle >= slopeLimit) {
            slidingVector = hit.normal;
            if (slidingVector.y == 0){
				slidingVector = Vector3.zero;
			}
        }
		else {
            slidingVector = Vector3.zero;
        }
 
        slidingAngle = Vector3.Angle(hit.normal, moveDirection - Vector3.up * moveDirection.y);
        if (slidingAngle > 90) {
            slidingAngle -= 90.0f;
            if (slidingAngle > slopeLimit){
				slidingAngle = slopeLimit;
			}
            if (slidingAngle < slopeLimit){
				slidingAngle = 0;
			}
        }
		
		//climbing walls/ladders
		if (hit.gameObject.tag == climbableTag2 && wallIsClimbable && jumpedOffClimbableObjectTimer >= 0.3f
		&& (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)){
			if (snapToCenterOfObject2 && !snappingToCenter && !currentlyClimbingWall){
				snapTimer = 0;
				snappingToCenter = true;
			}
			if (!currentlyClimbingWall && GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			currentlyClimbingWall = true;
			climbDirection = Vector3.zero;
			moveDirection = Vector3.zero;
			moveSpeed = 0;
			inMidAirFromJump = false;
			inMidAirFromWallJump = false;
		}
		
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		
		foreach (ContactPoint contact in hit.contacts) {
			contactPoint = contact.point;
			collisionSlopeAngle = Vector3.Angle(Vector3.up, contact.normal);
			noCollisionTimer = 0;
			
			//determining slope angles
			slidingAngle = Vector3.Angle(contact.normal, Vector3.up);
			if (slidingAngle >= slopeLimit) {
				slidingVector = contact.normal;
				if (slidingVector.y == 0){
					slidingVector = Vector3.zero;
				}
			}
			else {
				slidingVector = Vector3.zero;
			}
 
			slidingAngle = Vector3.Angle(contact.normal, moveDirection - Vector3.up * moveDirection.y);
			if (slidingAngle > 90) {
				slidingAngle -= 90.0f;
				if (slidingAngle > slopeLimit){
					slidingAngle = slopeLimit;
				}
				if (slidingAngle < slopeLimit){
					slidingAngle = 0;
				}
			}
        }
		
		//climbing walls/ladders
		if (hit.gameObject.tag == climbableTag2 && wallIsClimbable && jumpedOffClimbableObjectTimer >= 0.3f
		&& (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)){
			if (snapToCenterOfObject2 && !snappingToCenter && !currentlyClimbingWall){
				snapTimer = 0;
				snappingToCenter = true;
			}
			if (!currentlyClimbingWall && GetComponent<Rigidbody>()){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
			currentlyClimbingWall = true;
			moveDirection = Vector3.zero;
			moveSpeed = 0;
			inMidAirFromJump = false;
			inMidAirFromWallJump = false;
		}
		
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
	void OnTriggerStay (Collider hit) {
		
		//moving with moving platforms
		if (hit.gameObject.tag == movingPlatformTag && allowMovingPlatformSupport){
			//since we are colliding with the platform, set the no collision timer to 0
			noCollisionWithPlatformTimer = 0;
			
			//create and parent empty object (so that we can undo the parent's properties that affect the player's scale)
			if (emptyObject == null){
				emptyObject = new GameObject();
				emptyObject.transform.position = hit.transform.position;
			}
			emptyObject.name = "PlatformPlayerConnector";
			emptyObject.transform.parent = hit.transform;
			
			//undoing parent's properties that affect the player's scale
			emptyObject.transform.localScale = new Vector3(1/hit.transform.localScale.x, 1/hit.transform.localScale.y, 1/hit.transform.localScale.z);
			emptyObject.transform.localRotation = Quaternion.Euler(-hit.transform.localRotation.x, -hit.transform.localRotation.y, -hit.transform.localRotation.z);
			
			//setting player's parent to the empty object
			transform.parent = emptyObject.transform;
		}
		
	}
	
	void OnDisable() {
		
		//resetting values
		
		if (GetComponent<Rigidbody>()){
			if (!GetComponent<CharacterController>() || GetComponent<CharacterController>() && !GetComponent<CharacterController>().enabled){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
		}
		inMidAirFromJump = false;
		inMidAirFromWallJump = false;
        currentJumpNumber = totalJumpNumber;
		moveDirection.y = 0;
		moveSpeed = 0;
		jumpPressed = false;
		jumpPossible = false;
		jumpEnabled = false;
		doubleJumpPossible = true;
		middleWallJumpable = false;
		leftWallJumpable = false;
		rightWallJumpable = false;
		currentlyOnWall = false;
		currentlyClimbingWall = false;
		turnBack = false;
		back2 = false;
		canCrouch = false;
		crouching = false;
		enabledLastUpdate = false;
		slidingVector = Vector3.zero;
		slideMovement = Vector3.zero;
		
    }
	
}