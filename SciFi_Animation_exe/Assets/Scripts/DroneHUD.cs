using UnityEngine;

namespace SciFiAnimation
{
    public sealed class DroneHUD : MonoBehaviour
    {
        [SerializeField] private DroneController drone;

        public void SetDrone(DroneController target)
        {
            drone = target;
        }

        private void OnGUI()
        {
            float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.4f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUIStyle title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.2f, 0.95f, 1f) }
            };
            GUIStyle text = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.78f, 0.92f, 1f) }
            };
            GUIStyle mode = new GUIStyle(title)
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = drone != null && drone.CurrentMode == "EMERGENCY" ? Color.red : new Color(0.4f, 1f, 0.65f) }
            };

            GUI.Box(new Rect(18, 18, 340, 178), GUIContent.none);
            GUI.Label(new Rect(34, 28, 300, 30), "A-17 AUTONOMOUS DRONE", title);
            GUI.Label(new Rect(34, 62, 310, 120),
                "SPACE   Launch / dock / recover\n" +
                "W A S D   Fly\n" +
                "Q / E      Descend / ascend\n" +
                "ARROWS   Turn\n" +
                "F             Energy scan\n" +
                "X             Emergency mode", text);

            string state = drone == null ? "OFFLINE" : drone.CurrentMode;
            GUI.Label(new Rect(Screen.width / scale - 270, 24, 240, 34), state, mode);
            GUI.matrix = previous;
        }
    }
}
