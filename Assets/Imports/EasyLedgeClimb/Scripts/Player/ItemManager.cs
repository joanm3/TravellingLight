/// Easy Ledge Climb Character System
/// ItemManager.cs
///
/// This script allows the player to:
/// 1. Have an inventory of different items.
/// 2. Count and display the number of items the player currently holds (either by simply counting the number, showing the number relative to the item's limit (with zeros before it),
///	or by adding by a prefix or suffix to the number).
/// 3. Set limits for the number of items the player can have.
/// 4. Set GUI images and text for different items.
/// 5. Position the GUI images and text anywhere on the screen.
///
/// (C) 2015-2016 Grant Marrs

using UnityEngine;
using System.Collections;

public class ItemManager : MonoBehaviour {
	
	[System.Serializable]
	public class ItemList {
		
		//item data
		public string itemTag = "Coin"; //the tag of the item
		public Texture itemGUI; //the GUI of the item
		public Vector3 gUIPosition = new Vector3(0.8f, 76, 1); //the position of the item GUI
		public Vector2 gUIScale = new Vector2(50, 50); //the scale of the item GUI
		public Font font; //the font of the text
		public int fontSize = 60; //the size of the text font
		public int outlineSize = 19; //the size of the text outline
		public Color fontColor = Color.white; //the color of the text
		public Color outlineColor = Color.black; //the color of the text outline
		public string itemCountPrefix; //prefix that comes before the item count
		public string itemCountSuffix; //suffix that comes after the item count
		public bool useItemLimit = true; //determines whether or not limit the amount of items you can have
		public int maximumItemLimit = 99; //the maximum amount of items you can have
		public bool addZerosBeforeItemCount = true; //adds zeros before the item count (the number of zeros added is relative to the maximum item limit)
		public Vector3 gUITextPosition = new Vector3(4.7f, 36.1f, 1); //the position of the GUI item count text
		
		//item count variables
		[HideInInspector]
		public int currentValue;
		[HideInInspector]
		public string itemCount;
		[HideInInspector]
		public GameObject item;
		
		//GUI image position values
		[HideInInspector]
		public float valueX;
		[HideInInspector]
		public float valueY;
		[HideInInspector]
		public float valueX2;
		[HideInInspector]
		public float valueY2;
		[HideInInspector]
		public Vector3 imagePos;
		
		//GUI text position values
		[HideInInspector]
		public float textValueX;
		[HideInInspector]
		public float textValueY;
		[HideInInspector]
		public float textValueX2;
		[HideInInspector]
		public float textValueY2;
		[HideInInspector]
		public Vector3 textPos;
		
	}
	public ItemList[] itemList = new ItemList[1]; //the items that the player can carry
	
	//positioning variables
	private float newX;
	private float newY;
	
	private GameObject itemHolder; //the parent of the item GUI
	
	// Use this for initialization
	void Start () {
		
		//setting GUI position of items
		newX = ((Screen.width/476f) + 1.857143f) * 0.35f;
		newY = ((Screen.height/476f) + 1.857143f) * 0.35f;
		
		for (int i = 0; i < itemList.Length; i++){
			if (newY <= 1.0f){
				
				if (newY > 0.89f){
					itemList[i].valueY = Mathf.Abs(1 - newY)/2;
				}
				else if (newY >= 0.78f){
					itemList[i].valueY = Mathf.Abs(newY - 0.78f)/2;
				}
				
			}
			else {
				itemList[i].valueY = Mathf.Abs(1 - newY)/-1.5f;
			}
			
			if (newX <= 0.88f){
				itemList[i].valueX = Mathf.Abs(0.88f - newX)/-2;
			}
		}
		
	}
	
	void FixedUpdate () {
		
		newX = ((Screen.width/476f) + 1.857143f) * 0.35f;
		newY = ((Screen.height/476f) + 1.857143f) * 0.35f;
		
		if (itemHolder == null){
			itemHolder = new GameObject();
		}
		else {
			itemHolder.gameObject.name = "ItemHolder";
			itemHolder.transform.parent = transform;
			itemHolder.transform.position = transform.position;
			itemHolder.transform.rotation = Quaternion.identity;
		}
		
		for (int i = 0; i < itemList.Length; i++){
			
			if (itemList[i].useItemLimit && itemList[i].currentValue > itemList[i].maximumItemLimit){
				itemList[i].currentValue = itemList[i].maximumItemLimit;
			}
			
			string stringScoreLength = itemList[i].maximumItemLimit.ToString();
			
			// Change to how many digits you want
			int scoreLength = stringScoreLength.Length;
			
			// score as a string
			string scoreString = itemList[i].currentValue.ToString();
			
			// get number of 0s needed
			int numZeros = scoreLength - scoreString.Length;
			
			string newScore = "";
			for(int z = 0; z < numZeros; z++){
				newScore += "0";
			}
			
			if (itemList[i].useItemLimit && itemList[i].addZerosBeforeItemCount){
				itemList[i].itemCount = newScore + scoreString;
			}
			else {
				itemList[i].itemCount = scoreString;
			}
			
			
			//GUI positioning for image
			if ((itemList[i].gUIPosition.x/100) * 10f < 1){
				itemList[i].valueX2 = (itemList[i].gUIPosition.x/100) * 10f;
			}
			else {
				itemList[i].valueX2 = 1;
			}
			if ((itemList[i].gUIPosition.y/100) * 10f < 1){
				itemList[i].valueY2 = (itemList[i].gUIPosition.y/100) * 10f;
			}
			else {
				itemList[i].valueY2 = 1;
			}
			
			if (newY <= 1.0f){
				
				if (newY > 0.89f){
					itemList[i].valueY = (Mathf.Abs(1 - newY)/2) * itemList[i].valueY2;
				}
				else if (newY >= 0.78f){
					itemList[i].valueY = (Mathf.Abs(newY - 0.78f)/2) * itemList[i].valueY2;
				}
				else if (newY >= 0.76f){
					itemList[i].valueY = (Mathf.Abs(newY - 0.78f)*-1) * itemList[i].valueY2;
				}
				
			}
			else {
				itemList[i].valueY = (Mathf.Abs(1 - newY)/-1.5f) * itemList[i].valueY2;
			}
			
			if (newX <= 0.88f){
				itemList[i].valueX = (Mathf.Abs(0.88f - newX)*-1.5f) * itemList[i].valueX2;
			}
			else {
				itemList[i].valueX = 0;
			}
			
			itemList[i].imagePos = new Vector3(((itemList[i].gUIPosition.x/100) * newX) + itemList[i].valueX, ((itemList[i].gUIPosition.y/100) * newY) + itemList[i].valueY, itemList[i].gUIPosition.z);
			if (itemHolder != null){
				if (itemList[i].item == null){
					if (itemList[i].itemGUI != null && itemHolder.transform.childCount < itemList.Length){
						//creating item GUI Texture so that we can create the item's GUI image
						GameObject itemGUITexture = new GameObject();
						itemGUITexture.transform.localScale = new Vector3(0, 0, 0);
						itemGUITexture.AddComponent<GUITexture>();
						itemGUITexture.GetComponent<GUITexture>().texture = itemList[i].itemGUI;
						//creating the GUI image of the item
						itemList[i].item = (GameObject)Instantiate(itemGUITexture.gameObject, transform.position, Quaternion.identity);
						Destroy(itemGUITexture.gameObject);
					}
				}
				else {
					itemList[i].item.gameObject.name = itemList[i].itemTag + "GUI";
					itemList[i].item.transform.parent = itemHolder.transform;
					itemList[i].item.transform.position = itemList[i].imagePos;
					itemList[i].item.transform.rotation = Quaternion.identity;
					itemList[i].item.GetComponent<GUITexture>().texture = itemList[i].itemGUI;
					itemList[i].item.GetComponent<GUITexture>().pixelInset = new Rect(itemList[i].item.GetComponent<GUITexture>().pixelInset.x, itemList[i].item.GetComponent<GUITexture>().pixelInset.y, itemList[i].gUIScale.x, itemList[i].gUIScale.y);
				}
			}
			
			
			//GUI positioning for text
			if ((itemList[i].gUITextPosition.x*10) * 10f < 1){
				itemList[i].textValueX2 = (itemList[i].gUITextPosition.x*10) * 10f;
			}
			else {
				itemList[i].textValueX2 = 1;
			}
			if (((itemList[i].gUITextPosition.y - 41.3f)*-10) * 10f < 1){
				itemList[i].textValueY2 = ((itemList[i].gUITextPosition.y - 41.3f)*-10) * 10f;
			}
			else {
				itemList[i].textValueY2 = 1;
			}
			
			if (newY <= 1.0f){
				
				if (newY > 0.89f){
					itemList[i].textValueY = (Mathf.Abs(1 - newY)/2) * itemList[i].textValueY2;
				}
				else if (newY >= 0.78f){
					itemList[i].textValueY = (Mathf.Abs(newY - 0.78f)/2) * itemList[i].textValueY2;
				}
				else if (newY >= 0.76f){
					itemList[i].textValueY = (Mathf.Abs(newY - 0.78f)*-1) * itemList[i].textValueY2;
				}
				
			}
			else {
				itemList[i].textValueY = (Mathf.Abs(1 - newY)/-1.5f) * itemList[i].textValueY2;
			}
			
			if (newX <= 0.88f){
				itemList[i].textValueX = (Mathf.Abs(0.88f - newX)*-1.5f) * itemList[i].textValueX2;
			}
			else {
				itemList[i].textValueX = 0;
			}
			
			itemList[i].textPos = new Vector3( (((itemList[i].gUITextPosition.x*10) * newX) + itemList[i].textValueX) / (597.1f/Screen.width + 0.3f) , ((((itemList[i].gUITextPosition.y - 41.3f)*-10) * newY) + itemList[i].textValueY/2) / (547.4f/Screen.height - 0.15f), itemList[i].gUITextPosition.z);
			
		}
		
	}
	
	void LateUpdate () {
		
		//making sure we still have the item holder
		if (itemHolder == null){
			itemHolder = new GameObject();
		}
		else {
			itemHolder.gameObject.name = "ItemHolder";
			itemHolder.transform.parent = transform;
			itemHolder.transform.position = transform.position;
			itemHolder.transform.rotation = Quaternion.identity;
		}
		
		//making sure the GUI is not affected by the parent's transform
		for (int i = 0; i < itemList.Length; i++){
			
			itemList[i].imagePos = new Vector3(((itemList[i].gUIPosition.x/100) * newX) + itemList[i].valueX, ((itemList[i].gUIPosition.y/100) * newY) + itemList[i].valueY, itemList[i].gUIPosition.z);
			if (itemHolder != null){
				if (itemList[i].item == null){
					if (itemList[i].itemGUI != null && itemHolder.transform.childCount < itemList.Length){
						//creating item GUI Texture so that we can create the item's GUI image
						GameObject itemGUITexture = new GameObject();
						itemGUITexture.transform.localScale = new Vector3(0, 0, 0);
						itemGUITexture.AddComponent<GUITexture>();
						itemGUITexture.GetComponent<GUITexture>().texture = itemList[i].itemGUI;
						//creating the GUI image of the item
						itemList[i].item = (GameObject)Instantiate(itemGUITexture.gameObject, transform.position, Quaternion.identity);
						Destroy(itemGUITexture.gameObject);
					}
				}
				else {
					itemList[i].item.gameObject.name = itemList[i].itemTag + "GUI";
					itemList[i].item.transform.parent = itemHolder.transform;
					itemList[i].item.transform.position = itemList[i].imagePos;
					itemList[i].item.transform.rotation = Quaternion.identity;
					itemList[i].item.GetComponent<GUITexture>().texture = itemList[i].itemGUI;
					itemList[i].item.GetComponent<GUITexture>().pixelInset = new Rect(itemList[i].item.GetComponent<GUITexture>().pixelInset.x, itemList[i].item.GetComponent<GUITexture>().pixelInset.y, itemList[i].gUIScale.x, itemList[i].gUIScale.y);
				}
			}
			
			itemList[i].textPos = new Vector3( (((itemList[i].gUITextPosition.x*10) * newX) + itemList[i].textValueX) / (597.1f/Screen.width + 0.3f) , ((((itemList[i].gUITextPosition.y - 41.3f)*-10) * newY) + itemList[i].textValueY/2) / (547.4f/Screen.height - 0.15f), itemList[i].gUITextPosition.z);
			
		}
		
		
	}
	
	void OnGUI() {
		
		for (int i = 0; i < itemList.Length; i++){
			if (itemList[i].fontSize != 0){
				
				if (itemList[i].outlineSize != 0){
					DrawOutline(new Rect (itemList[i].textPos.x, itemList[i].textPos.y - 2, Screen.width, Screen.height), itemList[i].itemCountPrefix + itemList[i].itemCount + itemList[i].itemCountSuffix, itemList[i].font, itemList[i].fontSize, itemList[i].outlineSize, GUI.skin.GetStyle("label"), itemList[i].outlineColor);
				}
				DrawInside(new Rect (itemList[i].textPos.x, itemList[i].textPos.y - 2, Screen.width, Screen.height), itemList[i].itemCountPrefix + itemList[i].itemCount + itemList[i].itemCountSuffix, itemList[i].font, itemList[i].fontSize, GUI.skin.GetStyle("label"), itemList[i].fontColor);
			
			}
		}
		
	}
	
	//draw text of a specified color, with a specified outline color
	public static void DrawOutline(Rect position, string text, Font font, int size, int outline, GUIStyle style, Color outColor){
		Rect position2 = position;
		GUIStyle backupStyle = style;
		style.font = font;
		style.fontSize = size;
		style.normal.textColor = outColor;
		
		//middle
		position2.x = position.x;
		GUI.Label(position2, text, style);
		position2.y = position.y;
		GUI.Label(position2, text, style);
		
		//left and right
		position2.x = position.x - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x + ((float)outline/12);
		GUI.Label(position2, text, style);
		
		//up and down
		position2.x = position.x;
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x;
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		
		position2.y = position.y + 0.25f*((float)outline/12);
		GUI.Label(position2, text, style);
		position2.y = position.y - 0.25f*((float)outline/12);
		GUI.Label(position2, text, style);
		position2.y = position.y + 0.5f*((float)outline/12);
		GUI.Label(position2, text, style);
		position2.y = position.y - 0.5f*((float)outline/12);
		GUI.Label(position2, text, style);
		position2.y = position.y + 0.75f*((float)outline/12);
		GUI.Label(position2, text, style);
		position2.y = position.y - 0.75f*((float)outline/12);
		GUI.Label(position2, text, style);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and down
		position2.x = position.x - 0.25f*((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x - 0.5f*((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x - 0.75f*((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and up
		position2.x = position.x + 0.25f*((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x + 0.5f*((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x + 0.75f*((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and down
		position2.x = position.x + 0.25f*((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x + 0.5f*((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x + 0.75f*((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y - ((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and up
		position2.x = position.x - 0.25f*((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x - 0.5f*((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x - 0.75f*((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y + ((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and down
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y - 0.25f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and up
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y + 0.25f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and down
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y - 0.25f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and up
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y + 0.25f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and down
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y - 0.25f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and up
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y + 0.5f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and down
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y - 0.5f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and up
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y + 0.5f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and down
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y - 0.75f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and up
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y + 0.75f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//right and down
		position2.x = position.x + ((float)outline/12);
		position2.y = position.y - 0.75f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		//left and up
		position2.x = position.x - ((float)outline/12);
		position2.y = position.y + 0.75f*((float)outline/12);
		GUI.Label(position2, text, style);
		
		
		style = backupStyle;
	}
	
	//draw text of a specified color, with a specified outline color
	public static void DrawInside(Rect position, string text, Font font, int size, GUIStyle style, Color inColor){
		GUIStyle backupStyle = style;
		style.font = font;
		style.fontSize = size;
		style.normal.textColor = inColor;
		GUI.Label(position, text, style);
		style = backupStyle;
	}
	
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (!enabled) return;
		
		//items
		for (int i = 0; i < itemList.Length; i++){
			
			if (hit.gameObject.tag == itemList[i].itemTag){
				itemList[i].currentValue ++;
				Destroy(hit.gameObject);
			}
			
		}
		
	}
	
	void OnCollisionStay (Collision hit) {
		if (!enabled) return;
		
		//items
		for (int i = 0; i < itemList.Length; i++){
			
			if (hit.gameObject.tag == itemList[i].itemTag){
				itemList[i].currentValue ++;
				Destroy(hit.gameObject);
			}
			
		}
		
	}
	
	void OnTriggerStay (Collider hit) {
		if (!enabled) return;
		
		//items
		for (int i = 0; i < itemList.Length; i++){
			
			if (hit.gameObject.tag == itemList[i].itemTag){
				itemList[i].currentValue ++;
				Destroy(hit.gameObject);
			}
			
		}
		
	}
	
	void OnEnable () {
		
		if (itemHolder != null){
			itemHolder.SetActive(true);
		}
		
	}
	
	void OnDisable () {
		
		if (itemHolder != null){
			itemHolder.SetActive(false);
		}
		
	}
	
}