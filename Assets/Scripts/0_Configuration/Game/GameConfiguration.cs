using UnityEngine;

namespace Asteroids.Configuration.Game
{
    [CreateAssetMenu(menuName = "Configuration/Gameplay Configuration")]
    public class GameConfiguration : ScriptableObject
    {
        [SerializeField] private float _asteroidSpawnInterval;
        [SerializeField] private int _maxAsteroidQuantity;
        [SerializeField] private AsteroidGroupConfiguration[] _asteroidGroupConfigurations;

        [SerializeField] private PlayerConfiguration _playerConfiguration;
        [SerializeField] private BulletConfiguration _playerBulletConfiguration;

        [SerializeField] private int _targetFramerate;

        public float AsteroidSpawnInterval => _asteroidSpawnInterval;
        public int MaxAsteroidQuantity => _maxAsteroidQuantity;
        public AsteroidGroupConfiguration[] AsteroidGroupConfigurations => _asteroidGroupConfigurations;

        public PlayerConfiguration PlayerConfiguration => _playerConfiguration;
        public BulletConfiguration BulletConfiguration => _playerBulletConfiguration;

        public int TargetFramerate => _targetFramerate;
    }
}
