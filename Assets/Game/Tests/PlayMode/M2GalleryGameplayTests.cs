using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace HOne.Game.Tests
{
    public sealed class M2GalleryGameplayTests
    {
        private Scene scene;
        private PlayerStateMachine player;
        private Keyboard keyboard;
        private Mouse mouse;
        private InputSettings.EditorInputBehaviorInPlayMode oldBehavior;
        private InputSettings.BackgroundBehavior oldBackground;
        [UnitySetUp] public IEnumerator Load()
        {
            oldBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            oldBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Time.timeScale = 1f;
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(GameFlow.GalleryScene, new LoadSceneParameters(LoadSceneMode.Additive));
#endif
            scene = SceneManager.GetSceneByPath(GameFlow.GalleryScene);
            player = scene.GetRootGameObjects()[0].GetComponentInChildren<PlayerStateMachine>();
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
            player.Input.Actions.devices = new InputDevice[] { keyboard, mouse };
            player.Input.Actions.Disable(); player.Input.Actions.Enable();
            yield return new WaitForSeconds(0.2f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if (player != null) player.SetPaused(false);
            Time.timeScale = 1f;
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            InputSystem.settings.editorInputBehaviorInPlayMode = oldBehavior;
            InputSystem.settings.backgroundBehavior = oldBackground;
        }
        private static void Set<T>(InputControl<T> control, T value) where T : struct
        {
            using (UnityEngine.InputSystem.LowLevel.StateEvent.From(control.device, out var state))
            { control.WriteValueIntoEvent(value, state); InputSystem.QueueEvent(state); }
            InputSystem.Update();
        }
        private IEnumerator Shoot(Vector2 target)
        {
            Aim(target); yield return null; Set(keyboard.eKey, 1f);
            for (var i = 0; i < 42; i++) { Aim(target); yield return new WaitForFixedUpdate(); }
            Set(keyboard.eKey, 0f); yield return new WaitForSeconds(1.7f);
        }
        private void Aim(Vector2 target)
        {
            var camera = player.transform.root.GetComponentInChildren<Camera>();
            var point = camera.WorldToScreenPoint(target); Set(mouse.position, new Vector2(point.x, point.y));
        }
        [UnityTest] public IEnumerator GallerySupportsCollectionEmbeddingBridgeCrossingAndStonePlate()
        {
            yield return Shoot(new Vector2(-4, 2));
            Assert.IsTrue(player.Session.HasFragment("gallery-f1"));
            var root = player.transform.root;
            root.Find("Presentation/GalleryNavigation/DevelopmentNavigation/Station2").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return new WaitForSeconds(0.7f);
            yield return Shoot(new Vector2(12, 1.5f));
            Assert.IsTrue(player.Session.Shadows.HasShadow);
            Assert.AreEqual("PlatformSocket", player.Session.Shadows.ActiveSocket.name);
            Set(keyboard.rKey, 1f); yield return new WaitForFixedUpdate(); Set(keyboard.rKey, 0f);
            yield return new WaitForSeconds(1.1f); Assert.IsFalse(player.Session.Shadows.HasShadow);
            Set(keyboard.dKey, 1f); yield return new WaitForSeconds(1.35f); Set(keyboard.dKey, 0f);
            yield return new WaitForFixedUpdate();
            yield return Shoot(new Vector2(17, 2.6f));
            Assert.AreEqual("BridgeSocket", player.Session.Shadows.ActiveSocket?.name);
            Set(keyboard.dKey, 1f); yield return new WaitForSeconds(4.5f); Set(keyboard.dKey, 0f);
            yield return new WaitForFixedUpdate(); Assert.Greater(player.Position.x, 26f, "Human must walk across the formed bridge.");
            root.Find("Presentation/GalleryNavigation/DevelopmentNavigation/Station3").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return new WaitForSeconds(0.7f);
            var plate = root.GetComponentsInChildren<PressurePlate>().Single(p => p.name == "StonePlate");
            var gate = root.GetComponentsInChildren<SignalBarrier>().Single(p => p.name == "PlateGate");
            Set(keyboard.dKey, 1f);
            var deadline = Time.time + 5f;
            while (!plate.Value && Time.time < deadline) yield return new WaitForFixedUpdate();
            Set(keyboard.dKey, 0f); yield return new WaitForSeconds(0.6f);
            Assert.IsTrue(plate.Value, "The pushed stone should remain against its backstop on the plate.");
            Assert.IsFalse(gate.GetComponent<BoxCollider2D>().enabled);
            Assert.IsTrue(player.Session.ResetCurrentPuzzle(player)); yield return new WaitForSeconds(0.7f);
            Assert.IsFalse(plate.Value); Assert.IsTrue(gate.GetComponent<BoxCollider2D>().enabled);
            Assert.IsTrue(player.Session.HasFragment("gallery-f1"));
        }
    }
}
