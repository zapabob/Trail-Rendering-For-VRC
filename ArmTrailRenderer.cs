using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArmTrailRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer leftLineRenderer;
    [SerializeField] private LineRenderer rightLineRenderer;
    [SerializeField] private float minTrailDuration = 0.3f;
    [SerializeField] private float maxTrailDuration = 0.7f;
    [SerializeField] private float minTrailWidth = 0.005f;
    [SerializeField] private float maxTrailWidth = 0.015f;
    [SerializeField] private Gradient defaultTrailColorGradient;
    [SerializeField] private AudioClip beamSaberSound;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private int fftSize = 256;
    [SerializeField] private float noiseThreshold = 0.1f;
    [SerializeField] private float trailDurationMultiplier = 1f;
    [SerializeField] private float trailWidthMultiplier = 1f;
    [SerializeField] private float fftSizeMultiplier = 1f;

    private Animator animator;
    private AudioSource audioSource;
    private float[] fftSpectrum;

    private void Start()
    {
        SetupComponents();
        SetupAudio();
        SetupFFTSpectrum();
    }

    private void SetupComponents()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found. Please make sure the script is attached to the avatar prefab.");
            enabled = false;
            return;
        }

        SetupLineRendererMaterial(leftLineRenderer);
        SetupLineRendererMaterial(rightLineRenderer);
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = beamSaberSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void SetupFFTSpectrum()
    {
        fftSpectrum = new float[fftSize];
    }

    private void Update()
    {
        if (!enabled) return;

        UpdateTrail(leftLineRenderer, HumanBodyBones.LeftHand);
        UpdateTrail(rightLineRenderer, HumanBodyBones.RightHand);

        if (Random.value < 0.01f)
        {
            UpdateTrailParameters();
        }

        UpdateBeamSaberSound();
        ProcessAudioSpectrum();
    }

    private void UpdateTrail(LineRenderer lineRenderer, HumanBodyBones bone)
    {
        if (lineRenderer == null) return;

        Vector3 bonePosition;
        try
        {
            bonePosition = GetBonePosition(bone);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating trail: {e.Message}");
            return;
        }

        int maxPoints = CalculateMaxTrailPoints();
        UpdateLineRendererPositions(lineRenderer, bonePosition, maxPoints);
    }

    private Vector3 GetBonePosition(HumanBodyBones bone)
    {
        var boneTransform = animator.GetBoneTransform(bone) ?? throw new Exception($"Bone transform not found for {bone}.");
        return boneTransform.position;
    }

    private int CalculateMaxTrailPoints()
    {
        float trailDuration = BayesianOptimization(minTrailDuration, maxTrailDuration, Time.time * 0.1f) * trailDurationMultiplier;
        return Mathf.CeilToInt(trailDuration * 60f);
    }

    private void UpdateLineRendererPositions(LineRenderer lineRenderer, Vector3 bonePosition, int maxPoints)
    {
        lineRenderer.positionCount = Mathf.Min(lineRenderer.positionCount + 1, maxPoints);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, bonePosition);

        if (lineRenderer.positionCount >= maxPoints)
        {
            for (int i = 0; i < lineRenderer.positionCount - 1; i++)
            {
                lineRenderer.SetPosition(i, lineRenderer.GetPosition(i + 1));
            }
            lineRenderer.positionCount--;
        }
    }

    private void UpdateTrailParameters()
    {
        float trailWidth = BayesianOptimization(minTrailWidth, maxTrailWidth, Time.time * 0.1f) * trailWidthMultiplier;
        fftSize = Mathf.RoundToInt(fftSize * fftSizeMultiplier);

        SetLineRendererWidth(leftLineRenderer, trailWidth);
        SetLineRendererWidth(rightLineRenderer, trailWidth);
        SetLineRendererColorGradient(leftLineRenderer);
        SetLineRendererColorGradient(rightLineRenderer);
    }

    private void SetLineRendererWidth(LineRenderer lineRenderer, float width)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }

    private void SetLineRendererColorGradient(LineRenderer lineRenderer)
    {
        if (lineRenderer != null)
        {
            lineRenderer.colorGradient = GetCurrentColorGradient();
        }
    }

    private Gradient GetCurrentColorGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(defaultTrailColorGradient.colorKeys, defaultTrailColorGradient.alphaKeys);

        float hue = (GetHueOffset() + Random.Range(-GetColorVariation(), GetColorVariation())) % 1f;
        Color color = Color.HSVToRGB(hue, 1f, 1f);

        GradientColorKey[] colorKeys =
        {
            new GradientColorKey(color, 0f),
            new GradientColorKey(color, 1f)
        };
        gradient.SetKeys(colorKeys, gradient.alphaKeys);

        return gradient;
    }

    private void UpdateBeamSaberSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float velocity;
        try
        {
            velocity = CalculateArmVelocity();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating beam saber sound: {e.Message}");
            return;
        }

        float normalizedVelocity = NormalizeVelocity(velocity);
        float pitch = BayesianOptimization(minPitch, maxPitch, normalizedVelocity);
        audioSource.pitch = pitch;
    }

    private float CalculateArmVelocity()
    {
        Vector3 rightHand = GetBonePosition(HumanBodyBones.RightHand);
        Vector3 rightLowerArm = GetBonePosition(HumanBodyBones.RightLowerArm);
        return (rightHand - rightLowerArm).magnitude;
    }

    private float NormalizeVelocity(float velocity)
    {
        return Mathf.Clamp01(velocity / 5f);
    }

    private void ProcessAudioSpectrum()
    {
        audioSource.GetSpectrumData(fftSpectrum, 0, FFTWindow.Blackman);
        float normalizedSpectrum = CalculateNormalizedSpectrum();

        if (normalizedSpectrum > noiseThreshold)
        {
            float hue = Mathf.Lerp(0f, 1f, normalizedSpectrum);
            SetHueOffset(hue);

            float colorVariation = BayesianOptimization(0f, 1f, normalizedSpectrum);
            SetColorVariation(colorVariation);
        }
    }

    private float CalculateNormalizedSpectrum()
    {
        float spectrumSum = 0f;
        foreach (float sample in fftSpectrum)
        {
            spectrumSum += sample;
        }
        return spectrumSum / fftSpectrum.Length;
    }

    private void SetupLineRendererMaterial(LineRenderer lineRenderer)
    {
        if (lineRenderer != null)
        {
            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        }
    }

    private float BayesianOptimization(float min, float max, float t)
    {
        return Mathf.Lerp(min, max, Mathf.PerlinNoise(t, 0f));
    }

    public void SetTrailDurationMultiplier(float value)
    {
        trailDurationMultiplier = Mathf.Clamp01(value / 100f);
    }

    public void SetTrailWidthMultiplier(float value)
    {
        trailWidthMultiplier = Mathf.Clamp01(value / 100f);
    }

    public void SetFFTSizeMultiplier(float value)
    {
        fftSizeMultiplier = Mathf.Clamp01(value / 100f);
    }

    public void SetHueOffset(float value)
    {
        PlayerPrefs.SetFloat("HueOffset", Mathf.Clamp01(value));
    }

    private float GetHueOffset()
    {
        return PlayerPrefs.GetFloat("HueOffset", 0f);
    }

    public void SetColorVariation(float value)
    {
        PlayerPrefs.SetFloat("ColorVariation", Mathf.Clamp01(value));
    }

    private float GetColorVariation()
    {
        return PlayerPrefs.GetFloat("ColorVariation", 0f);
    }
}