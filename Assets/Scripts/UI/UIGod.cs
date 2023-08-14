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
    public GroupLine adventurerGroupLine;
    public GameObject resumeButton;
    public GameObject restartButton;
    public Slider lightLevelSlider;
    public Transform lightLevelBars;
    public Transform lightLevelTop;
    public Transform lightLevelBottom;

    [Header("Negotiation")]
    public TextWriter questTitle;
    public TextWriter questDescription;
    public Text groupName;

    [Header("Prefabs")]
    public GameObject replicPrefab;
    public GameObject responsePrefab;
    public GameObject questPrefab;
    public GameObject adventurerPrefab;
    public GameObject lightBarPrefab;

    private Window[] windows;
    private GameObject spawnedQuests;
    List<Transform> lightLevelPoints = new List<Transform>();

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
        if (!GameMode.IsTimersRunning())
            return;
        InterpolateLightLevel();
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

    public void FillPreviewDrawerWithAdventureGroup(AdventurerGroup adventurerGroup)
    {
        for (int i = 0; i < adventurersPreviewRoot.childCount; i++)
        {
            if (adventurersPreviewRoot.GetChild(i).GetComponent<AdventurerLine>() != null)
            {
                Destroy(adventurersPreviewRoot.GetChild(i).gameObject);
            }
        }
        foreach (Adventurer adventurer in adventurerGroup.adventurers)
        {
            GameObject spawnedAdventurer = Instantiate(adventurerPrefab);
            spawnedAdventurer.GetComponent<AdventurerLine>().SetAdventurer(adventurer);
            spawnedAdventurer.transform.SetParent(adventurersPreviewRoot, false);
        }
        adventurerGroupIcon.ChangeIcon(adventurerGroup.icon);
        adventurerGroupLine.SetGroup(adventurerGroup.groupStats, adventurerGroup.adventurers);
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
        GameMode.instance.AgreeToQuest();
        CloseAllWindows(true);
    }

    public void BeginDecliningQuest()
    {
        GameMode.instance.DisagreeToQuest();
        CloseAllWindows(true);
    }

    public void OpenWindow(WindowType windowType)
    {
        foreach (Window window in windows)
        {
            if (window.windowType == windowType)
            {
                if (!window.isOverlay)
                    CloseAllWindows();
                AudioRevolver.Fire(window.isOverlay ? AudioNames.Hover : AudioNames.Click);
                window.OpenWindow();
                activeWindow = window;
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

    public void OpenWindowSettings()
    {
        OpenWindow(WindowType.Settings);
    }

    public void OpenWindowNegotiation()
    {
        // init negotiation dialog
        questTitle.Write("negotiation will be here");
        questDescription.Write("imagine it for now");
        // questTitle.Write(GameMode.instance.selectedQuest.questName);
        // questDescription.Write(GameMode.instance.selectedQuest.questDescription);
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

    public void CloseAllWindows(bool includePinned = false)
    {
        foreach (Window window in windows)
        {
            CloseWindow(window, includePinned);
        }
    }

    public void CloseWindow(Window window, bool includePinned = false)
    {
        if (includePinned)
            window.Unpin();
        if (window.isOpen)
            window.CloseWindow();
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
        ShowMenu();
    }

    public void ResumeGame()
    {
        AudioRevolver.Fire(AudioNames.Click);
        HideMenu();
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

    public void ShowMenu()
    {
        restartButton.SetActive(true);
        if (GameMode.instance.gameOver)
            resumeButton.SetActive(false);
        menuFader.FadeIn();
        HUDFader.FadeOut();
        GameMode.instance.gamePaused = true;
    }

    public void HideMenu()
    {
        menuFader.FadeOut();
        HUDFader.FadeIn();
        GameMode.instance.gamePaused = false;
    }

    public void OnMaxWorldLightLevelChanged(int maxWorldLightLevel)
    {
        for (int i = 0; i < lightLevelBars.childCount; i++)
            Destroy(lightLevelBars.GetChild(i).gameObject);

        lightLevelPoints.Clear();
        lightLevelPoints.Add(lightLevelTop);

        for (int barsAmount = 0; barsAmount < (maxWorldLightLevel * 2) - 1; barsAmount++)
        {
            GameObject newPoint = Instantiate(lightBarPrefab, lightLevelBars);
            lightLevelPoints.Add(newPoint.transform);
        }

        lightLevelPoints.Add(lightLevelBottom);
    }

    private void InterpolateLightLevel()
    {
        int normalizedWorldLightLevel = GameMode.instance.maxWorldLightLevel - GameMode.instance.worldLightLevel;
        Transform targetLightBar = lightLevelPoints[normalizedWorldLightLevel];

        float targetSliderAlpha = (targetLightBar.position.y - lightLevelBottom.position.y) / (lightLevelTop.position.y - lightLevelBottom.position.y);
        float sliderAlpha = Mathf.Lerp(lightLevelSlider.value, targetSliderAlpha, Time.deltaTime * 5.0f);
        lightLevelSlider.value = sliderAlpha;
    }
}
