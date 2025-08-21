//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using PlayFab;
//using PlayFab.ClientModels;
//using TMPro;
//using System;
//using UnityEngine.Events;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//public class PlayfabHandler : MonoBehaviour
//{
//    // CONFIGURACIÓN ORIGINAL
//    private string titleID = "1AA4AF"; // El titulo de playfab al que esta vinculado nuestro proyecto
//    private string developerKey = "P1Z14HFM18Q9P7QRNEF4CINX7MZGW1TSCK3KHOOAYUZOXOBGEH"; // Es como una licencia de desarrollador, que te permite acceder a las funcionalidades de PlayFab, como crear cuentas, autenticar usuarios, etc.

//    // ELEMENTOS DE REGISTRO ORIGINAL
//    [Header("Register Elements")]
//    [SerializeField] private TMP_InputField register_UsernameInputField;
//    [SerializeField] private TMP_InputField register_EmailInputField;
//    [SerializeField] private TMP_InputField register_PasswordInputField;
//    [SerializeField] private TMP_InputField register_ConfirmPasswordInputField;
//    [SerializeField] private UnityEvent onRegisterSuccess; // Evento que se ejecuta cuando el registro es exitoso

//    // ELEMENTOS DE LOGIN ORIGINAL
//    [Header("Login Elements")]
//    [SerializeField] private TMP_InputField login_UsernameInputField;
//    [SerializeField] private TMP_InputField login_PasswordInputField;
//    [SerializeField] private UnityEvent onLoginSuccess; // Evento que se ejecuta cuando el registro es exitoso

//    // ELEMENTOS DE PERFIL ORIGINAL
//    [Header("Profile Elements")]
//    [SerializeField] private Image userAvatarImage; // Esa es la imagen de el canvas que muestra tu foto de perfil
//    [SerializeField] private TMP_Text userDisplayNameText; // Este es tu nombre de usuario que se muestra en el canvas
//    [SerializeField] private string userDisplayName; // aqui se guarda tu nombre de usuario por codigo
//    [SerializeField] private string userAvatarUrl; // Aqui se guarda el url que lleva a tu imagen de perfil
//    [SerializeField] private Sprite defaultAvatarSprite; // Imagen por defecto si no hay avatar o falla la descarga

//    // SCORE ORIGINAL + NUEVAS FUNCIONALIDADES AGREGADAS
//    [Header("Score System")]
//    public int score; // Variable para guardar la puntuacion del jugador, se puede cambiar en el inspector

//    [Header("Auto Score System - NUEVAS FUNCIONALIDADES")]
//    public bool autoSendScore = true;
//    [Tooltip("Puntos por acción del jugador")]
//    public int pointsPerAction = 10;
//    [Tooltip("Score acumulado actual")]
//    public int accumulatedScore = 0;

//    private GamePauseManager pauseManager;
//    private Sprite userAvatarSprite; // Variable para guardar la imagen del avatar del jugador
//    private bool isDownloadingAvatar = false; // Flag para evitar descargas múltiples

//    void Start()
//    {
//        // CONFIGURACIÓN ORIGINAL
//        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
//        {
//            PlayFabSettings.TitleId = titleID; // Set the title ID for PlayFab
//        }

//        if (string.IsNullOrEmpty(PlayFabSettings.DeveloperSecretKey))
//        {
//            PlayFabSettings.DeveloperSecretKey = developerKey; // Set the developer key for PlayFab
//        }

//        pauseManager = FindObjectOfType<GamePauseManager>();

//        // Establecer imagen por defecto si está asignada
//        if (defaultAvatarSprite != null && userAvatarImage != null)
//        {
//            userAvatarImage.sprite = defaultAvatarSprite;
//        }
//    }

//    public void CreatePlayfabAccount()
//    {
//        //ESTE METODO ES EL QUE SE ASIGNA AL DE CREAR CUENTA
//        if (register_PasswordInputField.text != register_ConfirmPasswordInputField.text)
//        {
//            Debug.LogError("Passwords do not match!");
//            return; // Si no coinciden las contraseñas, no continuamos con el registro
//        }

//        // Esta variable request solo te guarda los datos de tu request, es decir, aun no ejecutas la solicitud a PlayFab, solo la preparas
//        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
//        {
//            DisplayName = register_UsernameInputField.text, // DisplayName es el nombre que se muestra en el juego
//            Username = register_UsernameInputField.text, // Username es el nombre que te sirve para iniciar sesion
//            Email = register_EmailInputField.text,
//            Password = register_PasswordInputField.text,
//            RequireBothUsernameAndEmail = true
//        };

//        // Aqui se ejecuta el request, ya se manda
//        //                          Solicitud   Lo que pasa si sale bien  Lo que pasa si sale mal
//        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, ErrorMessage);
//    }

//    // Este metodo es el que se va a ejecutar si el registro es exitoso
//    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
//    {
//        Debug.Log("User registered successfully!");
//        onRegisterSuccess.Invoke();

//        // MEJORA: Obtener automáticamente el perfil después del registro
//        GetPlayerProfile();
//    }

//    public void LoginUser()
//    {
//        LoginWithPlayFabRequest request = new LoginWithPlayFabRequest
//        {
//            Username = login_UsernameInputField.text, // En este caso usamos el email como nombre de usuario
//            Password = login_PasswordInputField.text, // La contraseña que el usuario ha introducido
//        };

//        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, ErrorMessage);
//    }

//    private void OnLoginSuccess(LoginResult result)
//    {
//        Debug.Log("User logged in successfully!");
//        onLoginSuccess.Invoke();

//        // NUEVA FUNCIONALIDAD AGREGADA
//        if (pauseManager != null)
//            pauseManager.ResumeGame();

//        // MEJORA: Obtener automáticamente el perfil después del login
//        GetPlayerProfile();
//    }

//    [ContextMenu("Get Player Profile")]
//    public void GetPlayerProfile()
//    {
//        GetPlayerProfileRequest request = new GetPlayerProfileRequest // La solicitud es decir que datos quieres conseguir
//        {
//            ProfileConstraints = new PlayerProfileViewConstraints
//            {
//                ShowDisplayName = true, // Mostrar el nombre de usuario             
//                ShowAvatarUrl = true // Mostrar la URL del avatar del jugador
//            },
//        };

//        PlayFabClientAPI.GetPlayerProfile(request, OnGetPlayerProfileSuccess, ErrorMessage); // Aqui se ejecuta la solicitud
//    }

//    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
//    {
//        // Actualizar nombre de usuario
//        userDisplayName = result.PlayerProfile.DisplayName;
//        if (userDisplayNameText != null)
//        {
//            userDisplayNameText.text = userDisplayName;
//        }

//        // Actualizar avatar si existe URL
//        userAvatarUrl = result.PlayerProfile.AvatarUrl;

//        if (!string.IsNullOrEmpty(userAvatarUrl))
//        {
//            Debug.Log($"Avatar URL found: {userAvatarUrl}");
//            StartCoroutine(RetrievePlayerAvatar());
//        }
//        else
//        {
//            Debug.Log("No avatar URL found for this player");
//            // Mantener la imagen por defecto si no hay URL de avatar
//        }
//    }

//    IEnumerator RetrievePlayerAvatar()
//    {
//        // Evitar descargas múltiples simultáneas
//        if (isDownloadingAvatar)
//        {
//            yield break;
//        }

//        isDownloadingAvatar = true;
//        Debug.Log("Starting avatar download...");

//        UnityWebRequest request = UnityWebRequestTexture.GetTexture(userAvatarUrl); // Estoy creando una solicitud a la web, esta solicitud es especificamente para conseguir una imagen
//        request.timeout = 10; // Timeout de 10 segundos

//        yield return request.SendWebRequest(); // Esta linea envia la solicitud a la web y espera a que se complete

//        isDownloadingAvatar = false;

//        if (request.result != UnityWebRequest.Result.Success) // Si la solicitud no se pudo hacer
//        {
//            Debug.LogError($"Failed to download avatar: {request.error}");
//            // Mantener la imagen por defecto en caso de error
//        }
//        else // Si la solicitud fue exitosa, ya conseguir la textura, es decir la imagen
//        {
//            try
//            {
//                // Tengo un componente que maneja la descarga de texturas
//                DownloadHandlerTexture downloadHandler = request.downloadHandler as DownloadHandlerTexture; // Esta linea me guarda la imagen que consegui en una variable

//                if (downloadHandler?.texture != null)
//                {
//                    // Crear el sprite desde la textura descargada
//                    userAvatarSprite = Sprite.Create(
//                        downloadHandler.texture,
//                        new Rect(0.0f, 0.0f, downloadHandler.texture.width, downloadHandler.texture.height),
//                        new Vector2(0.5f, 0.5f) // Centro del sprite
//                    );

//                    // Asignar la imagen al componente UI
//                    if (userAvatarImage != null)
//                    {
//                        userAvatarImage.sprite = userAvatarSprite;
//                        Debug.Log("Avatar image successfully loaded and displayed!");
//                    }
//                    else
//                    {
//                        Debug.LogError("userAvatarImage is not assigned in the inspector!");
//                    }
//                }
//                else
//                {
//                    Debug.LogError("Downloaded texture is null");
//                }
//            }
//            catch (System.Exception ex)
//            {
//                Debug.LogError($"Error processing avatar image: {ex.Message}");
//            }
//        }

//        request.Dispose(); // Liberar recursos
//    }

//    // NUEVA FUNCIÓN: Para actualizar el avatar manualmente
//    [ContextMenu("Refresh Avatar")]
//    public void RefreshAvatar()
//    {
//        if (!string.IsNullOrEmpty(userAvatarUrl))
//        {
//            StopAllCoroutines(); // Detener cualquier descarga anterior
//            isDownloadingAvatar = false;
//            StartCoroutine(RetrievePlayerAvatar());
//        }
//        else
//        {
//            Debug.Log("No avatar URL available. Getting player profile first...");
//            GetPlayerProfile();
//        }
//    }

//    // NUEVA FUNCIÓN: Para resetear al avatar por defecto
//    [ContextMenu("Reset to Default Avatar")]
//    public void ResetToDefaultAvatar()
//    {
//        if (defaultAvatarSprite != null && userAvatarImage != null)
//        {
//            userAvatarImage.sprite = defaultAvatarSprite;
//            Debug.Log("Avatar reset to default");
//        }
//    }

//    [ContextMenu("Update Score")]
//    private void UpdateScore()
//    {
//        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest
//        {
//            Statistics = new List<StatisticUpdate>
//           {
//                new StatisticUpdate
//                {
//                    StatisticName = "Score", // Nombre de la tabla, o statistic que pusiste al crear la tabla
//                    Value = score
//                }
//           }
//        };
//        PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdateStatisticsSuccess, ErrorMessage);
//    }

//    private void OnUpdateStatisticsSuccess(UpdatePlayerStatisticsResult result)
//    {
//        Debug.Log("Player statistics updated successfully!");
//    }

//    [ContextMenu("Get Leaderboard")]
//    public void GetLeaderboard()
//    {
//        GetLeaderboardRequest request = new GetLeaderboardRequest
//        {
//            StatisticName = "Score", // Nombre de la tabla, o statistic que pusiste al crear la tabla
//            StartPosition = 0, // Posicion inicial del leaderboard
//            MaxResultsCount = 10 // Numero de resultados que quieres obtener
//        };
//        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, ErrorMessage);
//    }

//    private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
//    {
//        Debug.Log("Leaderboard retrieved successfully!");

//        // MEJORADA: Ahora muestra mensaje si no hay resultados
//        if (result.Leaderboard.Count == 0)
//        {
//            Debug.Log("No scores found in leaderboard. Make sure to send a score first!");
//            return;
//        }

//        foreach (PlayerLeaderboardEntry user in result.Leaderboard) // el for each nos sirve para revisar todos los elementos de la lista
//        {
//            Debug.Log($"Player: {user.DisplayName}, Score: {user.StatValue}, Position: {user.Position}");
//        }
//    }

//    [ContextMenu("Send Score to Leaderboard")]
//    public void SendScoreToLeaderboard()
//    {
//        var request = new UpdatePlayerStatisticsRequest
//        {
//            Statistics = new List<StatisticUpdate>
//            {
//                new StatisticUpdate
//                {
//                    StatisticName = "Score",
//                    Value = accumulatedScore > 0 ? accumulatedScore : score
//                }
//            }
//        };
//        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreUpdateSuccess, ErrorMessage);
//    }

//    // Función sobrecargada para enviar una puntuación específica
//    public void SendScoreToLeaderboard(int scoreToSend)
//    {
//        var request = new UpdatePlayerStatisticsRequest
//        {
//            Statistics = new List<StatisticUpdate>
//            {
//                new StatisticUpdate
//                {
//                    StatisticName = "Score",
//                    Value = scoreToSend
//                }
//            }
//        };
//        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreUpdateSuccess, ErrorMessage);
//    }

//    private void OnScoreUpdateSuccess(UpdatePlayerStatisticsResult result)
//    {
//        Debug.Log("Score sent to leaderboard successfully!");
//        // Opcional: obtener el leaderboard actualizado después de enviar
//        GetLeaderboard();
//    }

//    // Función para actualizar la puntuación y enviarla automáticamente
//    public void UpdateScoreAndSend(int newScore)
//    {
//        score = newScore;
//        SendScoreToLeaderboard(newScore);
//    }

//    // SISTEMA AUTOMÁTICO DE PUNTUACIÓN
//    [ContextMenu("Add Points")]
//    public void AddPoints()
//    {
//        AddPoints(pointsPerAction);
//    }

//    public void AddPoints(int points)
//    {
//        accumulatedScore += points;
//        Debug.Log($"Points added: {points}. Total accumulated: {accumulatedScore}");

//        if (autoSendScore)
//        {
//            SendScoreToLeaderboard(accumulatedScore);
//        }
//    }

//    [ContextMenu("Reset Accumulated Score")]
//    public void ResetAccumulatedScore()
//    {
//        accumulatedScore = 0;
//        Debug.Log("Accumulated score reset to 0");
//    }

//    // Función para establecer el score acumulado directamente
//    public void SetAccumulatedScore(int newScore)
//    {
//        accumulatedScore = newScore;
//        Debug.Log($"Accumulated score set to: {accumulatedScore}");

//        if (autoSendScore)
//        {
//            SendScoreToLeaderboard(accumulatedScore);
//        }
//    }

//    // Función que se puede llamar al final de una partida
//    public void EndGame()
//    {
//        Debug.Log($"Game ended! Final score: {accumulatedScore}");
//        SendScoreToLeaderboard(accumulatedScore);
//    }

//    private void ErrorMessage(PlayFabError error)
//    {
//        Debug.LogError($"{error.Error} \n {error.ErrorMessage} \n {error.ErrorDetails}");
//    }
//}

