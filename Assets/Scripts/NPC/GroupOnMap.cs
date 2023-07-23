using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupOnMap : MonoBehaviour
{
    public FontAwesome icon;
    public SpriteRenderer circle;

    public float fadeTime = 1.0f;
    public LeanTweenType fadeType = LeanTweenType.easeOutCubic;
    public LeanTweenType moveType = LeanTweenType.easeOutCubic;

    private Color initialIconColor;
    private Color initialCircleColor;
    private Color offIconColor;
    private Color offCircleColor;

    [HideInInspector] public List<Adventurer> adventurers;

    Vector3 movePosition;
    float moveTime = 1.0f;

    public void SetIcon(string iconName)
    {
        icon.ChangeIcon(iconName);
    }

    public void Move(Vector3 position, float time, bool fadeOutInTheEnd = false, bool dieInTheEnd = false)
    {
        moveTime = time;
        movePosition = position;
        LeanTween.move(gameObject, position, time).setEase(moveType).setOnComplete(() => {
            if (fadeOutInTheEnd)
                FadeOut(dieInTheEnd);
            else if (dieInTheEnd)
                Destroy(gameObject);
        });
    }

    public void FadeIn()
    {
        LeanTween.alpha(icon.gameObject, 1.0f, fadeTime).setOnUpdate((float val) => {
            icon.color = Color.Lerp(offIconColor, initialIconColor, val);
            circle.color = Color.Lerp(offCircleColor, initialCircleColor, val);
        }).setEase(fadeType);
    }

    public void FadeOut(bool die = false)
    {
        LeanTween.alpha(icon.gameObject, 1.0f, fadeTime).setOnUpdate((float val) => {
            icon.color = Color.Lerp(initialIconColor, offIconColor, val);
            circle.color = Color.Lerp(initialCircleColor, offCircleColor, val);
        }).setEase(fadeType).setOnComplete(() => {
            if (die)
            {
                AudioRevolver.Fire(AudioNames.DoorSquek);
                AudioRevolver.Fire(AudioNames.DoorClosing);
                AudioRevolver.Fire(AudioNames.Footsteps);

                // restore adventurers in bar
                foreach (Adventurer adventurer in adventurers)
                {
                    AdventurerManager.instance.SpawnAdventurer(true, adventurer.gameObject);
                }
                Destroy(gameObject);
            }
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        initialIconColor = icon.color;
        initialCircleColor = circle.color;
        offIconColor = new Color(initialIconColor.r, initialIconColor.g, initialIconColor.b, 0.0f);
        offCircleColor = new Color(initialCircleColor.r, initialCircleColor.g, initialCircleColor.b, 0.0f);
        icon.color = offIconColor;
        circle.color = offCircleColor;
        FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
