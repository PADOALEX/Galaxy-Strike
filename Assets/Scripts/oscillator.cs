using UnityEngine;

public class oscillator : MonoBehaviour
{
    [SerializeField]Vector3 MovementVector;
    [SerializeField]float speed;
    Vector3 startposition;
    Vector3 endPosition;
    float movementFactor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startposition = transform.position;
        endPosition = transform.position + MovementVector;
    }

    // Update is called once per frame
    void Update()
    {
        movementFactor = Mathf.PingPong(Time.time * speed, 1f);
       transform.position = Vector3.Lerp(startposition, endPosition, movementFactor); 
    }
}