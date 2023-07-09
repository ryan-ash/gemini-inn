using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private LayerMask raycastLayerMask;

    private bool isMapOpen = false;
    [HideInInspector] public List<Mission> Missions;
    private GameObject _selectedQuest;
    private bool scheduledHide = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMapOpen)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float maxDistance = 100.0f;
        if (Physics.Raycast(ray, out hit, maxDistance, raycastLayerMask))
        {
            QuestInfo questInfo = hit.transform.gameObject.GetComponentInParent<QuestInfo>();
            GameObject potentialNewSelection = questInfo.gameObject;
            if (potentialNewSelection != _selectedQuest)
            {
                _selectedQuest = potentialNewSelection;
                questInfo.ShowInfo();
            }
        }
        else if (_selectedQuest != null && !scheduledHide)
        {
            scheduledHide = true;
        }
        else if (_selectedQuest != null && scheduledHide)
        {
            scheduledHide = false;
            QuestInfo questInfo = _selectedQuest.GetComponent<QuestInfo>();
            questInfo.HideInfo();
            _selectedQuest = null;
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
                Vector3 questPosition = new Vector3(Random.Range(-_questGenerationRange.x, _questGenerationRange.x), Random.Range(-_questGenerationRange.y, _questGenerationRange.y), 0.0f);
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
