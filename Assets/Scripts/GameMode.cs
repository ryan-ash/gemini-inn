using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    [Header("Inn Generation")]
    [SerializeField] private GameObject innPrefab;
    [SerializeField] private GameObject innRoot;
    [SerializeField] private Vector2Int innGenerationRange = new Vector2Int(20, 10);
    [SerializeField] private Vector2Int innSandFillRange = new Vector2Int(5, 5);

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

    [Header("Quest Reporting")]
    [SerializeField] private GameObject _questEventPrefab;
    [SerializeField] private GameObject _questEventRoot;

    private bool isMapOpen = false;
    private bool isChoosingAdventurers = false;
    private bool isNegotiating = false;

    [HideInInspector] public List<Mission> activeMissions;
    [HideInInspector] public List<Mission> failedMissions;
    [HideInInspector] public List<Mission> successfulMissions;

    private GameObject _consideredQuestMarker;
    private Quest _consideredQuest;
    private AdventurerGroup _consideredAdventurerGroup;

    [HideInInspector] public GameObject selectedQuestMarker;
    [HideInInspector] public Quest selectedQuest;
    [HideInInspector] public AdventurerGroup selectedAdventurerGroup;

    private List<Transform> _generatedQuests = new List<Transform>();
    private GameObject generatedInn;

    void Start()
    {
        instance = this;
        CursorSetter.SetDefaultCursor();
    }

    void Update()
    {
        UpdateQuestTimers();

        if (!isMapOpen && !isChoosingAdventurers && !isNegotiating)
            return;

        if (isMapOpen)
        {
            if (_consideredQuestMarker != null && Input.GetMouseButtonDown(0))
            {
                selectedQuestMarker = _consideredQuestMarker;
                selectedQuest = _consideredQuest;
                selectedQuestMarker.GetComponent<QuestInfo>().HideInfo();

                switch (selectedQuest.questState)
                {
                    case QuestState.NotStarted:
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

        if (isChoosingAdventurers)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isChoosingAdventurers = false;
                if (_consideredAdventurerGroup != null && _consideredAdventurerGroup.adventurers.Count > 0)
                {
                    selectedAdventurerGroup = _consideredAdventurerGroup;
                    CursorSetter.ResetPriorityCursor();
                    MoveSelectedAdventurersToNegotiation();
                }
                else if (_consideredAdventurerGroup == null)
                {
                    CursorSetter.ResetPriorityCursor();
                    ToggleMap();
                    UIGod.instance.UpdateQuestTitle("");
                }
                else
                {
                    isChoosingAdventurers = true;
                    // spawn "there's no one there" replic
                }
                return;
            }

            TrackAdventureGroupUnderMouse();
        }

        if (isNegotiating)
        {
            // for now, only control negotiations through UI
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
            else if (quest.questState == QuestState.InProgress || quest.questState == QuestState.OnRoad)
            {
                quest.questTimer += Time.deltaTime;
                if (quest.questTimer >= quest.baseDuration)
                {
                    QuestState questResult = QuestState.Success;
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
            if (potentialNewSelection != _consideredQuestMarker)
            {
                _consideredQuestMarker = potentialNewSelection;
                _consideredQuest = questInfo.GetQuest();
                questInfo.ShowInfo();
            }
        }
        else if (_consideredQuestMarker != null)
        {
            QuestInfo questInfo = _consideredQuestMarker.GetComponent<QuestInfo>();
            questInfo.HideInfo();
            _consideredQuestMarker = null;
            _consideredQuest = null;
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
            if (adventurerGroup != _consideredAdventurerGroup)
            {
                if (_consideredAdventurerGroup != null)
                {
                    _consideredAdventurerGroup.UnfocusAdventurerTable();
                    var adventurerPreviewWindow = UIGod.instance.GetWindow(WindowType.AdventurerPreview);
                    var adventurerPreviewFader = adventurerPreviewWindow.GetComponent<Fader>();
                    adventurerPreviewFader.Switch(false);
                    adventurerPreviewWindow.isOpen = false;
                }
                _consideredAdventurerGroup = adventurerGroup;
                UIGod.instance.FillDrawerWithAdventurers(adventurerGroup.adventurers, true);
                UIGod.instance.OpenWindow(WindowType.AdventurerPreview);
                adventurerGroup.FocusAdventurerTable();
            }
        }
        else if (_consideredAdventurerGroup != null)
        {
            _consideredAdventurerGroup.UnfocusAdventurerTable();
            _consideredAdventurerGroup = null;
        }
    }

    public void ToggleMap()
    {
        if (Inn.instance.isCameraMoving && !Inn.instance.IsCloserToTarget())
            return;

        AudioRevolver.Fire(AudioNames.MapSound);
        AudioRevolver.Fire(AudioNames.CameraWoosh);
        isMapOpen = !isMapOpen;
        if (isMapOpen)
        {
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
        });

        isNegotiating = true;
    }

    private float CalculateMoveTime()
    {
        return moveTimeDuringNegotiation + Random.Range(-moveTimeRandomizationDuringNegotiation, moveTimeRandomizationDuringNegotiation);
    }

    public void AgreeToQuest()
    {
        selectedQuest.adventureGroup = selectedAdventurerGroup;
        selectedAdventurerGroup.quest = selectedQuest;
        UIGod.instance.UpdateQuestTitle("");

        GameObject newAdventurerGroup = Instantiate(selectedAdventurerGroup.gameObject, selectedAdventurerGroup.transform.position, selectedAdventurerGroup.transform.rotation);
        AdventurerGroup newAdventurerGroupComponent = newAdventurerGroup.GetComponent<AdventurerGroup>();
        newAdventurerGroupComponent.adventurers = new List<Adventurer>();

        selectedAdventurerGroup.AcceptQuest();
        selectedQuestMarker.GetComponent<QuestInfo>().SetInProgress();

        UpdateQuestCount();
        UpdateAdventureCount();
        selectedQuest.questTimer = 0.0f;

        AudioRevolver.Fire(AudioNames.QuestGoing);
        Wait.Run(2.0f, () => {
            AudioRevolver.Fire(AudioNames.DoorSquek);
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

    public void UpdateQuestState(Mission mission, Quest quest, QuestState newState, bool isTimeout = false)
    {
        quest.questState = newState;
        if (newState == QuestState.Success || newState == QuestState.Failure)
        {
            activeMissions.Remove(mission);
            quest.questInfo.SetOver(newState == QuestState.Success, isTimeout);
            quest.questLine.transform.SetParent(UIGod.instance.historyRoot, false);
        }
        if (newState == QuestState.Success)
        {
            successfulMissions.Add(mission);
        }
        else if (newState == QuestState.Failure)
        {
            failedMissions.Add(mission);
        }
        UpdateQuestCount();
    }

    public void StartGame()
    {
        AudioRevolver.Fire(AudioNames.Crowd);
        UIGod.instance.topTitleWriter.Write("", true);
        Wait.Run(0.1f, () => {
            InitGame();
        });
        Wait.Run(1.0f, () => { 
            UIGod.instance.SetInitialQuestTitle();
            UIGod.instance.mainFader.FadeOut();
            StartCoroutine(SpawnQuest());
        });
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

        int fillStartX = xCoord - innSandFillRange.x / 2;
        int fillStartY = yCoord - innSandFillRange.y / 2;

        // Map.instance.FillAreaWithBiom("Desert", fillStartX, fillStartY, innSandFillRange.x, innSandFillRange.y);
    }

    public void OnQuestUpdated(Quest quest)
    {
        if (_questEventPrefab == null || _questEventRoot == null)
            return;

        var questEventObject = Instantiate(_questEventPrefab, _questEventRoot.transform);
        var questEvent = questEventObject.GetComponent<QuestEvent>();
        questEvent.SetQuest(quest);
        questEvent.ShowEvent();
    }

    private bool CheckIfSafe(Vector3 positionToTest)
    {
        foreach (Transform questPosition in _generatedQuests)
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
        int count = failedMissions.Count + successfulMissions.Count;
        UIGod.instance.UpdateHistoryCounter(count);
    }

    public static bool IsTimersRunning()
    {
        // add conditions for timers if necessary (game pause? negotiation maybe?..)
        return !instance.isNegotiating;
    }

    IEnumerator SpawnQuest()
    {
        while (true)
        {
            List<Mission> bannedMissions = new List<Mission>();
            if (Random.Range(0.0f, 1.0f) <= _questGenerationChance || activeMissions.Count == 0 /*for now...*/)
            {
                Mission mission = useMockQuests ? Mission.GenerateRandomMission() : Deep.Clone(Mission.GrabRandomMissionFromDB());

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

                var availableBiomeTiles = Map.GetBiomeTiles(newAvailableQuest.Biomes.ToArray());
                if (availableBiomeTiles.Count == 0)
                {
                    Debug.LogWarning("No available tiles for quest: " + newAvailableQuest.questName);
                    continue;
                }

                int initialTileCount = availableBiomeTiles.Count;
                newAvailableQuest.mission = mission;

                Vector3 questPosition = Vector3.zero;
                bool isSafe = false;
                while (!isSafe)
                {
                    int tileID = Random.Range(0, availableBiomeTiles.Count);
                    questPosition = availableBiomeTiles[tileID].transform.localPosition;
                    questPosition.z = 0.0f;

                    isSafe = CheckIfSafe(questPosition);

                    if (isSafe)
                        break;                        

                    availableBiomeTiles.RemoveAt(tileID);

                    // to debug elimination of available tiles
                    // Debug.Log(availableBiomeTiles.Count.ToString());

                    if (availableBiomeTiles.Count < initialTileCount * _questTileRetryThreshold)
                        break;
                }
                if (!isSafe)
                    yield return new WaitForSeconds(_questGenerationInterval);

                newAvailableQuest.SetPosition(questPosition);

                GameObject questVisual = Instantiate(_questVisualPrefab, questPosition, Quaternion.identity);
                questVisual.transform.SetParent(_questRoot.transform, false);

                QuestInfo questInfo = questVisual.GetComponent<QuestInfo>();
                questInfo.SetQuest(newAvailableQuest);

                newAvailableQuest.questInfo = questInfo;
                newAvailableQuest.questTimer = 0.0f;
                UIGod.instance.SpawnActiveQuest(newAvailableQuest);

                _generatedQuests.Add(questVisual.transform);

                activeMissions.Add(mission);
                UpdateQuestCount();
            }

            yield return new WaitForSeconds(_questGenerationInterval);
        }
    }
}
