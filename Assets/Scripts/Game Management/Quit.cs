using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quit : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("¡Juego cerrado!"); // Mensaje en consola para cuando pruebes en editor
        Application.Quit();
    }
}

