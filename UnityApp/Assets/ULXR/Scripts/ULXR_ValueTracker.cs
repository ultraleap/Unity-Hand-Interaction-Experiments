using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Syncs values across all UI & provides a central place manage them from */
public class ULXR_ValueTracker : ULXR_Singleton<ULXR_ValueTracker>
{
    public SyncValue<bool> cast = new SyncValue<bool>(false, "cast");
    public SyncValue<bool> wifi = new SyncValue<bool>(false, "wifi");
    public SyncValue<bool> bluetooth = new SyncValue<bool>(false, "bluetooth");

    public SyncValue<int> battery = new SyncValue<int>(100, "battery");
    public SyncValue<int> volume_ambient = new SyncValue<int>(100, "volume_ambient");
    public SyncValue<int> volume_ui = new SyncValue<int>(100, "volume_ui");
    public SyncValue<int> volume_sfx = new SyncValue<int>(100, "volume_sfx");
    public SyncValue<int> volume_global = new SyncValue<int>(100, "volume_global");

    public SyncValue<bool> indexfinger_keyboard = new SyncValue<bool>(true, "indexfinger_keyboard");
    public SyncValue<bool> tutorial_turnoverhand = new SyncValue<bool>(true, "tutorial_turnoverhand");
    public SyncValue<bool> tutorial_arrow_enabled = new SyncValue<bool>(false, "tutorial_arrow_enabled");
    public SyncValue<bool> colourblind = new SyncValue<bool>(false, "colourblind");
    public SyncValue<bool> meshhands = new SyncValue<bool>(false, "meshhands");
    public SyncValue<bool> contactindicator = new SyncValue<bool>(true, "contactindicator");
    public SyncValue<bool> tutorials_enabled = new SyncValue<bool>(true, "tutorials_enabled");

    public SyncValue<string> language = new SyncValue<string>("english", "language");

    /* Get synced value by string (useful for Unity prefabs!) */
    public SyncValue<bool> GetSyncBoolByName(string name)
    {
        switch (name)
        {
            case "cast":
                return cast;
            case "wifi":
                return wifi;
            case "bluetooth":
                return bluetooth;
            case "tutorial_arrow_enabled":
            case "darkmode": //legacy
                return tutorial_arrow_enabled;
            case "indexfinger_keyboard":
                return indexfinger_keyboard;
            case "tutorial_turnoverhand":
                return tutorial_turnoverhand;
            case "colourblind":
                return colourblind;
            case "meshhands":
                return meshhands;
            case "contactindicator":
                return contactindicator;
            case "tutorials_enabled":
                return tutorials_enabled;
        }
        return null;
    }
    public SyncValue<int> GetSyncIntByName(string name)
    {
        switch (name)
        {
            case "battery":
                return battery;
            case "volume_ambient":
                return volume_ambient;
            case "volume_ui":
                return volume_ui;
            case "volume_sfx":
                return volume_sfx;
            case "volume_global":
            case "volume"://legacy
                return volume_global;
        }
        return null;
    }
    public SyncValue<string> GetSyncStringByName(string name)
    {
        switch (name)
        {
            case "language":
                return language;
        }
        return null;
    }
}

/* Templated sync value with on-change event */
public class SyncValue<T>
{
    public delegate void ValueUpdated(T val);
    public ValueUpdated ValueChanged;

    private T value;
    private string varName; //used to sync with playerprefs on server side

    public SyncValue(T val, string name)
    {
        value = val;
        varName = name;
    }

    public void Set(T val)
    {
        value = val;
        ValueChanged?.Invoke(value);
    }
    public T Get()
    {
        return value;
    }
}
