using UnityEngine;
using UnityEngine.InputSystem;

public class GJ_SO2026_CharacterControls : MonoBehaviour
{
    private CharacterController m_characterController;
    private PlayerInput m_playerInput;
    private Transform m_playerTransform;

    private Vector2 m_Rotation;

    public float m_moveSpeed = 5f;
    public float m_sprintSpeed = 10f;
    public float m_jumpVelocity = 3f;
    public float m_airDamping = 0.5f;
    public float gravityMultiplier = 2.5f;
    public AudioClip[] m_footstepClips;
    public GJ_AudioManager m_audioManager;
    public float footstepInterval = 0.5f;
    private float m_yvelocity = 0f;

    public float maxCameraAngle = 45f;

    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerInput = GetComponent<PlayerInput>();
        m_playerTransform = GetComponent<Transform>();

        m_Rotation = transform.eulerAngles;
    }

    void Update()
    {
        if (m_characterController.isGrounded)
        {
            m_yvelocity = -2f;
        }
        else
        {
            m_yvelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
        Move();
        Rotate();
        if(m_playerInput.actions.FindAction("Jump").WasPressedThisFrame())
        {
            Jump();
        }
        FootstepAudio();
    }

    void Move()
    {
        Vector2 moveInput = m_playerInput.actions.FindAction("Move").ReadValue<Vector2>();

        bool isSprinting = m_playerInput.actions.FindAction("Sprint").IsPressed();

        float forwardSpeed = isSprinting ? m_sprintSpeed : m_moveSpeed;
        float strafeSpeed = m_moveSpeed; // no sprint sideways

        Vector3 localMove = new Vector3(moveInput.x * strafeSpeed, 0f, moveInput.y * forwardSpeed);

        Vector3 moveDirection = m_playerTransform.TransformDirection(localMove);

        Vector3 horizontal = new Vector3(moveDirection.x, 0f, moveDirection.z);

        if (!m_characterController.isGrounded) horizontal *= m_airDamping;

        moveDirection = horizontal + Vector3.up * m_yvelocity;

        moveDirection.y = m_yvelocity;

        m_characterController.Move(moveDirection * Time.deltaTime);
    }

    void Rotate()
    {
        Vector2 lookInput = m_playerInput.actions.FindAction("Look").ReadValue<Vector2>();
        m_Rotation += new Vector2(-lookInput.y, lookInput.x)*0.1f;
        m_Rotation.x = Mathf.Clamp(m_Rotation.x, -maxCameraAngle, maxCameraAngle);
        Quaternion player_rotation = Quaternion.Euler(0, m_Rotation.y, 0);
        transform.rotation = player_rotation;

        Vector3 camEuler = Camera.main.transform.rotation.eulerAngles;
        camEuler.x = m_Rotation.x;
        Camera.main.transform.rotation = Quaternion.Euler(camEuler);
    }

    void Jump()
    {
        if (m_characterController.isGrounded)
        {
            m_yvelocity = m_jumpVelocity;
            m_characterController.Move(Vector3.up * m_yvelocity * Time.deltaTime);
        }
    }

    void FootstepAudio()
    {
        float counter = 0f;
        if (m_characterController.isGrounded && m_playerInput.actions.FindAction("Move").IsPressed())
        {
            while (counter < footstepInterval*Mathf.Lerp(m_sprintSpeed/m_moveSpeed, 1, m_playerInput.actions.FindAction("Sprint").IsPressed() ? 0f : 1f))
            {
                int index = Random.Range(0, m_footstepClips.Length);
                AudioClip clip = m_footstepClips[index];
                if (m_audioManager != null)
                {
                    m_audioManager.PlayOneshot(clip);
                }
            }
        }
    }
}
