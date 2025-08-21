using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("PlayFab Integration")]
    [SerializeField] private PlayfabHandler2 playfabHandler;
    [SerializeField] private bool autoSendToPlayFab = true;
    [SerializeField] private bool sendOnlyAtEndGame = true;

    private int score = 0;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Buscar PlayfabHandler si no está asignado
        if (playfabHandler == null)
        {
            playfabHandler = FindObjectOfType<PlayfabHandler2>();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Método para que HUDScore registre el TMP_Text dinámicamente si no se asignó en inspector
    public void AssignScoreText(TMP_Text text)
    {
        if (scoreText == null)
        {
            scoreText = text;
            UpdateScoreUI();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Solo buscar el scoreText si no está asignado
        if (scoreText == null)
        {
            GameObject obj = GameObject.Find("ScoreText");
            if (obj != null)
                scoreText = obj.GetComponent<TMP_Text>();
        }

        // Buscar PlayfabHandler en la nueva escena si es necesario
        if (playfabHandler == null)
        {
            playfabHandler = FindObjectOfType<PlayfabHandler2>();
        }

        UpdateScoreUI();
    }

    public static void AddScore(int amount)
    {
        if (Instance == null || Instance.gameEnded) return;
        Instance.score += amount;
        Instance.UpdateScoreUI();

        // Actualizar PlayfabHandler si está disponible
        Instance.UpdatePlayfabScore();
    }

    public static void ResetScore()
    {
        if (Instance == null) return;
        Instance.score = 0;
        Instance.gameEnded = false;
        Instance.UpdateScoreUI();
        Instance.UpdatePlayfabScore();
    }

    public static int GetScore()
    {
        return Instance != null ? Instance.score : 0;
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Puntaje: " + score;
    }

    public void ShowFinalScore()
    {
        if (finalScoreText != null)
            finalScoreText.text = "Puntaje final: " + score;
    }

    // Nueva función para actualizar PlayfabHandler
    private void UpdatePlayfabScore()
    {
        if (playfabHandler != null && autoSendToPlayFab)
        {
            // Actualizar el score acumulado en PlayfabHandler
            playfabHandler.score = score;

            // Solo enviar si no estamos configurados para enviar solo al final
            if (!sendOnlyAtEndGame)
            {
                playfabHandler.UpdateScore();
            }
        }
    }

    // Función para llamar al final del juego - AHORA ES AUTOMÁTICA
    public static void EndGame()
    {
        if (Instance == null || Instance.gameEnded) return;

        Instance.gameEnded = true;
        Debug.Log($"Game ended! Final Score: {Instance.score}");
        Instance.ShowFinalScore();

        // ENVIAR SCORE FINAL A PLAYFAB AUTOMÁTICAMENTE
        if (Instance.playfabHandler != null && Instance.autoSendToPlayFab)
        {
            // Actualizar el score en PlayfabHandler y enviarlo
            Instance.playfabHandler.score = Instance.score;
            Instance.playfabHandler.UpdateScore();
            Debug.Log("Score sent to PlayFab automatically!");
        }
    }

    // Función pública que pueden llamar otros scripts cuando detecten muerte
    public static void TriggerGameOver()
    {
        EndGame();
    }

    // Función para forzar envío a PlayFab
    [ContextMenu("Send Current Score to PlayFab")]
    public void ForceSendToPlayFab()
    {
        if (playfabHandler != null)
        {
            playfabHandler.score = score;
            playfabHandler.UpdateScore();
        }
    }

    // Método público para obtener el PlayfabHandler
    public static PlayfabHandler2 GetPlayfabHandler()
    {
        return Instance?.playfabHandler;
    }
}