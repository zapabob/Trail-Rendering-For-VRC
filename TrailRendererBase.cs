// TrailRendererBase.cs
using System;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

public abstract class TrailRendererBase : MonoBehaviour
{
    [SerializeField] protected LineRenderer[] lineRenderers;
    [SerializeField] protected float minTrailDuration = 0.3f;
    [SerializeField] protected float maxTrailDuration = 0.7f;
    [SerializeField] protected float minTrailWidth = 0.005f;
    [SerializeField] protected float maxTrailWidth = 0.015f;
    [SerializeField] protected Gradient defaultTrailColorGradient;
    [SerializeField] protected VRCExpressionParameters expressionParameters;
    [SerializeField] protected VRCAvatarDescriptor avatarDescriptor;
    [SerializeField] protected string[] boneNames;

    protected Animator animator;

    protected virtual void Start()
    {
        SetupComponents();
    }

    protected virtual void SetupComponents()
    {
        try
        {
            animator = GetComponentInParent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException("Animator component not found on the avatar.");
            }

            if (expressionParameters == null)
            {
                throw new InvalidOperationException("VRCExpressionParameters not assigned.");
            }

            if (avatarDescriptor == null)
            {
                throw new InvalidOperationException("VRCAvatarDescriptor not assigned.");
            }

            for (int i = 0; i < boneNames.Length; i++)
            {
                SetupLineRenderer(lineRenderers[i], boneNames[i]);
            }
        }
        catch (Exception e)
        {
            LogError($"Error setting up components: {e.Message}");
            enabled = false;
        }
    }

    protected virtual void SetupLineRenderer(LineRenderer lineRenderer, string boneName)
    {
        try
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetLineRendererForBone(boneName);
                if (lineRenderer == null)
                {
                    throw new InvalidOperationException($"LineRenderer not found for bone '{boneName}'.");
                }
            }

            lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        }
        catch (Exception e)
        {
            LogError($"Error setting up line renderer for bone '{boneName}': {e.Message}");
        }
    }

    protected virtual LineRenderer GetLineRendererForBone(string boneName)
    {
        var boneTransform = animator.GetBoneTransform(HumanBodyBones.Hips).Find(boneName);
        if (boneTransform == null)
        {
            throw new InvalidOperationException($"Bone '{boneName}' not found.");
        }

        return boneTransform.GetComponentInChildren<LineRenderer>();
    }

    protected virtual void Update()
    {
        if (!enabled) return;

        try
        {
            for (int i = 0; i < boneNames.Length; i++)
            {
                UpdateTrail(lineRenderers[i], boneNames[i]);
            }

            if (UnityEngine.Random.value < 0.01f)
            {
                UpdateTrailParameters();
            }
        }
        catch (Exception e)
        {
            LogError($"Error updating trail renderer: {e.Message}");
        }
    }

    protected abstract void UpdateTrail(LineRenderer lineRenderer, string boneName);
    protected abstract void UpdateTrailParameters();

    protected virtual void LogError(string message)
    {
        Debug.LogError($"[TrailRendererBase] {message}");
    }
}
