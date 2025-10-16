using UnityEngine;
using System.Collections.Generic;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    [System.Serializable]
    public class TeleportPoint
    {
        public string id;
        public Transform target;
    }

    [SerializeField] private List<TeleportPoint> teleportPoints = new List<TeleportPoint>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TeleportPlayerWithFade(GameObject player, string targetId)
    {
        var point = teleportPoints.Find(p => p.id == targetId);
        if (point == null)
        {
            Debug.LogWarning($"Teleport ID '{targetId}' not found!");
            return;
        }

        // Fade out, teleport, fade in
        ScreenFader.Instance.FadeOut(() =>
        {
            var controller = player.GetComponent<CharacterController>();
            if (controller) controller.enabled = false;

            player.transform.SetPositionAndRotation(point.target.position, point.target.rotation);

            if (controller) controller.enabled = true;

            ScreenFader.Instance.FadeIn();
        });
    }
}
