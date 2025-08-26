using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject gameOverUI;

    [Header("Objects to Activate on Resume")]
    public GameObject[] objectsToActivate;

    [Header("Leaderboard Panel")]
    public GameObject leaderboardPanel;

    private PlayfabHandler2 playfabHandler;

    private void Start()
    {
        playfabHandler = FindObjectOfType<PlayfabHandler2>();
        Time.timeScale = 0f;

        if (PlayFab.PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.Log("Player is already logged in, skipping login UI.");
            ResumeGame();
            playfabHandler.GetPlayerProfile();
        }
        else
        {
            ShowLogin();
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    public void ShowLogin()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void ShowRegister()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    public void ShowLeaderboardFromGameOver()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        playfabHandler.GetLeaderboard();
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
    }
}