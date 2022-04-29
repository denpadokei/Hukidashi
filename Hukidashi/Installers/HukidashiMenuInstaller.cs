using Zenject;

namespace Hukidashi.Installers
{
    public class HukidashiMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<HukidashiController>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
