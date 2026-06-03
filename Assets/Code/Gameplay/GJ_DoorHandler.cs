using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class GJ_DoorHandler : MonoBehaviour
{
    public GJ_DoorHandler m_linkedDoor;
    public bool m_active = true;
    public bool m_autoActivateParented = true;
    private PlayerInput m_playerInput;
    private Transform m_doorTransform;
    private GameObject m_player;

    void Start()
    {
        m_doorTransform = transform.Find("DoorTeleport_Transform");
        if (m_linkedDoor == null) Debug.Log("Linked door missing on " + name);
        if (m_doorTransform == null ) Debug.Log("Linked door transform missing on " + name);

        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerInput = m_player.GetComponent<PlayerInput>();
    }
    void Update()
    {
        
    }

    private Coroutine interactionRoutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && interactionRoutine == null && m_active)
        {
            interactionRoutine = StartCoroutine(DoorInteraction());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && interactionRoutine != null)
        {
            StopCoroutine(interactionRoutine);
            interactionRoutine = null;
        }
    }

    IEnumerator DoorInteraction()
    {
        Debug.Log("Door interaction started. Waiting for player input...");

        while (!m_playerInput.actions["Interact"].WasPressedThisFrame()) yield return null;

        CharacterController cc = m_player.GetComponent<CharacterController>();
        cc.enabled = false;

        m_player.transform.SetPositionAndRotation(m_linkedDoor.m_doorTransform.position, m_linkedDoor.m_doorTransform.rotation);

        Physics.SyncTransforms();

        cc.enabled = true;
        yield return new WaitForSeconds(0.1f);
        if (m_autoActivateParented)
        {
            m_linkedDoor.StartInteraction();
        }
    }
    public void StartInteraction()
    {
        if (interactionRoutine == null) interactionRoutine = StartCoroutine(DoorInteraction());
    }
}
