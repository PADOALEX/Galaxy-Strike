using UnityEngine;
using System.Collections;

public class RotatingFuelCanister : MonoBehaviour
{
    [Header("Fuel Settings")]
    [SerializeField] private float fuelAmount = 30f;
    
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0); // Default rotation around Y axis
    [SerializeField] private float rotationSpeed = 50f; // Degrees per second
    [SerializeField] private float hoverHeight = 0.2f; // How high it hovers
    [SerializeField] private float hoverSpeed = 1f; // Speed of hover animation
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float maxVolume = 0.7f;
    
    private AudioSource audioSource;
    private bool isBeingCollected = false;
    private Vector3 startPosition;
    
    void Start()
    {
        // Store the starting position for hover effect
        startPosition = transform.position;
        
        // Add AudioSource component if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure the audio source but don't play it yet
        audioSource.clip = pickupSound;
        audioSource.volume = 0f;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }
    
    void Update()
    {
        if (!isBeingCollected)
        {
            // Rotate the canister
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
            
            // Add a gentle hovering effect
            float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Movement movement = other.GetComponent<Movement>();
        if (movement != null && !isBeingCollected)
        {
            isBeingCollected = true;
            
            // Add fuel to the player
            movement.AddFuel(fuelAmount);
            
            // Play sound and start the collection process
            StartCoroutine(PlaySoundAndCollect());
        }
    }
    
    private IEnumerator PlaySoundAndCollect()
    {
        // Start playing the sound
        audioSource.Play();
        
        // Fade in the sound
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            float newVolume = Mathf.Lerp(0f, maxVolume, elapsedTime / fadeInDuration);
            audioSource.volume = newVolume;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Make sure we reach target volume
        audioSource.volume = maxVolume;
        
        // Add a pick-up animation - float upward and spin faster
        float collectionDuration = 0.5f;
        float initialYPos = transform.position.y;
        Vector3 initialScale = transform.localScale;
        Quaternion initialRotation = transform.rotation;
        elapsedTime = 0f;
        
        while (elapsedTime < collectionDuration)
        {
            // Move upward
            float yOffset = Mathf.Lerp(0, 1f, elapsedTime / collectionDuration);
            transform.position = new Vector3(
                transform.position.x,
                initialYPos + yOffset,
                transform.position.z
            );
            
            // Spin faster
            transform.Rotate(rotationAxis, rotationSpeed * 3 * Time.deltaTime);
            
            // Shrink slightly
            float scaleFactor = Mathf.Lerp(1, 0.5f, elapsedTime / collectionDuration);
            transform.localScale = initialScale * scaleFactor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Hide the visual component
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        // Disable the collider
        Collider canisterCollider = GetComponent<Collider>();
        if (canisterCollider != null)
        {
            canisterCollider.enabled = false;
        }
        
        // Fade out the sound
        elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            float newVolume = Mathf.Lerp(maxVolume, 0f, elapsedTime / fadeOutDuration);
            audioSource.volume = newVolume;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the volume is zero before destroying
        audioSource.volume = 0f;
        
        // Destroy the game object after the sound has finished
        Destroy(gameObject);
    }
}