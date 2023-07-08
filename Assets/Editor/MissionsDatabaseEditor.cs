using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionsDatabase))]
public class MissionsDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MissionsDatabase MyScript = (MissionsDatabase)target;
        if(GUILayout.Button("Generate Random Mission"))
        {
            Mission Mission = Mission.GenerateRandomMission();
            MyScript.Missions.Add(Mission);
            MissionsLoader.SaveMission(Mission, "Test" + Mission.Quests[0].QuestName + ".json");
        }
    }
}