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

    public Color ActiveColor { get { return activeColor; } set { activeColor = value; OnColorsChanged(); } }
    public Color InactiveColor { get { return inactiveColor; } set { inactiveColor = value; OnColorsChanged(); } }

    [SerializeField]
    private Color activeColor = Color.white;
    [SerializeField]
    private Color inactiveColor = Color.black;

    bool goActive = false;
    Renderer rend;
    Material mat;
    GameObject luzInstance = null;
    public Light luzLight = null;
    Color colorsInRange;
    Color actualColor;


    void Start()
    {
        goActive = false;
        rend = GetComponent<Renderer>();
        mat = rend.material;
        switch (colorType)
        {
            case ColorType.Albedo:
                mat.color = inactiveColor;
                actualColor = mat.color;
                break;
            case ColorType.Emission:
                mat.SetColor("_EmissionColor", inactiveColor);
                actualColor = mat.GetColor("_EmissionColor");
                break;
        }
        OnColorsChanged();

    }

    void Update()
    {
        //when the colors are far away from target, change them. 
        if (DistantFromTargetColor())
        {
            SetColorInRange();
            SetFinalColor();
        }

        //destroy light when not active and intensity is small. 
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
        //change the colors in a range from 0-1 to avoid the colors to sum differently. check also 
        //if the values of the target color is bigger or smaller to sum or substract them. 
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
        //return the values from 0 to 1 to its original range and then apply them in the color. 
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
                //ERROR!!! check why DynamicGI.SetEmissive is not working
                //it should be the better way as it takes less resources, but its not working. 
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


    void OnColorsChanged()
    {
        switch (colorType)
        {
            case ColorType.Albedo:
                colorsInRange = mat.color;
                break;
            case ColorType.Emission:
                colorsInRange = mat.GetColor("_EmissionColor");
                break;
        }
        colorsInRange.r = LFunctions.MapRange(colorsInRange.r, 0f, 1f, inactiveColor.r, activeColor.r);
        colorsInRange.g = LFunctions.MapRange(colorsInRange.g, 0f, 1f, inactiveColor.g, activeColor.g);
        colorsInRange.b = LFunctions.MapRange(colorsInRange.b, 0f, 1f, inactiveColor.b, activeColor.b);
    }
}
