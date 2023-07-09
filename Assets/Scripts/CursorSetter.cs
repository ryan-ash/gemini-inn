using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSetter : MonoBehaviour
{
    public CursorPreset[] cursors;
    public static CursorSetter instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;    
    }

    // Update is called once per frame
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

    public static void SetDefaultCursor()
    {
        SetCursor("Default");
    }

    public static void SetHoverCursor()
    {
        SetCursor("Hover");
    }

    public static void SetFistCursor()
    {
        SetCursor("Fist");
    }

    public static void SetBribeCursor()
    {
        SetCursor("Bribe");
    }
}

[System.Serializable]
public class CursorPreset
{
    public string Name;
    public Texture2D cursorTexture;
    public Vector2 hotspot;
}
