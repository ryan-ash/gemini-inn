using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;

// This is a super bare bones example of how to play and display a ink story in Unity.
public class StoryManager : MonoBehaviour {

	[Header("Settings")]
	public bool createEndDialogOption = false;

	[SerializeField]
	private TextAsset inkJSONAsset;
	private Story story;

	[SerializeField]
	private RectTransform storyParent, buttonsParent;

	// UI Prefabs
	[SerializeField]
	private Text textPrefab;
	[SerializeField]
	private Button buttonPrefab;

	public StoryTextControl storyText;

	private InkNarratorService inkService;

	public static StoryManager instance;

	public UnityAction endStoryAction;

	void Awake() {
		// Remove the default message
		RemoveChildren();
	}

	void Start() {
		instance = this;
	}

	// Creates a new Story object with the compiled story which we can then play!
	public void StartStory() {
		inkService = new InkNarratorService(inkJSONAsset.text);
		RefreshView();
	}

	public void SelectStory(TextAsset newStory) {
		inkJSONAsset = newStory;
	}
	
	// This is the main function called every time the story changes. It does a few things:
	// Destroys all the old content and choices.
	// Continues over all the lines of text, then displays all the choices. If there are no choices, the story is finished!
	void RefreshView() {

		// Remove all the UI on screen
		RemoveChildren ();
		
		storyText.ClearStoryBoard();

		// Read all the content until we can't continue any more
		while (inkService.isNextStoryLineAvailable()) {
			CreateContentView(inkService.getNextStoryLine());
		}

		storyText.NextText();
		storyText.ready = true;
	}

	public void DisplayButtons() {
		// Display all the choices, if there are any!
		if(inkService.isAnyChoiceAvailable()) {
			foreach (var choiceInfo in inkService.getChoices()) {
				Button button = CreateChoiceView (choiceInfo.text);
				// Debug.Log("Choice: " + choiceInfo.text);
				// Tell the button what to do when we press it
				button.onClick.AddListener (delegate {
					OnClickChoiceButton (choiceInfo.index);
				});
			}
		} else {
			if (createEndDialogOption)
			{
				Button choice = CreateChoiceView("[End Dialog]");
				choice.onClick.AddListener(delegate{
					TriggerEndStory();
				});
			}
			else
			{
				TriggerEndStory();
			}
		}
	}

	void TriggerEndStory() {
		if (endStoryAction != null) {
			endStoryAction();
		}
	}

	// When we click the choice button, tell the story to choose that choice!
	void OnClickChoiceButton(int index) {
		// Debug.Log("Clicked choice " + index);
		inkService.chooseChoiceIndex(index);
		RefreshView();
	}

	// Creates a button showing the choice text
	void CreateContentView(string text) {
		storyText.AddText(text);
	}

	// Creates a button showing the choice text
	Button CreateChoiceView(string text) {
		// Creates the button from a prefab
		Button choice = Instantiate (buttonPrefab) as Button;
		choice.transform.SetParent (buttonsParent, false);
		
		// Gets the text from the button prefab
		Text choiceText = choice.GetComponentInChildren<Text> ();
		choiceText.text = text;

		// Make the button expand to fit the text
		HorizontalLayoutGroup layoutGroup = choice.GetComponent <HorizontalLayoutGroup> ();
		layoutGroup.childForceExpandHeight = false;

		return choice;
	}

	void RemoveChildren() {
		// RemoveChildren(storyParent);
		RemoveChildren(buttonsParent);
	}

	// Destroys all the children of this gameobject (all the UI)
	void RemoveChildren(Transform targetParent) {
		List<Transform> deathRow = new List<Transform>();
		for (int i = 0; i < targetParent.childCount; i++) {
			deathRow.Add(targetParent.GetChild(i));
		}
		foreach(var item in deathRow) {
			Destroy(item.gameObject);
		}
	}
}
