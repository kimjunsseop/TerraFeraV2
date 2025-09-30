using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [Header("Day-Night Settings")]
    public Light directionalLight;
    public Gradient lightColor;
    public Gradient ambientColor;
    public AnimationCurve lightIntensityCurve;   // ✨ 추가
    public float dayDuration = 120f;

    private float currentTime = 0f;
    public bool isNight => currentTime > 0.5f;
    [Header("UI Settings")]
    public UnityEngine.UI.Image fillCircle;
    public UnityEngine.UI.Image icon;
    public Sprite sun;
    public Sprite moon;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        currentTime += Time.deltaTime / dayDuration;

        if (currentTime > 1f)
            currentTime -= 1f;

        if (directionalLight != null)
        {
            directionalLight.color = lightColor.Evaluate(currentTime);
            directionalLight.transform.rotation = Quaternion.Euler(new Vector3((currentTime * 360f) - 90f, 170f, 0));
            directionalLight.intensity = lightIntensityCurve.Evaluate(currentTime);
        }

        RenderSettings.ambientLight = ambientColor.Evaluate(currentTime);
        UpdateTimeUI();
    }

    public void AdvanceTimeByHours(float hours)
    {
        float timePerHour = 1f / 24f;
        currentTime += timePerHour * hours;

        if (currentTime > 1f)
            currentTime -= 1f;
    }
    void UpdateTimeUI()
    {
        if (fillCircle != null)
            fillCircle.fillAmount = currentTime;

        if (icon != null)
            icon.sprite = isNight ? moon : sun;
    }
}
