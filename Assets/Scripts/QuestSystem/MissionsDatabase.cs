using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MissionsDatabase : MonoBehaviour
{
    public List<Mission> Missions;
    public static MissionsDatabase instance;

    public MissionsDatabase()
    {
        Missions = new List<Mission>();
    }

    public void Start()
    {
        instance = this;
        LoadMissionsFromDirectory();
    }

    public void LoadMissionsFromDirectory()
    {
        string Path = MissionsLoader.MissionsFolder;
        // Get all json files in the directory
        string[] Files = Directory.GetFiles(Path, "*.json");

        foreach (string File in Files)
        {
            Mission Mission = MissionsLoader.LoadMission(File);

            if (Mission != null)
            {
                Missions.Add(Mission);
            }
        }
    }
}
