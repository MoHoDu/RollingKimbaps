namespace UIs
{
    public class ForeTreesPanel : BackgroundObjControllerPanel<TreeUI>
    {
        protected override void Initialize()
        {
            _prefabPath = "Prefabs/TreeObj_Fore";
            _hierarchyName = "TreeObj_Fore";
            _instantiateCount = 8;
            _onDestroyedPosX = -50f;
            _instantiateSpace = 10f;
            _instantiateSpaceGap = 10f;
            
            base.Initialize();
        }
    }
}