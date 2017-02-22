using UnityEngine;
using LightProject;

[RequireComponent(typeof(Renderer))]
public class SetColorMaterial : MonoBehaviour
{
    public enum ColorType { Albedo, Emission };
    public ColorType colorType = ColorType.Albedo;
    public bool desactivateOnExit = false;
    public float velocity = 1f;
    public GameObject luz;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.black;

    bool goActive = false;
    Renderer rend;
    Material mat;
    GameObject luzInstance = null;
    public Light luzLight = null;
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
        colorsInRange.r = LFunctions.MapRange(colorsInRange.r, 0f, 1f, inactiveColor.r, activeColor.r);
        colorsInRange.g = LFunctions.MapRange(colorsInRange.g, 0f, 1f, inactiveColor.g, activeColor.g);
        colorsInRange.b = LFunctions.MapRange(colorsInRange.b, 0f, 1f, inactiveColor.b, activeColor.b);

    }

    void Update()
    {
        if (DistantFromTargetColor())
        {
            SetColorInRange();
            SetFinalColor();
        }

        if (!goActive && luzLight != null)
        {
            if (luzLight.intensity < 0.1f)
            {
                //dont destroy it, put it in a pool later on
                Destroy(luzInstance);
                luzInstance = null;
                luzLight = null;
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == ("LightSourceCollider"))
        {
            if (luz != null && luzInstance == null)
            {
                luzInstance = Instantiate(luz, gameObject.transform.position, Quaternion.identity);
                luzInstance.transform.parent = this.transform;
                luzLight = luzInstance.GetComponentInChildren<Light>();
                if (luzLight != null)
                    luzLight.intensity = 0f;
            }
            goActive = true;
        }
    }


    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == ("LightSourceCollider"))
        {
            goActive = false;
        }
    }

    void SetColorInRange()
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
        actualColor.r = LFunctions.MapRange(colorsInRange.r, inactiveColor.r, activeColor.r, 0f, 1f);
        actualColor.g = LFunctions.MapRange(colorsInRange.g, inactiveColor.g, activeColor.g, 0f, 1f);
        actualColor.b = LFunctions.MapRange(colorsInRange.b, inactiveColor.b, activeColor.b, 0f, 1f);
        switch (colorType)
        {
            case ColorType.Albedo:
                mat.color = actualColor;
                break;
            case ColorType.Emission:
                mat.SetColor("_EmissionColor", actualColor);
                //check why DynamicGI.SetEmissive is not working
                //DynamicGI.SetEmissive(rend, actualColor);
                break;
        }

        if (luzLight)
        {
            luzLight.intensity = LFunctions.MapRange(colorsInRange.r, inactiveColor.r, activeColor.r, 0f, 8f);
        }
    }

    bool DistantFromTargetColor()
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
