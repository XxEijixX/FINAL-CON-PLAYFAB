using UnityEngine;

public class GamePauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;      // Panel de login
    public GameObject registerPanel;   // Panel de registro
    public GameObject gameOverUI;      // Panel "You Lost"

    [Header("Objects to Activate on Resume")]
    public GameObject[] objectsToActivate; // Objetos que se activan al reanudar

    [Header("Leaderboard Panel")]
    public GameObject leaderboardPanel; // Panel de leaderboard

    private PlayfabHandler2 playfabHandler; // Referencia al PlayfabHandler

    private void Start()
    {
        playfabHandler = FindObjectOfType<PlayfabHandler2>();
        // Pausa el juego al iniciar
        Time.timeScale = 0f;

        ShowLogin(); // Arrancamos mostrando login
    }

    public void ResumeGame()
    {
        // Reanuda el juego
        Time.timeScale = 1f;

        // Oculta todos los paneles de UI
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

        // Activa los objetos asignados en el inspector
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    // Mostrar Login 
    public void ShowLogin()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    // Mostrar Registro 
    public void ShowRegister()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    // Mostrar Leaderboard desde cualquier lugar
    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    // Mostrar Leaderboard desde Game Over
    public void ShowLeaderboardFromGameOver()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        playfabHandler.GetLeaderboard(); // Cargar el leaderboard al mostrarlo
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    // Volver al Game Over desde Leaderboard
    public void ShowGameOver()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
    }
}