using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class MissionsLoader
{
    public static string MissionsFolder = "Assets/Missions"; 
    public static void SaveMission(Mission InMission, string InFilename)
    {
        string Json = JsonUtility.ToJson(InMission, true);
        File.WriteAllText((MissionsFolder + "/" + InFilename), Json);
    }

    public static Mission LoadMission(string InFilename)
    {
        if (!File.Exists(InFilename)) 
        {
            Debug.LogError($"File does not exist: {InFilename}");
            return null;
        }
        
        string Json = File.ReadAllText(InFilename);
        Mission Mission = JsonConvert.DeserializeObject<Mission>(Json);
        return Mission;
    }
}
