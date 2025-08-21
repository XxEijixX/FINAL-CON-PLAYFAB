using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObstacleCollision : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverUI;
    public TextMeshProUGUI gameOverScoreText;
    public Button leaderboardButton; // Botón del leaderboard en el Game Over UI

    [Header("Puntaje")]
    public float detectionRadius = 3f;
    public int minPoints = 100;
    public int maxPoints = 200;

    [Header("Jugador")]
    public Transform player;
    public float jumpHeightThreshold = 1.2f;
    public float crouchHeightThreshold = 0.8f;

    private bool pointsGiven = false;
    private bool gameOverTriggered = false;
    private PlayfabHandler2 playfabHandler;

    void Start()
    {
        // Buscar el PlayfabHandler
        playfabHandler = ScoreManager.GetPlayfabHandler();
        if (playfabHandler == null)
        {
            playfabHandler = FindObjectOfType<PlayfabHandler2>();
        }

        // Configurar el botón del leaderboard si existe
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(ShowLeaderboard);
        }
    }

    void OnEnable()
    {
        pointsGiven = false;
        gameOverTriggered = false;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                Debug.Log("Jugador reasignado en OnEnable");
            }
            else
            {
                Debug.LogWarning("Jugador NO encontrado en OnEnable");
            }
        }
    }

    void Update()
    {
        if (player == null || gameOverTriggered) return;

        if (!pointsGiven && Vector3.Distance(transform.position, player.position) <= detectionRadius)
        {
            float playerHeight = player.position.y;
            if (playerHeight >= jumpHeightThreshold || playerHeight <= crouchHeightThreshold)
            {
                int points = Random.Range(minPoints, maxPoints + 1);
                ScoreManager.AddScore(points);
                pointsGiven = true;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !gameOverTriggered)
        {
            gameOverTriggered = true;

            // Pausar el juego
            Time.timeScale = 0f;

            // *** LÍNEA CLAVE: Enviar score a PlayFab automáticamente ***
            ScoreManager.EndGame();

            // Mostrar UI de Game Over
            if (gameOverUI != null)
                gameOverUI.SetActive(true);

            // Actualizar el puntaje final usando ScoreManager
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ShowFinalScore();

            // Alternativamente, si quieres reflejarlo directamente en TMP_Text
            if (gameOverScoreText != null)
                gameOverScoreText.text = "Puntaje final: " + ScoreManager.GetScore();

            Debug.Log("Game Over! Score automatically sent to PlayFab.");
        }
    }

    // Método que se llama cuando presionan el botón del leaderboard
    public void ShowLeaderboard()
    {
        if (playfabHandler != null)
        {
            Debug.Log("Showing leaderboard with final score...");

            // Asegurar que el PlayfabHandler tiene el score correcto
            playfabHandler.score = ScoreManager.GetScore();

            // Mostrar el leaderboard
            playfabHandler.GetLeaderboard();

            // Si tienes un panel específico del leaderboard, actívalo
            if (playfabHandler.leaderboardPanel != null)
            {
                playfabHandler.leaderboardPanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("PlayfabHandler not found! Cannot show leaderboard.");
        }
    }

    // Función para resetear el estado
    public void ResetObstacle()
    {
        pointsGiven = false;
        gameOverTriggered = false;
    }
}