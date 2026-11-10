using UnityEngine;
using UnityEngine.SceneManagement;

namespace HOne.Game
{
    public static class GameFlow
    {
        public const string MainMenuScene = "Assets/Game/Scenes/MainMenu.unity";
        public const string GalleryScene = "Assets/Game/Scenes/M2_MechanismGallery.unity";
        public const string FirstLevelScene = "Assets/Game/Scenes/Chapter01_01.unity";
        public const string SecondLevelScene = "Assets/Game/Scenes/Chapter01_02.unity";

        public static void StartChapter() => SceneManager.LoadScene(FirstLevelScene);
        public static void OpenGallery() => SceneManager.LoadScene(GalleryScene);
        public static void ReturnToMenu(PlayerStateMachine player)
        {
            if (player != null) player.SetPaused(false);
            SceneManager.LoadScene(MainMenuScene);
        }
    }
}
