using UnityEngine;

namespace HOne.Game
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private PlayerStateMachine player;
        [SerializeField] private SpriteRenderer human;
        [SerializeField] private SpriteRenderer star;
        [SerializeField] private SpriteRenderer ring;
        [SerializeField] private DeliveredArtSet art;
        private float reformStart;
        private float stateStart;
        private Rigidbody2D body;
        public void ConfigureArt(DeliveredArtSet delivery) => art = delivery;
        public void Configure(PlayerStateMachine owner, SpriteRenderer humanSprite, SpriteRenderer starSprite,
            SpriteRenderer ringSprite)
        { player = owner; human = humanSprite; star = starSprite; ring = ringSprite; }

        private void OnEnable() { if (player != null) player.StateChanged += OnState; }
        private void OnDisable() { if (player != null) player.StateChanged -= OnState; }
        private void OnState(PlayerState state)
        {
            stateStart = Time.time;
            if (state == PlayerState.Reforming) reformStart = Time.time;
        }

        private void LateUpdate()
        {
            var state = player.State;
            if (body == null) body = player.GetComponent<Rigidbody2D>();
            human.enabled = state == PlayerState.Human || state == PlayerState.Charging;
            star.enabled = state == PlayerState.StarFlight;
            ring.enabled = state != PlayerState.Human || player.Paused;
            if (state == PlayerState.Charging)
                ring.transform.localScale = Vector3.one * (0.7f + player.Charge * 0.5f);
            else if (state == PlayerState.Reforming)
                ring.transform.localScale = Vector3.one * (0.4f + (Time.time - reformStart) * 3f);
            else ring.transform.localScale = Vector3.one * 0.7f;
            if (state == PlayerState.Human && Mathf.Abs(body.linearVelocity.x) > 0.01f)
                human.flipX = body.linearVelocity.x < 0f;
            if (art != null)
            {
                var moving = state == PlayerState.Human && player.IsGrounded && Mathf.Abs(body.linearVelocity.x) > 0.1f;
                var humanFrame = DeliveredArtSet.Frame(moving ? art.Walk : art.Idle, Time.time, moving ? 6f : 4f);
                if (state == PlayerState.Human && !player.IsGrounded && art.Jump != null) humanFrame = art.Jump;
                if (humanFrame != null) human.sprite = humanFrame;
                if (state == PlayerState.StarFlight)
                {
                    // Launch is a visual overlap with flight, never an input/motion lock.
                    var elapsed = Time.time - stateStart;
                    if (elapsed < 0.18f && art.Launch != null && art.Launch.Length > 0)
                    {
                        star.sprite = art.Launch[Mathf.Min(art.Launch.Length - 1, Mathf.FloorToInt(elapsed / 0.18f * art.Launch.Length))];
                        star.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.55f, elapsed / 0.18f);
                    }
                    else
                    {
                        var frame = DeliveredArtSet.Frame(art.Star, elapsed, 6f);
                        if (frame != null) star.sprite = frame;
                        star.transform.localScale = Vector3.one * 0.55f;
                    }
                }
            }
        }
    }
}
