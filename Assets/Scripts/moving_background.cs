using UnityEngine;

public class MovingBackground : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = new Vector3(0, 0, 1); // Default: forward along Z-axis
    [SerializeField] private float moveSpeed = 0.1f; // Very slow movement speed
    
    [Header("Looping Settings (Optional)")]
    [SerializeField] private bool enableLooping = false;
    [SerializeField] private float resetDistance = 100f; // Distance before resetting position
    [SerializeField] private Vector3 resetPosition; // Position to reset to when distance threshold is reached
    
    private Vector3 initialPosition;
    
    void Start()
    {
        // Store the initial position
        initialPosition = transform.position;
        
        // Normalize the direction
        moveDirection = moveDirection.normalized;
    }
    
    void Update()
    {
        // Move the background slowly in the specified direction
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
        // Handle looping if enabled
        if (enableLooping)
        {
            // Calculate the distance traveled from the starting position
            float distanceTraveled = Vector3.Distance(transform.position, initialPosition);
            
            // If we've moved beyond the reset threshold, return to the reset position
            if (distanceTraveled >= resetDistance)
            {
                transform.position = resetPosition;
            }
        }
    }
}