using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextWriter : MonoBehaviour
{
    public float letterTime = 0.1f;
    public bool wipeOnChange = false;
    public bool fillWithSpaces = false;

    public Text MyText {
        get {
            if (myText == null) {
                myText = GetComponent<Text>();
                if (myText == null) {
                    myText = gameObject.AddComponent<Text>() as Text;
                }
            }
            return myText;
        }
    }
    private Text myText;

    private string targetText = "";
    private string currentText = "";

    public void Write(string text, bool instantUpdate = false)
    {
        if (wipeOnChange)
        {
            UpdateText("");
        }
        targetText = text;
        if (instantUpdate)
        {
            UpdateText(targetText);
            return;
        }
        StopAllCoroutines();
        StartCoroutine("WriteText");
    }

    private void UpdateText(string newText)
    {
        currentText = newText;
        string paddedText = currentText;
        if (fillWithSpaces)
        {
            paddedText = currentText.PadRight(targetText.Length, ' ');
        }
        MyText.text = paddedText;
    }

    IEnumerator WriteText()
    {
        while (MyText.text != targetText)
        {
            if (targetText.Contains(currentText) || currentText == "")
            {
                UpdateText(currentText + targetText[currentText.Length]);
            }
            else
            {
                UpdateText(currentText.Substring(0, currentText.Length - 1));
            }
            yield return new WaitForSeconds(letterTime);
        }
    }
}
