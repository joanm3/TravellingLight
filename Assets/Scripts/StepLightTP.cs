using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepLightTP : MonoBehaviour
{

    GameObject currentStep;
    public GameObject step;
    public float timer;
    float timerStart;

    //change that with a raycast to the ground
    public float avatarHight;

    public CharacterMotion character;

    void Start()
    {
        timerStart = timer;
        if (!character)
            character = GameObject.FindObjectOfType<CharacterMotion>();
    }
    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            if (character.IsGrounded)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    currentStep = Instantiate(step, gameObject.transform.position, Quaternion.identity);
                    currentStep.transform.localEulerAngles = new Vector3(0, (transform.forward.x) * Mathf.Rad2Deg, 0);
                    currentStep.transform.position += new Vector3(0, -avatarHight, 0);
                    //currentStep.transform.parent = gameObject.transform;
                    timer = timerStart;

                }
            }
        }
    }


}
