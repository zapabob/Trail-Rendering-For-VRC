using System;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Random = UnityEngine.Random;

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
        catch (Exception e)
        {
            Debug.LogError($"トレイルレンダラーのセットアップ中にエラーが発生しました: {e.Message}");
            Debug.LogError($"Error setting up TrailRenderer: {e.Message}");
            enabled = false;
        }
    }

    private void SetupComponents()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null)
        {
            throw new InvalidOperationException("アバターにAnimatorコンポーネントが見つかりません。");
            throw new InvalidOperationException("Animator component not found on the avatar.");
        }

        if (expressionParameters == null)
        {
            throw new InvalidOperationException("VRCExpressionParametersが割り当てられていません。");
            throw new InvalidOperationException("VRCExpressionParameters not assigned.");
        }

        if (avatarDescriptor == null)
        {
            throw new InvalidOperationException("VRCAvatarDescriptorが割り当てられていません。");
            throw new InvalidOperationException("VRCAvatarDescriptor not assigned.");
        }

        for (int i = 0; i < boneNames.Length; i++)
        {
            SetupLineRenderer(lineRenderers[i], boneNames[i]);
        }
    }

    private void SetupLineRenderer(LineRenderer lineRenderer, string boneName)
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetLineRendererForBone(boneName);
            if (lineRenderer == null)
            {
                throw new InvalidOperationException($"ボーン「{boneName}」のLineRendererが見つかりません。");
                throw new InvalidOperationException($"LineRenderer not found for bone '{boneName}'.");
            }
        }

        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
    }

    private LineRenderer GetLineRendererForBone(string boneName)
    {
        var boneTransform = animator.GetBoneTransform(HumanBodyBones.Hips).Find(boneName);
        if (boneTransform == null)
        {
            throw new InvalidOperationException($"ボーン「{boneName}」が見つかりません。");
            throw new InvalidOperationException($"Bone '{boneName}' not found.");
        }

        return boneTransform.GetComponentInChildren<LineRenderer>();
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
        if (!enabled) return;

        try
        {
            for (int i = 0; i < boneNames.Length; i++)
            {
                UpdateTrail(lineRenderers[i], boneNames[i]);
            }

            if (Random.value < 0.01f)
            {
                UpdateTrailParameters();
            }

            UpdateTrailSound();
            ProcessAudioSpectrum();
        }
        catch (Exception e)
        {
            Debug.LogError($"トレイルレンダラーの更新中にエラーが発生しました: {e.Message}");
            Debug.LogError($"Error updating TrailRenderer: {e.Message}");
        }
    }

    private void UpdateTrail(LineRenderer lineRenderer, string boneName)
    {
        if (lineRenderer == null) return;

        Vector3 bonePosition;
        try
        {
            bonePosition = GetBonePosition(boneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"トレイルの更新中にエラーが発生しました: {e.Message}");
            Debug.LogError($"Error updating trail: {e.Message}");
            return;
        }

        int maxPoints = CalculateMaxTrailPoints();
        UpdateLineRendererPositions(lineRenderer, bonePosition, maxPoints);
    }

    private Vector3 GetBonePosition(string boneName)
    {
        var boneTransform = animator.GetBoneTransform(HumanBodyBones.Hips).Find(boneName);
        if (boneTransform == null)
        {
            throw new InvalidOperationException($"ボーン「{boneName}」が見つかりません。");
            throw new InvalidOperationException($"Bone '{boneName}' not found.");
        }
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

        foreach (var lineRenderer in lineRenderers)
        {
            SetLineRendererWidth(lineRenderer, trailWidth);
            SetLineRendererColorGradient(lineRenderer);
        }
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

    private void UpdateTrailSound()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float velocity;
        try
        {
            velocity = CalculateVelocity();
        }
        catch (Exception e)
        {
            Debug.LogError($"トレイルサウンドの更新中にエラーが発生しました: {e.Message}");
            Debug.LogError($"Error updating trail sound: {e.Message}");
            return;
        }

        float normalizedVelocity = NormalizeVelocity(velocity);
        float pitch = BayesianOptimization(minPitch, maxPitch, normalizedVelocity);
        audioSource.pitch = pitch;
    }

    private float CalculateVelocity()
    {
        Vector3 hipPosition = GetBonePosition("Hips");
        Vector3 chestPosition = GetBonePosition("Chest");
        return (chestPosition - hipPosition).magnitude;
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
        SetAvatarParameterValue("TrailRendererHueOffset", Mathf.Clamp01(value));
    }

    private float GetHueOffset()
    {
        return GetAvatarParameterValue("TrailRendererHueOffset");
    }

    public void SetColorVariation(float value)
    {
        SetAvatarParameterValue("TrailRendererColorVariation", Mathf.Clamp01(value));
    }

    private float GetColorVariation()
    {
        return GetAvatarParameterValue("TrailRendererColorVariation");
    }

    private void SetAvatarParameterValue(string parameterName, float value)
    {
        if (expressionParameters != null)
        {
            var parameter = expressionParameters.FindParameter(parameterName);
            if (parameter != null)
            {
                parameter.valueFloat = value;
            }
        }
    }

    private float GetAvatarParameterValue(string parameterName)
    {
        if (expressionParameters != null)
        {
            var parameter = expressionParameters.FindParameter(parameterName);
            if (parameter != null)
            {
                return parameter.valueFloat;
            }
        }
        return 0f;
    }

    public void OnTrailToggle(bool isOn)
    {
        try
        {
            ToggleTrail(isOn);
        }
        catch (Exception e)
        {
            Debug.LogError($"トレイルの切り替え中にエラーが発生しました: {e.Message}");
            Debug.LogError($"Error toggling trail: {e.Message}");
        }
    }

    private void ToggleTrail(bool isOn)
    {
        enabled = isOn;

        foreach (var lineRenderer in lineRenderers)
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = isOn;
            }
        }

        if (!isOn && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
