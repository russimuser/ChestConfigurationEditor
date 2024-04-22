using System;
using System.Linq;
using Asteroids.Configuration;
using Asteroids.Extensions;
using Asteroids.GameplayECS.Components;
using Asteroids.GameplayECS.Extensions;
using Asteroids.GameplayECS.Factories;
using Asteroids.Services;
using Asteroids.Tools;
using Asteroids.ValueTypeECS.Entities;
using Asteroids.ValueTypeECS.EntityGroup;
using Asteroids.ValueTypeECS.System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Asteroids.GameplayECS.Systems.Asteroid
{
    public class AsteroidSpawningSystem : ISystem, IDisposable
    {
        private const float MaxAngle = 360;

        private readonly EntityFactory _entityFactory;

        private readonly GameConfiguration _gameConfiguration;

        private readonly float[] _asteroidSpawnProbabilitiesCached;
        private readonly float _asteroidSpawnProbabilitiesSum;

        private EntityGroup _ships;
        private EntityGroup _asteroids;

        private EntityGroup _timers;

        public AsteroidSpawningSystem(EntityFactory entityFactory, IFrameInfoService frameInfoService, GameConfiguration gameConfiguration, IInstanceSpawner instanceSpawner)
        {
            _entityFactory = entityFactory;

            _gameConfiguration = gameConfiguration;
            _asteroidSpawnProbabilitiesCached = _gameConfiguration.AsteroidGroupConfigurations.Select(e => e.SpawnProbability).ToArray();
            _asteroidSpawnProbabilitiesSum = _asteroidSpawnProbabilitiesCached.Sum();

            _ships = instanceSpawner.Instantiate<EntityGroupBuilder>()
                .RequireComponent<ShipComponent>()
                .Build();

            _asteroids = instanceSpawner.Instantiate<EntityGroupBuilder>()
                .RequireComponent<AsteroidComponent>()
                .RequireComponentAbsence<MeteoriteComponent>()
                .Build();

            _timers = instanceSpawner.Instantiate<EntityGroupBuilder>()
                .RequireComponent<AsteroidSpawningTimerComponent>().Build();

            _ships.EntityAdded += HandleShipAdded;
            _timers.EntityRemoved += HandleTimerEnded;
        }

        public void Dispose()
        {
            _ships.EntityAdded -= HandleShipAdded;
            _ships.Dispose();
            _ships = null;

            _asteroids.Dispose();
            _asteroids = null;

            _timers.EntityRemoved -= HandleTimerEnded;
            _timers.Dispose();
            _timers = null;
        }

        private void HandleShipAdded(ref Entity entity)
        {
            TryCreateAndScheduleAsteroidsCreation();
        }

        private void HandleTimerEnded(ref Entity referenced)
        {
            TryCreateAndScheduleAsteroidsCreation();
        }

        private void TryCreateAndScheduleAsteroidsCreation()
        {
            TryCreateAsteroids();
            ScheduleCreation();
        }

        private void TryCreateAsteroids()
        {
            var quantity = _gameConfiguration.MaxAsteroidQuantity - _asteroids.Count;
            if (quantity > 0)
            {
                CreateAsteroids(quantity);
            }
        }

        private void ScheduleCreation()
        {
            _entityFactory.CreateAsteroidSpawningTimer(_gameConfiguration.AsteroidSpawnInterval);
        }

        private void CreateAsteroids(int quantity)
        {
            if (_ships.Count == 0)
            {
                return;
            }

            ref var shipEntity = ref _ships.GetFirst();
            for (var i = 0; i < quantity; i++)
            {
                ref var positionComponent = ref shipEntity.GetComponent<PositionComponent>();

                while (_asteroids.Count < _gameConfiguration.MaxAsteroidQuantity)
                {
                    var groupConfigurationIndex = GetRandomAsteroidIndex();
                    var groupConfiguration = _gameConfiguration.AsteroidGroupConfigurations[groupConfigurationIndex];
                    var stateIndex = 0;
                    var asteroidStateInfo = groupConfiguration.AsteroidStates[stateIndex];
                    var asteroidConfiguration = asteroidStateInfo.AsteroidConfiguration;

                    var rotation = Quaternion.Euler(0, 0, Random.Range(0, MaxAngle));
                    var direction = (rotation * Vector3.up).normalized;
                    Vector2 offset = direction * asteroidConfiguration.MinMaxDistanceFromTarget.RandomRange();

                    var targetPosition = positionComponent.Position + offset;
                    Vector3 directionToTarget = (positionComponent.Position - targetPosition).normalized;
                    var targetOffsetAngle = asteroidConfiguration.MinMaxDiversionFromTargetDegrees.RandomRange() * RandomExtensions.RandomSign();
                    var directionOffset = Quaternion.Euler(0, 0, targetOffsetAngle);
                    var targetVelocityDirection = directionOffset * directionToTarget;
                    var targetVelocity = targetVelocityDirection * asteroidConfiguration.MinMaxVelocity.RandomRange();

                    var targetRotationDegrees = Random.Range(0, MaxAngle);
                    var targetAngularSpeed = asteroidConfiguration.MinMaxAngularSpeedDegrees.RandomRange();
                    _entityFactory.CreateAsteroid(groupConfigurationIndex, stateIndex, targetPosition, targetRotationDegrees, targetVelocity, targetAngularSpeed, asteroidStateInfo);
                }
            }
        }

        private int GetRandomAsteroidIndex()
        {
            var totalProbability = 0f;
            var targetProbability = Random.Range(0, _asteroidSpawnProbabilitiesSum);
            for (var i = 0; i < _asteroidSpawnProbabilitiesCached.Length; i++)
            {
                totalProbability += _asteroidSpawnProbabilitiesCached[i];
                if (targetProbability < totalProbability)
                {
                    return i;
                }
            }

            return _asteroidSpawnProbabilitiesCached.Length - 1;
        }
    }
}
