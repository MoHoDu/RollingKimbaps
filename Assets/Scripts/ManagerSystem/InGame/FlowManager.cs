﻿using System.Collections.Generic;
using System.Linq;
using GameDatas;
using Panels.Base;

namespace ManagerSystem.InGame
{
    public class FlowManager : BaseManager
    {
        // DI
        private RaceStatus _raceStatus;
        
        private List<FlowLayer> _flowLayers;

        public override void Initialize(params object[] datas)
        {
            foreach (var data in datas)
            {
                if (data is FlowLayer[] layers)
                {
                    _flowLayers = layers.ToList();
                }
            }
        }

        public override void Tick()
        {
            foreach (var layer in _flowLayers)
            {
                layer.Flow(_raceStatus.TickDistance);
            }
        }
    }
}