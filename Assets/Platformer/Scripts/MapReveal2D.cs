using System.Collections.Generic;
using UnityEngine;

namespace HOne.Platformer
{
    public sealed class MapReveal2D : MonoBehaviour
    {
        private const int MaximumActivePulses = 32;
        private const float PixelsPerUnit = 12f;

        [SerializeField] private LevelBounds2D worldBounds;
        [SerializeField] private Transform player;
        [SerializeField] private DistanceFootstepEmitter2D footstepSource;
        [SerializeField] private Material fogMaterialTemplate;
        [SerializeField, Min(0.1f)] private float playerVisibilityRadius = 1.7f;
        [SerializeField, Min(0.1f)] private float footstepRevealRadius = 3f;
        [SerializeField, Min(0.1f)] private float pulseDuration = 0.7f;
        [SerializeField, Min(0.01f)] private float edgeFeather = 0.6f;

        private readonly List<Pulse> pulses = new List<Pulse>();
        private readonly Vector4[] pulseData = new Vector4[MaximumActivePulses];
        private Texture2D revealTexture;
        private Material fogMaterial;
        private byte[] revealPixels;
        private int textureWidth;
        private int textureHeight;

        private struct Pulse
        {
            public Vector2 Position;
            public float Radius;
            public float Age;
        }

        public void Configure(LevelBounds2D bounds, Transform trackedPlayer,
            DistanceFootstepEmitter2D steps, Material materialTemplate)
        {
            worldBounds = bounds;
            player = trackedPlayer;
            footstepSource = steps;
            fogMaterialTemplate = materialTemplate;
        }

        private void Awake()
        {
            if (worldBounds == null || player == null || fogMaterialTemplate == null)
            {
                Debug.LogError("Map Reveal needs map bounds, a player, and a fog material.", this);
                enabled = false;
                return;
            }

            var size = worldBounds.Maximum - worldBounds.Minimum;
            textureWidth = Mathf.Max(1, Mathf.CeilToInt(size.x * PixelsPerUnit));
            textureHeight = Mathf.Max(1, Mathf.CeilToInt(size.y * PixelsPerUnit));
            revealPixels = new byte[textureWidth * textureHeight];
            revealTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.R8, false, true)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = "Map Reveal Mask"
            };
            revealTexture.SetPixelData(revealPixels, 0);
            revealTexture.Apply(false, false);

            fogMaterial = new Material(fogMaterialTemplate) { name = "Map Reveal Fog (Runtime)" };
            fogMaterial.SetTexture("_RevealMask", revealTexture);
            fogMaterial.SetVector("_MapMinimum", new Vector4(worldBounds.Minimum.x, worldBounds.Minimum.y, 0f, 0f));
            fogMaterial.SetVector("_MapSize", new Vector4(size.x, size.y, 0f, 0f));

            var overlay = new GameObject("Fog Overlay");
            overlay.transform.SetParent(transform, false);
            overlay.transform.position = (Vector3)((worldBounds.Minimum + worldBounds.Maximum) * 0.5f);
            overlay.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = overlay.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 5;
            renderer.sharedMaterial = fogMaterial;
            overlay.AddComponent<SolidShape2D>();
        }

        private void OnEnable()
        {
            if (footstepSource != null)
                footstepSource.Step += OnFootstep;
        }

        private void OnDisable()
        {
            if (footstepSource != null)
                footstepSource.Step -= OnFootstep;
        }

        private void OnDestroy()
        {
            if (fogMaterial != null)
                Destroy(fogMaterial);
            if (revealTexture != null)
                Destroy(revealTexture);
        }

        private void Update()
        {
            if (fogMaterial == null)
                return;

            for (var i = pulses.Count - 1; i >= 0; i--)
            {
                var pulse = pulses[i];
                pulse.Age += Time.deltaTime;
                if (pulse.Age >= pulseDuration)
                {
                    StampPermanentReveal(pulse.Position, pulse.Radius);
                    pulses.RemoveAt(i);
                }
                else
                {
                    pulses[i] = pulse;
                }
            }

            var playerPosition = player.position;
            fogMaterial.SetVector("_PlayerPosition", playerPosition);
            fogMaterial.SetFloat("_PlayerRadius", playerVisibilityRadius);
            fogMaterial.SetFloat("_EdgeFeather", edgeFeather);
            for (var i = 0; i < pulses.Count; i++)
            {
                var pulse = pulses[i];
                var progress = Mathf.Clamp01(pulse.Age / pulseDuration);
                var radius = Mathf.SmoothStep(0f, pulse.Radius, progress);
                pulseData[i] = new Vector4(pulse.Position.x, pulse.Position.y, radius, 1f - progress);
            }
            fogMaterial.SetInt("_PulseCount", pulses.Count);
            fogMaterial.SetVectorArray("_PulseData", pulseData);
        }

        public void RevealAt(Vector2 position, float radius)
        {
            if (revealTexture == null || radius <= 0f)
                return;

            if (pulses.Count == MaximumActivePulses)
            {
                StampPermanentReveal(pulses[0].Position, pulses[0].Radius);
                pulses.RemoveAt(0);
            }
            pulses.Add(new Pulse { Position = position, Radius = radius });
        }

        private void OnFootstep(Vector2 position)
        {
            RevealAt(position, footstepRevealRadius);
        }

        private void StampPermanentReveal(Vector2 center, float radius)
        {
            var minimum = worldBounds.Minimum;
            var size = worldBounds.Maximum - minimum;
            var minX = Mathf.Max(0, Mathf.FloorToInt((center.x - radius - edgeFeather - minimum.x) / size.x * textureWidth));
            var maxX = Mathf.Min(textureWidth - 1, Mathf.CeilToInt((center.x + radius + edgeFeather - minimum.x) / size.x * textureWidth));
            var minY = Mathf.Max(0, Mathf.FloorToInt((center.y - radius - edgeFeather - minimum.y) / size.y * textureHeight));
            var maxY = Mathf.Min(textureHeight - 1, Mathf.CeilToInt((center.y + radius + edgeFeather - minimum.y) / size.y * textureHeight));
            for (var y = minY; y <= maxY; y++)
            {
                var worldY = minimum.y + (y + 0.5f) / textureHeight * size.y;
                for (var x = minX; x <= maxX; x++)
                {
                    var worldX = minimum.x + (x + 0.5f) / textureWidth * size.x;
                    var distance = Vector2.Distance(center, new Vector2(worldX, worldY));
                    var edgePosition = Mathf.InverseLerp(radius - edgeFeather,
                        radius + edgeFeather, distance);
                    var reveal = 1f - Mathf.SmoothStep(0f, 1f, edgePosition);
                    var index = y * textureWidth + x;
                    revealPixels[index] = (byte)Mathf.Max(revealPixels[index], Mathf.RoundToInt(reveal * 255f));
                }
            }
            revealTexture.SetPixelData(revealPixels, 0);
            revealTexture.Apply(false, false);
        }
    }
}
