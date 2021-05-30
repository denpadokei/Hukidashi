using Hukidashi.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
