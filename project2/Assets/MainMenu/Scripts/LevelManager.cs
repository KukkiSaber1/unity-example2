using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour {
    public Button[] levelButtons; // assign in Inspector
    public UnityEvent onReturnFromLevel; // hook up popup, animation, sound, etc.

    void Start() {
        // Unlock buttons
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        for (int i = 0; i < levelButtons.Length; i++) {
            levelButtons[i].interactable = (i < unlockedLevel);
        }

        // Check if we just returned from a level
        int lastCompleted = PlayerPrefs.GetInt("LastCompletedLevel", 0);
        if (lastCompleted > 0) {
            // Fire event once
            onReturnFromLevel?.Invoke();

            // Clear the flag so it doesn't fire again
            PlayerPrefs.DeleteKey("LastCompletedLevel");
            PlayerPrefs.Save();
        }
    }
}
