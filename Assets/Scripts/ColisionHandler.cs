using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour
{
    [SerializeField] private float levelLoadDelay = 2f;
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private AudioClip finishSound;
    [SerializeField] private ParticleSystem crashParticles;
    [SerializeField] private ParticleSystem finishParticles;
    [SerializeField] private float crashForce = 20f;

    private AudioSource audioSource;
    private bool isTransitioning;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) LoadNextLevel();
        if (Input.GetKeyDown(KeyCode.C)) ReloadLevel();
    }

    void OnCollisionEnter(Collision other)
    {
        if (isTransitioning) return;

        switch (other.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("Safe landing!");
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            case "Fuel":
                // Do nothing; let FuelPickup handle it via trigger
                break;
            default:
                StartCrashSequence(other);
                break;
        }
    }

    void StartSuccessSequence()
    {
        isTransitioning = true;
        audioSource.PlayOneShot(finishSound);
        finishParticles.Play();
        DisableMovement();
        Invoke(nameof(LoadNextLevel), levelLoadDelay);
    }

    void StartCrashSequence(Collision collision)
    {
        isTransitioning = true;
        audioSource.PlayOneShot(crashSound);
        crashParticles.Play();
        DisableMovement();

        Movement movement = GetComponent<Movement>();
        if (movement != null) movement.AddFuel(-movement.GetFuelPercentage() * 50f); // Lose 50% fuel

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDir = (transform.position - collision.contacts[0].point).normalized;
            rb.AddForce(forceDir * crashForce, ForceMode.Impulse);
        }
        Invoke(nameof(ReloadLevel), levelLoadDelay);
    }

    void DisableMovement()
    {
        Movement movement = GetComponent<Movement>();
        if (movement != null)
        {
            movement.enabled = false;
            AudioSource[] sources = GetComponents<AudioSource>();
            foreach (var source in sources) if (source != audioSource) source.Stop();
        }
    }

    void LoadNextLevel()
    {
        int nextScene = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextScene);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}