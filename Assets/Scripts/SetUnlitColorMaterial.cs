using UnityEngine;
using ProjectLight.Functions;

[RequireComponent(typeof(Renderer))]
public class SetUnlitColorMaterial : MonoBehaviour
{
    public float velocity = 0.05f;
    public GameObject luz;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.black;
    public bool desactivateOnExit = false;

    bool goActive = false;
    Renderer rend;
    Material mat;
    GameObject luzInstance = null;
    Color colorsInRange;
    Color actualColor;


    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
        mat.color = inactiveColor;
        goActive = false;
        colorsInRange = mat.color;
        actualColor = mat.color;
        colorsInRange.r = Functions.MapRange(colorsInRange.r, 0f, 1f, inactiveColor.r, activeColor.r);
        colorsInRange.g = Functions.MapRange(colorsInRange.g, 0f, 1f, inactiveColor.g, activeColor.g);
        colorsInRange.b = Functions.MapRange(colorsInRange.b, 0f, 1f, inactiveColor.b, activeColor.b);

    }

    void Update()
    {
        if (goActive)
        {
            if (DistantFromTargetColor(goActive))
            {
                SetColorInRange(goActive);
                SetFinalColor();
            }
        }
        else
        {
            if (DistantFromTargetColor(goActive))
            {
                SetColorInRange(goActive);
                SetFinalColor();
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == ("LightSourceCollider"))
        {
            if (luz != null)
            {
                //gameObject.GetComponent<Renderer>().material = white;
                luzInstance = Instantiate(luz, gameObject.transform.position, Quaternion.identity);
            }
            goActive = true;
        }
    }


    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == ("LightSourceCollider"))
        {
            if (luzInstance != null)
            {
                //do it smoothly later on!
                Destroy(luzInstance);
            }
            goActive = false;
        }
    }

    void SetColorInRange(bool goActive)
    {
        if (goActive)
        {
            colorsInRange.r = (activeColor.r > inactiveColor.r) ?
                colorsInRange.r + velocity * Time.deltaTime :
                colorsInRange.r - velocity * Time.deltaTime;
            colorsInRange.g = (activeColor.g > inactiveColor.g) ?
                colorsInRange.g + velocity * Time.deltaTime :
                colorsInRange.g - velocity * Time.deltaTime;
            colorsInRange.b = (activeColor.b > inactiveColor.b) ?
                colorsInRange.b + velocity * Time.deltaTime :
                colorsInRange.b - velocity * Time.deltaTime;
        }
        else
        {
            colorsInRange.r = (inactiveColor.r > activeColor.r) ?
                colorsInRange.r + velocity * Time.deltaTime :
                colorsInRange.r - velocity * Time.deltaTime;
            colorsInRange.g = (inactiveColor.g > activeColor.g) ?
                colorsInRange.g + velocity * Time.deltaTime :
                colorsInRange.g - velocity * Time.deltaTime;
            colorsInRange.b = (inactiveColor.b > activeColor.b) ?
                colorsInRange.b + velocity * Time.deltaTime :
                colorsInRange.b - velocity * Time.deltaTime;
        }
    }


    void SetFinalColor()
    {
        actualColor.r = Functions.MapRange(colorsInRange.r, inactiveColor.r, activeColor.r, 0f, 1f);
        actualColor.g = Functions.MapRange(colorsInRange.g, inactiveColor.g, activeColor.g, 0f, 1f);
        actualColor.b = Functions.MapRange(colorsInRange.b, inactiveColor.b, activeColor.b, 0f, 1f);
        mat.color = actualColor;
    }

    bool DistantFromTargetColor(bool goActive)
    {
        if (goActive)
        {
            if (Mathf.Abs(actualColor.r - activeColor.r) > 0.01f)
                return true;
            if (Mathf.Abs(actualColor.g - activeColor.g) > 0.01f)
                return true;
            if (Mathf.Abs(actualColor.b - activeColor.b) > 0.01f)
                return true;
        }
        else
        {
            if (Mathf.Abs(actualColor.r - inactiveColor.r) > 0.01f)
                return true;
            if (Mathf.Abs(actualColor.g - inactiveColor.g) > 0.01f)
                return true;
            if (Mathf.Abs(actualColor.b - inactiveColor.b) > 0.01f)
                return true;
        }

        return false;
    }
}
