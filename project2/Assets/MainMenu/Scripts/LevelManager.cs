using UnityEngine;
using UnityEngine.UI; // For Button

public class LevelManager : MonoBehaviour {
    public Button[] levelButtons; // Assign in Inspector

    void Start() {
        // Load saved progress
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // Lock/unlock buttons
        for (int i = 0; i < levelButtons.Length; i++) {
            if (i < unlockedLevel) {
                levelButtons[i].interactable = true; // unlocked
            } else {
                levelButtons[i].interactable = false; // locked
            }
        }
    }

    public void UnlockLevel(int levelIndex) {
        int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (levelIndex > currentUnlocked) {
            PlayerPrefs.SetInt("UnlockedLevel", levelIndex);
            PlayerPrefs.Save();
        }
    }
}
