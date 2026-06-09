using UnityEngine;

public class SkyRadar : MonoBehaviour
{
    public Transform playerPlane;
    public Transform targetLetter;
    public Transform targetHarakah;
    public float pingInterval = 3f;

    private float pingTimer;

    void Update()
    {
        pingTimer += Time.deltaTime;

        if (pingTimer >= pingInterval)
        {
            TriggerRadarPulse();
            pingTimer = 0;
        }
    }

    private void TriggerRadarPulse()
    {
        // Requirement: "PING" temporary reveal
        Debug.Log("[Radar Ping] Tracking targets...");
        
        // Logic to show arrows/icons pointing to targetLetter and targetHarakah
        Vector2 dirToLetter = (targetLetter.position - playerPlane.position).normalized;
        Debug.Log($"[Radar] Letter Direction: {dirToLetter}");
    }
}
