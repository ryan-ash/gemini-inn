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

    [Header("Quest Reporting")]
    [SerializeField] private GameObject _questEventPrefab;
    [SerializeField] private GameObject _questEventRoot;

    private bool isMapOpen = false;
    private bool scheduledHide = false;
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

    void Start()
    {
        instance = this;
        CursorSetter.SetDefaultCursor();
    }

    void Update()
    {
        if (!isMapOpen && !isChoosingAdventurers && !isNegotiating)
            return;

        if (isMapOpen)
        {
            if (_consideredQuestMarker != null && Input.GetMouseButtonDown(0))
            {
                selectedQuestMarker = _consideredQuestMarker;
                selectedQuest = _consideredQuest;
                StartChoosingAdventurers();
                return;
            }

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
                    _consideredQuest = questInfo.quest;
                    questInfo.ShowInfo();
                }
            }
            else if (_consideredQuestMarker != null && !scheduledHide)
            {
                scheduledHide = true;
            }
            else if (_consideredQuestMarker != null && scheduledHide)
            {
                scheduledHide = false;
                QuestInfo questInfo = _consideredQuestMarker.GetComponent<QuestInfo>();
                questInfo.HideInfo();
                _consideredQuestMarker = null;
                _consideredQuest = null;
            }
        }

        if (isChoosingAdventurers)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isChoosingAdventurers = false;
                if (_consideredAdventurerGroup != null && _consideredAdventurerGroup.adventurers.Count > 0)
                {
                    selectedAdventurerGroup = _consideredAdventurerGroup;
                    bool useFistNotBribe = Random.Range(0.0f, 1.0f) <= 0.5f;
                    if (useFistNotBribe)
                        CursorSetter.SetFistCursor(true);
                    else
                        CursorSetter.SetBribeCursor(true);
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

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            float maxDistance = 100.0f;
            if (Physics.Raycast(ray, out hit, maxDistance, adventurerRaycastLayerMask))
            {
                AdventurerGroup adventurerGroup = hit.transform.gameObject.GetComponent<AdventurerGroup>();
                if (adventurerGroup != _consideredAdventurerGroup)
                {
                    if (_consideredAdventurerGroup != null)
                        _consideredAdventurerGroup.UnfocusAdventurerTable();
                    _consideredAdventurerGroup = adventurerGroup;
                    adventurerGroup.FocusAdventurerTable();
                }
            }
            else if (_consideredAdventurerGroup != null)
            {
                _consideredAdventurerGroup.UnfocusAdventurerTable();
                _consideredAdventurerGroup = null;
            }
        }

        if (isNegotiating)
        {
            // for now, only control negotiations through UI
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
        ToggleMap();
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
                    UIGod.instance.SpawnAdventurerReplic(adventurerTransform);
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
        selectedQuest = null;
        isNegotiating = false;
        isChoosingAdventurers = false;
        ToggleMap();
    }

    public void DisagreeToQuest()
    {
        isNegotiating = false;
        isChoosingAdventurers = true;
        UIGod.instance.UpdateQuestTitle(selectedQuest.questName);
        foreach (Adventurer adventurer in selectedAdventurerGroup.adventurers)
        {
            float moveTime = CalculateMoveTime();
            adventurer.MoveBack(moveTime);
        }
        selectedAdventurerGroup.LightUpAdventurerTable();
    }

    public void StartGame()
    {
        AudioRevolver.Fire(AudioNames.Crowd);
        Wait.Run(0.1f, () => {
            AdventurerManager.instance.StartSpawning();
            Map.instance.GenerateMap();
        });
        Wait.Run(1.0f, () => { 
            UIGod.instance.mainFader.FadeOut();
            StartCoroutine(SpawnQuest());
        });
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

    IEnumerator SpawnQuest()
    {
        while (true)
        {
            List<Mission> bannedMissions = new List<Mission>();
            if (Random.Range(0.0f, 1.0f) <= _questGenerationChance)
            {
                Mission mission = useMockQuests ? Mission.GenerateRandomMission() : Mission.GrabRandomMissionFromDB();

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

                var availableBiomeTiles = Map.GetBiomeTiles(newAvailableQuest.Biomes.ToArray());
                if (availableBiomeTiles.Count == 0)
                {
                    Debug.LogWarning("No available tiles for quest: " + newAvailableQuest.questName);
                    continue;
                }

                int initialTileCount = availableBiomeTiles.Count;


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
                questInfo.quest = newAvailableQuest;

                _generatedQuests.Add(questVisual.transform);

                activeMissions.Add(mission);
                UIGod.instance.UpdateQuestCounter(activeMissions.Count);
            }

            yield return new WaitForSeconds(_questGenerationInterval);
        }
    }
}
