using System.Collections.Generic;
using System.IO;
using HOne.Platformer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HOne.Platformer.Editor
{
    public static class PlatformerSceneBuilder
    {
        private const string RootName = "NORTHLINE LEVEL";
        private const string ArtFolder = "Assets/Platformer/Art";
        private const string SquarePath = ArtFolder + "/Square.png";
        private const string CirclePath = ArtFolder + "/Circle.png";
        private const string PhysicsMaterialPath = ArtFolder + "/Zero Friction.physicsMaterial2D";
        private static readonly Vector2 MapMinimum = Vector2.zero;
        private static readonly Vector2 MapMaximum = new Vector2(88f, 30f);

        [MenuItem("H-ONE/Build Northline Level")]
        public static void BuildScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                throw new System.InvalidOperationException("Create or open a saved scene before building Northline.");

            EnsureArtAssets();
            var square = AssetDatabase.LoadAssetAtPath<Sprite>(SquarePath);
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            var noFriction = EnsurePhysicsMaterial();

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == RootName)
                    Object.DestroyImmediate(root);
            }

            var rootObject = new GameObject(RootName);
            var world = CreateGroup("World", rootObject.transform);
            var structures = CreateGroup("Structures", rootObject.transform);
            var actors = CreateGroup("Actors", rootObject.transform);

            var boundsObject = new GameObject("Map Bounds");
            boundsObject.transform.SetParent(rootObject.transform, false);
            var bounds = boundsObject.AddComponent<LevelBounds2D>();
            bounds.SetBounds(MapMinimum, MapMaximum);

            BuildBackdrop(world, square);
            BuildBoundaryWalls(structures, square, noFriction);
            BuildRoute(structures, square, noFriction);

            var player = BuildPlayer(actors, square, noFriction);
            BuildExit(actors, square, circle);
            rootObject.AddComponent<LevelHud2D>();
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
            Debug.Log("Northline level built at " + scene.path);
        }

        private static Transform CreateGroup(string name, Transform parent)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static void BuildBackdrop(Transform parent, Sprite square)
        {
            CreateBox("Night Field", new Vector2(44f, 15f), new Vector2(88f, 30f),
                new Color(0.035f, 0.095f, 0.11f), parent, square, null, -100);

            var previousState = Random.state;
            Random.InitState(1701);
            for (var i = 0; i < 72; i++)
            {
                var position = new Vector2(Random.Range(1f, 87f), Random.Range(1.5f, 29f));
                var size = Random.Range(0.035f, 0.09f);
                var tint = i % 5 == 0
                    ? new Color(0.95f, 0.68f, 0.35f, 0.55f)
                    : new Color(0.35f, 0.76f, 0.72f, 0.36f);
                CreateBox("Distant Signal", position, Vector2.one * size, tint, parent, square, null, -90);
            }
            Random.state = previousState;

            for (var x = 4; x < 88; x += 4)
            {
                CreateBox("Grid Line", new Vector2(x, 15f), new Vector2(0.018f, 30f),
                    new Color(0.19f, 0.43f, 0.43f, 0.12f), parent, square, null, -85);
            }
        }

        private static void BuildBoundaryWalls(Transform parent, Sprite square, PhysicsMaterial2D material)
        {
            var wallColor = new Color(0.09f, 0.23f, 0.26f);
            CreateBox("Boundary West", new Vector2(-0.45f, 15f), new Vector2(0.9f, 30f), wallColor, parent, square, material, 0);
            CreateBox("Boundary East", new Vector2(88.45f, 15f), new Vector2(0.9f, 30f), wallColor, parent, square, material, 0);
            CreateBox("Boundary South", new Vector2(44f, -0.45f), new Vector2(88f, 0.9f), wallColor, parent, square, material, 0);
            CreateBox("Boundary North", new Vector2(44f, 30.45f), new Vector2(88f, 0.9f), wallColor, parent, square, material, 0);
        }

        private static void BuildRoute(Transform parent, Sprite square, PhysicsMaterial2D material)
        {
            CreateBox("Start Deck", new Vector2(5.5f, 1.15f), new Vector2(11f, 0.7f),
                new Color(0.14f, 0.36f, 0.34f), parent, square, material, 0);

            CreateBarrier(parent, square, material, 14f, 0.55f, 19.4f, new Color(0.14f, 0.31f, 0.34f));
            CreateBarrier(parent, square, material, 29f, 10.25f, 29.45f, new Color(0.18f, 0.28f, 0.31f));
            CreateBarrier(parent, square, material, 44f, 0.55f, 21.2f, new Color(0.14f, 0.31f, 0.34f));
            CreateBarrier(parent, square, material, 59f, 9.5f, 29.45f, new Color(0.18f, 0.28f, 0.31f));
            CreateBarrier(parent, square, material, 74f, 0.55f, 18.8f, new Color(0.14f, 0.31f, 0.34f));

            CreateDeck(parent, square, material, 8f, 6.1f, 6.5f, new Color(0.16f, 0.34f, 0.36f));
            CreateDeck(parent, square, material, 22f, 23.8f, 7.2f, new Color(0.19f, 0.31f, 0.34f));
            CreateDeck(parent, square, material, 38f, 5.4f, 7.4f, new Color(0.16f, 0.34f, 0.36f));
            CreateDeck(parent, square, material, 52f, 24.6f, 7.2f, new Color(0.19f, 0.31f, 0.34f));
            CreateDeck(parent, square, material, 68f, 5.7f, 7.4f, new Color(0.16f, 0.34f, 0.36f));
            CreateDeck(parent, square, material, 81f, 23.6f, 7.4f, new Color(0.19f, 0.31f, 0.34f));
        }

        private static void CreateBarrier(Transform parent, Sprite square, PhysicsMaterial2D material,
            float x, float bottom, float top, Color color)
        {
            var height = top - bottom;
            CreateBox("Signal Wall", new Vector2(x, (top + bottom) * 0.5f), new Vector2(1.45f, height),
                color, parent, square, material, 0);
            CreateBox("Wall Marker", new Vector2(x - 0.45f, (top + bottom) * 0.5f), new Vector2(0.12f, height - 0.4f),
                new Color(0.94f, 0.62f, 0.3f), parent, square, null, 1);
            CreateBox("Wall Cap", new Vector2(x, top - 0.12f), new Vector2(1.8f, 0.24f),
                new Color(0.29f, 0.77f, 0.65f), parent, square, null, 2);
        }

        private static void CreateDeck(Transform parent, Sprite square, PhysicsMaterial2D material,
            float centerX, float y, float width, Color color)
        {
            CreateBox("Raised Walkway", new Vector2(centerX, y), new Vector2(width, 0.72f), color, parent, square, material, 0);
            CreateBox("Walkway Edge", new Vector2(centerX, y + 0.38f), new Vector2(width, 0.08f),
                new Color(0.25f, 0.77f, 0.68f), parent, square, null, 1);
        }

        private static GameObject BuildPlayer(Transform parent, Sprite square, PhysicsMaterial2D material)
        {
            var player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = new Vector3(3f, 1.8f, 0f);
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
            var movement = player.AddComponent<PlayerMovement2D>();

            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = new Color(0.94f, 0.39f, 0.28f);
            renderer.sortingOrder = 10;
            player.transform.localScale = new Vector3(0.68f, 1.02f, 1f);

            CreatePlayerDetail("Suit Stripe", new Vector2(-0.2f, -0.05f), new Vector2(0.12f, 0.85f),
                new Color(0.25f, 0.91f, 0.73f), player.transform, square, 11);
            CreatePlayerDetail("Faceplate", new Vector2(0.09f, 0.22f), new Vector2(0.31f, 0.28f),
                new Color(0.99f, 0.82f, 0.49f), player.transform, square, 12);
            CreatePlayerDetail("Eye", new Vector2(0.18f, 0.23f), new Vector2(0.045f, 0.06f),
                new Color(0.04f, 0.1f, 0.11f), player.transform, square, 13);

            return player;
        }

        private static void CreatePlayerDetail(string name, Vector2 position, Vector2 size, Color color,
            Transform parent, Sprite square, int sortingOrder)
        {
            var detail = new GameObject(name);
            detail.transform.SetParent(parent, false);
            detail.transform.localPosition = position;
            detail.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = detail.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        private static void BuildExit(Transform parent, Sprite square, Sprite circle)
        {
            var gate = new GameObject("Exit Gate");
            gate.transform.SetParent(parent, false);
            gate.transform.position = new Vector3(84f, 2.55f, 0f);

            CreateSprite("Outer Signal", gate.transform, Vector2.zero, new Vector2(3.1f, 3.1f),
                new Color(0.18f, 0.78f, 0.66f, 0.28f), circle, 2);
            CreateSprite("Gate Core", gate.transform, Vector2.zero, new Vector2(2.35f, 2.35f),
                new Color(0.045f, 0.13f, 0.14f), circle, 3);
            CreateSprite("Gate Light", gate.transform, Vector2.zero, new Vector2(1.25f, 1.25f),
                new Color(0.97f, 0.66f, 0.32f, 0.75f), circle, 4);
            CreateBox("Gate Left Post", new Vector2(-1.42f, 0f), new Vector2(0.18f, 3.45f),
                new Color(0.26f, 0.85f, 0.71f), gate.transform, square, null, 5);
            CreateBox("Gate Right Post", new Vector2(1.42f, 0f), new Vector2(0.18f, 3.45f),
                new Color(0.26f, 0.85f, 0.71f), gate.transform, square, null, 5);
            CreateBox("Gate Header", new Vector2(0f, 1.72f), new Vector2(3.0f, 0.16f),
                new Color(0.97f, 0.66f, 0.32f), gate.transform, square, null, 6);

            var trigger = gate.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(2.5f, 3.1f);
            gate.AddComponent<ExitGate2D>();
        }

        private static void BuildCamera(Transform parent, Transform player, LevelBounds2D bounds)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.095f, 0.11f);

            var follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.Configure(player, bounds);
            var halfHeight = camera.orthographicSize;
            var halfWidth = halfHeight * 16f / 9f;
            var focus = player.position + new Vector3(halfWidth * 0.52f, halfHeight * 0.5f, 0f);
            cameraObject.transform.position = bounds.ClampCameraCenter(focus, halfWidth, halfHeight);
            cameraObject.transform.position = new Vector3(cameraObject.transform.position.x, cameraObject.transform.position.y, -10f);
        }

        private static GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color,
            Transform parent, Sprite square, PhysicsMaterial2D material, int sortingOrder)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            if (material != null)
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.sharedMaterial = material;
            }
            return gameObject;
        }

        private static GameObject CreateSprite(string name, Transform parent, Vector2 position, Vector2 size,
            Color color, Sprite sprite, int sortingOrder)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = position;
            gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return gameObject;
        }

        private static void EnsureArtAssets()
        {
            Directory.CreateDirectory(ArtFolder);
            CreateTextureAsset(SquarePath, 16, (x, y) => Color.white, 16);
            CreateTextureAsset(CirclePath, 64, (x, y) =>
            {
                var dx = (x + 0.5f - 32f) / 32f;
                var dy = (y + 0.5f - 32f) / 32f;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = Mathf.Clamp01((1f - distance) * 16f);
                return new Color(1f, 1f, 1f, alpha);
            }, 64);
        }

        private static void CreateTextureAsset(string assetPath, int size, System.Func<int, int, Color> pixel, int pixelsPerUnit)
        {
            if (!File.Exists(assetPath))
            {
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                        texture.SetPixel(x, y, pixel(x, y));
                }
                texture.Apply();
                File.WriteAllBytes(assetPath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
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
