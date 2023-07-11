using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public static Menu instance;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
        // todo: fade out, then disable
    }
}
