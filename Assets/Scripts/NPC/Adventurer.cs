using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : MonoBehaviour
{
    [Header("Character")]
    public string adventurerName;
    public bool femaleGender = false;
    public List<Stat> stats = new List<Stat>();
    public List<Ability> abilities = new List<Ability>();
    public int maxAbilitiesAtStart = 3;
    public float abilitySpawnChance = 0.25f;

    [Header("Generation")]
    [SerializeField]
    private SkinnedMeshRenderer HeadRenderer;
    [SerializeField]
    private SkinnedMeshRenderer BodyRenderer;
    [SerializeField]
    private SkinnedMeshRenderer LegsRenderer;
    [SerializeField]
    private SkinnedMeshRenderer BackpackRenderer;
    [SerializeField]
    private SkinnedMeshRenderer FeetRenderer;
    
    [SerializeField]
    private List<Color> SkinViableColors;
    [SerializeField]
    private List<Color> HairViableColors;
    [SerializeField]
    private List<Color> ViableColors;
    [SerializeField]
    private List<Mesh> BodyVariations;
    [SerializeField]
    private List<Mesh> HeadVariations;
    [SerializeField]
    private List<Mesh> LegsVariations;
    [SerializeField]
    private List<Mesh> BackpackVariations;
    [SerializeField]
    private List<Mesh> FeetVariations;

    [HideInInspector] public QuestGroup group;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private TextBuilder nameBuilder;

    public void RandomizeCharacter()
    {
        int Index = Random.Range(0, HeadVariations.Count);
        HeadRenderer.sharedMesh = HeadVariations[Index];
        //HeadRenderer.materials = HeadVariations[Index].materials;
        Index = Random.Range(0, BodyVariations.Count);
        BodyRenderer.sharedMesh = BodyVariations[Index];
        //BodyRenderer.materials = BodyVariations[Index].materials;
        Index = Random.Range(0, LegsVariations.Count);
        LegsRenderer.sharedMesh = LegsVariations[Index];
        //LegsRenderer.materials = LegsVariations[Index].materials;
        Index = Random.Range(0, FeetVariations.Count);
        FeetRenderer.sharedMesh = FeetVariations[Index];
        //FeetRenderer.materials = FeetRenderer[Index].materials;

        Index = Random.Range(0, SkinViableColors.Count);
        Color SkinColor = SkinViableColors[Index];
        Index = Random.Range(0, HairViableColors.Count);
        Color HairColor = HairViableColors[Index];
        ChangeRendererColors(HeadRenderer, SkinColor, HairColor);
        ChangeRendererColors(BodyRenderer, SkinColor, HairColor);
        ChangeRendererColors(LegsRenderer, SkinColor, HairColor);
        ChangeRendererColors(FeetRenderer, SkinColor, HairColor);
        ChangeRendererColors(BackpackRenderer, SkinColor, HairColor);
    }

    private void ChangeRendererColors(SkinnedMeshRenderer Renderer, Color SkinColor, Color HairColor)
    {
        
        Material[] Materials = Renderer.materials;

        for (int I = 0; I < Materials.Length; I++)
        {
            if (Materials[I].name == "Skin")
            {
                Materials[I].SetColor("_Color", SkinColor);
            }
            else if (Materials[I].name == "Eyebrows" || Materials[I].name == "Hair")
            {
                Materials[I].SetColor("_Color", HairColor);
            }
            else
            {
                int Index = Random.Range(0, ViableColors.Count);
                Materials[I].SetColor("_Color", ViableColors[Index]);
            }
        }
    }

    public void MoveTo(Vector3 position, float moveTime)
    {
        LeanTween.moveLocal(gameObject, position, moveTime).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float val) => {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Camera.main.transform.position - transform.position), val);
        });
    }

    public void MoveBack(float moveTime)
    {
        LeanTween.moveLocal(gameObject, initialPosition, moveTime).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float val) => {
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, val);
        });
    }

    public void SetRandomGender()
    {
        femaleGender = Random.Range(0, 2) == 0;
    }

    public void RunNameGenerator()
    {
        nameBuilder = GetComponent<TextBuilder>();
        adventurerName = nameBuilder.BuildText();
    }

    public void RandomizeStats()
    {
        stats.Clear();
        int statTypesNum = 4;
        for (int I = 0; I < statTypesNum; I++)
        {
            Stat NewStat = new Stat();
            NewStat.Type = (StatType)I + 1;
            NewStat.Value = Random.Range(0, 101);
            stats.Add(NewStat);
        }
    }

    public void RandomizeAbilities()
    {
        abilities.Clear();
        int abilityTypesNum = 4;
        for (int I = 0; I < abilityTypesNum; I++)
        {
            if (Random.Range(0.0f, 1.0f) < abilitySpawnChance)
            {
                Ability NewAbility = new Ability();
                NewAbility.Type = (AbilityType)I + 1;
                NewAbility.Level = Random.Range(0, 101); // levels are ignored for now
                abilities.Add(NewAbility);
                if (abilities.Count >= maxAbilitiesAtStart)
                    break;
            }
        }
    }

    void Start()
    {
        RandomizeCharacter();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }
}
