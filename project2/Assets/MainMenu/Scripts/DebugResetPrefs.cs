using UnityEngine;

public class DebugResetPrefs : MonoBehaviour {
    void Update() {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.R)) {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs cleared");
        }
        #endif
    }
}
