using UnityEngine;

namespace HOne.Platformer
{
    public sealed class LevelHud2D : MonoBehaviour
    {
        private bool completed;
        private GUIStyle titleStyle;
        private GUIStyle detailStyle;
        private GUIStyle completeStyle;

        public void SetCompleted()
        {
            completed = true;
        }

        private void OnGUI()
        {
            EnsureStyles();
            var scale = Mathf.Clamp(Screen.height / 720f, 0.8f, 1.35f);
            var panel = new Rect(24f, 24f, 264f * scale, 76f * scale);
            GUI.color = new Color(0.035f, 0.075f, 0.09f, 0.88f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = new Color(0.25f, 0.92f, 0.76f, 1f);
            GUI.DrawTexture(new Rect(panel.x, panel.y, 3f * scale, panel.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(panel.x + 16f * scale, panel.y + 11f * scale, panel.width - 24f * scale, 30f * scale), "NORTHLINE  /  01", titleStyle);
            GUI.Label(new Rect(panel.x + 16f * scale, panel.y + 44f * scale, panel.width - 24f * scale, 22f * scale), "WASD   MOVE   /   REACH THE GATE", detailStyle);

            if (completed)
            {
                var message = new Rect(Screen.width * 0.5f - 180f * scale, Screen.height * 0.5f - 42f * scale, 360f * scale, 84f * scale);
                GUI.color = new Color(0.035f, 0.075f, 0.09f, 0.94f);
                GUI.DrawTexture(message, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(message, "GATE REACHED", completeStyle);
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
                return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 19,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = new Color(0.9f, 0.97f, 0.94f, 1f);
            detailStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            detailStyle.normal.textColor = new Color(0.46f, 0.74f, 0.72f, 1f);
            completeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 27,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            completeStyle.normal.textColor = new Color(0.25f, 0.92f, 0.76f, 1f);
        }
    }
}
