using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSetter : MonoBehaviour
{
    public CursorPreset[] cursors;
    public static CursorSetter instance;
    public bool isPriorityCursor = false;

    void Start()
    {
        instance = this;    
    }

    void Update()
    {
        
    }

    public static void SetCursor(string cursorName)
    {
        foreach (CursorPreset preset in instance.cursors)
        {
            if (preset.Name == cursorName)
            {
                SetCursor(preset);
                return;
            }
        }
        Debug.LogError("Cursor preset not found: " + cursorName);
        CursorPreset fallbackCursor = instance.cursors[0];
        SetCursor(fallbackCursor);
    }

    public static void SetCursor(CursorPreset cursor)
    {
        Cursor.SetCursor(cursor.cursorTexture, cursor.hotspot, CursorMode.ForceSoftware);
    }

    public static void SetDefaultCursor(bool isPriority = false)
    {
        if (isPriority || !instance.isPriorityCursor)
        {
            SetCursor("Default");
            instance.isPriorityCursor = isPriority;
        }
    }

    public static void SetHoverCursor(bool isPriority = false)
    {
        if (isPriority || !instance.isPriorityCursor)
        {
            SetCursor("Hover");
            instance.isPriorityCursor = isPriority;
        }
    }

    public static void SetHoverSpecialCursor(bool isPriority = false)
    {
        if (isPriority || !instance.isPriorityCursor)
        {
            SetCursor("HoverSpecial");
            instance.isPriorityCursor = isPriority;
        }
    }

    public static void SetFistCursor(bool isPriority = false)
    {
        if (isPriority || !instance.isPriorityCursor)
        {
            SetCursor("Fist");
            instance.isPriorityCursor = isPriority;
        }
    }

    public static void SetBribeCursor(bool isPriority = false)
    {
        if (isPriority || !instance.isPriorityCursor)
        {
            SetCursor("Bribe");
            instance.isPriorityCursor = isPriority;
        }
    }

    public static void ResetPriorityCursor()
    {
        instance.isPriorityCursor = false;
        SetDefaultCursor();
    }
}

[System.Serializable]
public class CursorPreset
{
    public string Name;
    public Texture2D cursorTexture;
    public Vector2 hotspot;
}
