using UnityEngine;
using ImPulse.Core;

namespace ImPulse.Debug
{
    public class RhythmDebugVisualizer : MonoBehaviour
    {
        [Header("References")]
        public AudioHandler audioHandler;
        public RhythmInputHandler inputHandler;

        // State Tracking
        private int currentBeat = 0;
        private int currentHalfBeat = 0;
        private float timeSinceLastBeat = 0f;

        // Feedback Tracking
        private string feedbackText = "Waiting for input...";
        private Color feedbackColor = Color.white;
        private float feedbackTimer = 0f;

        private void OnEnable()
        {
            audioHandler.OnBeat += HandleBeat;
            audioHandler.OnHalfBeat += HandleHalfBeat;
            
            inputHandler.OnInputSuccess += HandleSuccess;
            inputHandler.OnInputMiss += HandleMiss;
        }

        private void OnDisable()
        {
            audioHandler.OnBeat -= HandleBeat;
            audioHandler.OnHalfBeat -= HandleHalfBeat;
            
            inputHandler.OnInputSuccess -= HandleSuccess;
            inputHandler.OnInputMiss -= HandleMiss;
        }

        private void HandleBeat(int beatIndex)
        {
            currentBeat = beatIndex;
            timeSinceLastBeat = 0f; // Reset for the visual metronome flash
        }

        private void HandleHalfBeat(int halfBeatIndex)
        {
            currentHalfBeat = halfBeatIndex;
        }

        private void HandleSuccess(int beatIndex, string timing)
        {
            feedbackTimer = 1.5f;

            if (timing == "Perfect") feedbackColor = Color.green;
            else feedbackColor = Color.yellow; // Early or Late
        }

        private void HandleMiss()
        {
            feedbackColor = Color.red;
            feedbackTimer = 1.5f;
        }

        private void Update()
        {
            // Fade the metronome flash and the feedback text over time
            timeSinceLastBeat += Time.deltaTime;
            
            if (feedbackTimer > 0)
            {
                feedbackTimer -= Time.deltaTime;
            }
            else
            {
                feedbackColor = Color.white;
                feedbackText = "Waiting for input...";
            }
        }

        private void OnGUI()
        {
            // Set up IMGUI styling
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 24;
            style.alignment = TextAnchor.UpperLeft;
            style.normal.textColor = Color.white;

            GUILayout.BeginArea(new Rect(20, 20, 400, 300), style);
            
            GUILayout.Label("<b>--- Rhythm Engine Debug ---</b>", style);
            GUILayout.Space(10);

            // Draw Audio Stats
            GUILayout.Label($"<b>Song Time:</b> {audioHandler.songPositionInSeconds:F2}s", style);
            GUILayout.Label($"<b>Current Beat:</b> {currentBeat}", style);
            GUILayout.Label($"<b>Current Half-Beat:</b> {currentHalfBeat}", style);

            GUILayout.Space(20);

            // Draw the visual metronome (Flashes white on the beat, fades to dark gray)
            float flashAlpha = Mathf.Clamp01(1.0f - (timeSinceLastBeat * 4f));
            GUI.color = Color.Lerp(new Color(0.2f, 0.2f, 0.2f, 1f), Color.white, flashAlpha);
            GUILayout.Box("METRONOME TICK", style);
            GUI.color = Color.white; // Reset GUI color

            GUILayout.Space(20);

            // Draw Input Feedback
            GUIStyle feedbackStyle = new GUIStyle(style);
            feedbackStyle.normal.textColor = feedbackColor;
            GUILayout.Label(feedbackText, feedbackStyle);

            GUILayout.EndArea();
        }
    }
}