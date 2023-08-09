using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameMode : MonoBehaviour
{
    public static GameMode instance;

    [Header("Quest Generation")]
    [SerializeField] private float _questGenerationInterval = 10.0f;
    [SerializeField] private float _questGenerationChance = 0.25f;
    [SerializeField] private float _questSafeDistance = 5.0f;
    [SerializeField] private float _questTileRetryThreshold = 0.5f;
    [SerializeField] private GameObject _questVisualPrefab;
    [SerializeField] private GameObject _questRoot;
    [SerializeField] private bool useMockQuests = false;
    public int QuestShadeSize = 5;

    [Header("Inn Generation")]
    [SerializeField] private GameObject innPrefab;
    [SerializeField] private GameObject innRoot;
    [SerializeField] private Vector2Int innGenerationRange = new Vector2Int(20, 10);
    [SerializeField] private Vector2Int innSandFillRange = new Vector2Int(5, 5);
    public int GeminiInnShadeSize = 8;

    [Header("Quest Selection")]
    [SerializeField] private LayerMask questRaycastLayerMask;

    [Header("Adventurer Selection")]
    [SerializeField] private LayerMask adventurerRaycastLayerMask;
    [SerializeField] private float distanceDuringNegotiation = 1.5f;
    [SerializeField] private float distanceRandomizationDuringNegotiation = 1.5f;
    [SerializeField] private float rotationLengthBeforeMovingDuringNegotiation = 1.0f;
    [SerializeField] private float delayBeforeMovingDuringNegotiation = 1.0f;
    [SerializeField] private float moveTimeRandomizationDuringNegotiation = 0.5f;
    [SerializeField] private float moveTimeDuringNegotiation = 1.0f;
    [SerializeField] private float mandatoryReplicSpawnDelayDuringNegotiation = 0.5f;
    [SerializeField] private float randomReplicSpawnDelayDuringNegotiation = 0.5f;
    [SerializeField] private Vector3 callbackReplicOffset = new Vector3(0.0f, 0.0f, 0.0f);

    [Header("Quest Progress")]
    [SerializeField] private GameObject questEventPrefab;
    [SerializeField] private GameObject questEventRoot;
    [SerializeField] private GameObject mapGroupPrefab;
    [SerializeField] private GameObject mapGroupRoot;
    [SerializeField] private float minMoveToQuestTime = 2.0f;
    [SerializeField] private float maxMoveToQuestTime = 10.0f;

    [Header("Quest Result")]
    [SerializeField] private GameObject questResultPrefab;
    [SerializeField] private GameObject questResultRoot;

    private bool isMapOpen = false;
    private bool isChoosingAdventurers = false;
    private bool isNegotiating = false;

    [HideInInspector] public List<Mission> activeMissions = new List<Mission>();
    [HideInInspector] public List<Mission> failedMissions = new List<Mission>();
    [HideInInspector] public List<Mission> successfulMissions = new List<Mission>();
    [HideInInspector] public List<QuestGroup> questGroups = new List<QuestGroup>();

    private GameObject consideredQuestMarker;
    private Quest consideredQuest;
    private AdventurerGroup consideredAdventurerGroup;

    [HideInInspector] public GameObject selectedQuestMarker;
    [HideInInspector] public Quest selectedQuest;
    [HideInInspector] public AdventurerGroup selectedAdventurerGroup;
    [HideInInspector] public List<QuestInfo> generatedQuestInfos = new List<QuestInfo>();
    [HideInInspector] public List<MapObject> generatedMapObjects = new List<MapObject>();

    private List<Transform> generatedQuests = new List<Transform>();
    private GameObject generatedInn;
    private int lastMissionID = 0;

    [HideInInspector] public bool gamePaused = false;
    [HideInInspector] public bool gameStarted = false;

    void Start()
    {
        instance = this;
        CursorSetter.SetDefaultCursor();
    }

    void Update()
    {
        if (!gameStarted)
            return;

        UpdateQuestTimers();

        if (isMapOpen)
        {
            if (consideredQuestMarker != null && Input.GetMouseButtonDown(0))
            {
                selectedQuestMarker = consideredQuestMarker;
                selectedQuest = consideredQuest;
                // selectedQuestMarker.GetComponent<QuestInfo>().HideInfo();

                switch (selectedQuest.questState)
                {
                    case QuestState.NotStarted:
                        selectedQuest.questInfo.Pin();
                        ToggleMap();
                        StartChoosingAdventurers();
                        break;
                    case QuestState.OnRoad:
                    case QuestState.InProgress:
                        UIGod.instance.OpenWindowQuests();
                        break;
                    case QuestState.Success:
                    case QuestState.Failure:
                        UIGod.instance.OpenWindowHistory();
                        break;
                }

                return;
            }

            TrackQuestUnderMouse();
        }
        else if (!isNegotiating)
        {
            if (isChoosingAdventurers)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isChoosingAdventurers = false;
                    if (consideredAdventurerGroup != null && consideredAdventurerGroup.adventurers.Count > 0)
                    {
                        selectedAdventurerGroup = consideredAdventurerGroup;
                        CursorSetter.ResetPriorityCursor();
                        MoveSelectedAdventurersToNegotiation();
                    }
                    else if (consideredAdventurerGroup == null)
                    {
                        CursorSetter.ResetPriorityCursor();
                        // ToggleMap();
                        UIGod.instance.UpdateQuestTitle("");
                    }
                    else
                    {
                        isChoosingAdventurers = true;
                        // spawn "there's no one there" replic
                    }
                    return;
                }
            }

            TrackAdventureGroupUnderMouse();
        }
    }

    private class QuestToStop
    {
        public Mission mission;
        public Quest quest;
        public QuestState questResult;
        public bool isTimeout;

        public QuestToStop(Mission mission, Quest quest, QuestState questResult, bool isTimeout)
        {
            this.mission = mission;
            this.quest = quest;
            this.questResult = questResult;
            this.isTimeout = isTimeout;
        }
    }

    private void UpdateQuestTimers()
    {
        if (!IsTimersRunning())
            return;

        List<QuestToStop> questsToStop = new List<QuestToStop>();

        foreach (Mission mission in activeMissions)
        {
            Quest quest = mission.GetCurrentQuest();
            if (quest == null)
                continue;

            quest.questTimer += Time.deltaTime;

            if (quest.questState == QuestState.NotStarted)
            {
                if (quest.questTimer >= quest.timeout && quest.timeout > 0.0f)
                {
                    QuestToStop questToStop = new QuestToStop(mission, quest, QuestState.Failure, true);
                    questsToStop.Add(questToStop);
                }
            }
            else if (quest.questState == QuestState.OnRoad)
            {
                if (quest.questTimer >= quest.roadDuration)
                {
                    UpdateQuestState(mission, quest, QuestState.InProgress, false);
                }
            }
            else if (quest.questState == QuestState.InProgress)
            {
                if (quest.questTimer >= quest.baseDuration)
                {
                    QuestState questResult = quest.RollSuccessDice() ? QuestState.Success : QuestState.Failure;
                    QuestToStop questToStop = new QuestToStop(mission, quest, questResult, false);
                    questsToStop.Add(questToStop);
                }
            }
        }

        foreach (QuestToStop questToStop in questsToStop)
        {
            UpdateQuestState(questToStop.mission, questToStop.quest, questToStop.questResult, questToStop.isTimeout);
        }
    }

    private void TrackQuestUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float maxDistance = 100.0f;
        if (Physics.Raycast(ray, out hit, maxDistance, questRaycastLayerMask))
        {
            QuestInfo questInfo = hit.transform.gameObject.GetComponentInParent<QuestInfo>();
            GameObject potentialNewSelection = questInfo.gameObject;
            if (potentialNewSelection != consideredQuestMarker)
            {
                consideredQuestMarker = potentialNewSelection;
                consideredQuest = questInfo.GetQuest();
                questInfo.ShowInfo();
            }
        }
        else if (consideredQuestMarker != null)
        {
            QuestInfo questInfo = consideredQuestMarker.GetComponent<QuestInfo>();
            questInfo.HideInfo();
            consideredQuestMarker = null;
            consideredQuest = null;
        }
    }

    private void TrackAdventureGroupUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float maxDistance = 100.0f;
        if (Physics.Raycast(ray, out hit, maxDistance, adventurerRaycastLayerMask))
        {
            AdventurerGroup adventurerGroup = hit.transform.gameObject.GetComponent<AdventurerGroup>();
            if (adventurerGroup != consideredAdventurerGroup)
            {
                if (consideredAdventurerGroup != null)
                {
                    consideredAdventurerGroup.UnfocusAdventurerTable();
                    var adventurerPreviewWindow = UIGod.instance.GetWindow(WindowType.AdventurerPreview);
                    var adventurerPreviewFader = adventurerPreviewWindow.GetComponent<Fader>();
                    adventurerPreviewFader.Switch(false);
                    adventurerPreviewWindow.isOpen = false;
                }
                consideredAdventurerGroup = adventurerGroup;
                if (adventurerGroup.adventurers.Count > 0)
                {
                    UIGod.instance.FillPreviewDrawerWithAdventureGroup(adventurerGroup);
                    UIGod.instance.OpenWindow(WindowType.AdventurerPreview);
                }
                adventurerGroup.FocusAdventurerTable();
            }
        }
        else if (consideredAdventurerGroup != null)
        {
            consideredAdventurerGroup.UnfocusAdventurerTable();
            consideredAdventurerGroup = null;
            UIGod.instance.CloseWindow(WindowType.AdventurerPreview);
        }
    }

    public void ToggleMap()
    {
        bool justStartedMovingIntoOtherState = Inn.instance.isCameraMoving && !Inn.instance.IsCloserToTarget();
        if (justStartedMovingIntoOtherState || isNegotiating)
            return;

        AudioRevolver.Fire(AudioNames.MapSound);
        AudioRevolver.Fire(AudioNames.CameraWoosh);
        isMapOpen = !isMapOpen;
        if (isMapOpen)
        {
            if (selectedQuest != null && selectedQuest.questInfo != null && selectedQuest.questInfo.IsPinned())
            {
                selectedQuest.questInfo.Unpin();
            }
            UIGod.instance.UpdateQuestTitle("");
            Inn.instance.ShowMap();
            Map.instance.ShowMap();
        }
        else
        {
            Inn.instance.HideMap();
            Map.instance.HideMap();
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void StartChoosingAdventurers()
    {
        CursorSetter.SetHoverCursor(true);
        isChoosingAdventurers = true;
        UIGod.instance.UpdateQuestTitle(selectedQuest.questName);
    }

    public void MoveSelectedAdventurersToNegotiation()
    {
        UIGod.instance.SpawnOwnerReplic();
        UIGod.instance.GetWindow(WindowType.AdventurerPreview).Pin();
        AudioRevolver.Fire(AudioNames.Footsteps);
        foreach (Adventurer adventurer in selectedAdventurerGroup.adventurers)
        {
            Vector3 directionFromOwner = Vector3.Normalize(adventurer.transform.position - Camera.main.transform.position);
            Vector3 directionToOwner = Vector3.Normalize(Camera.main.transform.position - adventurer.transform.position);
            float actualDistanceDuringNegotiation = distanceDuringNegotiation + Random.Range(-distanceRandomizationDuringNegotiation, distanceRandomizationDuringNegotiation);
            Vector3 newPosition = new Vector3(Camera.main.transform.position.x + directionFromOwner.x * actualDistanceDuringNegotiation, 0.0f, Camera.main.transform.position.z + directionFromOwner.z * actualDistanceDuringNegotiation);

            LeanTween.value(gameObject, 0.0f, 1.0f, rotationLengthBeforeMovingDuringNegotiation).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float value) => {
                adventurer.transform.rotation = Quaternion.Lerp(adventurer.transform.rotation, Quaternion.LookRotation(directionToOwner), value);
            }).setOnComplete(() => {
                float delayBeforeSpawningReplic = Random.Range(0.0f, randomReplicSpawnDelayDuringNegotiation) + mandatoryReplicSpawnDelayDuringNegotiation;
                Wait.Run(delayBeforeSpawningReplic, () => {
                    Transform adventurerTransform = adventurer.transform;
                    Vector3 spawnPosition = adventurerTransform.position + callbackReplicOffset;
                    UIGod.instance.SpawnAdventurerReplic(spawnPosition);
                });

                Wait.Run(delayBeforeMovingDuringNegotiation, () => {
                    float moveTime = CalculateMoveTime();
                    adventurer.MoveTo(newPosition, moveTime);
                    UIGod.instance.UpdateQuestTitle("");
                });
            });
                        
            selectedAdventurerGroup.LightDownAdventurerTable();
        }

        float OpenWindowTime = rotationLengthBeforeMovingDuringNegotiation + CalculateMoveTime() + delayBeforeMovingDuringNegotiation;
        Wait.Run(OpenWindowTime, () => {
            UIGod.instance.OpenWindowNegotiation();
            UIGod.instance.GetWindow(WindowType.Negotiation).Pin();
        });

        isNegotiating = true;
    }

    private float CalculateMoveTime()
    {
        return moveTimeDuringNegotiation + Random.Range(-moveTimeRandomizationDuringNegotiation, moveTimeRandomizationDuringNegotiation);
    }

    public void AgreeToQuest()
    {
        selectedQuest.questInfo.Unpin();
        selectedAdventurerGroup.AcceptQuest();

        GameObject spawnedMapGroup = Instantiate(mapGroupPrefab, generatedInn.transform.localPosition, Quaternion.identity);
        spawnedMapGroup.transform.SetParent(mapGroupRoot.transform, false);
        spawnedMapGroup.transform.localPosition = new Vector3(spawnedMapGroup.transform.localPosition.x, spawnedMapGroup.transform.localPosition.y, 0.1f);
        
        GroupOnMap groupOnMap = spawnedMapGroup.GetComponent<GroupOnMap>();
        groupOnMap.SetIcon(selectedQuest.questGroup.icon);
        groupOnMap.adventurers = selectedQuest.questGroup.adventurers;
        selectedQuest.groupOnMap = groupOnMap;

        // TODO: apply group speed modifier as well
        float maxMoveDistance = Vector3.Distance(mapGroupRoot.transform.position, Map.instance.GetTilePosition(0, 0));
        float distanceToQuest = Vector3.Distance(generatedInn.transform.position, selectedQuestMarker.transform.position);
        float moveTimeAlpha = Mathf.Clamp01(distanceToQuest / maxMoveDistance);
        float moveToQuestTime = Mathf.Lerp(minMoveToQuestTime, maxMoveToQuestTime, moveTimeAlpha);

        groupOnMap.Move(selectedQuestMarker.transform.position, moveToQuestTime, true);
        selectedQuest.roadDuration = moveToQuestTime;

        selectedAdventurerGroup = null;
        UIGod.instance.UpdateQuestTitle("");
        selectedQuestMarker.GetComponent<QuestInfo>().SetOnRoad();

        UpdateQuestCount();
        UpdateAdventureCount();
        selectedQuest.questTimer = 0.0f;

        AudioRevolver.Fire(AudioNames.QuestGoing);
        AudioRevolver.Fire(AudioNames.MugOnTable);
        Wait.Run(2.0f, () => {
            AudioRevolver.Fire(AudioNames.DoorClosing);
        });

        selectedQuest = null;
        isNegotiating = false;
        isChoosingAdventurers = false;
        ToggleMap();
    }

    public void DisagreeToQuest()
    {
        isNegotiating = false;
        StartChoosingAdventurers();
        foreach (Adventurer adventurer in selectedAdventurerGroup.adventurers)
        {
            float moveTime = CalculateMoveTime();
            adventurer.MoveBack(moveTime);
        }
        selectedAdventurerGroup.LightUpAdventurerTable();
    }

    public void RegisterQuestGroup(QuestGroup questGroup)
    {
        questGroups.Add(questGroup);
    }

    public void UpdateQuestState(Mission mission, Quest quest, QuestState newState, bool isTimeout = false)
    {
        quest.questTimer = 0.0f;
        quest.questState = newState;
        var tile = quest.tile;

        if (newState == QuestState.Success || newState == QuestState.Failure)
        {
            activeMissions.Remove(mission);
            quest.questInfo.SetOver(newState == QuestState.Success, isTimeout);
            quest.questLine.transform.SetParent(UIGod.instance.historyRoot, false);
            if (quest == selectedQuest && isChoosingAdventurers)
            {
                isChoosingAdventurers = false;
            }
        }
        if (newState == QuestState.Success)
        {
            //temp shading
            Map.instance.ChangeRegionShade(tile.X, tile.Y, QuestShadeSize, ShadeType.Light);

            quest.groupOnMap.FadeIn();
            quest.groupOnMap.Move(generatedInn.transform.position, quest.roadDuration, true, true);

            Quest newAvailableQuest = mission.GetAvailableQuest();
            bool newQuestSuccess = newAvailableQuest != null;
            if (newQuestSuccess)
            {
                newQuestSuccess = ProcessSpawnedQuest(mission, newAvailableQuest);
            }
            if (newQuestSuccess)
            {
                activeMissions.Add(mission);
            }
            else
            {
                successfulMissions.Add(mission);
            }
        }
        else if (newState == QuestState.Failure)
        {
            //temp shading
            Map.instance.ChangeRegionShade(tile.X, tile.Y, QuestShadeSize, ShadeType.Dark);

            failedMissions.Add(mission);
        }
        else if (newState == QuestState.InProgress)
        {
            quest.questInfo.SetInProgress();
        }
        UpdateQuestCount();
    }

    public void StartGame()
    {
        AudioRevolver.Fire(AudioNames.Crowd);
        Wait.Run(0.1f, () => {
            InitGame();
        });
        Wait.Run(1.0f, () => { 
            UIGod.instance.SetInitialQuestTitle();
            UIGod.instance.mainFader.FadeOut();
            StartCoroutine(SpawnQuest());
        });

        gameStarted = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void InitGame()
    {
        AdventurerManager.instance.StartSpawning();
        Map.instance.GenerateMap();
        GenerateInn();
    }

    private void GenerateInn()
    {
        int halfWidth = Map.instance.width / 2;
        int halfHeight = Map.instance.height / 2;

        int xCoord = halfWidth + Random.Range(-innGenerationRange.x / 2, innGenerationRange.x / 2);
        int yCoord = halfHeight + Random.Range(-innGenerationRange.y / 2, innGenerationRange.y / 2);
        
        Vector3 innPosition = Map.instance.GetTileLocalPosition(xCoord, yCoord);
        generatedInn = Instantiate(innPrefab, innPosition, Quaternion.identity);
        generatedInn.transform.SetParent(innRoot.transform, false);
        generatedMapObjects.Add(generatedInn.GetComponent<MapObject>());

        int fillStartX = xCoord - innSandFillRange.x / 2;
        int fillStartY = yCoord - innSandFillRange.y / 2;
        const int diffToCompensateInnSize = 2;

        Map.instance.ChangeRegionShade(xCoord, yCoord + diffToCompensateInnSize, GeminiInnShadeSize, ShadeType.Light);
       
        // Map.instance.FillAreaWithBiom("Desert", fillStartX, fillStartY, innSandFillRange.x, innSandFillRange.y);
    }

    public void OnQuestUpdated(Quest quest)
    {
        if (questEventPrefab == null || questEventRoot == null)
            return;

        var questEventObject = Instantiate(questEventPrefab, questEventRoot.transform);
        var questEvent = questEventObject.GetComponent<QuestEvent>();
        questEvent.SetQuest(quest);
        questEvent.ShowEvent();
    }

    private bool CheckIfSafe(Vector3 positionToTest)
    {
        foreach (Transform questPosition in generatedQuests)
        {
            if (Vector3.Distance(positionToTest, questPosition.localPosition) < _questSafeDistance)
                return false;
        }

        return true;
    }

    private void UpdateQuestCount()
    {
        List<Mission> activeMissionsWithQuests = activeMissions.FindAll(m => m.GetAvailableQuest() != null);
        int count = activeMissionsWithQuests.Count;
        UIGod.instance.UpdateQuestCounter(count);
        UpdateAdventureCount();
    }

    private void UpdateAdventureCount()
    {
        // List<Mission> activeMissionsWithNoQuests = activeMissions.FindAll(m => m.GetAvailableQuest() == null);
        int count = activeMissions.Count;
        UIGod.instance.UpdateAdventureCounter(count);
        UpdateHistoryCount();
    }

    private void UpdateHistoryCount()
    {
        UIGod.instance.UpdateHistoryCounter();
    }

    public static bool IsTimersRunning()
    {
        return !instance.isNegotiating && !instance.gamePaused;
    }

    private bool ProcessSpawnedQuest(Mission mission, Quest quest)
    {
        Debug.Log("Spawning quest [" + quest.questName + " | " + quest.ID +"] for mission [" + mission.MissionName + " | " + mission.ID + "]");

        var availableBiomeTiles = Map.GetBiomeTiles(quest.Biomes.ToArray());
        if (availableBiomeTiles.Count == 0)
        {
            Debug.LogWarning("No available tiles for quest: " + quest.questName);
            return false;
        }

        int initialTileCount = availableBiomeTiles.Count;
        quest.mission = mission;

        Vector3 questPosition = Vector3.zero;
        Tile questTile = null;
        bool isSafe = false;
        while (!isSafe)
        {
            int tileID = Random.Range(0, availableBiomeTiles.Count);
            questTile = availableBiomeTiles[tileID];
            questPosition = questTile.transform.localPosition;
            questPosition.z = 0.0f;

            isSafe = CheckIfSafe(questPosition);

            if (isSafe)
                break;

            availableBiomeTiles.RemoveAt(tileID);

            if (availableBiomeTiles.Count < initialTileCount * _questTileRetryThreshold)
                break;
        }
        if (!isSafe)
            return false;

        quest.SetTile(questTile);
        quest.SetPosition(questPosition);

        GameObject questVisual = Instantiate(_questVisualPrefab, questPosition, Quaternion.identity);
        questVisual.transform.SetParent(_questRoot.transform, false);

        QuestInfo questInfo = questVisual.GetComponent<QuestInfo>();
        questInfo.SetQuest(quest);
        generatedQuestInfos.Add(questInfo);
        generatedMapObjects.Add(questInfo.GetComponentInChildren<MapObject>());

        quest.questInfo = questInfo;
        quest.questTimer = 0.0f;
        UIGod.instance.SpawnActiveQuest(quest);

        generatedQuests.Add(questVisual.transform);

        return true;
    }

    IEnumerator SpawnQuest()
    {
        while (true)
        {
            List<Mission> bannedMissions = new List<Mission>();
            if (Random.Range(0.0f, 1.0f) <= _questGenerationChance || activeMissions.Count == 0 /*for now...*/)
            {
                Mission mission = useMockQuests ? Mission.GenerateRandomMission() : Deep.Clone(Mission.GrabRandomMissionFromDB());
                mission.ID = lastMissionID++;

                int questID = 0;
                foreach (Quest quest in mission.Quests)
                    quest.ID = questID++;

                if (!useMockQuests && bannedMissions.Count == MissionsDatabase.instance.Missions.Count)
                {
                    Debug.LogWarning("No available missions for now...");
                    yield return new WaitForSeconds(_questGenerationInterval);
                    continue;
                }

                if (!useMockQuests && bannedMissions.Find(m => m.MissionName == mission.MissionName) != null)
                    continue;

                if (mission.RecurrenceType == MissionRecurrenceType.Repeating)
                {
                    if (activeMissions.Find(m => m.MissionName == mission.MissionName) != null)
                    {
                        bannedMissions.Add(mission);
                        continue;
                    }
                }
                else if (mission.RecurrenceType == MissionRecurrenceType.Unique)
                {
                    if (activeMissions.Find(m => m.MissionName == mission.MissionName) != null)
                    {
                        bannedMissions.Add(mission);
                        continue;
                    }
                    if (failedMissions.Find(m => m.MissionName == mission.MissionName) != null)
                    {
                        bannedMissions.Add(mission);
                        continue;
                    }
                    if (successfulMissions.Find(m => m.MissionName == mission.MissionName) != null)
                    {
                        bannedMissions.Add(mission);
                        continue;
                    }
                }

                Quest newAvailableQuest = mission.GetAvailableQuest();
                if (newAvailableQuest == null)
                {
                    Debug.LogWarning("No available quests for mission: " + mission.MissionName);
                    continue;
                }
                if (Map.GetBiomeTiles(newAvailableQuest.Biomes.ToArray()).Count == 0)
                {
                    Debug.LogWarning("No available tiles for quest: " + newAvailableQuest.questName);
                    continue;
                }

                bool questProcessSuccess = ProcessSpawnedQuest(mission, newAvailableQuest);
                if (questProcessSuccess)
                {
                    activeMissions.Add(mission);
                    UpdateQuestCount();
                }
                else
                {
                    yield return new WaitForSeconds(_questGenerationInterval);
                    continue;
                }
            }

            yield return new WaitForSeconds(_questGenerationInterval);
        }
    }
}
