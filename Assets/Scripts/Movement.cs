using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private InputAction thrustAction;
    [SerializeField] private InputAction rotateAction;
    [SerializeField] private InputAction boostAction;
    [SerializeField] private float thrustForce = 300f;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float boostForce = 400f; // Reduced for continuous boost balance
    [SerializeField] private float maxAngularSpeed = 1.5f;
    [SerializeField] private float rotationDamping = 0.1f;

    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float fuelConsumptionRate = 2f;
    [SerializeField] private float boostFuelRate = 10f; // Fuel cost per second while boosting
    private float currentFuel;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip thrustSound;
    [SerializeField] private AudioClip boostSound;
    [SerializeField] private AudioClip fallingSound;
    [SerializeField] private float audioFadeSpeed = 2f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem thrustParticles;
    [SerializeField] private ParticleSystem leftThrusterParticles;
    [SerializeField] private ParticleSystem rightThrusterParticles;
    [SerializeField] private ParticleSystem boostParticles;

    private Rigidbody rb;
    private AudioSource thrustAudio;
    private AudioSource boostAudio;
    private AudioSource fallAudio;
    private float currentRotationSpeed;
    private bool isBoosting;

    public float MaxFuel => maxFuel;

    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ | 
                         RigidbodyConstraints.FreezeRotationX | 
                         RigidbodyConstraints.FreezeRotationY;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.mass = 1f;

        SetupAudio();
        currentFuel = maxFuel;

        if (thrustParticles != null) thrustParticles.Stop();
        if (leftThrusterParticles != null) leftThrusterParticles.Stop();
        if (rightThrusterParticles != null) rightThrusterParticles.Stop();
        if (boostParticles != null) boostParticles.Stop();
    }

    void SetupAudio()
    {
        thrustAudio = gameObject.AddComponent<AudioSource>();
        boostAudio = gameObject.AddComponent<AudioSource>();
        fallAudio = gameObject.AddComponent<AudioSource>();

        thrustAudio.clip = thrustSound; thrustAudio.loop = true;
        boostAudio.clip = boostSound; boostAudio.loop = true; // Loop for continuous boost
        fallAudio.clip = fallingSound; fallAudio.loop = true;
    }

    void OnEnable()
    {
        thrustAction.Enable();
        rotateAction.Enable();
        boostAction.Enable();
    }

    void OnDisable()
    {
        thrustAction.Disable();
        rotateAction.Disable();
        boostAction.Disable();
    }

    void FixedUpdate()
    {
        ProcessThrust();
        ProcessRotation();
        ProcessBoost();
        Debug.Log($"Fuel: {currentFuel:F1}/{maxFuel}, Velocity: {rb.linearVelocity.y:F2}, Boosting: {isBoosting}");
    }

    void ProcessThrust()
    {
        float thrustInput = thrustAction.ReadValue<float>();
        Debug.Log($"Thrust Input: {thrustInput}");

        if (thrustInput > 0 && currentFuel > 0)
        {
            rb.AddRelativeForce(Vector3.up * thrustForce * thrustInput * Time.fixedDeltaTime);
            currentFuel -= fuelConsumptionRate * thrustInput * Time.fixedDeltaTime;
            currentFuel = Mathf.Max(currentFuel, 0f);

            if (!thrustAudio.isPlaying) thrustAudio.Play();
            thrustAudio.volume = Mathf.Lerp(thrustAudio.volume, thrustInput * 0.5f, Time.fixedDeltaTime * audioFadeSpeed);
            if (thrustParticles != null && !thrustParticles.isPlaying) thrustParticles.Play();

            fallAudio.volume = Mathf.Lerp(fallAudio.volume, 0f, Time.fixedDeltaTime * audioFadeSpeed);
            if (fallAudio.volume < 0.01f && fallAudio.isPlaying) fallAudio.Stop();
        }
        else if (rb.linearVelocity.y < -0.5f)
        {
            if (!fallAudio.isPlaying) fallAudio.Play();
            fallAudio.volume = Mathf.Lerp(fallAudio.volume, 0.3f, Time.fixedDeltaTime * audioFadeSpeed);

            thrustAudio.volume = Mathf.Lerp(thrustAudio.volume, 0f, Time.fixedDeltaTime * audioFadeSpeed);
            if (thrustAudio.volume < 0.01f && thrustAudio.isPlaying) thrustAudio.Stop();

            if (thrustParticles != null && thrustParticles.isPlaying) thrustParticles.Stop();
        }
        else
        {
            thrustAudio.volume = Mathf.Lerp(thrustAudio.volume, 0f, Time.fixedDeltaTime * audioFadeSpeed);
            fallAudio.volume = Mathf.Lerp(fallAudio.volume, 0f, Time.fixedDeltaTime * audioFadeSpeed);

            if (thrustAudio.volume < 0.01f && thrustAudio.isPlaying) thrustAudio.Stop();
            if (fallAudio.volume < 0.01f && fallAudio.isPlaying) fallAudio.Stop();

            if (thrustParticles != null && thrustParticles.isPlaying) thrustParticles.Stop();
        }
    }

    void ProcessRotation()
    {
        float rotateInput = rotateAction.ReadValue<float>();
        if (rotateInput != 0)
        {
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, rotateInput * rotationSpeed, Time.fixedDeltaTime * 5f);
            rb.AddTorque(Vector3.back * currentRotationSpeed * Time.fixedDeltaTime);

            if (rotateInput > 0 && rightThrusterParticles != null && !rightThrusterParticles.isPlaying)
                rightThrusterParticles.Play();
            else if (rotateInput < 0 && leftThrusterParticles != null && !leftThrusterParticles.isPlaying)
                leftThrusterParticles.Play();
        }
        else
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, rotationDamping);
            if (leftThrusterParticles != null && leftThrusterParticles.isPlaying) leftThrusterParticles.Stop();
            if (rightThrusterParticles != null && rightThrusterParticles.isPlaying) rightThrusterParticles.Stop();
        }
        rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularSpeed);
    }

    void ProcessBoost()
    {
        float boostInput = boostAction.ReadValue<float>(); // 1 when held, 0 when released

        if (boostInput > 0 && currentFuel > 0)
        {
            if (!isBoosting)
            {
                isBoosting = true;
                if (!boostAudio.isPlaying) boostAudio.Play();
                if (boostParticles != null && !boostParticles.isPlaying) boostParticles.Play();
            }

            rb.AddRelativeForce(Vector3.up * boostForce * Time.fixedDeltaTime);
            currentFuel -= boostFuelRate * Time.fixedDeltaTime;
            currentFuel = Mathf.Max(currentFuel, 0f);

            boostAudio.volume = Mathf.Lerp(boostAudio.volume, 0.7f, Time.fixedDeltaTime * audioFadeSpeed);
        }
        else if (isBoosting)
        {
            isBoosting = false;
            boostAudio.volume = Mathf.Lerp(boostAudio.volume, 0f, Time.fixedDeltaTime * audioFadeSpeed);
            if (boostAudio.volume < 0.01f && boostAudio.isPlaying) boostAudio.Stop();
            if (boostParticles != null && boostParticles.isPlaying) boostParticles.Stop();
        }
    }

    public float GetFuelPercentage() => currentFuel / maxFuel;
    public void AddFuel(float amount) => currentFuel = Mathf.Clamp(currentFuel + amount, 0f, maxFuel);
}