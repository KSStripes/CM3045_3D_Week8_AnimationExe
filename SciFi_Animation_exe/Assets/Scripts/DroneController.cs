using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace SciFiAnimation
{
    /// <summary>
    /// Controls drone behavior including deployment, flight, and animations.
    /// Handles player input for movement, scanning, and emergency operations.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class DroneController : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float moveSpeed = 5f; // Horizontal movement speed (units/second)
        [SerializeField] private float verticalSpeed = 3f; // Vertical movement speed (units/second)
        [SerializeField] private float turnSpeed = 110f; // Rotation speed (degrees/second)
        [SerializeField] private Vector3 flightBounds = new Vector3(10f, 4f, 8f); // Clamp boundaries (X, Y, Z) around dock position

        [Header("References")]
        [SerializeField] private Transform visualRig; // Visual mesh that tilts during flight
        [SerializeField] private ParticleSystem leftThruster; // Left engine particle effect
        [SerializeField] private ParticleSystem rightThruster; // Right engine particle effect
        [SerializeField] private Light scanLight; // Light that activates during scan animation

        private Animator animator; // Animation controller
        private Vector3 dockPosition; // Starting position for returning to dock
        private bool deployed; // Whether drone is launched or docked
        private bool emergency; // Emergency mode activation state

        /// <summary>Current animation state (DOCKED, FLIGHT, LAUNCHING, DOCKING, SCANNING, EMERGENCY)</summary>
        public string CurrentMode { get; private set; } = "DOCKED";

        /// <summary>Initialize animator and store home position</summary>
        private void Awake()
        {
            animator = GetComponent<Animator>();
            dockPosition = transform.position; // Store starting dock position
        }

        /// <summary>Main update loop: process input, update state, handle flight, and visual effects</summary>
        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            HandleStateInput(keyboard); // Process deployment and action inputs
            UpdateMode(); // Sync current mode from animator state

            // Only allow flight controls when in flight
            if (CurrentMode == "FLIGHT" && !emergency)
            {
                HandleFlight(keyboard);
            }

            UpdateEffects(); // Update thruster and scan light effects
        }

        /// <summary>Process deployment, scan, and emergency mode inputs</summary>
        private void HandleStateInput(Keyboard keyboard)
        {
            // SPACE: Toggle deployment and emergency mode
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                bool canToggleDocking = CurrentMode == "DOCKED" || CurrentMode == "FLIGHT" || CurrentMode == "EMERGENCY";
                if (!canToggleDocking)
                {
                    return; // Prevent toggling during transitions
                }

                if (emergency)
                {
                    // Exit emergency mode
                    emergency = false;
                    animator.SetTrigger("Recover");
                }
                else if (deployed)
                {
                    // Dock the drone
                    deployed = false;
                    animator.SetTrigger("Dock");
                    transform.position = dockPosition; // Return to home
                    transform.rotation = Quaternion.identity; // Reset rotation
                }
                else
                {
                    // Launch the drone
                    deployed = true;
                    animator.SetTrigger("Launch");
                }
            }

            // F: Scan action (only while actively in flight)
            if (CurrentMode == "FLIGHT" && keyboard.fKey.wasPressedThisFrame)
            {
                animator.SetTrigger("Scan");
            }

            // X: Toggle emergency mode (only when deployed)
            if (deployed && keyboard.xKey.wasPressedThisFrame)
            {
                emergency = !emergency;
                animator.SetTrigger(emergency ? "Emergency" : "Recover");
            }
        }

        /// <summary>Handle drone movement and rotation based on keyboard input</summary>
        private void HandleFlight(Keyboard keyboard)
        {
            // Get axis inputs: W/S (forward), A/D (strafe), Q/E (vertical), Arrows (turn)
            float forward = Axis(keyboard.wKey, keyboard.sKey);
            float strafe = Axis(keyboard.dKey, keyboard.aKey);
            float vertical = Axis(keyboard.eKey, keyboard.qKey);
            float turn = Axis(keyboard.rightArrowKey, keyboard.leftArrowKey);

            // Calculate planar movement relative to drone orientation
            Vector3 planar = transform.forward * forward + transform.right * strafe;
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize(); // Prevent faster diagonal movement
            }

            // Apply movement forces
            transform.position += planar * (moveSpeed * Time.deltaTime);
            transform.position += Vector3.up * (vertical * verticalSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);

            // Clamp position within flight bounds relative to dock position
            Vector3 offset = transform.position - dockPosition;
            offset.x = Mathf.Clamp(offset.x, -flightBounds.x, flightBounds.x);
            offset.y = Mathf.Clamp(offset.y, 0f, flightBounds.y);
            offset.z = Mathf.Clamp(offset.z, -flightBounds.z, flightBounds.z);
            transform.position = dockPosition + offset;

            // Bank visual rig based on movement direction
            if (visualRig != null)
            {
                Quaternion bank = Quaternion.Euler(forward * 7f, 0f, -strafe * 10f);
                visualRig.localRotation = Quaternion.Slerp(visualRig.localRotation, bank, Time.deltaTime * 5f);
            }
        }

        /// <summary>Sync CurrentMode property with actual animator state</summary>
        private void UpdateMode()
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Docked")) CurrentMode = "DOCKED";
            else if (state.IsName("Launching")) CurrentMode = "LAUNCHING";
            else if (state.IsName("Docking")) CurrentMode = "DOCKING";
            else if (state.IsName("Scanning")) CurrentMode = "SCANNING";
            else if (state.IsName("Emergency")) CurrentMode = "EMERGENCY";
            else CurrentMode = "FLIGHT"; // Default to flight for all other states
        }

        /// <summary>Update particle effects and lights based on current state</summary>
        private void UpdateEffects()
        {
            // Thrusters active only when deployed and not in emergency
            bool thrustersOn = deployed && !emergency;
            SetEmission(leftThruster, thrustersOn);
            SetEmission(rightThruster, thrustersOn);

            // Scan light is controlled by animator during scan animation
            if (scanLight != null && CurrentMode != "SCANNING")
            {
                scanLight.enabled = false;
            }
        }

        /// <summary>Get axis value from two opposing keys (-1 to 1)</summary>
        private static float Axis(KeyControl positive, KeyControl negative)
        {
            return (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
        }

        /// <summary>Enable or disable particle emission for a system</summary>
        private static void SetEmission(ParticleSystem system, bool enabled)
        {
            if (system == null) return;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = enabled;
        }
    }
}
