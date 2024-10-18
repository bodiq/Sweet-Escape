using System.Collections;
using UnityEngine;

namespace PowerUps
{
    public class SpikeImmunePowerUp: MonoBehaviour, PowerUp
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        private const Enums.PowerUps PowerUp = Enums.PowerUps.ChillBlast;
        private Player _player;
        private Coroutine _spikeImmuneActivation;

        private const float PowerUpLifeTime = 6f;
        private readonly WaitForSeconds _powerUpActive = new(PowerUpLifeTime);

        public GameObject GameObject => gameObject;
        Transform ITrigger.Transform => transform;

        private void OnEnable()
        {
            GameManager.Instance.OnPlayerRespawn += ResetPowerUp;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerRespawn -= ResetPowerUp;
        }

        public void Trigger(Player player)
        {
            _player = player;
            _player.PowerUpsActivated[PowerUp] = true;
            _spikeImmuneActivation ??= StartCoroutine(PowerUpActive());
            GameManager.Instance.OnPlayerPowerUpStart?.Invoke(PowerUp);
            spriteRenderer.enabled = false;
            boxCollider2D.enabled = false;
        }

        public void StopCoroutines()
        {
            Debug.LogError("Stop");
        }

        private IEnumerator PowerUpActive()
        {
            yield return _powerUpActive;
            GameManager.Instance.OnPlayerPowerUpEnd?.Invoke(PowerUp);
            _player.PowerUpsActivated[PowerUp] = false;
            _spikeImmuneActivation = null;
            gameObject.SetActive(false);
        }

        private void ResetPowerUp()
        {
            _player.PowerUpsActivated[PowerUp] = false;
            if (_spikeImmuneActivation != null)
            {
                StopCoroutine(_spikeImmuneActivation);
                _spikeImmuneActivation = null;
            }

            spriteRenderer.enabled = true;
            boxCollider2D.enabled = true;
        }
    }
}