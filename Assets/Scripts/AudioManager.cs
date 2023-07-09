using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            AudioClip clip = child.GetComponent<AudioClip>();
            if (clip != null)
            {
                audioClips.Add(child.name, clip);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void PlaySound(string soundName)
    {
        if (instance.audioClips.ContainsKey(soundName))
        {
            AudioSource.PlayClipAtPoint(instance.audioClips[soundName], Camera.main.transform.position);
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