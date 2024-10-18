using Configs;
using UnityEngine;

namespace PowerUps
{
    public class PowerUpManager : MonoBehaviour
    {
        [SerializeField] private Transform spawnPosition;

        private PowerUp _spawnedPowerUp;

        private void OnEnable()
        {
            ResetPowerUp();
            GameManager.Instance.OnPlayerRespawn += ResetPowerUp;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPlayerRespawn -= ResetPowerUp;
        }

        private void ResetPowerUp()
        {
            if (_spawnedPowerUp != null) 
            {
                Destroy(_spawnedPowerUp.GameObject);
                _spawnedPowerUp = null;
            }

            if (!PowerUpConfig.Instance.PowerUpAppearance.TryGetValue(GameManager.Instance.SelectedCharacter,
                    out var powerUpData)) return;

            var randomPowerUp = powerUpData.GetRandomValue();
            if (randomPowerUp == Enums.PowerUps.Nothing) return;

            var powerUpObject = PowerUpConfig.Instance.PowerUpPrefabs[randomPowerUp];
            _spawnedPowerUp = Instantiate(powerUpObject, spawnPosition.position, Quaternion.identity, transform)
                .GetComponent<PowerUp>();
        }
    }
}