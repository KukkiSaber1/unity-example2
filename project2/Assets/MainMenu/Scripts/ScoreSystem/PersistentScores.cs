using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ScoreEntry
{
    public string sceneName;
    public int score;
    public string timestamp;

    public ScoreEntry(string sceneName, int score)
    {
        this.sceneName = sceneName;
        this.score = score;
        this.timestamp = DateTime.UtcNow.ToString("s");
    }
}

[Serializable]
public class ScoreList
{
    public List<ScoreEntry> scores = new List<ScoreEntry>();
}

public static class PersistentScores
{
    private const string PrefsKey = "HighScores_v1";
    public static int MaxEntries = 10;

    public static List<ScoreEntry> Load()
    {
        if (!PlayerPrefs.HasKey(PrefsKey)) return new List<ScoreEntry>();
        string json = PlayerPrefs.GetString(PrefsKey);
        try
        {
            var wrapper = JsonUtility.FromJson<ScoreList>(json);
            return wrapper?.scores ?? new List<ScoreEntry>();
        }
        catch
        {
            return new List<ScoreEntry>();
        }
    }

    public static void Save(List<ScoreEntry> list)
    {
        var wrapper = new ScoreList { scores = list };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PrefsKey, json);
        PlayerPrefs.Save();
    }

    public static void AddScore(string sceneName, int score)
    {
        var list = Load();
        list.Add(new ScoreEntry(sceneName, score));
        list = list.OrderByDescending(s => s.score).Take(MaxEntries).ToList();
        Save(list);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(PrefsKey);
    }
}
