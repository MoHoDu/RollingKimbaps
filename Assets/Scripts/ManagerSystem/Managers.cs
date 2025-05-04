using System.Collections.Generic;

namespace ManagerSystem
{
    public static class Managers
    {
        private static HashSet<IBaseManager> _managers = new();

        public static void Initialize()
        {
            
            foreach (var manager in _managers)
            {
                manager.Initialize();
            }
        }
    }
}