using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace SciFiAnimation
{
    [RequireComponent(typeof(Animator))]
    public sealed class DroneController : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float verticalSpeed = 3f;
        [SerializeField] private float turnSpeed = 110f;
        [SerializeField] private Vector3 flightBounds = new Vector3(10f, 4f, 8f);

        [Header("References")]
        [SerializeField] private Transform visualRig;
        [SerializeField] private ParticleSystem leftThruster;
        [SerializeField] private ParticleSystem rightThruster;
        [SerializeField] private Light scanLight;

        private Animator animator;
        private Vector3 dockPosition;
        private bool deployed;
        private bool emergency;

        public string CurrentMode { get; private set; } = "DOCKED";

        private void Awake()
        {
            animator = GetComponent<Animator>();
            dockPosition = transform.position;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            HandleStateInput(keyboard);
            UpdateMode();

            if (deployed && !emergency)
            {
                HandleFlight(keyboard);
            }

            UpdateEffects();
        }

        private void HandleStateInput(Keyboard keyboard)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                bool canToggleDocking = CurrentMode == "DOCKED" || CurrentMode == "FLIGHT" || CurrentMode == "EMERGENCY";
                if (!canToggleDocking)
                {
                    return;
                }

                if (emergency)
                {
                    emergency = false;
                    animator.SetTrigger("Recover");
                }
                else if (deployed)
                {
                    deployed = false;
                    animator.SetTrigger("Dock");
                    transform.position = dockPosition;
                    transform.rotation = Quaternion.identity;
                }
                else
                {
                    deployed = true;
                    animator.SetTrigger("Launch");
                }
            }

            if (deployed && keyboard.fKey.wasPressedThisFrame)
            {
                animator.SetTrigger("Scan");
            }

            if (deployed && keyboard.xKey.wasPressedThisFrame)
            {
                emergency = !emergency;
                animator.SetTrigger(emergency ? "Emergency" : "Recover");
            }
        }

        private void HandleFlight(Keyboard keyboard)
        {
            float forward = Axis(keyboard.wKey, keyboard.sKey);
            float strafe = Axis(keyboard.dKey, keyboard.aKey);
            float vertical = Axis(keyboard.eKey, keyboard.qKey);
            float turn = Axis(keyboard.rightArrowKey, keyboard.leftArrowKey);

            Vector3 planar = transform.forward * forward + transform.right * strafe;
            if (planar.sqrMagnitude > 1f)
            {
                planar.Normalize();
            }

            transform.position += planar * (moveSpeed * Time.deltaTime);
            transform.position += Vector3.up * (vertical * verticalSpeed * Time.deltaTime);
            transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime, Space.World);

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

        private void UpdateMode()
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Docked")) CurrentMode = "DOCKED";
            else if (state.IsName("Launching")) CurrentMode = "LAUNCHING";
            else if (state.IsName("Docking")) CurrentMode = "DOCKING";
            else if (state.IsName("Scanning")) CurrentMode = "SCANNING";
            else if (state.IsName("Emergency")) CurrentMode = "EMERGENCY";
            else CurrentMode = "FLIGHT";
        }

        private void UpdateEffects()
        {
            bool thrustersOn = deployed && !emergency;
            SetEmission(leftThruster, thrustersOn);
            SetEmission(rightThruster, thrustersOn);

            if (scanLight != null && CurrentMode != "SCANNING")
            {
                scanLight.enabled = false;
            }
        }

        private static float Axis(KeyControl positive, KeyControl negative)
        {
            return (positive.isPressed ? 1f : 0f) - (negative.isPressed ? 1f : 0f);
        }

        private static void SetEmission(ParticleSystem system, bool enabled)
        {
            if (system == null) return;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = enabled;
        }
    }
}
