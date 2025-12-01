namespace NgoUyenNguyen
{
    internal class SceneServiceLocatorBootstrapper : ServiceLocatorBootstrapper
    {
        protected override void Bootstrap()
        {
            Container.ConfigureForScene();
        }
    }
}