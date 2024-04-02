using System;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public class TrailRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer[] lineRenderers;
    [SerializeField] private float minTrailDuration = 0.3f;
    [SerializeField] private float maxTrailDuration = 0.7f;
    [SerializeField] private float minTrailWidth = 0.005f;
    [SerializeField] private float maxTrailWidth = 0.015f;
    [SerializeField] private Gradient defaultTrailColorGradient;
    [SerializeField] private AudioClip trailSound;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;
    [SerializeField] private int fftSize = 256;
    [SerializeField] private float noiseThreshold = 0.1f;
    [SerializeField] private float trailDurationMultiplier = 1f;
    [SerializeField] private float trailWidthMultiplier = 1f;
    [SerializeField] private float fftSizeMultiplier = 1f;
    [SerializeField] private VRCExpressionParameters expressionParameters;
    [SerializeField] private VRCAvatarDescriptor avatarDescriptor;
    [SerializeField] private string[] boneNames;

    private Animator animator;
    private AudioSource audioSource;
    private float[] fftSpectrum;

    private void Start()
    {
        try
        {
            SetupComponents();
            SetupAudio();
            SetupFFTSpectrum();
        }
        catch (Exception ex)
        {
            LogError($"Failed to setup TrailRenderer: {ex.Message}");
            enabled = false;
        }
    }

    private void SetupComponents()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null)
        {
            throw new InvalidOperationException("Animator component not found on the avatar.");
        }

        if (expressionParameters == null)
        {
            throw new InvalidOperationException("VRCExpressionParameters is not assigned.");
        }

        if (avatarDescriptor == null)
        {
            throw new InvalidOperationException("VRCAvatarDescriptor is not assigned.");
        }

        foreach (string boneName in boneNames)
        {
            SetupLineRenderer(boneName);
        }
    }

    private void SetupLineRenderer(string boneName)
    {
        LineRenderer lineRenderer = GetLineRendererForBone(boneName);
        if (lineRenderer == null)
        {
            LogWarning($"LineRenderer not found for bone: {boneName}");
            return;
        }

        // Apply lighting settings for Poiyomi/lilToon shaders
        Shader shader = Shader.Find("Poiyomi/Toon") ?? Shader.Find("lilToon");
        if (shader != null)
        {
            lineRenderer.material.shader = shader;
            lineRenderer.material.SetFloat("_ShadowReceive", 1f);
            lineRenderer.material.SetFloat("_ShadowStrength", 0.5f);
            lineRenderer.material.SetFloat("_LightColorAttenuation", 0.8f);
            lineRenderer.material.SetFloat("_IndirectLightIntensity", 0.8f);
        }
        else
        {
            LogWarning("Poiyomi/lilToon shader not found. Using default shader.");
        }
    }

    private LineRenderer GetLineRendererForBone(string boneName)
    {
        Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.Hips)?.Find(boneName);
        return boneTransform != null ? boneTransform.GetComponentInChildren<LineRenderer>() : null;
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = trailSound;
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
        if (!enabled)
        {
            return;
        }

        try
        {
            foreach (string boneName in boneNames)
            {
                UpdateTrail(boneName);
            }

            if (UnityEngine.Random.value < 0.01f)
            {
                UpdateTrailParameters();
            }

            UpdateAudio();
            ProcessAudioSpectrum();
        }
        catch (Exception ex)
        {
            LogError($"An error occurred during TrailRenderer update: {ex.Message}");
        }
    }

    private void UpdateTrail(string boneName)
    {
        LineRenderer lineRenderer = GetLineRendererForBone(boneName);
        if (lineRenderer == null)
        {
            return;
        }

        Vector3 bonePosition = GetBonePosition(boneName);
        int maxPoints = CalculateMaxTrailPoints();
        UpdateLineRendererPositions(lineRenderer, bonePosition, maxPoints);
    }

    private Vector3 GetBonePosition(string boneName)
    {
        Transform boneTransform = animator.GetBoneTransform(HumanBodyBones.Hips)?.Find(boneName);
        return boneTransform != null ? boneTransform.position : Vector3.zero;
    }

    private int CalculateMaxTrailPoints()
    {
        float trailDuration = Mathf.Lerp(minTrailDuration, maxTrailDuration, Mathf.PerlinNoise(Time.time * 0.1f, 0f));
        return Mathf.CeilToInt(trailDuration * trailDurationMultiplier * 60f);
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
        float trailWidth = Mathf.Lerp(minTrailWidth, maxTrailWidth, Mathf.PerlinNoise(Time.time * 0.1f, 0f));

        foreach (LineRenderer lineRenderer in lineRenderers)
        {
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = trailWidth * trailWidthMultiplier;
                lineRenderer.endWidth = trailWidth * trailWidthMultiplier;
                lineRenderer.colorGradient = GetCurrentColorGradient();
            }
        }

        fftSize = Mathf.RoundToInt(fftSize * fftSizeMultiplier);
        if (fftSpectrum == null || fftSpectrum.Length != fftSize)
        {
            fftSpectrum = new float[fftSize];
        }
    }

    private Gradient GetCurrentColorGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(defaultTrailColorGradient.colorKeys, defaultTrailColorGradient.alphaKeys);

        float hueOffset = GetExpressionParameterValue("TrailRendererHueOffset");
        float colorVariation = GetExpressionParameterValue("TrailRendererColorVariation");
        float hue = (hueOffset + UnityEngine.Random.Range(-colorVariation, colorVariation)) % 1f;
        Color color = Color.HSVToRGB(hue, 1f, 1f);

        GradientColorKey[] colorKeys = { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) };
        gradient.SetKeys(colorKeys, gradient.alphaKeys);

        return gradient;
    }

    private void UpdateAudio()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float armVelocity = CalculateArmVelocity();
        float normalizedVelocity = NormalizeVelocity(armVelocity);
        float pitch = Mathf.Lerp(minPitch, maxPitch, normalizedVelocity);
        audioSource.pitch = pitch;
    }

    private float CalculateArmVelocity()
    {
        Vector3 rightHand = GetBonePosition("RightHand");
        Vector3 rightLowerArm = GetBonePosition("RightLowerArm");
        return Vector3.Distance(rightHand, rightLowerArm);
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
            SetExpressionParameterValue("TrailRendererHueOffset", hue);

            float colorVariation = Mathf.Lerp(0f, 1f, Mathf.PerlinNoise(Time.time * 0.1f, normalizedSpectrum));
            SetExpressionParameterValue("TrailRendererColorVariation", colorVariation);
        }
    }

    private float CalculateNormalizedSpectrum()
    {
        if (fftSpectrum == null || fftSpectrum.Length == 0)
        {
            return 0f;
        }

        float sum = 0f;
        for (int i = 0; i < fftSpectrum.Length; i++)
        {
            sum += fftSpectrum[i];
        }
        return sum / fftSpectrum.Length;
    }

    private float GetExpressionParameterValue(string parameterName)
    {
        if (expressionParameters == null)
        {
            return 0f;
        }

        VRCExpressionParameters.Parameter parameter = expressionParameters.FindParameter(parameterName);
        return parameter != null ? parameter.valueFloat : 0f;
    }

    private void SetExpressionParameterValue(string parameterName, float value)
    {
        if (expressionParameters == null)
        {
            return;
        }

        VRCExpressionParameters.Parameter parameter = expressionParameters.FindParameter(parameterName);
        if (parameter != null)
        {
            parameter.valueFloat = Mathf.Clamp01(value);
        }
    }

    private void LogError(string message)
    {
        Debug.LogError($"[TrailRenderer] {message}");
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[TrailRenderer] {message}");
    }
}
