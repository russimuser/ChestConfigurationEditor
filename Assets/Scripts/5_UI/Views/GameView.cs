using Asteroids.Gameplay.States;
using Asteroids.Installation;
using Asteroids.GameplayECS.Components;
using Asteroids.GameplayECS.Extensions;
using Asteroids.Services;
using Asteroids.Services.Project;
using Asteroids.Tools;
using Asteroids.UnsafeTools;
using Asteroids.ValueTypeECS.EntityGroup;
using TMPro;
using UnityEngine;
using Zenject;

namespace Asteroids.UI.Views
{
    [RequireComponent(typeof(Injector))]
    [DisallowMultipleComponent]
    public class GameView : MonoBehaviour, IInjectable
    {
        [SerializeField] private TMP_Text _positionLabel;
        [SerializeField] private TMP_Text _rotationLabel;
        [SerializeField] private TMP_Text _velocityLabel;
        [SerializeField] private TMP_Text _chargesCountLabel;
        [SerializeField] private TMP_Text _rechargeTimerLabel;

        [Inject] private readonly IStateContext _stateContext;
        [Inject] private readonly IInstanceSpawner _instanceSpawner;
        [Inject] private readonly IFrameInfoService _frameInfoService;

        private EntityGroup _playerGroup;

        private Vector2MutableStringPresenter _positionText = new Vector2MutableStringPresenter(NumericFormats.G4);
        private SingleMutableStringPresenter _rotationText = new SingleMutableStringPresenter(NumericFormats.G4);
        private SingleMutableStringPresenter _velocityText = new SingleMutableStringPresenter(NumericFormats.G4);
        private IntegerMutableStringPresenter _chargesCountText = new IntegerMutableStringPresenter(NumericFormats.G4);
        private SingleMutableStringPresenter _rechargeTimerText = new SingleMutableStringPresenter(NumericFormats.G4);

        private void Awake()
        {
            _playerGroup = _instanceSpawner.Instantiate<EntityGroupBuilder>()
                .RequireComponent<PlayerComponent>()
                .RequireComponent<PositionComponent>()
                .RequireComponent<RotationComponent>()
                .RequireComponent<VelocityComponent>()
                .RequireComponent<LaserGunComponent>()
                .RequireComponent<LaserAutoChargingComponent>()
                .Build();
        }

        private void OnDestroy()
        {
            _playerGroup.Dispose();
            _playerGroup = null;
        }

        private void Update()
        {
            if (_playerGroup.Count == 0)
            {
                return;
            }

            ref var playerEntity = ref _playerGroup.GetFirst();
            ref var positionComponent = ref playerEntity.GetComponent<PositionComponent>();
            ref var rotationComponent = ref playerEntity.GetComponent<RotationComponent>();
            ref var velocityComponent = ref playerEntity.GetComponent<VelocityComponent>();
            ref var laserGunComponent = ref playerEntity.GetComponent<LaserGunComponent>();

            _positionText.UpdateContent(positionComponent.Position);
            _positionLabel.SetText(_positionText.ToString());

            _rotationText.UpdateContent(rotationComponent.RotationDegrees);
            _rotationLabel.SetText(_rotationText.ToString());

            _velocityText.UpdateContent((velocityComponent.Velocity * _frameInfoService.DeltaTime).magnitude);
            _velocityLabel.SetText(_velocityText.ToString());

            _chargesCountText.UpdateContent(laserGunComponent.ChargesCount);
            _chargesCountLabel.SetText(_chargesCountText.ToString());

            var laserReadyTime = laserGunComponent.LastFireTime + laserGunComponent.Configuration.FiringInterval;
            var isLaserReady = laserReadyTime < _frameInfoService.StartTime;
            var timerValue = isLaserReady ? 0 : laserReadyTime - _frameInfoService.StartTime;
            _rechargeTimerText.UpdateContent(timerValue);
            _rechargeTimerLabel.SetText(_rechargeTimerText.ToString());
        }

        [ContextMenu("Test")]
        public void Test()
        {
            var playerGroup = _instanceSpawner.Instantiate<EntityGroupBuilder>()
                .RequireComponent<PlayerComponent>()
                .RequireComponent<PositionComponent>()
                .RequireComponent<RotationComponent>()
                .RequireComponent<VelocityComponent>()
                .RequireComponent<LaserGunComponent>()
                .RequireComponent<LaserAutoChargingComponent>()
                .Build();

            var first = playerGroup.GetFirst();
            playerGroup.Dispose();
        }
    }
}
