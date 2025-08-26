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
    [SerializeField] private UnityEvent onRegisterSuccess;

    [Header("Login Elements")]
    [SerializeField] private TMP_InputField login_UsernameInputField;
    [SerializeField] private TMP_InputField login_PasswordInputField;
    [SerializeField] private UnityEvent onLoginSuccess;

    [Header("Player Profile Elements")]
    [SerializeField] private Image userAvatarImage;
    [SerializeField] private TMP_Text userDisplayNameText;
    [SerializeField] private string userDisplayName;
    [SerializeField] private string userAvatarUrl;

    [Header("Leaderboard")]
    [SerializeField] private GameObject userScorePrefab;
    [SerializeField] private Transform content;

    [Header("Death System")]
    public GameObject leaderboardPanel;

    public int score;
    #endregion VARIABLES

    void Awake()
    {
        // Mantener una sola instancia en todas las escenas
        if (FindObjectsOfType<PlayfabHandler2>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Time.timeScale = 0f;
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = titleID;
        }
    }

    public void Tiemporeaunudado()
    {
        Time.timeScale = 1f;
        GetPlayerProfile();
    }

    public void CreatePlayfabAccount()
    {
        if (register_PasswordInputField.text != register_ConfirmPasswordInputField.text)
        {
            Debug.LogError("Passwords do not match!");
            return;
        }

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

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
        {
            DisplayName = register_UsernameInputField.text,
            Username = register_UsernameInputField.text,
            Email = register_EmailInputField.text,
            Password = register_PasswordInputField.text,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, ErrorMessage);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("User registered successfully!");
        onRegisterSuccess.Invoke();
    }

    public void LoginUser()
    {
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
            Username = login_UsernameInputField.text,
            Password = login_PasswordInputField.text,
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
        GetPlayerProfileRequest request = new GetPlayerProfileRequest
        {
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true
            },
        };

        PlayFabClientAPI.GetPlayerProfile(request, OnGetPlayerProfileSuccess, ErrorMessage);
    }

    private void OnGetPlayerProfileSuccess(GetPlayerProfileResult result)
    {
        userDisplayName = result.PlayerProfile.DisplayName;
        userAvatarUrl = result.PlayerProfile.AvatarUrl;

        userDisplayNameText.text = userDisplayName;

        if (!string.IsNullOrEmpty(userAvatarUrl))
        {
            StartCoroutine(RetrievePlayerAvatar());
        }
    }

    IEnumerator RetrievePlayerAvatar()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(userAvatarUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load avatar: " + request.error);
        }
        else
        {
            DownloadHandlerTexture downloadHandler = request.downloadHandler as DownloadHandlerTexture;

            if (downloadHandler != null && downloadHandler.texture != null)
            {
                Sprite playerImage = Sprite.Create(downloadHandler.texture, new Rect(0.0f, 0.0f, downloadHandler.texture.width, downloadHandler.texture.height), Vector2.zero);
                userAvatarImage.sprite = playerImage;
            }
        }

        request.Dispose();
    }

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
        GetPlayerStatistics();
    }

    private void OnUpdateStatisticsError(PlayFabError error)
    {
        Debug.LogError($"Failed to update statistics: {error.ErrorMessage}");
        Debug.LogError($"Error details: {error.ErrorDetails}");
    }

    [ContextMenu("Get Leaderboard")]
    public void GetLeaderboard()
    {
        Debug.Log("Starting GetLeaderboard request...");

        GetLeaderboardRequest request = new GetLeaderboardRequest
        {
            StatisticName = "Score",
            StartPosition = 0,
            MaxResultsCount = 10,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true,
                ShowAvatarUrl = true
            }
        };

        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, OnGetLeaderboardError);
    }

    public void SetFinalScore(int finalScore)
    {
        score = finalScore;
        UpdateScore();
        StartCoroutine(ShowLeaderboardAfterScoreUpdate());
    }

    private IEnumerator ShowLeaderboardAfterScoreUpdate()
    {
        yield return new WaitForSeconds(2f);
        GetLeaderboard();
    }

    private void CreateSinglePlayerEntry()
    {
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
                scoreText.text = score.ToString();
                scoreText.color = Color.yellow;
            }
        }
    }

    private void OnGetLeaderboardError(PlayFabError error)
    {
        Debug.LogError($"Leaderboard Error: {error.Error}");
        Debug.LogError($"Message: {error.ErrorMessage}");
        Debug.LogError($"Details: {error.ErrorDetails}");
    }

    private void OnGetLeaderboardSuccess(GetLeaderboardResult result)
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        if (result.Leaderboard == null || result.Leaderboard.Count == 0)
        {
            CreateSinglePlayerEntry();
            return;
        }

        int position = 1;
        foreach (PlayerLeaderboardEntry user in result.Leaderboard)
        {
            GameObject userScoreObject = Instantiate(userScorePrefab, content);
            userScoreObject.SetActive(true);

            Transform nameTransform = userScoreObject.transform.Find("Name");
            Transform scoreTransform = userScoreObject.transform.Find("Score");
            Transform avatarTransform = userScoreObject.transform.Find("Avatar Image");

            if (nameTransform != null)
            {
                TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = $"{position}. {user.DisplayName}";
                }
            }

            if (scoreTransform != null)
            {
                TextMeshProUGUI scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
                if (scoreText != null)
                {
                    scoreText.text = user.StatValue.ToString();
                }
            }

            if (avatarTransform != null && user.Profile != null && !string.IsNullOrEmpty(user.Profile.AvatarUrl))
            {
                Image avatarImage = avatarTransform.GetComponent<Image>();
                if (avatarImage != null)
                {
                    StartCoroutine(LoadUserAvatar(avatarImage, user.Profile.AvatarUrl));
                }
            }

            if (user.DisplayName == userDisplayName)
            {
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
    }

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
        foreach (var stat in result.Statistics)
        {
            Debug.Log($"Statistic: {stat.StatisticName} = {stat.Value}");
        }
    }

    private void OnGetPlayerStatsError(PlayFabError error)
    {
        Debug.LogError($"Failed to get player statistics: {error.ErrorMessage}");
    }

    private void ErrorMessage(PlayFabError error)
    {
        Debug.Log($"PlayFab Error: {error.Error}\nMessage: {error.ErrorMessage}\nDetails: {error.ErrorDetails}");
    }
}