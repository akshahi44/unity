using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using TMPro;

public class EggGrabDisappear : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public AudioSource eggSound; // Assign in Inspector
    public static int eggCount = 0; // Keeps track of collected eggs
    private static TextMeshProUGUI eggCountText; // UI text for egg count
    private static GameObject eggCountPanel; // Panel containing the egg count text
    private static TextMeshProUGUI gameResultText; // UI text for Win/Lose message
    private static GameObject gameResultPanel; // Panel for win/lose message
    private static int totalEggs; // Total eggs in the scene
    private static bool gameEnded = false; // Track if game has ended

    public float gameDuration = 50f; // Set game timer to 50 seconds
    private static float timer;
    private static bool timerStarted = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Find EggCountText and its parent panel only once
        if (eggCountText == null)
        {
            GameObject textObject = GameObject.Find("EggCountText");
            if (textObject != null)
            {
                eggCountText = textObject.GetComponent<TextMeshProUGUI>();
                eggCountPanel = textObject.transform.parent.gameObject; // Get the panel
                eggCountPanel.SetActive(false); // Hide UI initially
            }
        }

        // Find the GameResult UI panel and text
        if (gameResultText == null)
        {
            GameObject resultTextObject = GameObject.Find("GameResultText");
            if (resultTextObject != null)
            {
                gameResultText = resultTextObject.GetComponent<TextMeshProUGUI>();
                gameResultPanel = resultTextObject.transform.parent.gameObject;
                gameResultPanel.SetActive(false); // Hide initially
            }
        }

        // Count total eggs in the scene at the start
        totalEggs = GameObject.FindGameObjectsWithTag("Egg").Length;
        eggCount = 0; // Reset egg count
        timer = gameDuration; // Reset timer
        timerStarted = true; // Start the timer
        gameEnded = false; // Reset game status
    }

    private void Update()
    {
        if (timerStarted && !gameEnded)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EndGame(false); // Time's up, player loses
            }
        }
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (gameEnded) return; // Stop actions if the game has ended

        eggCount++; // Increase count

        if (eggSound != null)
        {
            eggSound.Play(); // Play egg sound when grabbed
        }

        if (eggCountText != null && eggCountPanel != null)
        {
            eggCountText.text = "Egg Collected: " + eggCount;
            PositionEggUI(); // Move UI in front of the player's view
            eggCountPanel.SetActive(true);
            StartCoroutine(HideMessage());
        }

        StartCoroutine(DestroyAfterGrab());

        // Check if all eggs are collected
        if (eggCount >= totalEggs)
        {
            EndGame(true); // Player wins
        }
    }

    // Moves the egg count UI in front of the player's view
    private void PositionEggUI()
    {
        if (Camera.main != null && eggCountPanel != null)
        {
            Transform camTransform = Camera.main.transform;
            eggCountPanel.transform.position = camTransform.position + camTransform.forward * 1.2f; // 1.2m in front of the player
            eggCountPanel.transform.rotation = Quaternion.LookRotation(camTransform.forward); // Always face the player
        }
    }

    private IEnumerator HideMessage()
    {
        yield return new WaitForSeconds(2f); // Show message for 2 seconds
        if (eggCountPanel != null)
        {
            eggCountPanel.SetActive(false); // Hide panel (text will hide too)
        }
    }

    private IEnumerator DestroyAfterGrab()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void EndGame(bool won)
    {
        gameEnded = true;
        timerStarted = false;

        if (gameResultText != null && gameResultPanel != null)
        {
            gameResultText.text = won ? "You Win!" : "Your time is out! You lose!";
            gameResultPanel.SetActive(true); // Show result message
            StartCoroutine(HideGameResult());
        }
    }

    private IEnumerator HideGameResult()
    {
        yield return new WaitForSeconds(5f); // Show message for 5 seconds
        if (gameResultPanel != null)
        {
            gameResultPanel.SetActive(false); // Hide message after 5 seconds
        }
    }
}
