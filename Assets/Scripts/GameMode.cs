using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMode : MonoBehaviour
{
    [SerializeField] private GameObject _menu;

    [Header("Quest Generation")]
    [SerializeField] private float _questGenerationInterval = 10.0f;
    [SerializeField] private float _questGenerationChance = 0.25f;
    [SerializeField] private Vector2 _questGenerationRange = new Vector2(160.0f, 90.0f);
    [SerializeField] private GameObject _questVisualPrefab;
    [SerializeField] private GameObject _questRoot;

    public List<Mission> Missions;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        // reset world state
        // hide ui
        // open inn / global map (decide later)
        _menu.SendMessage("HideMenu");
        StartCoroutine(SpawnQuest());
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
                questVisual.transform.SetParent(_questRoot.transform);

                Missions.Add(mission);
            }

            yield return new WaitForSeconds(_questGenerationInterval);
        }
    }
}
