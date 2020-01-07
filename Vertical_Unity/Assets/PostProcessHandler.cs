using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessHandler : MonoBehaviour
{
    public PostProcessProfile originProfile;
    public float hurtEffectTime;
    [Header("SlowMo settings")]
    public PostProcessProfile slowMoProfile;
    [Range(0.0f, 20.0f)] public float slowMoTransitionlerpSpeed;
    [Range(-60.0f, 60.0f)] public float slowMoLensDistortion;
    [Range(0.0f, 1.0f)] public float slowMoChromaticAberrationIntensity;
    public float lerpStop;
    [Header("GameOver settings")]
    public PostProcessProfile gameOverProfile;
    [Range(0.0f, 1.0f)] public float vignetteItensity;
    [Range(0.0f, 30.0f)] public float vignettelerpSpeed;

    private PostProcessVolume postProcessVolume;
    private LensDistortion lens;
    private ChromaticAberration aberration;
    private bool slowMoActivated;
    private float transitionState;
    private float lastRealTime;
    private float realTimeSpend;
    private Vignette vignette;
    private ColorGrading hurtColorGrading;

    private void Start()
    {
        postProcessVolume = GetComponent<PostProcessVolume>();
        lens = slowMoProfile.GetSetting<LensDistortion>();
        aberration = slowMoProfile.GetSetting<ChromaticAberration>();
        vignette = gameOverProfile.GetSetting<Vignette>();
        hurtColorGrading = originProfile.GetSetting<ColorGrading>();
        transitionState = 1;
        slowMoActivated = false;
        lastRealTime = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        realTimeSpend = Time.realtimeSinceStartup - lastRealTime;
        lastRealTime = Time.realtimeSinceStartup;

        if((slowMoActivated ? transitionState : -transitionState) > (slowMoActivated ? lerpStop : - (1 - lerpStop)))
        {
            lens.intensity.value = (1 - transitionState) * slowMoLensDistortion;
            aberration.intensity.value = (1 - transitionState) * slowMoChromaticAberrationIntensity;

            transitionState -= (slowMoActivated ? transitionState : -(1 - transitionState)) * slowMoTransitionlerpSpeed * realTimeSpend;
        }
        else
        {
            if(slowMoActivated)
            {
                lens.intensity.value = slowMoLensDistortion;
                aberration.intensity.value = slowMoChromaticAberrationIntensity;
            }
            else if(postProcessVolume.profile == slowMoProfile)
            {
                lens.intensity.value = 0;
                aberration.intensity.value = 0;
                postProcessVolume.profile = originProfile;
            }
        }
    }

    public void EnableSlowMoEffect()
    {
        postProcessVolume.profile = slowMoProfile;
        slowMoActivated = true;
    }

    public void DisableSlowMoEffect()
    {
        slowMoActivated = false;
    }

    public IEnumerator ActivateDeathVignette()
    {
        postProcessVolume.profile = gameOverProfile;
        float lerpState = 1;
        while (lerpState > lerpStop)
        {
            vignette.intensity.value = (1 - lerpState) * vignetteItensity;

            lerpState -= lerpState * vignettelerpSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        vignette.intensity.value = vignetteItensity;
    }

    public IEnumerator TriggerHurtEffect()
    {
        postProcessVolume.profile = originProfile;
        ColorGrading colorGrading = postProcessVolume.profile.GetSetting<ColorGrading>();
        colorGrading.enabled.value = true;
        yield return new WaitForSecondsRealtime(hurtEffectTime);
        colorGrading.enabled.value = false;
    }
}
