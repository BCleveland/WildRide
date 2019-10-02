using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance;
    /*
        FX Manager will be an interface between logic objects and effects.
        Sounds, Particles, Camera effects, etc. Time slowdown is also managed here
    */
    [Header("Components")]
    [SerializeField] private Camera m_Camera;
    [SerializeField] private AudioSource m_SourceBGM = null;
    [SerializeField] private AudioSource m_SourceSFX = null;

    [SerializeField] private AudioClip m_ClipDie = null;
    [SerializeField] private AudioClip m_ClipWin = null;

    [SerializeField] private ParticleSystem m_ParticleDie = null;

    [Header("Values")]
    [SerializeField] private float m_GameOverSlowdownRecoveryTime = 1.0f;
    [SerializeField] private float m_TargetTimeScaleRate = 0.2f;
    [SerializeField] private float m_CameraMinFOV = 40;
    [SerializeField] private float m_CameraMaxFOV = 60;
    private bool m_IsTimeLocked = false;
    private float m_TargetTimeScale = 1.0f;
    private void Awake() 
    {
        Instance = this;   
    }
    private void Update() 
    {
        if(!m_IsTimeLocked)
        {
            SetTimescale(Mathf.Lerp(Time.timeScale, m_TargetTimeScale, m_TargetTimeScaleRate*Time.unscaledDeltaTime));
        }
        
    }
    private void SetTimescale(float timeScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = 0.02f * timeScale;
        m_SourceBGM.pitch = Interpolation.CircularOut(timeScale);
        m_Camera.fieldOfView = Mathf.LerpUnclamped(m_CameraMinFOV, m_CameraMaxFOV, timeScale);
    }
    public void SetTargetTimeScale(float scale)
    {
        m_TargetTimeScale = scale;
    }
    public void OnDie()
    {
        m_SourceSFX.PlayOneShot(m_ClipDie);
        m_SourceBGM.Stop();

        m_ParticleDie.Play(true);
        StartCoroutine(LockTimeForDuration(1.0f, 0.2f, m_GameOverSlowdownRecoveryTime));
    }
    public void OnWin()
    {
        m_SourceSFX.PlayOneShot(m_ClipWin);
    }


    private IEnumerator LockTimeForDuration(float duration, float value, float recoveryTime)
    {
        m_IsTimeLocked = true;
        SetTimescale(value);
        yield return new WaitForSecondsRealtime(duration);
        for(float time = 0.0f; time < recoveryTime; time += Time.unscaledDeltaTime)
        {
            yield return null;
            SetTimescale(Mathf.Lerp(value, 1.0f, time/recoveryTime));
        }
        SetTargetTimeScale(1.0f);
        SetTimescale(1.0f);
    }
}
