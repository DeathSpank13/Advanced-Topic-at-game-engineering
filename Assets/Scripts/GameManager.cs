using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // REQUIRED for reloading the game
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameDuration = 90f;
    public int itemsToCollect = 5;
    private float timeElapsed = 0f; // Tracks how fast the player won

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject inGameUIPanel;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("UI Elements")]
    public Text timerText;
    public Text shoppingListText;
    public List<string> collectedItems = new List<string>(); //
    public Text winTimeText; // Displays the final time

    [Header("Script References")]
    public ItemSpawner spawner;

    public List<string> currentShoppingList = new List<string>();
    private bool isGameActive = false;

    void Start()
    {
        SwitchToMenu(mainMenuPanel);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!isGameActive) return;

        gameDuration -= Time.deltaTime;
        timeElapsed += Time.deltaTime; // Count up for the win screen
        UpdateTimerUI();

        if (gameDuration <= 0)
        {
            gameDuration = 0;
            EndGame(false);
        }
    }

    // ==========================================
    // BUTTON METHODS (Link these in the Inspector)
    // ==========================================

    public void StartGame()
    {
        SwitchToMenu(inGameUIPanel);
        Time.timeScale = 1f; // Unfreeze the game

        // Generate the list right as the game starts
        if (currentShoppingList.Count == 0)
        {
            GenerateList();
        }

        isGameActive = true;

        // Optional: Hide the cursor so the player can look around
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Crucial: Unfreeze time before reloading!
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    // ==========================================
    // GAME LOGIC
    // ==========================================

    void SwitchToMenu(GameObject activeMenu)
    {
        // Turn them all off first
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (inGameUIPanel) inGameUIPanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        // Turn on the requested one
        if (activeMenu) activeMenu.SetActive(true);
    }

    public void EndGame(bool won)
    {
        isGameActive = false;
        Time.timeScale = 0f; // Freeze the game

        // Unlock cursor so they can click the Retry button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (won)
        {
            SwitchToMenu(winPanel);
            if (winTimeText != null)
            {
                // Formats the float to show 2 decimal places (e.g., "45.23 seconds")
                winTimeText.text = "Checkout Complete!\nTime: " + timeElapsed.ToString("F2") + " seconds";
            }
        }
        else
        {
            SwitchToMenu(losePanel);
        }
    }

    // Called by the Cart when an item drops in
    public void ItemCollected(string itemName)
    {
        collectedItems.Add(itemName);
        UpdateListUI();
    }

    // Called by the Cart if the player takes an item out or throws it
    public void ItemRemoved(string itemName)
    {
        if (collectedItems.Contains(itemName))
        {
            collectedItems.Remove(itemName);
            UpdateListUI();
        }
    }

    // --- Keeping your existing UI/List generation logic intact below ---

    void GenerateList()
    {
        ItemInfo[] spawnedItems = FindObjectsByType<ItemInfo>(FindObjectsSortMode.None);

        if (spawnedItems.Length < itemsToCollect) return;

        for (int i = 0; i < spawnedItems.Length; i++)
        {
            ItemInfo temp = spawnedItems[i];
            int randomIndex = Random.Range(i, spawnedItems.Length);
            spawnedItems[i] = spawnedItems[randomIndex];
            spawnedItems[randomIndex] = temp;
        }

        currentShoppingList.Clear();
        int addedItems = 0;

        foreach (ItemInfo item in spawnedItems)
        {
            if (!currentShoppingList.Contains(item.itemName))
            {
                currentShoppingList.Add(item.itemName);
                addedItems++;
            }
            if (addedItems >= itemsToCollect) break;
        }

        UpdateListUI();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.FloorToInt(gameDuration % 60);
            int minutes = Mathf.FloorToInt(gameDuration / 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdateListUI()
    {
        if (shoppingListText != null)
        {
            shoppingListText.text = "Shopping List:\n\n";
            int matchedItems = 0;

            foreach (string item in currentShoppingList)
            {
                // If the cart contains this specific item from the list
                if (collectedItems.Contains(item))
                {
                    // Draw it Green with a Checkmark
                    shoppingListText.text += "<color=#00FF00>✓ " + item + "</color>\n";
                    matchedItems++;
                }
                else
                {
                    // Draw it White with an empty box
                    shoppingListText.text += "<color=#FFFFFF>☐ " + item + "</color>\n";
                }
            }

            // AUTO-CHECKOUT: If we matched all items, instantly win the game!
            if (matchedItems >= itemsToCollect && isGameActive)
            {
                EndGame(true);
            }
        }
    }
}