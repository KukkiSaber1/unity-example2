// TutorialPopup.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using TMPro;

public class TutorialPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Button nextButton;
    public Button skipButton;
    public Button closeButton;

    [Header("Timeline Reference")]
    public PlayableDirector timelineDirector;

    [Header("Tutorial Content")]
    public string tutorialTitle = "Controls Tutorial";
    [TextArea(3, 5)]
    public string tutorialDescription = "Learn the basic controls...";

    private bool isTutorialActive = false;

    void Start()
    {
        // Hide tutorial at start
        tutorialPanel.SetActive(false);

        // Setup button listeners
        nextButton.onClick.AddListener(OnNextClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        closeButton.onClick.AddListener(OnCloseClicked);

        // Start tutorial (you can call this from timeline or other triggers)
        StartTutorial();
    }

    public void StartTutorial()
    {
        if (isTutorialActive) return;

        isTutorialActive = true;
        tutorialPanel.SetActive(true);
        
        // Set tutorial content
        titleText.text = tutorialTitle;
        descriptionText.text = tutorialDescription;

        // Pause timeline if exists
        if (timelineDirector != null)
        {
            timelineDirector.Pause();
        }

        // Enable cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f; // Pause game time
    }

    private void OnNextClicked()
    {
        // Move to next tutorial page or close
        // You can implement multiple pages here
        CloseTutorial();
    }

    private void OnSkipClicked()
    {
        CloseTutorial();
    }

    private void OnCloseClicked()
    {
        CloseTutorial();
    }

    private void CloseTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);

        // Resume timeline if exists
        if (timelineDirector != null)
        {
            timelineDirector.Play();
        }

        // Restore cursor state
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f; // Resume game time
    }

    void OnDestroy()
    {
        // Clean up listeners
        nextButton.onClick.RemoveAllListeners();
        skipButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
    }
}