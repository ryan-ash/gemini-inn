using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGod : MonoBehaviour
{
    public static UIGod instance;
    public Window activeWindow;
    
    [Header("Mapping")]
    public TextWriter topTitleWriter;
    public Counter questCounter;
    public Counter adventureCounter;
    public Counter historyCounter;
    public Counter adventurersCounter;
    public GameObject innHUD;
    public Fader mainFader;
    public Transform ownerReplicsRoot;
    public Transform questsRoot;
    public Transform historyRoot;

    [Header("Negotiation")]
    public Text questTitle;
    public Text questDescription;
    public Text groupName;

    [Header("Prefabs")]
    public GameObject replicPrefab;
    public GameObject responsePrefab;
    public GameObject questPrefab;

    private Window[] windows;
    private GameObject spawnedQuests;

    void Start()
    {
        instance = this;
        topTitleWriter.Write("", true);
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
        topTitleWriter.Write(text);
    }

    public void UpdateQuestCounter(int newCount)
    {
        // AudioRevolver.Fire(AudioNames.PencilWriting);
        questCounter.UpdateCounter(newCount);
    }

    public void UpdateAdventureCounter(int newCount)
    {
        // AudioRevolver.Fire(AudioNames.PencilWriting);
        adventureCounter.UpdateCounter(newCount);
    }

    public void UpdateHistoryCounter(int newCount)
    {
        // AudioRevolver.Fire(AudioNames.PencilWriting);
        historyCounter.UpdateCounter(newCount);
    }

    public void UpdateAdventurersCounter(int newCount)
    {
        // AudioRevolver.Fire(AudioNames.PencilWriting);
        adventurersCounter.UpdateCounter(newCount);
    }

    public void SpawnActiveQuest(Quest quest)
    {
        GameObject spawnedQuest = Instantiate(questPrefab, questsRoot);
        spawnedQuest.GetComponent<QuestLine>().SetQuest(quest);
        quest.questLine = spawnedQuest.GetComponent<QuestLine>();
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

    public void BeginAcceptingQuest()
    {
        CloseAllWindows();
        GameMode.instance.AgreeToQuest();
    }

    public void BeginDecliningQuest()
    {
        CloseAllWindows();
        GameMode.instance.DisagreeToQuest();
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
        questTitle.text = GameMode.instance.selectedQuest.questName;
        questDescription.text = GameMode.instance.selectedQuest.questDescription;
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

    public void SpawnAdventurerReplic(Vector3 replicSpawnPosition)
    {
        GameObject response = Instantiate(responsePrefab);
        response.transform.position = replicSpawnPosition;
        response.transform.LookAt(Camera.main.transform);
        response.transform.Rotate(0.0f, 180.0f, 0.0f);
    }
}
