using UnityEngine;

namespace SciFiAnimation
{
    /// <summary>
    /// Displays HUD overlay with drone status and control instructions.
    /// Shows current mode (DOCKED, FLIGHT, EMERGENCY, etc.) and color-coded controls.
    /// </summary>
    public sealed class DroneHUD : MonoBehaviour
    {
        [SerializeField] private DroneController drone; // Reference to the drone being monitored

        /// <summary>Set the drone reference for status display</summary>
        public void SetDrone(DroneController target)
        {
            drone = target;
        }

        /// <summary>Render HUD overlay with drone status and control instructions</summary>
        private void OnGUI()
        {
            // Scale GUI based on screen height for responsive layout
            float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.4f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            // Define GUI styles for title, text, and mode display
            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) } // Cyan title color
            };
            GUIStyle text = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.78f, 0.92f, 1f) } // Light blue text
            };
            // Mode style: red during emergency, green during normal flight
            GUIStyle mode = new GUIStyle(title)
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = drone != null && drone.CurrentMode == "EMERGENCY" ? Color.red : new Color(0.4f, 1f, 0.65f) }
            };

            // Draw main control information box
            GUI.Box(new Rect(18, 18, 340, 178), GUIContent.none);
            GUI.Label(new Rect(34, 28, 300, 30), "A-17 AUTONOMOUS DRONE", title);
            GUI.Label(new Rect(34, 62, 310, 120),
                "SPACE   Launch / Dock / Recover\n" +
                "W A S D   Fly\n" +
                "Q / E      Descend / Ascend\n" +
                "ARROWS   Turn\n" +
                "F             Test Scanning Function\n" +
                "X             Test Emergency Mode", text);

            // Display current drone mode in top-right corner
            string state = drone == null ? "OFFLINE" : drone.CurrentMode;
            GUI.Label(new Rect(Screen.width / scale - 270, 24, 240, 34), state, mode);

            // Restore previous GUI matrix
            GUI.matrix = previous;
        }
    }
}
