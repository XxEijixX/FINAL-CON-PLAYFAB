using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Retry : MonoBehaviour
{
    public void TryAgain()
    {
        Time.timeScale = 1f;

        // Resetea el puntaje
        ScoreManager.ResetScore();

        // Recarga la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}