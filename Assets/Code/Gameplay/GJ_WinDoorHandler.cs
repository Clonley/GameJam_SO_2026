using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class GJ_WinDoorHandler : MonoBehaviour
{
    private PlayerInput m_playerInput;
    private GameObject m_player;
    public GJ_PlayerStateScript m_playerStateScript;

    void Start()
    {

        m_player = GameObject.FindGameObjectWithTag("Player");
        m_playerInput = m_player.GetComponent<PlayerInput>();
    }
    void Update()
    {
        
    }

    private Coroutine interactionRoutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && interactionRoutine == null)
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
        m_playerStateScript.Win_willNeverDoThatInLife();
    }
}
