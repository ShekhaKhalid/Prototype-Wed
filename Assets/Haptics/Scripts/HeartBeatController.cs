using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.XR;

public class HeartAttackController : MonoBehaviour
{
    public float normalRate = 1.0f; // Normal heartbeat interval in seconds
    public float attackRate = 0.2f; // Fast heartbeat interval in seconds (simulating heart attack)
    public float transitionTime = 10.0f; // Time to transition to heart attack rate
    public float maxIntensity = 1.0f; // Max intensity of the haptic feedback

    private float currentRate;
    private float timer = 0f;
    private bool isAttacking = false;

    // Global Volume and post-processing effects references
    private Volume globalVolume;
    private DepthOfField depthOfField;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;

    // Sweat effect references
    public Image sweatOverlay; // Assign a UI Image with sweat texture in the inspector


    private void Start()
    {
        currentRate = normalRate;
        StartCoroutine(HeartbeatCoroutine());

        // Find and assign the Global Volume
        globalVolume = FindObjectOfType<Volume>();

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out depthOfField);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out colorAdjustments);
        }
        else
        {
            Debug.LogError("Global Volume not found. Please ensure there is a Global Volume in the scene.");
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (!isAttacking && timer >= transitionTime)
        {
            isAttacking = true;
            timer = 0f; // Reset timer for the attack phase
        }

        if (isAttacking)
        {
            // Smoothly transition the rate to simulate heart attack
            currentRate = Mathf.Lerp(normalRate, attackRate, timer / transitionTime);
            if (currentRate <= attackRate)
            {
                currentRate = attackRate; // Cap the rate to the attack rate
            }

            // Increase blur (Depth of Field effect)
            if (depthOfField != null)
            {
                depthOfField.focusDistance.value = Mathf.Lerp(10f, 0.1f, timer / transitionTime);
                depthOfField.aperture.value = Mathf.Lerp(5.6f, 0.5f, timer / transitionTime);
            }

            // Increase sweat intensity
            if (sweatOverlay != null)
            {
                Color color = sweatOverlay.color;
                color.a = Mathf.Lerp(0f, 0.5f, timer / transitionTime);
                sweatOverlay.color = color;
            }

            // Increase vignette effect to full blackness
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(0.2f, 1.0f, timer / transitionTime); // Max intensity
                vignette.smoothness.value = Mathf.Lerp(0.2f, 1.0f, timer / transitionTime); // Smooth transition to black
            }

            // Decrease saturation and brightness to blackness
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = Mathf.Lerp(0f, -100f, timer / transitionTime);
                colorAdjustments.postExposure.value = Mathf.Lerp(0f, -10f, timer / transitionTime); // Dramatic darkening
            }
        }
    }

    private IEnumerator HeartbeatCoroutine()
    {
        while (true)
        {
            VibrateControllers();
            yield return new WaitForSeconds(currentRate);
        }
    }

    private void VibrateControllers()
    {
        SendHapticFeedback(XRNode.LeftHand);
        SendHapticFeedback(XRNode.RightHand);
    }

    private void SendHapticFeedback(XRNode node)
    {
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (device.isValid)
        {
            float intensity = Mathf.Lerp(0.5f, maxIntensity, (normalRate - currentRate) / (normalRate - attackRate));
            device.SendHapticImpulse(0, intensity, 0.1f); // Pulse duration remains constant
        }
    }
}
