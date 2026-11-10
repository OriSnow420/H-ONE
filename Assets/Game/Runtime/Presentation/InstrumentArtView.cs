using UnityEngine;

namespace HOne.Game
{
    public sealed class InstrumentArtView : MonoBehaviour
    {
        [SerializeField] private StarInstrument instrument;
        [SerializeField] private SpriteRenderer drawing;
        [SerializeField] private DeliveredArtSet art;
        [SerializeField] private LineRenderer wave;
        private float hitTime = float.NegativeInfinity;
        private LevelSession boundSession;
        public void Configure(StarInstrument source, SpriteRenderer view, DeliveredArtSet delivery, LineRenderer ring)
        {
            Unbind();
            instrument = source; drawing = view; art = delivery; wave = ring;
            if (isActiveAndEnabled) Bind();
        }
        private void OnEnable() => Bind();
        private void OnDisable() { Unbind(); ResetVisual(); }
        private void Bind()
        {
            if (instrument == null) return;
            instrument.Struck += Hit;
            BindSession();
        }
        private void BindSession()
        {
            if (boundSession != null) { boundSession.ProgressReset -= ResetVisual; boundSession.PuzzleReset -= ResetVisual; }
            boundSession = instrument != null ? instrument.Session : null;
            if (boundSession != null) { boundSession.ProgressReset += ResetVisual; boundSession.PuzzleReset += ResetVisual; }
        }
        private void Unbind()
        {
            if (instrument != null) instrument.Struck -= Hit;
            if (boundSession != null) { boundSession.ProgressReset -= ResetVisual; boundSession.PuzzleReset -= ResetVisual; }
            boundSession = null;
        }
        private void ResetVisual() { hitTime = float.NegativeInfinity; if (wave != null) wave.enabled = false; }
        private void Hit() => hitTime = Time.time;
        private void LateUpdate()
        {
            if (instrument.Session != boundSession) BindSession();
            var elapsed = Time.time - hitTime;
            if (instrument.Kind == StarInstrumentKind.Drum)
            {
                var frame = Mathf.FloorToInt(elapsed * 20f);
                drawing.sprite = frame >= 0 && art.DrumHit != null && frame < art.DrumHit.Length ? art.DrumHit[frame] : art.DrumIdle;
            }
            else drawing.color = instrument.Value ? new Color(1f,1f,.78f) : Color.white;
            if (wave == null) return;
            wave.enabled = elapsed >= 0f && elapsed < .4f;
            if (!wave.enabled) return;
            wave.startColor = wave.endColor = new Color(1f,.8f,.35f,1f-elapsed/.4f);
            var radius = instrument.WaveRadius * elapsed/.4f;
            for (var i=0;i<48;i++)
            {
                var angle=i*Mathf.PI*2f/48;
                wave.SetPosition(i,new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,0));
            }
        }
    }
}
