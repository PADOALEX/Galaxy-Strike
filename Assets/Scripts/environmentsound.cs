using UnityEngine;
using System.Collections;

public class EnvironmentSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float maxVolume = 0.3f; // Keep it as background music
    [SerializeField] private float fadeInDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 2.5f;
    
    private AudioSource audioSource;
    private bool isFadingOut = false;
    
    void Start()
    {
        // Add an AudioSource component if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure the audio source
        audioSource.clip = backgroundMusic;
        audioSource.volume = 0f; // Start with zero volume
        audioSource.loop = true; // Loop the background music
        audioSource.spatialBlend = 0f; // Make it 2D sound (non-positional)
        audioSource.Play();
        
        // Start fading in
        StartCoroutine(FadeIn());
    }
    
    void Update()
    {
        // No need for anything in Update for this functionality
    }
    
    // Called when another collider enters this object's trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object has the "Finish" tag
        if (other.CompareTag("Finish") && !isFadingOut)
        {
            // Start fading out the music
            isFadingOut = true;
            StartCoroutine(FadeOut());
        }
    }
    
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            // Calculate the volume based on elapsed time
            float newVolume = Mathf.Lerp(0f, maxVolume, elapsedTime / fadeInDuration);
            audioSource.volume = newVolume;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we reach the exact target volume
        audioSource.volume = maxVolume;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startVolume = audioSource.volume;
        
        while (elapsedTime < fadeOutDuration)
        {
            // Calculate the volume based on elapsed time
            float newVolume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutDuration);
            audioSource.volume = newVolume;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the volume is zero and stop playing
        audioSource.volume = 0f;
        audioSource.Stop();
    }
}