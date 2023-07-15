using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Window : MonoBehaviour
{
    public WindowType windowType;
    public Text windowTitle;

    [HideInInspector] public bool isOpen = false;
    
    private Fader fader;

    void Start()
    {
        fader = GetComponent<Fader>();
        windowTitle.text = windowType.ToString();
    }

    void Update()
    {
        
    }

    public void OpenWindow()
    {
        fader.FadeIn();
        isOpen = true;
    }

    public void CloseWindow()
    {
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
    Adventurers
}