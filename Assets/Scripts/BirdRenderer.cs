using UnityEngine;

[RequireComponent(typeof(Boid))] //! IMPORTANT
//This allows the Flock class to instance the Prefab as type Boid 
public class BirdRenderer : MonoBehaviour
{
    private Boid m_boid = null; //Private referenc to the boid
    private Animator m_animator;
    private float m_wingsFlappingSpeed;

    void Start()
    {
        m_boid = GetComponent<Boid>();
        m_animator = GetComponentInChildren<Animator>();
        Debug.Log(m_animator);
        m_wingsFlappingSpeed = Random.Range(1.5f, 2.5f);
    }
    public float AnimationSpeed
    {
        get => m_animator.speed;
        set => m_animator.speed = value;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boid == null) return;

        Vector3 velocity = m_boid.Velocity;
        Vector3 position = m_boid.Position;

        // Check for invalid values
        if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z) ||
            float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
        {
            Debug.LogError($"Invalid position or velocity detected for {gameObject.name}. Pos: {position}, Vel: {velocity}");
            return;
        }

        if (velocity.sqrMagnitude > 0.001f)
        {
            Quaternion target = Quaternion.LookRotation(velocity, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * 6);
        }

        transform.position = position;
        
        if (transform.parent != null)
        {
            transform.position += new Vector3(0, 0, transform.parent.position.z); //inherit z-position
        }

        if (m_animator != null && !float.IsNaN(velocity.magnitude))
        {
            m_animator.speed = m_wingsFlappingSpeed - (velocity.magnitude * 0.06f);
        }
    }
}
