using UnityEngine;

public class SceneGoal : MonoBehaviour {
    public void MissionComplete() {
        // Unlock Scene 2 (index 2 in LevelManager)
        PlayerPrefs.SetInt("UnlockedLevel", 2);
        PlayerPrefs.Save();

        Debug.Log("Scene 1 completed! Scene 2 unlocked.");
    }
}
