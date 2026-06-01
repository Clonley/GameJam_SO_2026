using UnityEngine;
using UnityEngine.InputSystem;

public class GJ_SO2026_CharacterControls : MonoBehaviour
{
    private CharacterController m_characterController;
    private PlayerInput m_playerInput;
    private Animator m_animator;

    public float m_moveSpeed = 5f;
    public float m_airDamping = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_playerInput = GetComponent<PlayerInput>();
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = m_playerInput.actions["Move"].ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        if (m_characterController.isGrounded)
        {
            m_characterController.Move(moveDirection * m_moveSpeed * Time.deltaTime);
        }
        else
        {
            m_characterController.Move((new Vector3 (0, -9.81f, 0) + moveDirection* m_moveSpeed* m_airDamping) * Time.deltaTime);
        }
    }
}
