using UnityEngine;
using UnityEngine.InputSystem;

public class GJ_SO2026_CharacterControls : MonoBehaviour
{
    private CharacterController m_characterController;
    private PlayerInput m_playerInput;
    private Transform m_cameraTransform;

    private Transform m_playerTransform;

    private Vector2 m_Rotation;

    public float m_moveSpeed = 5f;
    public float m_airDamping = 0.5f;

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
        Move();
        Rotate();
    }

    void Move()
    {
        Vector2 moveInput = m_playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        moveDirection = m_playerTransform.TransformDirection(moveDirection);

        if (m_characterController.isGrounded)
        {
            m_characterController.Move(moveDirection * m_moveSpeed * Time.deltaTime);
        }
        else
        {
            m_characterController.Move((new Vector3(0, -9.81f, 0) + moveDirection * m_moveSpeed * m_airDamping) * Time.deltaTime);
        }
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
}
