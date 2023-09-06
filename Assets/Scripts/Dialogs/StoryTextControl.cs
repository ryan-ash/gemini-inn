using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StoryTextControl : MonoBehaviour
{
    public List<string> storyboard;
    private int currentTextIndex = 0;

    public GameObject storyTextTemplate;
    private GameObject currentNewText;

    [HideInInspector]
    public bool ready = false;

    public StoryManager storyManager;

    private List<GameObject> previousLines = new List<GameObject>();
    
    public void ClearStoryBoard() {
        currentTextIndex = 0;
        storyboard.Clear();
    }

    public void AddText(string text) {
        storyboard.Add(text);
    }

    public void Show(string text) {
        HidePrevious();

        GameObject newText = Instantiate(storyTextTemplate) as GameObject;
        newText.transform.SetParent(transform, false);
        newText.GetComponent<Text>().text = text;
        newText.GetComponent<Fader>().FadeIn();

        previousLines.Add(newText);

        float initialY = -1 * newText.GetComponent<RectTransform>().sizeDelta.y;
        UpdateTextY(newText, initialY);
        LeanTween.value(gameObject, 0f, 1f, Settings.instance.dialogAnimationTime).setOnUpdate(
            (float value) => {
                UpdateTextY(newText, Mathf.Lerp(initialY, 0, value));
            }
        ).setEase(Settings.instance.globalTweenConfig);

        currentNewText = newText;
    }

    public void HidePrevious() {
        for (int i = 0; i < previousLines.Count; i++) {
            GameObject line = previousLines[i];
            if (line != null) {
                float finalY = line.GetComponent<RectTransform>().sizeDelta.y;
                LeanTween.value(gameObject, 0f, 1f, Settings.instance.dialogAnimationTime).setOnUpdate(
                    (float value) => {
                        UpdateTextY(line, Mathf.Lerp(0, finalY, value));
                    }
                ).setOnComplete(
                    () => {
                        Destroy(line);
                    }
                ).setEase(Settings.instance.globalTweenConfig);
                line.GetComponent<Fader>().FadeOut();
            }
        }
        previousLines.Clear();
    }

    private void UpdateTextY(GameObject targetText, float y) {
        Vector3 newTextPosition = targetText.transform.localPosition;
        newTextPosition.y = y;
        targetText.transform.localPosition = newTextPosition;
    }

    void Update() {
        if (ready && Input.anyKeyDown) {
            storyManager.PrepareAndPushNextLine();
        }
    }

    public void NextText() {
        if (currentTextIndex >= storyboard.Count) {
            return;
        }

        if (currentTextIndex > 0) {
            AudioRevolver.Fire(AudioNames.Hover);
        }

        Show(storyboard[currentTextIndex]);
        currentTextIndex += 1;

        if (currentTextIndex >= storyboard.Count && !storyManager.IsNextLineAvailable()) {
            storyManager.DisplayButtons();
        }
    }
}
