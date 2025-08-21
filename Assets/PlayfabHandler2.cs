using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayfabHandler2 : MonoBehaviour
{
    #region VARIABLES
    private string titleID = "1AA4AF"; // El titulo de playfab al que esta vinculado nuestro proyecto

    [Header("Register Elements")]
    [SerializeField] private TMP_InputField register_UsernameInputField;
    [SerializeField] private TMP_InputField register_EmailInputField;
    [SerializeField] private TMP_InputField register_PasswordInputField;
    [SerializeField] private TMP_InputField register_ConfirmPasswordInputField;
    [SerializeField] private UnityEvent onRegisterSuccess; // Evento que se ejecuta cuando el registro es exitoso

    [Header("Login Elements")]
    [SerializeField] private TMP_InputField login_UsernameInputField;
    [SerializeField] private TMP_InputField login_PasswordInputField;
    [SerializeField] private UnityEvent onLoginSuccess; // Evento que se ejecuta cuando el registro es exitoso

    [Header("Player Profile Elements")]
    [SerializeField] private Image userAvatarImage; // Esa es la imagen de el canvas que muestra tu foto de perfil
    [SerializeField] private TMP_Text userDisplayNameText; // Este es tu nombre de usuario que se muestra en el canvas
    [SerializeField] private string userDisplayName; // aqui se guarda tu nombre de usuario por codigo
    [SerializeField] private string userAvatarUrl; // Aqui se guarda el url que lleva a tu imagen de perfil

    [Header("Leaderboard")]
    [SerializeField] private GameObject userScorePrefab; // Prefab de la tabla de clasificación
    [SerializeField] private Transform content; // Padre de los elementos de la tabla de clasificación

    [Header("Death System")]
    public GameObject leaderboardPanel; // Panel del leaderboard (opcional)



    public int score; // Variable para guardar la puntuacion del jugador, se puede cambiar en el inspector

    #endregion VARIABLES
    void Start()
    {
        Time.timeScale = 0f; // Pausar el tiempo al inicio del jueg
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = titleID;
        }
    }

    public void Tiemporeaunudado()
    {
        Time.timeScale = 1f; // Reanudar el tiempo al iniciar el juego
        GetPlayerProfile(); // Obtener el perfil del jugador al iniciar el juego
    }

    public void CreatePlayfabAccount()
    {
        //ESTE METODO ES EL QUE SE ASIGNA AL DE CREAR CUENTA
        if (register_PasswordInputField.text != register_ConfirmPasswordInputField.text)
        {
            Debug.LogError("Passwords do not match!");
            return; // Si no coinciden las contraseñas, no continuamos con el registro
        }

        // Validaciones adicionales
        if (string.IsNullOrEmpty(register_UsernameInputField.text))
        {
            Debug.LogError("Username cannot be empty!");
            return;
        }

        if (string.IsNullOrEmpty(register_EmailInputField.text))
        {
            Debug.LogError("Email cannot be empty!");
            return;
        }

        if (register_PasswordInputField.text.Length < 6)
        {
            Debug.LogError("Password must be at least 6 characters long!");
            return;
        }

        // Esta variable request solo te guarda los datos de tu request, es decir, aun no ejecutas la solicitud a PlayFab, solo la preparas
        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
        {
            DisplayName = register_UsernameInputField.text, // DisplayName es el nombre que se muestra en el juego
            Username = register_UsernameInputField.text, // Username es el nombre que te sirve para iniciar sesion
            Email = register_EmailInputField.text,
            Password = register_PasswordInputField.text,
            RequireBothUsernameAndEmail = true
        };

        // Aqui se ejecuta el request, ya se manda
        //                          Solicitud   Lo que pasa si sale bien  Lo que pasa si sale mal
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, ErrorMessage);
    }

    // Este metodo es el que se va a ejecutar si el registro es exitoso
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("User registered successfully!");
        onRegisterSuccess.Invoke();
    }

    // Este metodo se pone en el boton de iniciar sesion en el canvas
    public void LoginUser()
    {
        // Validaciones
        if (string.IsNullOrEmpty(login_UsernameInputField.text))
        {
            Debug.Log("Username cannot be empty!");
            return;
        }

        if (string.IsNullOrEmpty(login_PasswordInputField.text))
        {
            Debug.Log("Password cannot be empty!");
            return;
        }

        LoginWithPlayFabRequest request = new LoginWithPlayFabRequest
        {
            Username = login_UsernameInputField.text, // En este caso usamos el username
            Password = login_PasswordInputField.text, // La contraseña que el usuario ha introducido
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, ErrorMessage);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("User logged in successfully!");
        onLoginSuccess.Invoke();
    }

    [ContextMenu("Get Player Profile")]
    public void GetPlayerProfile()
    {
        GetPlayerProfileRequest request = new GetPlayerProfileRequest // La solicitud es decir que datos quieres conseguir
        {
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true, // Mostrar el nombre de usuario             
                ShowAvatarUrl = true // Mostrar la URL del avatar del jugador
            },
        };

        PlayFabClientAPI.GetPlayerProfile(request, OnGetPlayerProfileSuccess, ErrorMessage); // Aqui se ejecuta la solicitud
    }

    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
    {
        userDisplayName = result.PlayerProfile.DisplayName;
        userAvatarUrl = result.PlayerProfile.AvatarUrl;

        userDisplayNameText.text = userDisplayName;

        // Solo intentar cargar avatar si hay URL
        if (!string.IsNullOrEmpty(userAvatarUrl))
        {
            StartCoroutine(RetrievePlayerAvatar()); // Inicia la corrutina para descargar la imagen del avatar del jugador
        }
    }

    IEnumerator RetrievePlayerAvatar()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(userAvatarUrl); // Estoy creando una solicitud a la web, esta solicitud es especificamente para conseguir una imagen

        yield return request.SendWebRequest(); // Esta linea envia la solicitud a la web y espera a que se complete

        if (request.result != UnityWebRequest.Result.Success) // Si la solicitud no se pudo hacer
        {
            Debug.LogError("Failed to load avatar: " + request.error);
        }
        else // Si la solicitud fue exitosa, ya conseguir la textura, es decir la imagen
        {
            // Tengo un componente que maneja la descarga de texturas
            DownloadHandlerTexture downloadHandler = request.downloadHandler as DownloadHandlerTexture; // Esta linea me guarda la imagen que consegui en una variable

            if (downloadHandler != null && downloadHandler.texture != null)
            {
                Sprite playerImage = Sprite.Create(downloadHandler.texture, new Rect(0.0f, 0.0f, downloadHandler.texture.width, downloadHandler.texture.height), Vector2.zero);
                userAvatarImage.sprite = playerImage; // Asigno la imagen que consegui al componente de la UI
            }
        }

        request.Dispose(); // Liberar recursos
    }

    // MÉTODO PARA ACTUALIZAR SCORE
    [ContextMenu("Update Score")]
    public void UpdateScore()
    {
        Debug.Log($"Updating score to: {score}");

        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogError("Cannot update score: Player is not logged in!");
            return;
        }

        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
           {
                new StatisticUpdate
                {
                    StatisticName = "Score",
                    Value = score
                }
           }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, OnUpdateStatisticsSuccess, OnUpdateStatisticsError);
    }

    private void OnUpdateStatisticsSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log($"Player statistics updated successfully! Score: {score}");

        // Verificar que se actualizó correctamente
        GetPlayerStatistics();
    }

    private void OnUpdateStatisticsError(PlayFabError error)
    {
        Debug.LogError($"Failed to update statistics: {error.ErrorMessage}");
        Debug.LogError($"Error details: {error.ErrorDetails}");
    }

    // MÉTODO PARA OBTENER LEADERBOARD MEJORADO
    [ContextMenu("Get Leaderboard")]
    public void GetLeaderboard()
    {
        Debug.Log("Starting GetLeaderboard request...");

        GetLeaderboardRequest request = new GetLeaderboardRequest
        {
            StatisticName = "Score", // Nombre de la tabla, o statistic que pusiste al crear la tabla
            StartPosition = 0, // Posicion inicial del leaderboard
            MaxResultsCount = 10, // Numero de resultados que quieres obtener
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true
            }
        };

        Debug.Log($"Request configured: StatisticName={request.StatisticName}, MaxResults={request.MaxResultsCount}");

        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, OnGetLeaderboardError);
    }

    // Método para establecer el score final y mostrar leaderboard
    public void SetFinalScore(int finalScore)
    {
        score = finalScore;
        Debug.Log($"Final score set to: {finalScore}");

        // Actualizar score en PlayFab y luego mostrar leaderboard
        UpdateScore();
        StartCoroutine(ShowLeaderboardAfterScoreUpdate());
    }

    // Corrutina para mostrar leaderboard después de actualizar score
    private IEnumerator ShowLeaderboardAfterScoreUpdate()
    {
        Debug.Log("Waiting for score update...");

        // Esperar un poco para asegurar que el score se actualizó en el servidor
        yield return new WaitForSeconds(2f);

        Debug.Log("Requesting leaderboard...");

        // Obtener y mostrar el leaderboard
        GetLeaderboard();
    }

    private void CreateSinglePlayerEntry()
    {
        Debug.Log("Creating single player entry...");

        GameObject userScoreObject = Instantiate(userScorePrefab, content);
        userScoreObject.SetActive(true);

        Transform nameTransform = userScoreObject.transform.Find("Name");
        Transform scoreTransform = userScoreObject.transform.Find("Score");

        if (nameTransform != null)
        {
            TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = $"1. {userDisplayName ?? "You"}";
                nameText.color = Color.yellow;
            }
        }

        if (scoreTransform != null)
        {
            TextMeshProUGUI scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
            if (scoreText != null)
            {
                scoreText.text = score.ToString(); // Aquí se usa la variable 'score' que contiene el finalScore
                scoreText.color = Color.yellow;
            }
        }

        Debug.Log($"Single entry created for {userDisplayName ?? "You"} with score {score}");
    }
    // Método específico para errores del leaderboard
    private void OnGetLeaderboardError(PlayFabError error)
    {
        Debug.LogError($"Leaderboard Error: {error.Error}");
        Debug.LogError($"Message: {error.ErrorMessage}");
        Debug.LogError($"Details: {error.ErrorDetails}");
    }

    // Método para mostrar el leaderboard con prefabs
    private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
    {
        Debug.Log($"Leaderboard retrieved successfully! Entries: {result.Leaderboard.Count}");

        // Verificar referencias
        if (userScorePrefab == null)
        {
            Debug.LogError("userScorePrefab is NULL!");
            return;
        }

        if (content == null)
        {
            Debug.LogError("content Transform is NULL!");
            return;
        }

        // Limpiar leaderboard anterior
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("Previous entries cleared.");

        // Verificar si hay entries
        if (result.Leaderboard == null || result.Leaderboard.Count == 0)
        {
            Debug.LogWarning("Leaderboard is empty or null!");

            // Crear entrada solo para el jugador actual
            CreateSinglePlayerEntry();
            return;
        }

        // Crear prefabs para cada entrada del leaderboard
        int position = 1;
        foreach (PlayerLeaderboardEntry user in result.Leaderboard)
        {
            Debug.Log($"Processing user: {user.DisplayName} - Score: {user.StatValue} - Position: {user.Position}");

            GameObject userScoreObject = Instantiate(userScorePrefab, content);
            userScoreObject.SetActive(true);

            // Buscar componentes
            Transform nameTransform = userScoreObject.transform.Find("Name");
            Transform scoreTransform = userScoreObject.transform.Find("Score");
            Transform avatarTransform = userScoreObject.transform.Find("Avatar Image");

            // Configurar nombre
            if (nameTransform != null)
            {
                TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = $"{position}. {user.DisplayName}";
                }
            }

            // Configurar score
            if (scoreTransform != null)
            {
                TextMeshProUGUI scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
                if (scoreText != null)
                {
                    scoreText.text = user.StatValue.ToString();
                }
            }

            // Configurar avatar (si existe)
            if (avatarTransform != null && user.Profile != null && !string.IsNullOrEmpty(user.Profile.AvatarUrl))
            {
                Image avatarImage = avatarTransform.GetComponent<Image>();
                if (avatarImage != null)
                {
                    StartCoroutine(LoadUserAvatar(avatarImage, user.Profile.AvatarUrl));
                }
            }

            // Resaltar jugador actual
            if (user.DisplayName == userDisplayName)
            {
                Debug.Log($"Highlighting current player: {user.DisplayName}");

                if (nameTransform != null)
                {
                    TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                    if (nameText != null) nameText.color = Color.yellow;
                }
                if (scoreTransform != null)
                {
                    TextMeshProUGUI scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
                    if (scoreText != null) scoreText.color = Color.yellow;
                }
            }

            position++;
        }

        Debug.Log($"Leaderboard creation completed. Total entries: {content.childCount}");
    }

    // Crear entrada solo para el jugador actual (cuando no hay otros en el leaderboard)

    // Corrutina para cargar avatar de usuario en el leaderboard
    private IEnumerator LoadUserAvatar(Image avatarImage, string avatarUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(avatarUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            DownloadHandlerTexture downloadHandler = request.downloadHandler as DownloadHandlerTexture;
            if (downloadHandler != null && downloadHandler.texture != null)
            {
                Sprite avatarSprite = Sprite.Create(downloadHandler.texture,
                    new Rect(0.0f, 0.0f, downloadHandler.texture.width, downloadHandler.texture.height),
                    Vector2.zero);
                avatarImage.sprite = avatarSprite;
            }
        }
        else
        {
            Debug.LogWarning($"Failed to load avatar from {avatarUrl}: {request.error}");
        }

        request.Dispose();
    }

    public void GetPlayerStatistics()
    {
        GetPlayerStatisticsRequest request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { "Score" }
        };

        PlayFabClientAPI.GetPlayerStatistics(request, OnGetPlayerStatsSuccess, OnGetPlayerStatsError);
    }

    private void OnGetPlayerStatsSuccess(GetPlayerStatisticsResult result)
    {
        Debug.Log("Player statistics retrieved:");

        if (result.Statistics == null || result.Statistics.Count == 0)
        {
            Debug.LogWarning("No statistics found for this player!");
            return;
        }

        foreach (var stat in result.Statistics)
        {
            Debug.Log($"Statistic: {stat.StatisticName} = {stat.Value}");
        }
    }

    private void OnGetPlayerStatsError(PlayFabError error)
    {
        Debug.LogError($"Failed to get player statistics: {error.ErrorMessage}");
    }


    // Este metodo nos va a servir para todos los errores que nos puedan ocurrir al hacer solicitudes a PlayFab
    private void ErrorMessage(PlayFabError error)
    {
        Debug.Log($"PlayFab Error: {error.Error}\nMessage: {error.ErrorMessage}\nDetails: {error.ErrorDetails}");
    }
}