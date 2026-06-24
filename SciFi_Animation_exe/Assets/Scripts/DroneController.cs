using UnityEngine;
using UnityEngine.InputSystem;

// Controls drone launch, flight, docking, scanning, and emergency behavior.
[RequireComponent(typeof(Animator))]
public class DroneController : MonoBehaviour
{
    // Animator-driven gameplay states used by the HUD and input logic.
    public enum DroneMode
    {
        Docked,
        Launching,
        Flight,
        Docking,
        Scanning,
        Emergency
    }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float turnSpeed = 110f;
    [SerializeField] private Vector3 flightBounds = new Vector3(10f, 4f, 8f);
    [SerializeField] private Transform visualRig;
    [SerializeField] private ParticleSystem leftThruster;
    [SerializeField] private ParticleSystem rightThruster;
    [SerializeField] private Light scanLight;

    private Animator animator;
    private Vector3 dockPosition;
    private Quaternion dockRotation;
    private bool deployed;
    private bool emergency;

    public DroneMode CurrentMode { get; private set; } = DroneMode.Docked;

    // Cache the animator and the starting dock position and rotation.
    private void Awake()
    {
        animator = GetComponent<Animator>();
        dockPosition = transform.position;
        dockRotation = transform.rotation;
    }

    // Reads player input and routes it to launch, flight, scan, or emergency actions.
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        UpdateMode();

        if (keyboard.spaceKey.wasPressedThisFrame && (CurrentMode == DroneMode.Docked || CurrentMode == DroneMode.Flight || CurrentMode == DroneMode.Emergency))
        {
            if (emergency)
            {
                emergency = false;
                animator.SetTrigger("Recover");
            }
            else if (deployed)
            {
                deployed = false;
                animator.SetTrigger("Dock");
                transform.SetPositionAndRotation(dockPosition, dockRotation);
            }
            else
            {
                deployed = true;
                animator.SetTrigger("Launch");
            }
        }

        if (CurrentMode == DroneMode.Flight && keyboard.fKey.wasPressedThisFrame)
        {
            animator.SetTrigger("Scan");
        }

        if (deployed && keyboard.xKey.wasPressedThisFrame)
        {
            emergency = !emergency;
            animator.SetTrigger(emergency ? "Emergency" : "Recover");
        }

        if (CurrentMode == DroneMode.Flight && !emergency)
        {
            HandleFlight(keyboard);
        }

        UpdateEffects();
    }

    // Moves and turns the drone while it is in flight.
    private void HandleFlight(Keyboard keyboard)
    {
        float forward = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
        float strafe = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
        float vertical = (keyboard.eKey.isPressed ? 1f : 0f) - (keyboard.qKey.isPressed ? 1f : 0f);
        float turn = (keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.leftArrowKey.isPressed ? 1f : 0f);

        Vector3 planar = transform.forward * forward + transform.right * strafe;
        if (planar.sqrMagnitude > 1f)
        {
            planar.Normalize();
        }

        transform.position += planar * (moveSpeed * Time.deltaTime);
        transform.position += Vector3.up * (vertical * verticalSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);

        // Keep the drone inside the allowed flight area around the dock.
        Vector3 offset = transform.position - dockPosition;
        offset.x = Mathf.Clamp(offset.x, -flightBounds.x, flightBounds.x);
        offset.y = Mathf.Clamp(offset.y, 0f, flightBounds.y);
        offset.z = Mathf.Clamp(offset.z, -flightBounds.z, flightBounds.z);
        transform.position = dockPosition + offset;

        if (visualRig != null)
        {
            Quaternion bank = Quaternion.Euler(forward * 7f, 0f, -strafe * 10f);
            visualRig.localRotation = Quaternion.Slerp(visualRig.localRotation, bank, Time.deltaTime * 5f);
        }
    }

    // Converts the Animator's current state into a simpler gameplay mode.
    private void UpdateMode()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Docked")) CurrentMode = DroneMode.Docked;
        else if (state.IsName("Launching")) CurrentMode = DroneMode.Launching;
        else if (state.IsName("Docking")) CurrentMode = DroneMode.Docking;
        else if (state.IsName("Scanning")) CurrentMode = DroneMode.Scanning;
        else if (state.IsName("Emergency")) CurrentMode = DroneMode.Emergency;
        else CurrentMode = DroneMode.Flight;
    }

    // Turns thrusters on and off and keeps the scan light disabled outside the scan state.
    private void UpdateEffects()
    {
        bool thrustersOn = deployed && !emergency;

        if (leftThruster != null)
        {
            ParticleSystem.EmissionModule emission = leftThruster.emission;
            emission.enabled = thrustersOn;
        }

        if (rightThruster != null)
        {
            ParticleSystem.EmissionModule emission = rightThruster.emission;
            emission.enabled = thrustersOn;
        }

        if (scanLight != null && CurrentMode != DroneMode.Scanning)
        {
            scanLight.enabled = false;
        }
    }
}
