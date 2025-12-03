using UnityEngine;

namespace NgoUyenNguyen
{
    [DefaultExecutionOrder(-999)]
    internal class SceneServiceLocatorBootstrapper : ServiceLocatorBootstrapper
    {
        protected override void Bootstrap()
        {
            Container.ConfigureForScene();
        }
    }
}