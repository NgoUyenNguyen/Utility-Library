using UnityEngine;

namespace NgoUyenNguyen
{
    [DefaultExecutionOrder(-1000)]
    internal class GlobalServiceLocatorBootstrapper : ServiceLocatorBootstrapper
    {
        [SerializeField] private bool dontDestroyOnLoad = true;

        protected override void Bootstrap()
        {
            Container.ConfigureAsGlobal(dontDestroyOnLoad);
        }
    }
}