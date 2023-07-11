using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGod : MonoBehaviour
{
    public static UIGod instance;
    public Window activeWindow;
    
    [Header("Mapping")]
    public Text topTitle;
    public Text questCounter;
    public GameObject innHUD;
    public Fader mainFader;

    void Start()
    {
        instance = this;
        topTitle.text = "";
    }

    void Update()
    {
        
    }

    public void UpdateQuestTitle(string text)
    {
        topTitle.text = text;
    }

    public void UpdateQuestCounter(int newCount)
    {
        AudioManager.PlaySound(AudioNames.PencilWriting);
        questCounter.text = newCount.ToString();
    }

    public void BeginStartingGame()
    {
        AudioManager.PlaySound(AudioNames.Click);
        mainFader.FadeIn("EndStartingGame");
    }

    public void EndStartingGame()
    {
        Menu.instance.HideMenu();
        innHUD.SetActive(true);
        GameMode.instance.StartGame();
        mainFader.FadeOut();
    }

    public void StartQuitingGame()
    {
        AudioManager.PlaySound(AudioNames.Click);
        mainFader.FadeIn("EndQuitingGame");
    }

    public void EndQuitingGame()
    {
        Application.Quit();
    }
}
