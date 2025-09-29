using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Playables; // Added missing using directive

[System.Serializable]
public class TutorialPage
{
    public string title;
    [TextArea(3, 5)]
    public string description;
    public Sprite image;
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI pageCounterText;
    public Image tutorialImage;
    public Button nextButton;
    public Button previousButton;
    public Button skipButton;

    [Header("Tutorial Content")]
    public List<TutorialPage> tutorialPages = new List<TutorialPage>();

    [Header("Timeline Reference")]
    public PlayableDirector timelineDirector; // Now visible in inspector

    private int currentPageIndex = 0;
    private bool isTutorialActive = false;

    void Start()
    {
        tutorialPanel.SetActive(false);
        
        // Setup button listeners
        nextButton.onClick.AddListener(NextPage);
        previousButton.onClick.AddListener(PreviousPage);
        skipButton.onClick.AddListener(SkipTutorial);
    }

    // Public method to start tutorial - call this from other scripts or inspectors
    public void StartTutorial()
    {
        if (isTutorialActive || tutorialPages.Count == 0) return;

        isTutorialActive = true;
        currentPageIndex = 0;

        // Pause timeline if assigned
        if (timelineDirector != null)
        {
            timelineDirector.Pause();
        }

        // Setup UI
        tutorialPanel.SetActive(true);
        UpdateTutorialPage();

        // Pause game time
        Time.timeScale = 0f;
    }

    // Method to start tutorial with specific timeline - useful for dynamic assignment
    public void StartTutorialWithTimeline(PlayableDirector director)
    {
        timelineDirector = director;
        StartTutorial();
    }

    private void UpdateTutorialPage()
    {
        if (currentPageIndex < tutorialPages.Count)
        {
            TutorialPage currentPage = tutorialPages[currentPageIndex];
            
            titleText.text = currentPage.title;
            descriptionText.text = currentPage.description;
            
            if (tutorialImage != null && currentPage.image != null)
            {
                tutorialImage.sprite = currentPage.image;
                tutorialImage.gameObject.SetActive(true);
            }
            else if (tutorialImage != null)
            {
                tutorialImage.gameObject.SetActive(false);
            }

            // Update page counter
            if (pageCounterText != null)
            {
                pageCounterText.text = $"{currentPageIndex + 1}/{tutorialPages.Count}";
            }

            // Update button states
            if (previousButton != null)
                previousButton.interactable = currentPageIndex > 0;
            
            if (nextButton != null)
            {
                TextMeshProUGUI nextButtonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (nextButtonText != null)
                {
                    nextButtonText.text = currentPageIndex == tutorialPages.Count - 1 ? "Finish" : "Next";
                }
            }
        }
    }

    private void NextPage()
    {
        if (currentPageIndex < tutorialPages.Count - 1)
        {
            currentPageIndex++;
            UpdateTutorialPage();
        }
        else
        {
            FinishTutorial();
        }
    }

    private void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateTutorialPage();
        }
    }

    private void SkipTutorial()
    {
        FinishTutorial();
    }

    private void FinishTutorial()
    {
        isTutorialActive = false;
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Resume timeline if was paused
        if (timelineDirector != null)
        {
            timelineDirector.Play();
        }

        // Resume game time
        Time.timeScale = 1f;
    }

    // Editor helper method - you can call this from inspector buttons
    [ContextMenu("Test Start Tutorial")]
    public void TestStartTutorial()
    {
        StartTutorial();
    }

    [ContextMenu("Test Finish Tutorial")]
    public void TestFinishTutorial()
    {
        FinishTutorial();
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (nextButton != null)
            nextButton.onClick.RemoveAllListeners();
        if (previousButton != null)
            previousButton.onClick.RemoveAllListeners();
        if (skipButton != null)
            skipButton.onClick.RemoveAllListeners();
    }
}