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
    public float m_jumpVelocity = 5f;
    public float m_jumpCooldown = 0.5f;
    private float m_lastJumpTime = Mathf.Infinity;
    public float m_airDamping = 0.5f;
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
        if (m_characterController.isGrounded&& !m_playerInput.actions.FindAction("Jump").WasPressedThisFrame())
        {
            m_yvelocity = -0.1f;
        }
        else
        {
            m_yvelocity += Physics.gravity.y * Time.deltaTime;
        }
        Move();
        Rotate();
        m_lastJumpTime += Time.deltaTime;
        if(m_playerInput.actions.FindAction("Jump").WasPressedThisFrame() && m_lastJumpTime >= m_jumpCooldown)
        {
            Jump();
        }
    }

    void Move()
    {
        Vector2 moveInput = m_playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        moveDirection = m_playerTransform.TransformDirection(moveDirection);
        m_characterController.Move((new Vector3(0, m_yvelocity, 0) + moveDirection * (m_playerInput.actions.FindAction("Sprint").IsPressed() ? m_sprintSpeed : m_moveSpeed) * m_airDamping) * Time.deltaTime);
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
            m_lastJumpTime = 0f;
        }
    }
}
