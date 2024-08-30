using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Start()
    {
        currentRate = normalRate;
        StartCoroutine(HeartbeatCoroutine());
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
