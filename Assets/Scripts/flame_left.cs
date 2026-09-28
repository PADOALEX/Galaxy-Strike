using UnityEngine;

public class RocketFlameRightController : MonoBehaviour
{
    [SerializeField] private ParticleSystem leftThrusterEffect;

    void Start()
    {
        if (leftThrusterEffect != null)
        {
            leftThrusterEffect.Stop();
        }
    }

    void Update()
    {
        if (leftThrusterEffect == null) return;

        if (Input.GetKey(KeyCode.A))
        {
            if (!leftThrusterEffect.isPlaying)
            {
                leftThrusterEffect.Play();
                Debug.Log("Left thruster ON (pressing A)");
            }
        }
        else if (leftThrusterEffect.isPlaying)
        {
            leftThrusterEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Debug.Log("Left thruster OFF (released A)");
        }
    }
}
