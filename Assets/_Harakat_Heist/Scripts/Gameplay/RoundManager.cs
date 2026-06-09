using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    [Header("Match Settings")]
    public int scoreToWin = 10;
    public float minSpawnDistance = 50f; // Requirement: Items must spawn far apart

    [Header("Assets (Reference via Inspector)")]
    public string[] letters = { "Alif", "Baa", "Taa", "Thaa" }; // Existing asset names
    public string[] harakat = { "Fatha", "Damma", "Kasra" };

    [Header("Current Round State")]
    public string currentLetter;
    public string currentHarakah;
    
    private int player1Score = 0;
    private int player2Score = 0;

    void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        // 1. Randomize Letter and Harakah (No sequential order)
        currentLetter = letters[Random.Range(0, letters.Length)];
        currentHarakah = harakat[Random.Range(0, harakat.Length)];

        Debug.Log($"[Match Announcement] New Round: Collect {currentLetter} + {currentHarakah}");
        
        SpawnItems();
    }

    private void SpawnItems()
    {
        // 2. Spawn logic with Minimum Separation Requirement
        Vector2 letterPos = Random.insideUnitCircle * 100f;
        Vector2 harakahPos;

        // Ensure Harakah is far enough from Letter
        do {
            harakahPos = Random.insideUnitCircle * 100f;
        } while (Vector2.Distance(letterPos, harakahPos) < minSpawnDistance);

        // Place items in the 360 world
        Debug.Log($"[Spawn] {currentLetter} spawned at {letterPos}. {currentHarakah} spawned at {harakahPos}.");
    }

    // Called by the PlaneHeistController when it delivers to a portal
    public void OnSyllableDelivered(string playerName)
    {
        Debug.Log($"[Voice Announcement] {playerName} delivered {currentLetter} {currentHarakah}!");
        
        // Update Score
        if (playerName == "Ahmed") player1Score++;
        else player2Score++;

        CheckVictory();
    }

    private void CheckVictory()
    {
        if (player1Score >= scoreToWin || player2Score >= scoreToWin)
        {
            Debug.Log("[Victory] Match Finished. Displaying Victory Screen.");
        }
        else
        {
            StartNewRound();
        }
    }
}
