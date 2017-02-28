using System;
using UnityEngine;
using UnityEngine.Events;


public class InputManager : PlayerPrefsManager<InputManager>
{


    [SerializeField]
    public KeyEvent left = new KeyEvent();

    [SerializeField]
    public KeyEvent right = new KeyEvent();

    [SerializeField]
    public KeyEvent up = new KeyEvent();

    [SerializeField]
    public KeyEvent down = new KeyEvent();

    [SerializeField]
    public KeyEvent select = new KeyEvent();

    [SerializeField]
    public KeyEvent placeLight = new KeyEvent();

    [SerializeField]
    public KeyEvent showMenu = new KeyEvent();

    [SerializeField]
    public KeyEvent resetCamera = new KeyEvent();

    private const string InputButtonsKey = "InputButtons";



    public Action<Vector2> OnDrag;

    internal void OnDragDelta(Vector2 delta)
    {
        if (OnDrag != null)
            OnDrag.Invoke(delta);
    }

    public bool InputButtons
    {
        get;
        private set;
    }



    public void SetInputButtons(bool value)
    {
        if (value != InputButtons)
        {
            Debug.Log("Changing InputButtons from " + InputButtons + " to " + value, this);
            InputButtons = value;
        }
    }

    protected override void OnLoad()
    {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        const int defaultValue = 1;
#else
        const int defaultValue = 0;
#endif
        SetInputButtons(GetInt(InputButtonsKey, defaultValue) > 0);
    }

    protected override void OnSave()
    {
        SetInt(InputButtonsKey, InputButtons ? 1 : 0);
    }
}
