using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window : MonoBehaviour
{
    public WindowType windowType;
    public Text windowTitle;
    public bool isOverlay = false;

    [HideInInspector] public bool isOpen = false;
    [HideInInspector] public bool isPinned = false;
    
    private Fader fader;

    void Start()
    {
        fader = GetComponent<Fader>();
        if (windowTitle != null)
            windowTitle.text = windowType.ToString();
    }

    void Update()
    {
        
    }

    public void Pin()
    {
        isPinned = true;
    }

    public void Unpin()
    {
        isPinned = false;
    }

    public void OpenWindow()
    {
        fader.FadeIn();
        isOpen = true;
    }

    public void CloseWindow()
    {
        if (isPinned)
            return;
        fader.FadeOut();
        isOpen = false;
    }
}

[System.Serializable]
public enum WindowType
{
    None,
    Settings,
    Negotiation,
    Quests,
    History,
    Adventurers,
    AdventurerPreview,
    GameOverLight,
    GameOverDarkness
}