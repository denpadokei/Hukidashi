using Hukidashi.WebSockets;
using Zenject;

namespace Hukidashi.Installers
{
    public class HukidashiAppInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<HukidashiWebSocketServer>().AsSingle();
        }
    }
}
