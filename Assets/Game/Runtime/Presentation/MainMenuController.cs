using UnityEngine;

namespace HOne.Game
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button start, settings, quit, back;
        [SerializeField] private GameObject homePanel, settingsPanel;
        public void Configure(UnityEngine.UI.Button play, UnityEngine.UI.Button options, UnityEngine.UI.Button exit,
            UnityEngine.UI.Button close, GameObject home, GameObject controls)
        { start = play; settings = options; quit = exit; back = close; homePanel = home; settingsPanel = controls; }

        private void OnEnable()
        {
            start.onClick.AddListener(GameFlow.OpenGallery);
            settings.onClick.AddListener(OpenSettings);
            back.onClick.AddListener(CloseSettings);
            quit.onClick.AddListener(Quit);
            quit.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
        }
        private void OnDisable()
        {
            start.onClick.RemoveListener(GameFlow.OpenGallery);
            settings.onClick.RemoveListener(OpenSettings);
            back.onClick.RemoveListener(CloseSettings);
            quit.onClick.RemoveListener(Quit);
        }
        private void OpenSettings() { homePanel.SetActive(false); settingsPanel.SetActive(true); }
        private void CloseSettings() { settingsPanel.SetActive(false); homePanel.SetActive(true); }
        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
