using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGod : MonoBehaviour
{
    public static UIGod instance;
    public Window activeWindow;
    
    [Header("Texts")]
    public string menuInitialTitle = "Welcome to Gemini Inn!";
    public string innInitialTitle = "Send adventurers on quests!";

    [Header("Mapping")]
    public TextWriter topTitleWriter;
    public Counter questCounter;
    public Counter adventureCounter;
    public Counter historyCounter;
    public Counter adventurersCounter;
    public Fader mainFader;
    public Fader menuFader;
    public Fader HUDFader;
    public Transform ownerReplicsRoot;
    public Transform questsRoot;
    public Transform historyRoot;
    public Transform adventurersRoot;
    public Transform adventurersPreviewRoot;
    public FontAwesome adventurerGroupIcon;
    public GameObject restartButton;
    public Slider lightLevelSlider;

    [Header("Negotiation")]
    public TextWriter questTitle;
    public TextWriter questDescription;
    public Text groupName;

    [Header("Prefabs")]
    public GameObject replicPrefab;
    public GameObject responsePrefab;
    public GameObject questPrefab;
    public GameObject adventurerPrefab;

    private Window[] windows;
    private GameObject spawnedQuests;

    private bool priorityWindowPinned = false;

    void Start()
    {
        instance = this;
        topTitleWriter.Write("", true);
        Wait.Run(0.5f, () => { 
            mainFader.FadeOut();
            UpdateQuestTitle(menuInitialTitle);
        });
        windows = GetComponentsInChildren<Window>();
    }

    void Update()
    {
        
    }

    public void SetInitialQuestTitle()
    {
        UpdateQuestTitle(innInitialTitle);
    }

    public void UpdateQuestTitle(string text)
    {
        topTitleWriter.Write(text);
    }

    public void UpdateQuestCounter(int newCount)
    {
        if (questCounter.GetCurrentCount() != newCount)
            AudioRevolver.Fire(AudioNames.PencilWriting);
        questCounter.UpdateCounter(newCount);
    }

    public void UpdateAdventureCounter(int newCount)
    {
        adventureCounter.UpdateCounter(newCount);
    }

    public void UpdateHistoryCounter()
    {
        historyCounter.UpdateCounter(historyRoot.childCount);
    }

    public void UpdateAdventurersCounter(int newCount)
    {
        adventurersCounter.UpdateCounter(newCount);
    }

    public void SpawnActiveQuest(Quest quest)
    {
        GameObject spawnedQuest = Instantiate(questPrefab, questsRoot);
        spawnedQuest.GetComponent<QuestLine>().SetQuest(quest);
        quest.questLine = spawnedQuest.GetComponent<QuestLine>();
    }

    public void FillDrawerWithAdventureGroup(AdventurerGroup adventurerGroup, bool usePreviewRoot = false)
    {
        Transform root = usePreviewRoot ? adventurersPreviewRoot : adventurersRoot;
        for (int i = 0; i < root.childCount; i++)
        {
            if (root.GetChild(i).GetComponent<AdventurerLine>() != null)
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
        foreach (Adventurer adventurer in adventurerGroup.adventurers)
        {
            GameObject spawnedAdventurer = Instantiate(adventurerPrefab);
            spawnedAdventurer.GetComponent<AdventurerLine>().SetAdventurer(adventurer);
            spawnedAdventurer.transform.SetParent(root, false);
        }
        UpdateAdventurersCounter(AdventurerManager.instance.adventurers.Count);
        if (usePreviewRoot)
        {
            adventurerGroupIcon.ChangeIcon(adventurerGroup.icon);
        }
    }

    public void AppendDrawerWithAdventurer(Adventurer adventurer, bool usePreviewRoot = false)
    {
        Transform root = usePreviewRoot ? adventurersPreviewRoot : adventurersRoot;
        GameObject spawnedAdventurer = Instantiate(adventurerPrefab, root);
        spawnedAdventurer.GetComponent<AdventurerLine>().SetAdventurer(adventurer);
        UpdateAdventurersCounter(AdventurerManager.instance.adventurers.Count);
    }

    public void ReleaseRemovedAdventurers()
    {
        List<AdventurerLine> adventurerLines = new List<AdventurerLine>();
        for (int i = 0; i < adventurersRoot.childCount; i++)
        {
            adventurerLines.Add(adventurersRoot.GetChild(i).GetComponent<AdventurerLine>());
        }

        var adventurerList = AdventurerManager.instance.adventurers;
        foreach (AdventurerLine adventurerLine in adventurerLines)
        {
            if (!adventurerList.Contains(adventurerLine.adventurer))
            {
                Destroy(adventurerLine.gameObject);
            }
        }
        UpdateAdventurersCounter(AdventurerManager.instance.adventurers.Count);
    }

    public void BeginStartingGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        CursorSetter.SetDefaultCursor();
        if (GameMode.instance.gameStarted)
        {
            ResumeGame();
            return;
        }
        UIGod.instance.topTitleWriter.Write("", false);
        mainFader.FadeIn("EndStartingGame");
    }

    public void EndStartingGame()
    {
        menuFader.FadeOut();
        HUDFader.FadeIn();
        GameMode.instance.StartGame();
    }

    public void BeginQuitingGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        CursorSetter.SetDefaultCursor();
        mainFader.FadeIn("EndQuitingGame");
    }

    public void EndQuitingGame()
    {
        Application.Quit();
    }

    public void BeginAcceptingQuest()
    {
        UnpinPriorityWindow();
        CloseAllWindows();
        GameMode.instance.AgreeToQuest();
    }

    public void BeginDecliningQuest()
    {
        UnpinPriorityWindow();
        CloseAllWindows();
        GameMode.instance.DisagreeToQuest();
    }

    public void OpenWindow(WindowType windowType)
    {
        if (priorityWindowPinned)
            return;
        foreach (Window window in windows)
        {
            if (window.windowType == windowType)
            {
                if (!window.isOverlay)
                    CloseAllWindows();
                AudioRevolver.Fire(window.isOverlay ? AudioNames.Hover : AudioNames.Click);
                window.OpenWindow();
                break;
            }
        }
        CursorSetter.SetDefaultCursor();
    }

    public void CloseWindow(WindowType windowType)
    {
        foreach (Window window in windows)
        {
            if (window.windowType == windowType)
            {
                window.CloseWindow();
                break;
            }
        }
    }

    public Window GetWindow(WindowType windowType)
    {
        foreach (Window window in windows)
        {
            if (window.windowType == windowType)
            {
                return window;
            }
        }
        return null;
    }

    public void PinPriorityWindow()
    {
        priorityWindowPinned = true;
    }

    public void UnpinPriorityWindow()
    {
        priorityWindowPinned = false;
    }

    public void OpenWindowSettings()
    {
        OpenWindow(WindowType.Settings);
    }

    public void OpenWindowNegotiation()
    {
        questTitle.Write(GameMode.instance.selectedQuest.questName);
        questDescription.Write(GameMode.instance.selectedQuest.questDescription);
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

    public void OpenWindowAdventurers()
    {
        OpenWindow(WindowType.Adventurers);
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

    public void PauseGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        menuFader.FadeIn();
        HUDFader.FadeOut();
        GameMode.instance.gamePaused = true;
        restartButton.SetActive(true);
    }

    public void ResumeGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        menuFader.FadeOut();
        HUDFader.FadeIn();
        GameMode.instance.gamePaused = false;
    }

    public void BeginRestartingGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        mainFader.FadeIn("EndRestartingGame");
    }

    public void EndRestartingGame()
    {
        GameMode.instance.RestartGame();
    }
}
