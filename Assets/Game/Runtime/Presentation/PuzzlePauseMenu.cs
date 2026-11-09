using UnityEngine;

namespace HOne.Game
{
    public sealed class PuzzlePauseMenu : MonoBehaviour
    {
        [SerializeField] private PlayerStateMachine player;
        [SerializeField] private RuntimeLevelEditor levelEditor;
        [SerializeField] private GameObject panel;
        [SerializeField] private UnityEngine.UI.Button resume, resetPuzzle, restartLevel;
        [SerializeField] private PauseArtwork artwork;
        private bool shown;
        public void ConfigureArt(PauseArtwork view) => artwork = view;
        public void Configure(PlayerStateMachine owner, GameObject view, UnityEngine.UI.Button resumeButton,
            UnityEngine.UI.Button puzzleButton, UnityEngine.UI.Button restartButton)
        { player = owner; panel = view; resume = resumeButton; resetPuzzle = puzzleButton; restartLevel = restartButton; }
        private void OnEnable()
        {
            if (levelEditor == null) levelEditor = GetComponentInParent<RuntimeLevelEditor>();
            resume.onClick.AddListener(Resume);
            resetPuzzle.onClick.AddListener(ResetPuzzle);
            restartLevel.onClick.AddListener(RestartLevel);
        }
        private void OnDisable()
        {
            resume.onClick.RemoveListener(Resume);
            resetPuzzle.onClick.RemoveListener(ResetPuzzle);
            restartLevel.onClick.RemoveListener(RestartLevel);
            if (artwork != null) artwork.SetVisible(false);
            shown = false;
        }
        private void Update()
        {
            var shouldShow = player.Paused && (levelEditor == null || !levelEditor.IsEditing);
            if (artwork == null)
            { if (panel.activeSelf != shouldShow) panel.SetActive(shouldShow); }
            else if (shown != shouldShow)
            { shown = shouldShow; artwork.SetVisible(shouldShow); }
        }
        public void Resume() => player.SetPaused(false);
        public void ResetPuzzle() { if (player.Session.ResetCurrentPuzzle(player)) Resume(); }
        public void RestartLevel() { if (player.Session.RestartLevel(player)) Resume(); }
    }
}
