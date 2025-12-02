using System.Collections.Generic;
using UnityEngine;
using System.IO;


[System.Serializable]

public class SaveData {
    public List<int> unlockedScenes = new List<int>();
}

public class SaveManager : MonoBehaviour {
    public static SaveManager Instance;
    private string path;
    private SaveData data = new SaveData();

    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        path = Path.Combine(Application.persistentDataPath, "save.json");
        Load();
    }

    public void UnlockScene(int sceneIndex) {
        if (!data.unlockedScenes.Contains(sceneIndex)) {
            data.unlockedScenes.Add(sceneIndex);
            Save();
        }
    }

    public bool IsSceneUnlocked(int sceneIndex) {
        return data.unlockedScenes.Contains(sceneIndex);
    }

    public void Save() {
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    public void Load() {
        if (File.Exists(path)) {
            var json = File.ReadAllText(path);
            data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        } else {
            // default: unlock scene 1
            data.unlockedScenes = new List<int>{1};
            Save();
        }
    }

    void OnApplicationQuit() { Save(); }
}