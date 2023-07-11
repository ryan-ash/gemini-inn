using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextBuilder : MonoBehaviour
{
    public List<TextPreset> textPresets = new List<TextPreset>();
    public string lastGeneratedText = "";

    public string BuildText()
    {
        var textPreset = textPresets[Random.Range(0, textPresets.Count)];

        for (int i = 0; i < textPreset.pieces.Count; i++)
        {
            var piece = textPreset.pieces[i];
            if (piece.options.Count == 0)
            {
                continue;
            }

            // todo: test this

            // 1. calculate total weight
            var totalWeight = 0;
            foreach (var possibleOption in piece.options)
            {
                totalWeight += possibleOption.weight;
            }

            // 2. generate random number between 0 and totalWeight
            var randomNumber = Random.Range(0, totalWeight);

            // 3. iterate through options and subtract their weight from randomNumber until randomNumber is less than 0
            var currentWeight = 0;
            TextOption option = null;
            for (int j = 0; j < piece.options.Count; j++)
            {
                option = piece.options[j];
                currentWeight += option.weight;
                if (randomNumber < currentWeight)
                {
                    break;
                }
            }

            lastGeneratedText += option.text;
            if (i < textPreset.pieces.Count - 1)
            {
                lastGeneratedText += " ";
            }
        }

        return lastGeneratedText;
    }
}

[System.Serializable]
public class TextPreset
{
    public List<TextPiece> pieces;
}

[System.Serializable]
public class TextPiece
{
    public List<TextOption> options;
    public string name { 
        get 
        {
            var optionsString = "";
            if (options.Count == 0)
            {
                return "";
            }
            foreach (var option in options)
            {
                optionsString += option.text + " | ";
            }
            optionsString = optionsString.Substring(0, optionsString.Length - 3);
            return optionsString;
        }
    }
}

[System.Serializable]
public class TextOption
{
    public string text;
    public int weight = 1;
}