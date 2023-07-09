using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class GameMode : MonoBehaviour
{
    [Header("Mapping")]
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _inn;
    [SerializeField] private GameObject _innHUD;
    [SerializeField] private GameObject _map;
    [SerializeField] private Text _questCounter;

    [Header("Quest Generation")]
    [SerializeField] private float _questGenerationInterval = 10.0f;
    [SerializeField] private float _questGenerationChance = 0.25f;
    [SerializeField] private Vector2 _questGenerationRange = new Vector2(160.0f, 90.0f);
    [SerializeField] private GameObject _questVisualPrefab;
    [SerializeField] private GameObject _questRoot;

    [Header("Quest Selection")]
    [SerializeField] private LayerMask questRaycastLayerMask;

    [Header("Adventurer Selection")]
    [SerializeField] private LayerMask adventurerRaycastLayerMask;

    private bool isMapOpen = false;
    private bool scheduledHide = false;
    private bool isChoosingAdventurers = false;

    [HideInInspector] public List<Mission> Missions;

    private GameObject _consideredQuest;
    private GameObject _selectedQuest;
    private GameObject _consideredAdventurerGroup;
    private GameObject _selectedAdventurerGroup;

    void Start()
    {
        CursorSetter.SetDefaultCursor();
    }

    void Update()
    {
        if (!isMapOpen && !isChoosingAdventurers)
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
                        _consideredAdventurerGroup.GetComponent<AdventurerGroup>().LightDownAdventurerTable();
                    _consideredAdventurerGroup = potentialNewSelection;
                    adventurerGroup.LightUpAdventurerTable();
                }
            }
            else if (_consideredAdventurerGroup != null)
            {
                _consideredAdventurerGroup.GetComponent<AdventurerGroup>().LightDownAdventurerTable();
                _consideredAdventurerGroup = null;
            }
        }
    }

    public void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        if (isMapOpen)
        {
            _inn.SendMessage("ShowMap");
            _map.SendMessage("ShowMap");
        }
        else
        {
            _inn.SendMessage("HideMap");
            _map.SendMessage("HideMap");
        }

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void StartChoosingAdventurers()
    {
        ToggleMap();
        CursorSetter.SetHoverCursor(true);
        isChoosingAdventurers = true;
    }

    public void StartGame()
    {
        _menu.SendMessage("HideMenu");
        _map.SendMessage("GenerateMap");
        _innHUD.SetActive(true);
        StartCoroutine(SpawnQuest());
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void UpdateQuestCounter()
    {
        _questCounter.text = Missions.Count.ToString();
    }

    IEnumerator SpawnQuest()
    {
        while (true)
        {
            if (Random.Range(0.0f, 1.0f) <= _questGenerationChance)
            {
                Mission mission = Mission.GenerateRandomMission();
                Quest newAvailableQuest = mission.GetAvailableQuest();

                var availableBiomeTiles = Map.GetBiomeTiles(newAvailableQuest.Biomes.ToArray());

                Vector3 questPosition = new Vector3(Random.Range(-_questGenerationRange.x, _questGenerationRange.x), Random.Range(-_questGenerationRange.y, _questGenerationRange.y), 0.0f);

                var closestTile = availableBiomeTiles
                .GroupBy(x => System.Math.Pow(questPosition.x - x.X, 2) + System.Math.Pow(questPosition.y - x.Y, 2))
                .OrderBy(x => x.Key)
                .First().First();

                questPosition.x = closestTile.X;
                questPosition.y = closestTile.Y;

                newAvailableQuest.SetPosition(questPosition);

                GameObject questVisual = Instantiate(_questVisualPrefab, questPosition, Quaternion.identity);
                questVisual.transform.SetParent(_questRoot.transform, false);

                QuestInfo questInfo = questVisual.GetComponent<QuestInfo>();
                questInfo.quest = newAvailableQuest;

                Missions.Add(mission);

                UpdateQuestCounter();
            }

            yield return new WaitForSeconds(_questGenerationInterval);
        }
    }
}
