using UIs.Base;

namespace UIs
{
    public class BackTerrainsPanel : BackgroundObjControllerPanel<BackTerrainUI>
    {
        protected override void Initialize()
        {
            _prefabPath = "Prefabs/Back_terrain";
            _hierarchyName = "Back_terrains";
            _instantiateCount = 8;
            _onDestroyedPosX = -50f;
            _instantiateSpace = 14.7f;
            
            base.Initialize();
        }
    }
}