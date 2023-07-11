using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGod : MonoBehaviour
{
    public static UIGod instance;
    
    [Header("Mapping")]
    public Text topTitle;
    public Text questCounter;
    public GameObject innHUD;

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
        Fader.instance.FadeIn("EndStartingGame");
    }

    public void EndStartingGame()
    {
        Menu.instance.HideMenu();
        innHUD.SetActive(true);
        GameMode.instance.StartGame();
        Fader.instance.FadeOut();
    }

    public void QuitGame()
    {
        AudioManager.PlaySound(AudioNames.Click);
        Application.Quit();
    }
}
