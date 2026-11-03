using UnityEngine;

namespace HOne.Platformer
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ExitGate2D : MonoBehaviour
    {
        private bool completed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (completed || other.GetComponentInParent<PlayerMovement2D>() == null)
                return;

            completed = true;
            Debug.Log("Exit reached.", other.gameObject);
        }
    }
}
