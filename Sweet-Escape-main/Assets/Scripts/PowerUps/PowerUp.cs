using UnityEngine;

namespace PowerUps
{
    public interface PowerUp : ITrigger
    {
        public void StopCoroutines();
        public GameObject GameObject { get; }
    }
}