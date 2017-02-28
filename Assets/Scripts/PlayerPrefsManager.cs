using UnityEngine;

public abstract class PlayerPrefsManager<T> : Singleton<T> where T : PlayerPrefsManager<T>
{
    private bool saveLoadEnabled = true;

    [SerializeField]
    private bool autoSave = true;

    [SerializeField]
    private bool autoLoad = true;

    private string key = "";

    protected override void OnAwake()
    {
        base.OnAwake();
        key = GetType().Name;
    }

    private void OnEnable()
    {
        if (autoLoad)
            Load();
    }

    private void OnDisable()
    {
        if (autoSave)
            Save();
    }

    protected bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    protected void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }


    #region Load

    protected void Load()
    {
        if (saveLoadEnabled)
        {
            saveLoadEnabled = false;
            OnLoad();
            saveLoadEnabled = true;
            Debug.Log("Loaded " + key, this);
        }
    }

    protected abstract void OnLoad();

    protected string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(this.key + key, defaultValue);
    }

    protected bool GetBool(string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetInt(this.key + key, defaultValue ? 1 : 0) != 0;
    }

    protected int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(this.key + key, defaultValue);
    }

    #endregion Load


    #region Save

    protected void Save()
    {
        if (saveLoadEnabled)
        {
            saveLoadEnabled = false;
            OnSave();
            SaveValues();
            Debug.Log("Saved " + key, this);
            saveLoadEnabled = true;
        }
    }

    protected abstract void OnSave();

    protected void SetString(string key, string value)
    {
        PlayerPrefs.SetString(this.key + key, value);
    }

    protected void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(this.key + key, value ? 1 : 0);
    }

    protected void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(this.key + key, value);
    }

    private void SaveValues()
    {
        PlayerPrefs.Save();
    }

    #endregion Save
}
