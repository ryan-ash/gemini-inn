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
    public Transform ownerReplicsRoot;

    [Header("Prefabs")]
    public GameObject replicPrefab;
    public GameObject responsePrefab;

    private Window[] windows;

    void Start()
    {
        instance = this;
        topTitle.text = "";
        Wait.Run(0.5f, () => { 
            mainFader.FadeOut();
            UpdateQuestTitle("Welcome to the Inn!");
        });
        windows = GetComponentsInChildren<Window>();
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
        AudioRevolver.Fire(AudioNames.PencilWriting);
        questCounter.text = newCount.ToString();
    }

    public void BeginStartingGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        mainFader.FadeIn("EndStartingGame");
        CursorSetter.SetDefaultCursor();
    }

    public void EndStartingGame()
    {
        Menu.instance.HideMenu();
        innHUD.SetActive(true);
        GameMode.instance.StartGame();
    }

    public void BeginQuitingGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        mainFader.FadeIn("EndQuitingGame");
        CursorSetter.SetDefaultCursor();
    }

    public void EndQuitingGame()
    {
        Application.Quit();
    }

    public void OpenWindow(WindowType windowType)
    {
        AudioRevolver.Fire(AudioNames.Click);
        CloseAllWindows();
        foreach (Window window in windows)
        {
            if (window.windowType == windowType)
            {
                window.OpenWindow();
                break;
            }
        }
        CursorSetter.SetDefaultCursor();
    }

    public void OpenWindowSettings()
    {
        OpenWindow(WindowType.Settings);
    }

    public void OpenWindowNegotiation()
    {
        OpenWindow(WindowType.Negotiation);
    }

    public void OpenWindowQuests()
    {
        OpenWindow(WindowType.Quests);
    }

    public void OpenWindowHistory()
    {
        OpenWindow(WindowType.History);
    }

    public void CloseAllWindows()
    {
        foreach (Window window in windows)
        {
            if (window.isOpen)
            {
                window.CloseWindow();
                break;
            }
        }
    }

    public void SpawnOwnerReplic()
    {
        GameObject replic = Instantiate(replicPrefab, ownerReplicsRoot);
    }

    public void SpawnAdventurerReplic(Transform adventurerReplicsRoot)
    {
        GameObject response = Instantiate(responsePrefab);
        response.transform.SetParent(adventurerReplicsRoot, false);
    }
}
