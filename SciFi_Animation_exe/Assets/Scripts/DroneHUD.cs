using UnityEngine;

// Draws a simple on-screen panel with control hints and the current drone mode.
public class DroneHUD : MonoBehaviour
{
    [SerializeField] private DroneController drone;

    // Lets other scripts assign the drone reference if needed.
    public void SetDrone(DroneController target)
    {
        drone = target;
    }

    // Renders the HUD each frame.
    // Added GUI styling functionality from Unity's inbuilt system to make the HUD more visually appealing and readable.
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
            normal = { textColor = drone != null && drone.CurrentMode == DroneController.DroneMode.Emergency ? Color.red : new Color(0.4f, 1f, 0.65f) }
        };

        GUI.Box(new Rect(18, 18, 340, 178), GUIContent.none);
        GUI.Label(new Rect(34, 28, 300, 30), "A-17 AUTONOMOUS DRONE", title);
        GUI.Label(new Rect(34, 62, 310, 120),
            "SPACE   Launch / Dock / Recover\n" +
            "W A S D   Fly\n" +
            "Q / E      Descend / Ascend\n" +
            "ARROWS   Turn\n" +
            "F             Test Scanning Function\n" +
            "X             Test Emergency Mode", text);

        string state = drone == null ? "OFFLINE" : drone.CurrentMode.ToString().ToUpperInvariant();
        GUI.Label(new Rect(Screen.width / scale - 270, 24, 240, 34), state, mode);

        GUI.matrix = previous;
    }
}
