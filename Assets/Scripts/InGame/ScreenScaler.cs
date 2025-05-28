using UnityEngine;

namespace InGame
{
    public static class ScreenScaler
    {
        public const float MAX_WIDTH = 1920f;
        public const float MARGIN = 500f;
        public static float WIDTH;
        public static Vector3 CAM_LEFTSIDE;
        public static Vector3 CAM_RIGHTSIDE;
        
        public static void Initialize()
        {
            Camera cam = Camera.main;
            CAM_LEFTSIDE = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
            CAM_RIGHTSIDE = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, 0));
            WIDTH = CAM_RIGHTSIDE.x - CAM_LEFTSIDE.x;
        }
    }
}