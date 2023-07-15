using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TextWriter : MonoBehaviour
{
    public float letterTime = 0.1f;
    public bool wipeOnChange = false;

    private Text MyText {
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

    public void Write(string text, bool instantUpdate = false)
    {
        if (wipeOnChange)
        {
            MyText.text = "";
        }
        targetText = text;
        if (instantUpdate)
        {
            MyText.text = targetText;
            return;
        }
        StopAllCoroutines();
        StartCoroutine("WriteText");
    }

    IEnumerator WriteText()
    {
        while (MyText.text != targetText)
        {
            if (targetText.Contains(MyText.text) || MyText.text == "")
            {
                MyText.text += targetText[MyText.text.Length];
            }
            else
            {
                MyText.text = MyText.text.Substring(0, MyText.text.Length - 1);
            }
            yield return new WaitForSeconds(letterTime);
        }
    }
}
