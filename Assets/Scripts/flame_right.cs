using UnityEngine;

public class RocketFlameLeftController : MonoBehaviour
{
    [SerializeField] private ParticleSystem rightThrusterEffect;

    void Start()
    {
        if (rightThrusterEffect != null)
        {
            rightThrusterEffect.Stop();
        }
    }

    void Update()
    {
        if (rightThrusterEffect == null) return;

        if (Input.GetKey(KeyCode.D))
        {
            if (!rightThrusterEffect.isPlaying)
            {
                rightThrusterEffect.Play();
                Debug.Log("Right thruster ON (pressing D)");
            }
        }
        else if (rightThrusterEffect.isPlaying)
        {
            rightThrusterEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Debug.Log("Right thruster OFF (released D)");
        }
    }
}
