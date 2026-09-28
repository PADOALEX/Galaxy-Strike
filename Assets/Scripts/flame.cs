using UnityEngine;

public class RocketFlameController : MonoBehaviour
{
    [SerializeField] private ParticleSystem flameEffect; // Main thrust flame (Space)
    [SerializeField] private ParticleSystem turboFlameEffect; // Turbo flame (Left Shift)
    [SerializeField] private float turboEmissionRate = 50f; // Max emission rate for turbo flame
    [SerializeField] private float fadeSpeed = 5f; // Speed of emission fade in/out

    private float currentTurboEmission;

    void Start()
    {
        // Get particle systems if not assigned in Inspector
        if (flameEffect == null) flameEffect = GetComponentInChildren<ParticleSystem>();
        if (turboFlameEffect == null)
        {
            Debug.LogWarning("TurboFlameEffect not assigned; looking for second ParticleSystem.");
            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
            if (systems.Length > 1) turboFlameEffect = systems[1]; // Assume second system is turbo
        }

        // Initialize states
        if (flameEffect != null) flameEffect.Stop();
        if (turboFlameEffect != null) turboFlameEffect.Stop();
        currentTurboEmission = 0f;
    }

    void Update()
    {
        if (flameEffect == null || turboFlameEffect == null)
        {
            Debug.LogError("One or both ParticleSystems are missing!");
            return;
        }

        // Main flame (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!flameEffect.isPlaying)
            {
                flameEffect.Play();
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (flameEffect.isPlaying)
            {
                flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        // Turbo flame (Left Shift) with smooth transition
        bool isBoosting = Input.GetKey(KeyCode.LeftShift);
        float targetEmission = isBoosting ? turboEmissionRate : 0f;
        currentTurboEmission = Mathf.Lerp(currentTurboEmission, targetEmission, Time.deltaTime * fadeSpeed);

        var emission = turboFlameEffect.emission;
        emission.rateOverTime = currentTurboEmission;

        if (!turboFlameEffect.isPlaying && currentTurboEmission > 0.1f)
        {
            turboFlameEffect.Play();
        }
        else if (turboFlameEffect.isPlaying && currentTurboEmission < 0.1f)
        {
            turboFlameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}