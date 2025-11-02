namespace NgoUyenNguyen.ServiceLocator
{
    internal class SceneServiceLocatorBootstrapper : ServiceLocatorBootstrapper
    {
        protected override void Bootstrap()
        {
            Container.ConfigureForScene();
        }
    }
}