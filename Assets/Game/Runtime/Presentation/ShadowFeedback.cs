using UnityEngine;
using UnityEngine.Audio;

namespace HOne.Game
{
    // Replace these technical cues with P0 deliveries without changing gameplay or colliders.
    public sealed class ShadowFeedback : MonoBehaviour
    {
        [SerializeField] private ShadowService shadows;
        [SerializeField] private PlayerStateMachine player;
        [SerializeField] private AudioMixerGroup bus;
        [SerializeField] private AudioClip embed, recallStart, recallEnd;
        private AudioSource source;
        private bool hadShadow, paused;
        private Transform recallingVisual;
        private Vector3 recallingScale;
        private float nextUnavailableCue;
        private StarSocket[] sockets;
        public void Configure(ShadowService service, PlayerStateMachine owner, AudioMixerGroup output,
            AudioClip placed, AudioClip start, AudioClip end)
        { shadows = service; player = owner; bus = output; embed = placed; recallStart = start; recallEnd = end; }
        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0f; source.outputAudioMixerGroup = bus;
            RefreshSockets();
        }
        public void RefreshSockets() => sockets = shadows.GetComponentsInChildren<StarSocket>(true);
        private void OnEnable()
        { shadows.Changed += Changed; shadows.RecallStarted += Recall; shadows.Recalled += Recalled; player.StarImpact += Impact; }
        private void OnDisable()
        {
            shadows.Changed -= Changed; shadows.RecallStarted -= Recall; shadows.Recalled -= Recalled;
            player.StarImpact -= Impact;
            RestoreVisual();
            if (source != null) source.Stop();
        }
        private void Play(AudioClip clip, float pitch)
        { if (clip != null) { source.pitch = pitch; source.PlayOneShot(clip, 0.6f); } }
        private void RestoreVisual()
        { if (recallingVisual != null) recallingVisual.localScale = recallingScale; recallingVisual = null; }
        private void Changed()
        {
            RestoreVisual();
            if (!shadows.HasShadow && source != null) source.Stop();
            if (shadows.HasShadow && !hadShadow) Play(embed, 1.4f);
            hadShadow = shadows.HasShadow;
        }
        private void Recall()
        {
            var visual = shadows.ActiveSocket.Shadow.transform.Find("Visual");
            // Keep the supporting edge at its physical width until the shadow is removed.
            recallingVisual = visual != null && visual.childCount > 0 ? visual.GetChild(0) : null;
            if (recallingVisual != null) recallingScale = recallingVisual.localScale;
            Play(recallStart, 0.7f);
        }
        private void Recalled() => Play(recallEnd, 0.9f);
        private void Impact(StarContact contact)
        {
            if (!contact.Embedded && contact.Collider.GetComponent<StarSocket>() != null && Time.time >= nextUnavailableCue)
            { nextUnavailableCue = Time.time + 0.15f; Play(recallStart, 0.45f); }
        }
        private void Update()
        {
            foreach (var socket in sockets)
            {
                if (socket == null) continue;
                if (socket.TryGetComponent<StarSocketArtView>(out var artView) && artView.isActiveAndEnabled) continue;
                var view = socket.transform.Find("Visual");
                if (view != null && view.TryGetComponent<SpriteRenderer>(out var renderer))
                    renderer.color = socket.Available || shadows.ActiveSocket == socket ? new Color(1f, 0.8f, 0.3f) : new Color(0.45f, 0.5f, 0.55f);
            }
            if (recallingVisual != null)
                recallingVisual.localScale = recallingScale * (1f - 0.35f * shadows.RecallProgress);
            if (paused == player.Paused) return;
            paused = player.Paused;
            if (paused) source.Pause(); else source.UnPause();
        }
    }
}
