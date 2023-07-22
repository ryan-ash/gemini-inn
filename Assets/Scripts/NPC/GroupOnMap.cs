using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupOnMap : MonoBehaviour
{
    public FontAwesome3D icon;
    public SpriteRenderer circle;

    public float fadeTime = 1.0f;
    public LeanTweenType fadeType = LeanTweenType.easeOutCubic;
    public LeanTweenType moveType = LeanTweenType.easeOutCubic;

    private Color initialIconColor;
    private Color initialCircleColor;
    private Color offIconColor;
    private Color offCircleColor;

    Vector3 movePosition;
    float moveTime = 1.0f;

    public void SetIcon(string iconName)
    {
        icon.ChangeIcon(iconName);
    }

    public void Move(Vector3 position, float time, bool dieInTheEnd = false)
    {
        moveTime = time;
        movePosition = position;
        LeanTween.move(gameObject, position, time).setEase(moveType).setOnComplete(() => {
            if (dieInTheEnd)
                Die();
        });
    }

    public void FadeIn()
    {
        // LeanTween.alpha(gameObject, 0.0f, 0.0f);
        LeanTween.alpha(gameObject, 1.0f, fadeTime).setOnUpdate((float val) => {
            icon.color = Color.Lerp(offIconColor, initialIconColor, val);
            circle.color = Color.Lerp(offCircleColor, initialCircleColor, val);
        }).setEase(fadeType);
    }

    public void Die()
    {
        LeanTween.alpha(gameObject, 0.0f, fadeTime).setOnUpdate((float val) => {
            icon.color = Color.Lerp(initialIconColor, offIconColor, val);
            circle.color = Color.Lerp(initialCircleColor, offCircleColor, val);
        }).setEase(fadeType).setOnComplete(() => {
            Destroy(gameObject);
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
