using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Vector2 _questGenerationRange = new Vector2(160.0f, 90.0f);
    [SerializeField] private GameObject _questVisualPrefab;
    [SerializeField] private GameObject _questRoot;
    [SerializeField] private bool useMockQuests = false;

    [Header("Quest Selection")]
    [SerializeField] private LayerMask questRaycastLayerMask;

    [Header("Adventurer Selection")]
    [SerializeField] private LayerMask adventurerRaycastLayerMask;
    [SerializeField] private float distanceDuringNegotiation = 1.5f;
    [SerializeField] private float distanceRandomizationDuringNegotiation = 1.5f;

    [Header("Quest Reporting")]
    [SerializeField] private GameObject _questEventPrefab;
    [SerializeField] private GameObject _questEventRoot;

    private bool isMapOpen = false;
    private bool scheduledHide = false;
    private bool isChoosingAdventurers = false;
    private bool isNegotiating = false;

    [HideInInspector] public List<Mission> Missions;

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
                if (_consideredAdventurerGroup != null)
                {
                    selectedAdventurerGroup = _consideredAdventurerGroup;
                    bool useFistNotBribe = Random.Range(0.0f, 1.0f) <= 0.5f;
                    if (useFistNotBribe)
                        CursorSetter.SetFistCursor(true);
                    else
                        CursorSetter.SetBribeCursor(true);
                    MoveSelectedAdventurersToNegotiation();
                }
                else
                {
                    CursorSetter.ResetPriorityCursor();
                    ToggleMap();
                    UIGod.instance.UpdateQuestTitle("");
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
            if (Input.GetMouseButtonDown(0))
            {
                DisagreeToQuest();
            }
        }
    }

    public void ToggleMap()
    {
        if (Inn.instance.isCameraMoving)
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
        AudioRevolver.Fire(AudioNames.Footsteps);
        foreach (Adventurer adventurer in selectedAdventurerGroup.adventurers)
        {
            Vector3 directionFromOwner = Vector3.Normalize(adventurer.transform.position - Camera.main.transform.position);
            float actualDistanceDuringNegotiation = distanceDuringNegotiation + Random.Range(-distanceRandomizationDuringNegotiation, distanceRandomizationDuringNegotiation);
            Vector3 newPosition = new Vector3(Camera.main.transform.position.x + directionFromOwner.x * actualDistanceDuringNegotiation, 0.0f, Camera.main.transform.position.z + directionFromOwner.z * actualDistanceDuringNegotiation);
            adventurer.MoveTo(newPosition);
            selectedAdventurerGroup.LightDownAdventurerTable();
        }
        isNegotiating = true;
    }

    public void AgreeToQuest()
    {
        selectedQuest.adventureGroup = selectedAdventurerGroup;
        selectedAdventurerGroup.quest = selectedQuest;
        UIGod.instance.UpdateQuestTitle("");
        // move group to quest
        // spawn new empty group
        // replace group in inn with new group
    }

    public void DisagreeToQuest()
    {
        isNegotiating = false;
        isChoosingAdventurers = true;
        CursorSetter.SetHoverCursor(true);
        foreach (Adventurer adventurer in selectedAdventurerGroup.adventurers)
        {
            adventurer.MoveBack();
        }
        selectedAdventurerGroup.LightUpAdventurerTable();
    }

    public void StartGame()
    {
        AudioRevolver.Fire(AudioNames.Crowd);
        Wait.Run(1.0f, () => { 
            AdventurerManager.instance.StartSpawning();
            Map.instance.GenerateMap();
            StartCoroutine(SpawnQuest());
        });
    }

    public void OnQuestUpdated(Quest quest)
    {
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
            if (Random.Range(0.0f, 1.0f) <= _questGenerationChance)
            {
                Mission mission = useMockQuests ? Mission.GenerateRandomMission() : Mission.GrabRandomMissionFromDB();
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

                    Debug.Log(availableBiomeTiles.Count.ToString());

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

                Missions.Add(mission);
                UIGod.instance.UpdateQuestCounter(Missions.Count);
            }

            yield return new WaitForSeconds(_questGenerationInterval);
        }
    }
}
