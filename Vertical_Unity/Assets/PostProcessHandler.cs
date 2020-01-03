using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessHandler : MonoBehaviour
{
    public PostProcessProfile originProfile;
    public PostProcessProfile slowMoProfile;
    [Range(0.0f, 20.0f)] public float slowMoTransitionlerpSpeed;
    public float lerpAmplitude;
    [Range(-60.0f, 60.0f)] public float slowMoLensDistortion;
    [Range(0.0f, 1.0f)] public float slowMoChromaticAberrationIntensity;
    public float lerpStop;

    private PostProcessVolume postProcessVolume;
    private LensDistortion lens;
    private ChromaticAberration aberration;
    private bool stopEnabling;
    private bool stopDisabling;

    private void Start()
    {
        postProcessVolume = GetComponent<PostProcessVolume>();
        lens = slowMoProfile.GetSetting<LensDistortion>();
        aberration = slowMoProfile.GetSetting<ChromaticAberration>();
    }

    public IEnumerator ActivateSlowMoEffect()
    {
        stopDisabling = true;
        stopEnabling = false;
        postProcessVolume.profile = slowMoProfile;
        lens.intensity.value = 0;
        aberration.intensity.value = 0;
        float transitionState = 1;
        float lastRealTime = Time.realtimeSinceStartup;
        float realTimeSpend;
        while (transitionState >= lerpStop && !stopEnabling)
        {
            lens.intensity.value = (1 - transitionState) * slowMoLensDistortion;
            aberration.intensity.value = (1 - transitionState) * slowMoChromaticAberrationIntensity;
            realTimeSpend = Time.realtimeSinceStartup - lastRealTime;
            lastRealTime = Time.realtimeSinceStartup;
            transitionState -= transitionState * slowMoTransitionlerpSpeed * realTimeSpend;
            yield return new WaitForEndOfFrame();
        }

        lens.intensity.value = slowMoLensDistortion;
        aberration.intensity.value = slowMoChromaticAberrationIntensity;
    }

    public IEnumerator StopSlowMoEffect()
    {
        stopEnabling = true;
        stopDisabling = false;
        postProcessVolume.profile = slowMoProfile;
        lens.intensity.value = slowMoLensDistortion;
        aberration.intensity.value = slowMoChromaticAberrationIntensity;
        float transitionState = 1;
        while (transitionState >= lerpStop && !stopDisabling)
        {
            lens.intensity.value = transitionState * slowMoLensDistortion;
            aberration.intensity.value = transitionState * slowMoChromaticAberrationIntensity;

            transitionState -= transitionState * slowMoTransitionlerpSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        lens.intensity.value = 0;
        aberration.intensity.value = 0;
        postProcessVolume.profile = originProfile;
    }
}
