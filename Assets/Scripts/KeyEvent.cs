using UnityEngine;
using System;

public enum KeyState
{
    Up,
    Down,
    Hold,
}

[Serializable]
public class KeyEvent
{
    public KeyCode KeyCode = KeyCode.None;
    public KeyState KeyState = KeyState.Up;
    public bool Alt = false;
    public bool Ctrl = false;
    public bool Shift = false;

    public bool IsCurrentEvent()
    {
        if (KeyCode == KeyCode.None)
            return false;
        if (Alt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            return false;
        if (Shift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            return false;
        if (Ctrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            return false;

        switch (KeyState)
        {
            case KeyState.Up:   return Input.GetKeyUp(KeyCode);
            case KeyState.Down: return Input.GetKeyDown(KeyCode);
            case KeyState.Hold: return Input.GetKey(KeyCode);
            default:            return false;
        }
    }

    public override string ToString()
    {
        string log = KeyCode.ToString();
        log += " " + KeyState.ToString() + ": ";

        switch (KeyState)
        {
            case KeyState.Up: log += Input.GetKeyUp(KeyCode); break;
            case KeyState.Down: log += Input.GetKeyDown(KeyCode); break;
            case KeyState.Hold: log += Input.GetKey(KeyCode); break;
        }

        log += "   Alt: " + Alt + ": " + (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
        log += "   Shift: " + Shift + ": " + (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        log += "   Ctrl: " + Ctrl + ": " + (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

        return log;
    }
}
