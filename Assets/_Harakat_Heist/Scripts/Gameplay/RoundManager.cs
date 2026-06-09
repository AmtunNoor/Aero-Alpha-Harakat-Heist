using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    [Header("Match Settings")]
    public int scoreToWin = 10;
    public float minSpawnDistance = 50f;

    [Header("Arabic Font Text Arrays")]
    // All 28 standard Arabic letters linked natively to match your imported font theme
    public string[] letters = { 
        "أ", "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر", "ز", "س", "ش", "ص", 
        "ض", "ط", "ظ", "ع", "غ", "ف", "ق", "ك", "ل", "م", "ن", "هـ", "و", "ي" 
    }; 
    
    // Core diacritics using native escape sequences for clean stack layout rendering
    public string[] harakat = { 
        "\u064E", // Fatha ( َ )
        "\u064F", // Damma ( ُ )
        "\u0650"  // Kasra ( ِ )
    };

    [Header("Current Round State")]
    public string currentLetter;
    public string currentHarakah;
    public string currentCombinedSyllable;
    
    private int player1Score = 0;
    private int player2Score = 0;

    void Start()
    {
        StartNewRound();
    }

    public void StartNewRound()
    {
        // Randomly pick a letter and a harakah (No sequential order constraint)
        currentLetter = letters[Random.Range(0, letters.Length)];
        currentHarakah = harakat[Random.Range(0, harakat.Length)];
        
        // Fuses the selected vowel marker directly over/under the chosen base letter character
        currentCombinedSyllable = currentLetter + currentHarakah;

        Debug.Log($"[Match Setup] New Target Assembled: {currentCombinedSyllable}");
        SpawnItems();
    }

    private void SpawnItems()
    {
        Vector2 letterPos = Random.insideUnitCircle * 100f;
        Vector2 harakahPos;

        // Requirement: Maintain minimum spacing boundary so things never cluster at spawn
        do {
            harakahPos = Random.insideUnitCircle * 100f;
        } while (Vector2.Distance(letterPos, harakahPos) < minSpawnDistance);

        Debug.Log($"[Spawned] Target letter '{currentLetter}' and marker '{currentHarakah}' placed dynamically in space.");
    }

    public void OnSyllableDelivered(string playerName)
    {
        Debug.Log($"[Voice Announcement Bridge] {playerName} successfully delivered: {currentCombinedSyllable}!");
        
        if (playerName == "Ahmed") player1Score++;
        else player2Score++;

        if (player1Score >= scoreToWin || player2Score >= scoreToWin)
        {
            Debug.Log("[Victory Screen] Target points achieved. Match finished.");
        }
        else
        {
            StartNewRound();
        }
    }
}
