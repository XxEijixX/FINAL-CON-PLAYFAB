using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDScore : MonoBehaviour
{
    void Awake()
    {
        TMP_Text scoreText = GetComponent<TMP_Text>();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AssignScoreText(scoreText);
            // Fuerza la actualizaci�n inmediata del puntaje
            ScoreManager.Instance.UpdateScoreUI();
        }
    }
}
