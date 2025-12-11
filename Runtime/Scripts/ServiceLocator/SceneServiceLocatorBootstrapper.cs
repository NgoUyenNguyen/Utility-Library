using UnityEngine;

namespace NgoUyenNguyen
{
    [DefaultExecutionOrder(-999)]
    public class SceneServiceLocatorBootstrapper : ServiceLocatorBootstrapper
    {
        protected override void Bootstrap()
        {
            Container.ConfigureForScene();
        }
    }
}