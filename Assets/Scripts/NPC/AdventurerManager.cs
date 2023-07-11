using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventurerManager : MonoBehaviour
{
    public static AdventurerManager instance;

    [Header("Settings")]
    public int maxAdventurers = 30;
    public int minAdventurers = 5;
    public int initialRandomizer = 5;
    public float spawnInterval = 3.0f;
    public float spawnChance = 0.1f;
    public float offsetFromCenter = 1.0f;
    public float randomizerStep = 0.1f;
    public float spawnCheckRadius = 0.5f;
    public LayerMask spawnCheckLayerMask;

    [Header("Mapping")]
    public GameObject adventurerPrefab;
    public GameObject adventurerParent;

    private AdventurerGroup[] adventurerGroups;
    private List<Adventurer> adventurers = new List<Adventurer>();

    void Start()
    {
        adventurerGroups = FindObjectsOfType<AdventurerGroup>();
        instance = this;
    }

    void Update()
    {
        
    }

    public void StartSpawning()
    {
        int initialAdventurerNumber = minAdventurers + Random.Range(0, initialRandomizer);
        for (int i = 0; i < initialAdventurerNumber; i++)
        {
            SpawnAdventurer(true);
        }

        StartCoroutine(TryToSpawnAdventurer());
    }

    private void SpawnAdventurer(bool ignoreChance = false)
    {
        if (adventurers.Count >= maxAdventurers)
            return;

        if (Random.Range(0.0f, 1.0f) > spawnChance && !ignoreChance)
            return;

        int adventurerGroupIndex = Random.Range(0, adventurerGroups.Length);
        AdventurerGroup adventurerGroup = adventurerGroups[adventurerGroupIndex];

        // find optimal spawn position
        Vector3 proposedPosition = new Vector3(adventurerGroup.transform.position.x, 0.0f, adventurerGroup.transform.position.z);

        // // offset in random direction
        // float directionAngle = Random.Range(0.0f, 2.0f * Mathf.PI);
        // proposedPosition += new Vector3(Mathf.Cos(directionAngle), 0.0f, Mathf.Sin(directionAngle)) * offsetFromCenter;

        // TODO: fix, works badly
        bool isColliding = true;
        float randomizer = 0.0f;
        while (isColliding)
        {
            randomizer += randomizerStep;
            proposedPosition = new Vector3(adventurerGroup.transform.position.x + Random.Range(-randomizer, randomizer), 0.0f, adventurerGroup.transform.position.z + Random.Range(-randomizer, randomizer));
            Collider[] colliders = Physics.OverlapSphere(proposedPosition, spawnCheckRadius, spawnCheckLayerMask);
            isColliding = colliders.Length > 0;
        }

        // look at light source
        GameObject adventurerObject = Instantiate(adventurerPrefab, proposedPosition, Quaternion.identity);
        adventurerObject.transform.SetParent(adventurerParent.transform, true);
        GameObject groupLight = adventurerGroup.adventurerTableLights;

        Vector3 targetDirection = groupLight.transform.position - adventurerObject.transform.position;
        Vector3 newDirection = Vector3.RotateTowards(adventurerObject.transform.forward, targetDirection, Mathf.PI, 0.0f);
        adventurerObject.transform.rotation = Quaternion.LookRotation(newDirection);
        float adventurerInitialYRotation = adventurerObject.transform.localEulerAngles.y + 90.0f;
        adventurerInitialYRotation = adventurerInitialYRotation % 360.0f;
        if (adventurerInitialYRotation < 0.0f)
            adventurerInitialYRotation += 180.0f;
        if (adventurerInitialYRotation > 180.0f)
            adventurerInitialYRotation -= 180.0f;
        adventurerObject.transform.localEulerAngles = new Vector3(0.0f, adventurerInitialYRotation, 0.0f);

        Adventurer adventurer = adventurerObject.GetComponent<Adventurer>();
        adventurerGroups[adventurerGroupIndex].AddAdventurer(adventurer);
        adventurers.Add(adventurer);

        if (!ignoreChance)
        {
            AudioManager.PlaySound(AudioNames.DoorSquek);
            AudioManager.PlaySound(AudioNames.DoorClosing);
            AudioManager.PlaySound(AudioNames.Footsteps);
        }
    }

    IEnumerator TryToSpawnAdventurer()
    {
        while (true)
        {
            SpawnAdventurer();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
