using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    public WindowType windowType;
    [HideInInspector] public bool isOpen = false;
    
    private Fader fader;

    void Start()
    {
        fader = GetComponent<Fader>();
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
    Negotiation
}