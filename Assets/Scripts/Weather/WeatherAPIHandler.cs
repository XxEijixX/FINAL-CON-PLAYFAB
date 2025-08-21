using System;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;


public class WeatherAPIHandler : MonoBehaviour
{
    [Header("Weather Data")]
    [SerializeField] private WeatherData weatherData;

    [Header("Cities")]
    [SerializeField]
    private CityData[] cities = new CityData[]
    {
        new CityData("New York", "40.7128", "-74.0060"),
        new CityData("London", "51.5072", "-0.1276"),
        new CityData("Tokyo", "35.6895", "139.6917"),
        new CityData("Paris", "48.8566", "2.3522"),
        new CityData("Mexico City", "19.4326", "-99.1332"),
        new CityData("Sydney", "-33.8688", "151.2093"),
        new CityData("Moscow", "55.7558", "37.6173"),
        new CityData("Rio de Janeiro", "-22.9068", "-43.1729"),
        new CityData("Cairo", "30.0444", "31.2357"),
        new CityData("Toronto", "43.6532", "-79.3832")
    };

    private CityData selectedCity;
    private string url;
    private string jsonRaw;

    [Header("Scene References")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Volume globalVolume;

    [SerializeField] private float lightColorTransitionSpeed = 0.5f;
    [SerializeField] private float exposureTransitionSpeed = 0.5f;

    private Color targetLightColor;
    private bool isTransitioningColor = false;
    private float targetExposure;
    private bool isTransitioningExposure = false;

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

    [Header("City Change Settings")]
    [SerializeField] private float cityChangeInterval = 0f; // 0 = no automatic change

    [Header("UI Elements")]
    [SerializeField] private TMP_Text cityText;
    [SerializeField] private TMP_Text weatherText;
    [SerializeField] private TMP_Text coordinatesText;

    private void Start()
    {
        SelectRandomCity();

        if (directionalLight == null)
            directionalLight = FindObjectOfType<Light>();

        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
                Debug.LogError("No se encontró ColorAdjustments en el perfil del Global Volume.");
            if (!globalVolume.profile.TryGet(out vignette))
                Debug.LogError("No se encontró Vignette en el perfil del Global Volume.");
        }

        StartCoroutine(WeatherUpdate());

        if (cityChangeInterval > 0f)
            StartCoroutine(ChangeCityEvery(cityChangeInterval));
    }

    private void SelectRandomCity()
    {
        selectedCity = cities[UnityEngine.Random.Range(0, cities.Length)];
        url = $"https://api.openweathermap.org/data/3.0/onecall?lat={selectedCity.latitude}&lon={selectedCity.longitude}&appid=7fe45acb4f5a69f83c45312aad97613a&units=metric";
        Debug.Log("Ciudad seleccionada: " + selectedCity.name);
    }

    IEnumerator WeatherUpdate()
    {
        UnityWebRequest request = new UnityWebRequest(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError(request.error);
        else
        {
            jsonRaw = request.downloadHandler.text;
            DecodeJson();
        }
    }

    private void DecodeJson()
    {
        JSONNode json = JSON.Parse(jsonRaw);

        string timezone = json["timezone"].Value;
        weatherData.continent = timezone.Split('/')[0];
        weatherData.city = timezone.Split('/')[1];
        weatherData.actualTemp = json["current"]["temp"];
        weatherData.description = json["current"]["weather"][0]["description"];
        weatherData.windSpeed = json["current"]["wind_speed"];

        UpdateDirectionalLight();
        UpdateVolumeEffects();
        UpdateUI();
    }

    private void UpdateDirectionalLight()
    {
        if (weatherData.actualTemp < 0)
            targetLightColor = new Color(182f / 255f, 248f / 255f, 255f / 255f);
        else if (weatherData.actualTemp <= 10)
            targetLightColor = new Color(228f / 255f, 237f / 255f, 250f / 255f);
        else if (weatherData.actualTemp <= 25)
            targetLightColor = new Color(255f / 255f, 235f / 255f, 160f / 255f);
        else if (weatherData.actualTemp <= 40)
            targetLightColor = new Color(255f / 255f, 150f / 255f, 0f / 255f);
        else
            targetLightColor = new Color(255f / 255f, 0f / 255f, 0f / 255f);

        if (!isTransitioningColor)
            StartCoroutine(TransitionLightColor());

        UpdateExposureBasedOnTemperature();
    }

    IEnumerator TransitionLightColor()
    {
        isTransitioningColor = true;
        float t = 0f;
        Color startColor = directionalLight.color;

        while (t < 1f)
        {
            t += Time.deltaTime * lightColorTransitionSpeed;
            directionalLight.color = Color.Lerp(startColor, targetLightColor, t);
            yield return null;
        }

        directionalLight.color = targetLightColor;
        isTransitioningColor = false;
    }

    private void UpdateExposureBasedOnTemperature()
    {
        if (weatherData.actualTemp < 0)
            targetExposure = -1f;
        else if (weatherData.actualTemp <= 10)
            targetExposure = -0.5f;
        else if (weatherData.actualTemp <= 25)
            targetExposure = 0f;
        else if (weatherData.actualTemp <= 40)
            targetExposure = 0.5f;
        else
            targetExposure = 1f;

        if (!isTransitioningExposure)
            StartCoroutine(TransitionExposure());
    }

    IEnumerator TransitionExposure()
    {
        isTransitioningExposure = true;
        float t = 0f;
        float startExposure = colorAdjustments.postExposure.value;

        while (t < 1f)
        {
            t += Time.deltaTime * exposureTransitionSpeed;
            colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, targetExposure, t);
            yield return null;
        }

        colorAdjustments.postExposure.value = targetExposure;
        isTransitioningExposure = false;
    }

    private void UpdateVolumeEffects()
    {
        if (weatherData.description.Contains("rain") || weatherData.description.Contains("storm"))
        {
            colorAdjustments.saturation.value = -70f;
            colorAdjustments.colorFilter.value = Color.blue * 0.7f;
            colorAdjustments.postExposure.value = -0.3f;
            vignette.intensity.value = 0.6f;
        }
        else if (weatherData.description.Contains("cloud"))
        {
            colorAdjustments.saturation.value = -40f;
            colorAdjustments.colorFilter.value = Color.gray * 0.6f;
            colorAdjustments.postExposure.value = -0.1f;
            vignette.intensity.value = 0.3f;
        }
        else if (weatherData.description.Contains("clear") || weatherData.description.Contains("sun"))
        {
            colorAdjustments.saturation.value = 40f;
            colorAdjustments.colorFilter.value = new Color(1f, 0.95f, 0.7f);
            colorAdjustments.postExposure.value = 0.2f;
            vignette.intensity.value = 0f;
        }
        else if (weatherData.actualTemp <= 0)
        {
            colorAdjustments.saturation.value = -60f;
            colorAdjustments.colorFilter.value = Color.cyan * 0.6f;
            colorAdjustments.postExposure.value = -0.2f;
            vignette.intensity.value = 0.4f;
        }
        else if (weatherData.actualTemp >= 35)
        {
            colorAdjustments.saturation.value = 50f;
            colorAdjustments.colorFilter.value = Color.red * 0.5f;
            colorAdjustments.postExposure.value = 0.3f;
            vignette.intensity.value = 0.2f;
        }
        else
        {
            colorAdjustments.saturation.value = 0f;
            colorAdjustments.colorFilter.value = Color.white;
            colorAdjustments.postExposure.value = 0f;
            vignette.intensity.value = 0.1f;
        }
    }

    private void UpdateUI()
    {
        Color uiColor = Color.white;

        if (weatherData.description.Contains("rain") || weatherData.description.Contains("storm"))
            uiColor = Color.cyan;
        else if (weatherData.description.Contains("cloud"))
            uiColor = Color.gray;
        else
            uiColor = Color.yellow;

        if (cityText != null)
        {
            cityText.text = "Ciudad: " + selectedCity.name;
            cityText.color = uiColor;
        }

        if (weatherText != null)
        {
            weatherText.text = $"Clima: {weatherData.description} ({weatherData.actualTemp}°C)";
            weatherText.color = uiColor;
        }

        if (coordinatesText != null)
        {
            coordinatesText.text = $"Lat: {selectedCity.latitude}, Lon: {selectedCity.longitude}";
            coordinatesText.color = uiColor;
        }
    }

    IEnumerator ChangeCityEvery(float seconds)
    {
        while (true)
        {
            yield return new WaitForSeconds(seconds);
            SelectRandomCity();
            StartCoroutine(WeatherUpdate());
        }
    }
}

[Serializable]
public struct WeatherData
{
    public string continent;
    public string city;
    public float actualTemp;
    public string description;
    public float windSpeed;
}

[Serializable]
public struct CityData
{
    public string name;
    public string latitude;
    public string longitude;

    public CityData(string name, string lat, string lon)
    {
        this.name = name;
        this.latitude = lat;
        this.longitude = lon;
    }
}
