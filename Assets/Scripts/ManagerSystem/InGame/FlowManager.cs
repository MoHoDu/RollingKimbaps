using System.Collections.Generic;
using System.Linq;
using GameDatas;
using UIs.Base;
using ManagerSystem.Base;


namespace ManagerSystem.InGame
{
    public class FlowManager : BaseManager
    {
        // DI
        private PrapManager _prapManager;
        private RaceStatus _raceStatus;
        private List<FlowLayer> _flowLayers;

        private float _tickDuration = 1f;

        public override void Initialize(params object[] datas)
        {
            foreach (var data in datas)
            {
                if (data is InGameManager inGameManager)
                {
                    _prapManager = inGameManager.Prap;
                    _raceStatus = inGameManager.Status.RaceStatus;
                    _tickDuration = _raceStatus.TickTime;
                }
                else if (data is FlowLayer[] layers)
                {
                    _flowLayers = layers.ToList();
                }
            }

            foreach (FlowLayer layer in _flowLayers)
            {
                layer.SetDuration(_tickDuration);
            }
        }

        public override void Tick()
        {
            foreach (var layer in _flowLayers)
            {
                layer.Flow(_raceStatus.TickDistance);
            }
        }

        public void OnStopGame()
        {
            foreach (var layer in _flowLayers)
            {
                layer.StopFlow();
            }
        }
    }
}