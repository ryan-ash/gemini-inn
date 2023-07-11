using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class GameMode : MonoBehaviour
{
    [Header("Mapping")]
    [SerializeField] private GameObject _innHUD;
    [SerializeField] private Text _questCounter;

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

    private bool isMapOpen = false;
    private bool scheduledHide = false;
    private bool isChoosingAdventurers = false;
    private bool isNegotiating = false;

    [HideInInspector] public List<Mission> Missions;

    private GameObject _consideredQuest;
    private GameObject _selectedQuest;
    private GameObject _consideredAdventurerGroup;
    private GameObject _selectedAdventurerGroup;

    private List<Transform> _generatedQuests = new List<Transform>();

    void Start()
    {
        CursorSetter.SetDefaultCursor();
    }

    void Update()
    {
        if (!isMapOpen && !isChoosingAdventurers && !isNegotiating)
            return;

        if (isMapOpen)
        {
            if (_consideredQuest != null && Input.GetMouseButtonDown(0))
            {
                _selectedQuest = _consideredQuest;
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
                if (potentialNewSelection != _consideredQuest)
                {
                    _consideredQuest = potentialNewSelection;
                    questInfo.ShowInfo();
                }
            }
            else if (_consideredQuest != null && !scheduledHide)
            {
                scheduledHide = true;
            }
            else if (_consideredQuest != null && scheduledHide)
            {
                scheduledHide = false;
                QuestInfo questInfo = _consideredQuest.GetComponent<QuestInfo>();
                questInfo.HideInfo();
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
                    _selectedAdventurerGroup = _consideredAdventurerGroup;
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
                }
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            float maxDistance = 100.0f;
            if (Physics.Raycast(ray, out hit, maxDistance, adventurerRaycastLayerMask))
            {
                AdventurerGroup adventurerGroup = hit.transform.gameObject.GetComponent<AdventurerGroup>();
                GameObject potentialNewSelection = adventurerGroup.gameObject;
                if (potentialNewSelection != _consideredAdventurerGroup)
                {
                    if (_consideredAdventurerGroup != null)
                        _consideredAdventurerGroup.GetComponent<AdventurerGroup>().UnfocusAdventurerTable();
                    _consideredAdventurerGroup = potentialNewSelection;
                    adventurerGroup.FocusAdventurerTable();
                }
            }
            else if (_consideredAdventurerGroup != null)
            {
                _consideredAdventurerGroup.GetComponent<AdventurerGroup>().UnfocusAdventurerTable();
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

        AudioManager.PlaySound(AudioNames.MapSound);
        AudioManager.PlaySound(AudioNames.CameraWoosh);
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
    }

    public void MoveSelectedAdventurersToNegotiation()
    {
        AudioManager.PlaySound(AudioNames.Footsteps);
        AdventurerGroup adventurerGroup = _selectedAdventurerGroup.GetComponent<AdventurerGroup>();
        foreach (Adventurer adventurer in adventurerGroup.adventurers)
        {
            Vector3 directionFromOwner = Vector3.Normalize(adventurer.transform.position - Camera.main.transform.position);
            float actualDistanceDuringNegotiation = distanceDuringNegotiation + Random.Range(-distanceRandomizationDuringNegotiation, distanceRandomizationDuringNegotiation);
            Vector3 newPosition = new Vector3(Camera.main.transform.position.x + directionFromOwner.x * actualDistanceDuringNegotiation, 0.0f, Camera.main.transform.position.z + directionFromOwner.z * actualDistanceDuringNegotiation);
            adventurer.MoveTo(newPosition);
            adventurerGroup.LightDownAdventurerTable();
        }
        isNegotiating = true;
    }

    public void AgreeToQuest()
    {
        // generate result
        // show result
    }

    public void DisagreeToQuest()
    {
        isNegotiating = false;
        isChoosingAdventurers = true;
        CursorSetter.SetHoverCursor(true);
        AdventurerGroup adventurerGroup = _selectedAdventurerGroup.GetComponent<AdventurerGroup>();
        foreach (Adventurer adventurer in adventurerGroup.adventurers)
        {
            adventurer.MoveBack();
        }
        adventurerGroup.LightUpAdventurerTable();
    }

    public void StartGame()
    {
        AudioManager.PlaySound(AudioNames.Click);
        AudioManager.PlaySound(AudioNames.Crowd);
        AdventurerManager.instance.StartSpawning();
        Menu.instance.HideMenu();
        Map.instance.GenerateMap();
        _innHUD.SetActive(true);
        StartCoroutine(SpawnQuest());
    }

    public void QuitGame()
    {
        AudioManager.PlaySound(AudioNames.Click);
        Application.Quit();
    }

    private void UpdateQuestCounter()
    {
        AudioManager.PlaySound(AudioNames.PencilWriting);
        _questCounter.text = Missions.Count.ToString();
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
                    Debug.LogError("No available tiles for quest: " + newAvailableQuest.questName);
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

                UpdateQuestCounter();
            }

            yield return new WaitForSeconds(_questGenerationInterval);
        }
    }
}
