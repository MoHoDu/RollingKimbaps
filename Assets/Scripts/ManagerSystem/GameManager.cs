using UnityEngine;

namespace ManagerSystem
{
    [RequireComponent(typeof(ResourceManager))]
    public class GameManager : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);

            ResourceManager resource = GetComponent<ResourceManager>();
            Managers.Initialize(resource);
        }

        private void Start()
        {
            Managers.Start();
        }

        private void Update()
        {
            Managers.Update();
        }

        private void FixedUpdate()
        {
            Managers.FixedUpdate();
        }
    }
}