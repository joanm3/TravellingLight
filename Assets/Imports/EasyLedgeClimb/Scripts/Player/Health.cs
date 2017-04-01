/// Easy Ledge Climb Character System
/// Health.cs
///
/// This script allows the player to:
/// 1. Have a set health.
/// 2. Take damage from items and enemies.
/// 3. Regain health from items.
/// 4. Respawn.
/// 4. Set GUI images for the health.
/// 5. Position the health GUI anywhere on the screen, and limit the number of GUI images (hearts) that can be on each row of health.
///
/// (C) 2015-2016 Grant Marrs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Health : MonoBehaviour {
	
	//health
	[System.Serializable]
	public class PlayerHealth {
		
		//hearts
		public int numberOfHearts = 3; //the number of full hearts the player has
		public int maxHeartsPerRow = 8; //the maximum number of hearts per row
		public bool regainHealthOverTime = false; //allows the player to regain his health over a certain amount of time
		public int quartersOfHealthToRegain = 2; //the quarters of health that the player regains after a certain amount of time
		public float timeNeededToRegainHealth = 7; //the amount of time before the player regains health
		[HideInInspector]
		public bool debugHealth = false; //allows the user to test the functions of the health script (regain health, take damage, add heart, remove heart)
		
		//health GUI
		[System.Serializable]
		public class HealthGUI {
			public Texture[] heartImages = new Texture[5]; //images of the hearts being used to represent the player's health; in order from no health (0 quarters) to full health (4 quarters)
			public bool overlayHearts = true; //makes each new heart overlay the last one
			public Vector2 gUISpacing = new Vector2(1, 1); //the horizontal and vertical spacing between each heart image/GUI
			public Vector3 gUIPosition = new Vector3(0.8f, 88, 0); //the position of the heart images/GUI
			public Vector2 gUIScale = new Vector2(58, 58); //the width and height of the heart images/GUI
		}
		public HealthGUI healthGUI = new HealthGUI(); //variables that control the player's health GUI
	
	}
	public PlayerHealth health = new PlayerHealth(); //variables that control the player's health
	
	//damage
	[System.Serializable]
	public class PlayerDamage {
		public string enemyTag = "Enemy"; //the tag of the enemies in the scene
		public float secondsToStayInvincibleAfterAttacking = 0.7f; //the amount of time the player stays invincible after attacking an enemy
		public float secondsToStayInvincibleAfterBeingHurt = 1.0f; //the amount of time the player stays invincible after being hurt
		public float damageBlinkSpeed = 0.1f; //the speed at which the player blinks (becomes invisible and visible again) after being hurt
	}
	public PlayerDamage damage = new PlayerDamage(); //variables that control the player's health
	
	//respawn
	[System.Serializable]
	public class Respawn {
		public Vector3 respawnLocation; //the location at which the player respawns
		public Vector3 respawnRotation; //the rotation at which the player respawns
	}
	public Respawn respawn = new Respawn(); //variables that control the player's respawn
	
	//health items
	[System.Serializable]
	public class HealthItems {
		public string healthItemTag = "Heart"; //the tag of an item that gives the player health
		public int quartersOfHealthToRegain = 4; //the amount of health you regain from the health item (measured in quarter hearts)
	}
	public HealthItems[] healthItems = new HealthItems[1]; //items that give the player health
	
	//damage items
	[System.Serializable]
	public class DamageItems {
		public string damageItemTag = "Hurt"; //the tag of an item that takes the player's health
		public int quartersOfHealthToLose = 2; //the amount of health you lose from the damage item (measured in quarter hearts)
	}
	public DamageItems[] damageItems = new DamageItems[1]; //items that take the player's health
	
	//private health variables
	[HideInInspector]
	public int currentHealth; //the current health of the player
	private GameObject healthHolder; //the gameObject that holds the health GUI
	private List<GameObject> hearts = new List<GameObject>(); //the health GUI images
	private int newNumberOfHearts = 3; //the player's new and current number of hearts
	private int lastNumberOfHearts = 3; //the player's last number of hearts
	[HideInInspector]
	public bool canDamage = true; //detects whether the player can be damaged
	[HideInInspector]
	public bool canDamage2 = true; //detects whether the player cannot be damaged because he has just become invincible
	private float damageTimer = 0.0f; //the amount of time since the player was last damaged
	private bool invincible = false; //determines if the player is currently invincible
	private float blinkTimer; //the amount of time between the character becoming visible and invisible (blinking) while the player is damaged
	private float damageTimeoutTimer; //the amount of time since the player was last damaged (used to check if we can still be invincible)
	private Renderer[] renderers; //the renderers attached to the player and the player's children
	[HideInInspector]
	public bool applyDamage = false; //determines whether the player is currently being damaged
	[HideInInspector]
	public bool blink = false; //determines whether the player can blink while damaged
	[HideInInspector]
	public int enemyAttackPower = 2; //the amount of damage you take from enemies (measured in quarter hearts)
	private bool disableCollider; //disables the player's collider while respawning (so that we don't take any objects with us)
	private float regainHealthTimer; //the amount of time since we were last damaged (used to regain health)
	
	//private health item variables
	private int quartersOfHealthToRegain; //the amount of health you regain from health items (measured in quarter hearts)
	//private damage item variables
	private int quartersOfHealthToLose; //the amount of damage you take from damage items (measured in quarter hearts)
	
	//GUI spacing
	private float spacingX;
	private float spacingY;
	
	//GUI positioning
	private float newX;
	private float newY;
	private float valueX;
	private float valueY;
	private float valueX2;
	private float valueY2;
	
	//distance and angle measurements
	private float dist; //distance (on the x and z-axis) between the player and the closest enemy
	private float distY; //distance (on the y-axis) between the player and the closest enemy
	private float angle; //angle between the player and the closest enemy
	private GameObject closest; //the current, closest enemy to the player
	
	void Start () {
		
		//setting position of hearts
		newX = ((Screen.width/476f) + 1.857143f) * 0.35f;
		newY = ((Screen.height/476f) + 1.857143f) * 0.35f;
		
		if (newY <= 1.0f){
			
			if (newY > 0.89f){
				valueY = Mathf.Abs(1 - newY)/2;
			}
			else if (newY >= 0.78f){
				valueY = Mathf.Abs(newY - 0.78f)/2;
			}
			
		}
		else {
			valueY = Mathf.Abs(1 - newY)/-1.5f;
		}
		
		if (newX <= 0.88f){
			valueX = Mathf.Abs(0.88f - newX)/-2;
		}
		
		newNumberOfHearts = health.numberOfHearts;
		lastNumberOfHearts = health.numberOfHearts;
		spacingX = health.healthGUI.gUIScale.x;
		spacingY = -health.healthGUI.gUIScale.y;
		
		//creating hearts
		AddHearts(newNumberOfHearts);
		
		//setting damage timers
		blinkTimer = damage.damageBlinkSpeed + 0.02f;
		damageTimeoutTimer = damage.secondsToStayInvincibleAfterBeingHurt + 0.02f;
		
	}
	
	void Update() {
		
		renderers = transform.GetComponentsInChildren<Renderer>(true);
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag(damage.enemyTag); 
		float distance = Mathf.Infinity; 
		Vector3 position = transform.position; 
		// Iterate through them and find the closest one
		foreach (GameObject go in gos)  { 
			Vector3 diff = (go.transform.position - position);
			float curDistance = diff.sqrMagnitude; 
			if (curDistance < distance) { 
				closest = go; 
				distance = curDistance; 
			} 
		}
		if (closest == null){
			return;
		}
		Vector3 dir = (closest.transform.position - transform.position);
		angle = Vector3.Angle(dir, transform.forward);
		dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(closest.transform.position.x - transform.position.x),2) + Mathf.Pow(Mathf.Abs(closest.transform.position.z - transform.position.z),2));
		distY = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(closest.transform.position.y - transform.position.y),2));
		if (angle <= 60 && dist <= 3 && distY <= 5){
			invincible = true;
		}
		else {
			invincible = false;
		}

		if (damageTimeoutTimer > damage.secondsToStayInvincibleAfterBeingHurt){
			foreach (Renderer r in renderers) {
				r.enabled = true;
				canDamage = true;
				canDamage2 = true;
			}
		}
		
		//keeping the player from being damaged if he just attacked
		if (invincible){
			if (transform.GetComponent<PlayerController>().attackTimer <= damage.secondsToStayInvincibleAfterAttacking){
				canDamage2 = false;
			}
		}
		else {
			canDamage2 = true;
		}
		
		//applying damage to player
		if (applyDamage && canDamage && enemyAttackPower != 0){
			ApplyDamage();
			applyDamage = false;
		}
		damageTimer += 0.02f;
		if (damageTimer >= damage.secondsToStayInvincibleAfterBeingHurt){
			canDamage = true;
		}
		
		//blinking after being damaged
		if (blink && damageTimeoutTimer >= damage.secondsToStayInvincibleAfterBeingHurt && enemyAttackPower != 0){
			BlinkOut();
			blink = false;
		}
		if (blinkTimer > damage.damageBlinkSpeed){
			blinkTimer = 0;
		}
		if (damage.damageBlinkSpeed > 0){
			if (blinkTimer == 0 && damageTimeoutTimer <= damage.secondsToStayInvincibleAfterBeingHurt - 0.2f){
				foreach (Renderer r in renderers) {
					if (r.enabled == true){
						r.enabled = false;
					}	
					else if (r.enabled == false){
						r.enabled = true;
					}
				}
			}
			else if (damageTimeoutTimer > damage.secondsToStayInvincibleAfterBeingHurt - 0.2f){
				foreach (Renderer r in renderers) {
					r.enabled = true;
				}
			}
		}
		blinkTimer += 0.02f;
		damageTimeoutTimer += 0.02f;
		
	}
	
	void FixedUpdate() {
		
		//reloading health if player is killed
		if (currentHealth <= 0){
			//reloading player's health
			Invoke("Reload", 0.25f);
			//disabling player's collider so that our sudden change in position (when we respawn) does not throw an enemy, object, etc.
			if (GetComponent<Collider>() && GetComponent<Collider>().enabled){
				disableCollider = true;
			}
			else {
				disableCollider = false;
			}
			if (disableCollider){
				GetComponent<Collider>().enabled = false;
			}
			//respawn
			transform.position = respawn.respawnLocation;
			transform.rotation = Quaternion.Euler(respawn.respawnRotation.x, respawn.respawnRotation.y, respawn.respawnRotation.z);
			
			//setting player's side-scrolling rotation to what it is currently closest to (if we are side scrolling / an axis is locked)
			if (GetComponent<PlayerController>()){
				float yRotationValue;
				if (respawn.respawnRotation.y > 180){
					yRotationValue = transform.eulerAngles.y - 360;
				}
				else {
					yRotationValue = transform.eulerAngles.y;
				}
				//getting rotation on z-axis (x-axis is locked)
				if (GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnXAxis && !GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnZAxis){
					//if our rotation is closer to the right, set the bodyRotation to the right
					if (yRotationValue >= 90){
						GetComponent<PlayerController>().horizontalValue = -1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = 180.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = 179.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						GetComponent<PlayerController>().horizontalValue = 1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = -0.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = 0.001f;
						}
					}
				}
				//getting rotation on x-axis (z-axis is locked)
				else if (GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnZAxis && !GetComponent<PlayerController>().movement.sideScrolling.lockMovementOnXAxis){
					//if our rotation is closer to the right, set the bodyRotation to the right
					if (yRotationValue >= 0){
						GetComponent<PlayerController>().horizontalValue = 1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = 90.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = 89.999f;
						}
					}
					//if our rotation is closer to the left, set the bodyRotation to the left
					else {
						GetComponent<PlayerController>().horizontalValue = -1;
						if (GetComponent<PlayerController>().movement.sideScrolling.rotateInwards){
							GetComponent<PlayerController>().bodyRotation = -90.001f;
						}
						else {
							GetComponent<PlayerController>().bodyRotation = -89.999f;
						}
					}
				}
			}
			
			
			
			//re-enable collider
			if (disableCollider){
				GetComponent<Collider>().enabled = true;
			}
		}
		
		//getting number of hearts
		if (health.numberOfHearts >= 0 && health.maxHeartsPerRow > 0){
			newNumberOfHearts = health.numberOfHearts;
		}
		else {
			newNumberOfHearts = 0;
		}
		
		//positioning hearts
		for (int i = 0; i < hearts.Count; i++){
			
			int posY;
			if (health.maxHeartsPerRow > 0){
				posY = (int)(Mathf.FloorToInt(i / health.maxHeartsPerRow));
			}
			else {
				posY = (int)(Mathf.FloorToInt(0));
			}
			int posX = (int)(i - posY * health.maxHeartsPerRow);
			
			//spacing hearts
			if (hearts[i].GetComponent<GUITexture>()){
				hearts[i].GetComponent<GUITexture>().pixelInset = new Rect((posX * spacingX) * health.healthGUI.gUISpacing.x, (posY * spacingY) * health.healthGUI.gUISpacing.y, health.healthGUI.gUIScale.x, health.healthGUI.gUIScale.y);
				if (health.healthGUI.overlayHearts){
					hearts[i].transform.localPosition = new Vector3(hearts[i].transform.localPosition.x, hearts[i].transform.localPosition.y, i/10f);
				}
				else {
					hearts[i].transform.localPosition = new Vector3(hearts[i].transform.localPosition.x, 0);
				}
			}
			
			if (i + 1 > newNumberOfHearts){
				Destroy(hearts[i]);
				hearts.RemoveAt(i);
			}
			
		}
		
		if (currentHealth > newNumberOfHearts*40){
			currentHealth = newNumberOfHearts*40;
		}
		
		if (newNumberOfHearts > lastNumberOfHearts){
			AddHearts(newNumberOfHearts - lastNumberOfHearts);
		}
		lastNumberOfHearts = newNumberOfHearts;
		
		newX = ((Screen.width/476f) + 1.857143f) * 0.35f;
		newY = ((Screen.height/476f) + 1.857143f) * 0.35f;
		
		if ((health.healthGUI.gUIPosition.x/100) * 10f < 1){
			valueX2 = (health.healthGUI.gUIPosition.x/100) * 10f;
		}
		else {
			valueX2 = 1;
		}
		if ((health.healthGUI.gUIPosition.y/100) * 10f < 1){
			valueY2 = (health.healthGUI.gUIPosition.y/100) * 10f;
		}
		else {
			valueY2 = 1;
		}
		
		if (newY <= 1.0f){
			
			if (newY > 0.89f){
				valueY = (Mathf.Abs(1 - newY)/2) * valueY2;
			}
			else if (newY >= 0.78f){
				valueY = (Mathf.Abs(newY - 0.78f)/2) * valueY2;
			}
			else if (newY >= 0.76f){
				valueY = (Mathf.Abs(newY - 0.78f)*-1) * valueY2;
			}
			
		}
		else {
			valueY = (Mathf.Abs(1 - newY)/-1.5f) * valueY2;
		}
		
		if (newX <= 0.88f){
			valueX = (Mathf.Abs(0.88f - newX)*-1.5f) * valueX2;
		}
		else {
			valueX = 0;
		}
		
		//creating health for player
		if (healthHolder == null){
			healthHolder = new GameObject();
		}
		else {
			healthHolder.gameObject.name = "PlayerHealth";
			healthHolder.transform.parent = transform;
			healthHolder.transform.position = new Vector3(((health.healthGUI.gUIPosition.x/100) * newX) + valueX, ((health.healthGUI.gUIPosition.y/100) * newY) + valueY, health.healthGUI.gUIPosition.z);
			healthHolder.transform.rotation = Quaternion.identity;
		}
		
		//regaining health (if the enemy is allowed to)
		if (health.regainHealthOverTime){
			//increasing regainHealthTimer
			if (currentHealth > 0 && currentHealth < newNumberOfHearts*40){
				regainHealthTimer += 0.02f;
			}
			else {
				regainHealthTimer = 0.0f;
			}
			
			//regaining health
			if (regainHealthTimer > health.timeNeededToRegainHealth){
				modifyHealth(health.quartersOfHealthToRegain);
				regainHealthTimer = 0.0f;
			}
		}
		else {
			regainHealthTimer = 0.0f;
		}
		
	}
	
	void LateUpdate () {
		
		//making sure the GUI is not affected by the parent's transform
		if (healthHolder == null){
			healthHolder = new GameObject();
		}
		else {
			healthHolder.gameObject.name = "PlayerHealth";
			healthHolder.transform.parent = transform;
			healthHolder.transform.position = new Vector3(((health.healthGUI.gUIPosition.x/100) * newX) + valueX, ((health.healthGUI.gUIPosition.y/100) * newY) + valueY, health.healthGUI.gUIPosition.z);
			healthHolder.transform.rotation = Quaternion.identity;
		}
		
		//positioning hearts
		for (int i = 0; i < hearts.Count; i++){
			
			int posY;
			if (health.maxHeartsPerRow > 0){
				posY = (int)(Mathf.FloorToInt(i / health.maxHeartsPerRow));
			}
			else {
				posY = (int)(Mathf.FloorToInt(0));
			}
			int posX = (int)(i - posY * health.maxHeartsPerRow);
			
			//spacing hearts
			if (hearts[i].GetComponent<GUITexture>()){
				hearts[i].GetComponent<GUITexture>().pixelInset = new Rect((posX * spacingX) * health.healthGUI.gUISpacing.x, (posY * spacingY) * health.healthGUI.gUISpacing.y, health.healthGUI.gUIScale.x, health.healthGUI.gUIScale.y);
				if (health.healthGUI.overlayHearts){
					hearts[i].transform.localPosition = new Vector3(hearts[i].transform.localPosition.x, hearts[i].transform.localPosition.y, i/10f);
				}
				else {
					hearts[i].transform.localPosition = new Vector3(hearts[i].transform.localPosition.x, 0);
				}
			}
			
			if (i + 1 > newNumberOfHearts){
				Destroy(hearts[i]);
				hearts.RemoveAt(i);
			}
			
		}
		
	}
	
	void Reload() {
		if (!enabled) return;
		currentHealth = newNumberOfHearts*40;
		modifyHealth(1);
	}
	
	public void AddHearts(int n) {
		if (!enabled) return;
		
		if (healthHolder == null){
			healthHolder = new GameObject();
		}
		
		if (newNumberOfHearts >= 0 && health.maxHeartsPerRow > 0){
			for (int i = 0; i < n; i ++) {
				//creating heart GUI Texture so that we can create the heart images
				GameObject heartGUI = new GameObject();
				heartGUI.transform.localScale = new Vector3(0, 0, 0);
				heartGUI.AddComponent<GUITexture>();
				//creating hearts
				Transform newHeart = ((GameObject)Instantiate(heartGUI, healthHolder.transform.position, Quaternion.identity)).transform; // Creates a new heart
				newHeart.gameObject.name = "Heart" + (i + 1);
				newHeart.parent = healthHolder.transform;
				//destroying heart GUI Texture
				Destroy(heartGUI);
				
				int posY;
				if (health.maxHeartsPerRow > 0){
					posY = (int)(Mathf.FloorToInt(i / health.maxHeartsPerRow));
				}
				else {
					posY = (int)(Mathf.FloorToInt(0));
				}
				int posX = (int)(hearts.Count - posY * health.maxHeartsPerRow);
				
				newHeart.GetComponent<GUITexture>().pixelInset = new Rect((posX * spacingX) * health.healthGUI.gUISpacing.x, (posY * spacingY) * health.healthGUI.gUISpacing.y, health.healthGUI.gUIScale.x, health.healthGUI.gUIScale.y);
				newHeart.GetComponent<GUITexture>().texture = health.healthGUI.heartImages[0];
				hearts.Add(newHeart.gameObject);
				
			}
		}
		currentHealth = newNumberOfHearts*40;
		UpdateHearts();
	}

	
	public void modifyHealth(int amount) {
		if (!enabled) return;
		currentHealth += (10)*amount;
		regainHealthTimer = 0.0f;
		UpdateHearts();
	}

	void UpdateHearts() {
		if (!enabled) return;
		bool restAreEmpty = false;
		int i = 0;
		
		foreach (GameObject heart in hearts) {
			
			if (restAreEmpty) {
				heart.GetComponent<GUITexture>().texture = health.healthGUI.heartImages[0]; // heart is empty
			}
			else {
				i += 1; // current iteration
				if (currentHealth >= i * 40) {
					heart.GetComponent<GUITexture>().texture = health.healthGUI.heartImages[health.healthGUI.heartImages.Length-1]; // health of current heart is full
				}
				else {
					int currentHeartHealth = (int)(40 - (40 * i - currentHealth));
					int healthPerImage = 40 / health.healthGUI.heartImages.Length; // how much health is there per image
					int imageIndex;
					if ((int)(currentHeartHealth / healthPerImage) >= 0) {
						imageIndex = currentHeartHealth / healthPerImage;
					}
					else {
						imageIndex = 0;
					}
					
					
					if (imageIndex == 0 && currentHeartHealth > 0) {
						imageIndex = 1;
					}

					heart.GetComponent<GUITexture>().texture = health.healthGUI.heartImages[imageIndex];
					restAreEmpty = true;
				}
			}
			
		}
	}
	
	void OnGUI() {
		
		if (health.debugHealth){
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4, 100, 25), "Regain Health")) {
				modifyHealth(1);
			}
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4 + Screen.height/10, 100, 25), "Take Damage")) {
				modifyHealth(-1);
			}
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4 + Screen.height/10 * 2, 100, 25), "Add Heart")) {
				health.numberOfHearts += 1;
				currentHealth = health.numberOfHearts;
			}
			if (GUI.Button(new Rect(Screen.width / 1.5f, Screen.height/4 + Screen.height/10 * 3, 100, 25), "Remove Heart") && newNumberOfHearts != 0) {
				health.numberOfHearts -= 1;
			}
		}
		
	}
	
	
	public void RegainHealth () {
		if (!enabled) return;
		modifyHealth(quartersOfHealthToRegain);
	}
	
	public void LoseHealth () {
		if (!enabled) return;
		modifyHealth(-quartersOfHealthToLose);
	}
	
	public void ApplyDamage () {
		if (!enabled) return;
		modifyHealth(-enemyAttackPower);
		canDamage = false;
		damageTimer = 0.0f;
	}

	public void BlinkOut() {
		if (!enabled) return;
		blinkTimer = 0;
		damageTimeoutTimer = 0;
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (!enabled) return;
		
		//health items
		for (int i = 0; i < healthItems.Length; i++){
			
			if (hit.gameObject.tag == healthItems[i].healthItemTag){
				
				quartersOfHealthToRegain = healthItems[i].quartersOfHealthToRegain;
				RegainHealth();
				Destroy(hit.gameObject);
				
			}
			
		}
		
		//damage items
		for (int i = 0; i < damageItems.Length; i++){
			
			if (hit.gameObject.tag == damageItems[i].damageItemTag && canDamage && canDamage2){
				
				quartersOfHealthToLose = damageItems[i].quartersOfHealthToLose;
				LoseHealth();
				blinkTimer = 0;
				damageTimeoutTimer = 0;
				canDamage = false;
				damageTimer = 0.0f;
				blink = true;
				
			}
			
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		if (!enabled) return;

		//health items
		for (int i = 0; i < healthItems.Length; i++){
			
			if (hit.gameObject.tag == healthItems[i].healthItemTag){
				
				quartersOfHealthToRegain = healthItems[i].quartersOfHealthToRegain;
				RegainHealth();
				Destroy(hit.gameObject);
				
			}
			
		}
		
		//damage items
		for (int i = 0; i < damageItems.Length; i++){
			
			if (hit.gameObject.tag == damageItems[i].damageItemTag && canDamage && canDamage2){
				
				quartersOfHealthToLose = damageItems[i].quartersOfHealthToLose;
				LoseHealth();
				blinkTimer = 0;
				damageTimeoutTimer = 0;
				canDamage = false;
				damageTimer = 0.0f;
				blink = true;
				
			}
			
		}
		
	}
	
	void OnTriggerStay (Collider hit) {
		if (!enabled) return;
		
		//health items
		for (int i = 0; i < healthItems.Length; i++){
			
			if (hit.gameObject.tag == healthItems[i].healthItemTag){
				
				quartersOfHealthToRegain = healthItems[i].quartersOfHealthToRegain;
				RegainHealth();
				Destroy(hit.gameObject);
				
			}
			
		}
		
		//damage items
		for (int i = 0; i < damageItems.Length; i++){
			
			if (hit.gameObject.tag == damageItems[i].damageItemTag && canDamage && canDamage2){
				
				quartersOfHealthToLose = damageItems[i].quartersOfHealthToLose;
				LoseHealth();
				blinkTimer = 0;
				damageTimeoutTimer = 0;
				canDamage = false;
				damageTimer = 0.0f;
				blink = true;
				
			}
			
		}
		
	}
	
	void OnEnable () {
		
		if (healthHolder != null){
			healthHolder.SetActive(true);
		}
		
	}
	
	void OnDisable () {
		
		if (healthHolder != null){
			healthHolder.SetActive(false);
		}
		
	}
	
}