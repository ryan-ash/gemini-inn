using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Replic : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 5.0f;
    public List<Color> possibleColors = new List<Color>();

    private Fader fader;
    private Mover mover;
    private Image image;
    private Outline outline;
    private Text text;
    private TextBuilder textBuilder;
    private TextWriter textWriter;

    void Start()
    {
        fader = GetComponent<Fader>();
        mover = GetComponent<Mover>();
        image = GetComponent<Image>();
        outline = GetComponent<Outline>();
        text = GetComponentInChildren<Text>();
        textBuilder = GetComponentInChildren<TextBuilder>();
        textWriter = GetComponentInChildren<TextWriter>();
        if (possibleColors.Count > 0)
        {
            UpdateColor();
        }
        BuildText();
        Wait.Run(0.2f, () => { 
            fader.FadeIn();
            Wait.Run(lifetime, () => {
                fader.FadeOut("Destroy", true);
            });
        });
    }

    void Update()
    {
        
    }

    public void BuildText()
    {
        var newReplic = textBuilder.BuildText();
        if (textWriter != null)
        {
            textWriter.Write(newReplic);
        }
        else
        {
            text.text = newReplic;
        }
    }

    public void UpdateColor()
    {
        var newColor = possibleColors[Random.Range(0, possibleColors.Count)];
        outline.effectColor = newColor;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
