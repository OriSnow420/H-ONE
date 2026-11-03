using System.Collections.Generic;
using HOne.Platformer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HOne.Platformer.Editor
{
    public static class PlatformerSceneBuilder
    {
        private const string RootName = "Platformer Level";
        private const string ArtFolder = "Assets/Platformer/Art";
        private const string PhysicsMaterialPath = "Assets/Platformer/Zero Friction.physicsMaterial2D";
        private static readonly Vector2 MapMinimum = Vector2.zero;
        private static readonly Vector2 MapMaximum = new Vector2(54f, 20f);

        [MenuItem("H-ONE/Build Platformer Level")]
        public static void BuildScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                throw new System.InvalidOperationException("Create or open a saved scene before building the level.");

            var basicSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (basicSprite == null)
                throw new System.InvalidOperationException("Unity's built-in UI sprite could not be loaded.");

            if (AssetDatabase.IsValidFolder(ArtFolder))
                AssetDatabase.DeleteAsset(ArtFolder);

            var noFriction = EnsurePhysicsMaterial();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == RootName || root.name == "NORTHLINE LEVEL")
                    Object.DestroyImmediate(root);
            }

            var rootObject = new GameObject(RootName);
            var world = CreateGroup("World", rootObject.transform);
            var structures = CreateGroup("Platforms and Walls", rootObject.transform);
            var actors = CreateGroup("Actors", rootObject.transform);

            var boundsObject = new GameObject("Map Bounds");
            boundsObject.transform.SetParent(rootObject.transform, false);
            var bounds = boundsObject.AddComponent<LevelBounds2D>();
            bounds.SetBounds(MapMinimum, MapMaximum);

            BuildWorld(world, structures, basicSprite, noFriction);
            var player = BuildPlayer(actors, basicSprite, noFriction);
            BuildExit(actors, basicSprite);
            BuildCamera(rootObject.transform, player.transform, bounds);

            var buildScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(scene.path, true)
            };
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (buildScene.path != scene.path)
                    buildScenes.Add(buildScene);
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Platformer level built at " + scene.path);
        }

        private static Transform CreateGroup(string name, Transform parent)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static void BuildWorld(Transform world, Transform structures, Sprite sprite, PhysicsMaterial2D material)
        {
            CreateBox("Map Background", new Vector2(27f, 10f), new Vector2(54f, 20f),
                new Color(0.16f, 0.16f, 0.16f), world, sprite, null, -10);
            CreateBox("Ground", new Vector2(27f, 0.35f), new Vector2(54f, 0.7f),
                new Color(0.38f, 0.38f, 0.38f), structures, sprite, material, 0);
            CreateBox("West Boundary", new Vector2(-0.45f, 10f), new Vector2(0.9f, 20f),
                new Color(0.38f, 0.38f, 0.38f), structures, sprite, material, 0);
            CreateBox("East Boundary", new Vector2(54.45f, 10f), new Vector2(0.9f, 20f),
                new Color(0.38f, 0.38f, 0.38f), structures, sprite, material, 0);
            CreateBox("North Boundary", new Vector2(27f, 20.45f), new Vector2(54f, 0.9f),
                new Color(0.38f, 0.38f, 0.38f), structures, sprite, material, 0);

            CreateBarrier(structures, sprite, material, 12f, 0.7f, 12.5f);
            CreateBarrier(structures, sprite, material, 25f, 7.5f, 19.3f);
            CreateBarrier(structures, sprite, material, 38f, 0.7f, 11.5f);

            CreateBox("Platform A", new Vector2(6.5f, 4f), new Vector2(6f, 0.45f),
                new Color(0.46f, 0.46f, 0.46f), structures, sprite, material, 1);
            CreateBox("Platform B", new Vector2(19f, 15f), new Vector2(5f, 0.45f),
                new Color(0.46f, 0.46f, 0.46f), structures, sprite, material, 1);
            CreateBox("Platform C", new Vector2(33f, 5f), new Vector2(6f, 0.45f),
                new Color(0.46f, 0.46f, 0.46f), structures, sprite, material, 1);
        }

        private static void CreateBarrier(Transform parent, Sprite sprite, PhysicsMaterial2D material,
            float x, float bottom, float top)
        {
            var size = new Vector2(1.2f, top - bottom);
            CreateBox("Barrier", new Vector2(x, (top + bottom) * 0.5f), size,
                new Color(0.52f, 0.52f, 0.52f), parent, sprite, material, 1);
        }

        private static GameObject BuildPlayer(Transform parent, Sprite sprite, PhysicsMaterial2D material)
        {
            var player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = new Vector3(2.5f, 1.45f, 0f);
            player.tag = "Player";

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.sharedMaterial = material;

            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.72f, 1.05f);
            collider.sharedMaterial = material;
            player.AddComponent<PlayerMovement2D>();

            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 10;
            player.transform.localScale = new Vector3(0.68f, 1f, 1f);
            return player;
        }

        private static void BuildExit(Transform parent, Sprite sprite)
        {
            var exit = new GameObject("Exit Trigger");
            exit.transform.SetParent(parent, false);
            exit.transform.position = new Vector3(51f, 1.9f, 0f);
            exit.transform.localScale = new Vector3(0.6f, 2.4f, 1f);

            var renderer = exit.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.78f, 0.78f, 0.78f);
            renderer.sortingOrder = 5;

            var trigger = exit.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 1f);
            exit.AddComponent<ExitGate2D>();
        }

        private static void BuildCamera(Transform parent, Transform player, LevelBounds2D bounds)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.12f, 0.12f);

            var follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.Configure(player, bounds);
            var halfHeight = camera.orthographicSize;
            var halfWidth = halfHeight * 16f / 9f;
            var focus = player.position + new Vector3(halfWidth * 0.52f, halfHeight * 0.5f, 0f);
            cameraObject.transform.position = bounds.ClampCameraCenter(focus, halfWidth, halfHeight);
            cameraObject.transform.position = new Vector3(cameraObject.transform.position.x, cameraObject.transform.position.y, -10f);
        }

        private static GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color,
            Transform parent, Sprite sprite, PhysicsMaterial2D material, int sortingOrder)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            if (material != null)
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.sharedMaterial = material;
            }
            return gameObject;
        }

        private static PhysicsMaterial2D EnsurePhysicsMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(PhysicsMaterialPath);
            if (material == null)
            {
                material = new PhysicsMaterial2D("Zero Friction")
                {
                    friction = 0f,
                    bounciness = 0f
                };
                AssetDatabase.CreateAsset(material, PhysicsMaterialPath);
            }
            return material;
        }
    }
}
