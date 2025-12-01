using UnityEngine;

namespace NgoUyenNguyen
{
    internal class GlobalServiceLocatorBootstrapper : ServiceLocatorBootstrapper
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap()
        {
            Container.ConfigureAsGlobal(dontDestroyOnLoad);
        }
    }
}