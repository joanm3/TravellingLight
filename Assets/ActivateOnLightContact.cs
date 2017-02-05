using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ActivateOnLightContact : MonoBehaviour
{

    public bool IsActive = false;
    public Color ActiveColor = Color.green;
    public Color UnactiveColor = Color.red;
    private MeshRenderer meshRenderer;
    private bool m_lastActive = false;

    void Start()
    {
        m_lastActive = IsActive;
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = IsActive ? ActiveColor : UnactiveColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_lastActive != IsActive)
        {
            meshRenderer.material.color = IsActive ? ActiveColor : UnactiveColor;
            m_lastActive = IsActive;
        }
    }
}
