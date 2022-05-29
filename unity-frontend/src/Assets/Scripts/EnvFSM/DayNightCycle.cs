using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField]
    private float fullDayLength = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float timeOfDay;

    [SerializeField]
    private float dayNumber = 0;

    [SerializeField]
    private float timeScale = 100f;

    public bool pause = false;

    [SerializeField]
    private Transform lightRotation;
    [SerializeField]
    private Light sun;
    private float intensity;
    [SerializeField]
    private float sunBaseIntensity = 1f;
    [SerializeField]
    private float sunVariation = 1.5f;
    [SerializeField]
    private Gradient lightGradient;

    [SerializeField]
    private AnimationCurve timeShift;
    [SerializeField]
    private float timeShiftNormalization;

    [SerializeField]
    private Transform starParticles;
    [SerializeField]
    private float starsFadeLength = 0.04f;
    private float starFade;
    private Color starTint = new Color(0.5f, 0.5f, 0.5f);
    private Renderer starRenderer;

    [SerializeField]
    private GameObject lowClouds;
    [SerializeField]
    private GameObject highClouds;
    [SerializeField]
    private float cloudFadeLength = 0.08f;
    private float cloudFade;
    private Color cloudTint = new Color(1f, 1f, 1f);
    private Renderer lowCloudRenderer;
    private Renderer highCloudRenderer;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private Material stars;
    [SerializeField]
    private Material defaultSky;
    [SerializeField]
    private Light moon;
    [SerializeField]
    private MoonModule moonModule;

    [SerializeField]
    private string envSound = "";
    private AudioManager audioManager;

    private void Start()
    {
        NormailizeTimeShift();
        starRenderer = starParticles.GetComponent<ParticleSystem>().GetComponent<Renderer>();
        lowCloudRenderer = lowClouds.GetComponent<Renderer>();
        highCloudRenderer = highClouds.GetComponent<Renderer>();
        audioManager = FindObjectOfType<AudioManager>();
    }


    private void Update()
    {
        // Update all important aspects of the day-night cycle
        if(!pause)
        {
            UpdateTimeScale();
            UpdateTime();
        }
        AdjustLightRotation();
        SunIntensity();
        AdjustLightColor();
        UpdateMoon();
        UpdateStars();
        UpdateClouds();
        UpadteEnvPosition();
        UpadteSounds();
    }
    // Update how fast time moves
    private void UpdateTimeScale()
    {
        timeScale = 24 / (fullDayLength / 60);
        timeScale *= timeShift.Evaluate(timeOfDay);
        timeScale /= timeShiftNormalization;
    }
    // Update the time of day
    private void UpdateTime()
    {
        timeOfDay += Time.deltaTime * timeScale / 86400;
        // Increment the day count if we reach a new day
        if(timeOfDay > 1)
        {
            dayNumber++;
            timeOfDay -= 1;
        }
    }
    // Rotate the light sources
    private void AdjustLightRotation()
    {
        float lightAngle = timeOfDay * 360f;
        lightRotation.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, lightAngle));
    }
    // Adjust the brightness of the light sources, capping at a certain fixed value
    private void SunIntensity()
    {
        intensity = Mathf.Clamp01(Vector3.Dot(sun.transform.forward, Vector3.down));
        sun.intensity = intensity * sunVariation + sunBaseIntensity;
        if (sun.intensity > 1.75f) sun.intensity = 1.75f;
    }
    // Adjust the color of the sun
    private void AdjustLightColor()
    {
        sun.color = lightGradient.Evaluate(intensity);
    }
    // Used to update the time scale to ensure we're moving at the proper speed
    private void NormailizeTimeShift()
    {
        float step = 0.01f;
        int numSteps = Mathf.FloorToInt(1f / step);
        float curveTotal = 0;

        for(int i = 0; i < numSteps; i++)
        {
            curveTotal += timeShift.Evaluate(step*i);
        }

        timeShiftNormalization = curveTotal / numSteps;
    }
    // Update the color and intensity of the moon (The rotation is done in the same method above as the sun)
    private void UpdateMoon()
    {
        moonModule.UpdateModule(intensity);
    }
    // Accessor method to determine the time of day
    public string getTimeOfDay() {
        if(timeOfDay < 0.19f) return "Night";
        if(timeOfDay < 0.29f) return "Dawn";
        if(timeOfDay < 0.75f) return "Day";
        if(timeOfDay < 0.81f) return "Dusk";
        return "Night";
    }
    // Update the alpha of the star material (visibility)
    private void UpdateStars()
    {
        string partOfDay = getTimeOfDay();
        
        if(partOfDay == "Night") starFade = 1;
        else if(partOfDay == "Day") starFade = 0;
        else if(partOfDay == "Dusk") {
            starFade = (timeOfDay - 0.75f) / starsFadeLength;
        }
        else {
            starFade = 1 - ((timeOfDay - 0.19f) / starsFadeLength);
        }

        if(starFade > 1) starFade = 1;
        if(starFade < 0) starFade = 0;

        starTint.a = starFade;
        starRenderer.material.SetColor("_TintColor", starTint);
    }

    // Update the alpha of the cloud material (visibility)
    private void UpdateClouds()
    {
        string partOfDay = getTimeOfDay();
        
        if(partOfDay == "Night") cloudFade = 0;
        else if(partOfDay == "Dusk" || partOfDay == "Day") {
            cloudFade = (1f - ((timeOfDay - 0.71f) / cloudFadeLength)) / 2f;
        }
        else {
            cloudFade = ((timeOfDay - 0.21f) / cloudFadeLength) / 2f;
        }

        if(cloudFade > 0.5f) cloudFade = 0.5f;
        if(cloudFade < 0) cloudFade = 0;

        cloudTint.a = cloudFade;
        lowCloudRenderer.material.SetColor("_CloudColor", cloudTint);
        highCloudRenderer.material.SetColor("_CloudColor", cloudTint);
    }
    // Update the position of the environment (Stars, Clouds)
    private void UpadteEnvPosition()
    {
        transform.position = player.position;
    }

    private void UpadteSounds()
    {
        string partOfDay = getTimeOfDay();
        if(envSound != "Savannah_Day" && (partOfDay == "Day" || partOfDay == "Dawn")) {
            if(envSound != "")
                audioManager.StopSound(envSound);
            envSound = "Savannah_Day";
            audioManager.PlaySound(envSound);
        }

        else if(envSound != "Savannah_Night" && (partOfDay == "Dusk" || partOfDay == "Night")) {
            if(envSound != "")
                audioManager.StopSound(envSound);
            envSound = "Savannah_Night";
            audioManager.PlaySound(envSound);
        }

    }
}
