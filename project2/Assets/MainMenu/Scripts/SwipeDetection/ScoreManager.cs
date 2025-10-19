using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int currentScore = 0;
    public int swipeSuccessPoints = 100;

    [Header("Audio Settings")]
    public AudioClip swipeSuccessSound;
    private AudioSource audioSource;

    public static ScoreManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Add score points
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"Added {points} points! Total: {currentScore}");
        
        // Update UI if you have one
        // UIManager.Instance.UpdateScore(currentScore);
    }

    /// <summary>
    /// Add default swipe success score
    /// </summary>
    public void AddSwipeScore()
    {
        AddScore(swipeSuccessPoints);
    }

    /// <summary>
    /// Play swipe sound
    /// </summary>
    public void PlaySwipeSound()
    {
        if (audioSource != null && swipeSuccessSound != null)
        {
            audioSource.PlayOneShot(swipeSuccessSound);
        }
        Debug.Log("Playing swipe sound!");
    }

    /// <summary>
    /// Reset score
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0");
    }
}