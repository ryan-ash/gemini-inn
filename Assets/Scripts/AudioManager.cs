using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

    void Start()
    {
        instance = this;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            AudioSource source = child.GetComponent<AudioSource>();
            if (source != null)
            {
                audioSources.Add(child.name, source);
            }
        }
    }

    void Update()
    {
        
    }

    public static void PlaySound(string soundName)
    {
        if (instance.audioSources.ContainsKey(soundName))
        {
            instance.audioSources[soundName].Play();
        }
        else
        {
            Debug.LogError("Sound not found: " + soundName);
        }
    }
}

public class AudioNames {
    public static string QuestFailed = "QuestFailed";
    public static string QuestCompleted = "QuestCompleted";
    public static string Footsteps = "Footsteps";
    public static string BirdsSound = "BirdsSound";
    public static string BookPages = "BookPages";
    public static string DoorSquek = "DoorSquek";
    public static string DoorClosing = "DoorClosing";
    public static string CameraWoosh = "CameraWoosh";
    public static string Crowd = "Crowd";
    public static string Click = "Click";
    public static string MapSound = "MapSound";
    public static string PencilWriting = "PencilWriting";
    public static string MugOnTable = "MugOnTable";
}