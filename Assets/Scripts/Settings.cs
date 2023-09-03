using UnityEngine;

public class Settings : MonoBehaviour {
    public static Settings instance;

    public LeanTweenType globalTweenConfig;

    [Header("Story")]
    public float dialogAnimationTime = 0.25f;

    private void Awake() {
        instance = this;
    }
}
