using UnityEngine;

public class SceneGoal : MonoBehaviour {
    public int levelIndex = 1; // current level index (1-based)

    public void MissionComplete() {
        int nextUnlocked = levelIndex + 1;
        PlayerPrefs.SetInt("UnlockedLevel", nextUnlocked);
        PlayerPrefs.SetInt("LastCompletedLevel", levelIndex);
        PlayerPrefs.Save();

        Debug.Log($"Level {levelIndex} completed! Level {nextUnlocked} unlocked.");
    }
}
