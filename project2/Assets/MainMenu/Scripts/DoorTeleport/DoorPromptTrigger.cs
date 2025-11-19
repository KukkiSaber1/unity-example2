using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DoorPromptTrigger : MonoBehaviour
{
    [SerializeField] private string teleportId;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private TextMeshProUGUI promptText; 

    private bool playerInside = false;
    private GameObject playerObj;

    private void Start()
    {
        // Hide the prompt at launch
        promptText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;
        playerObj = other.gameObject;
        promptText.text = "Press E";
        promptText.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
        promptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            TeleportManager.Instance.TeleportPlayerWithFade(playerObj, teleportId);
        }
    }
}
