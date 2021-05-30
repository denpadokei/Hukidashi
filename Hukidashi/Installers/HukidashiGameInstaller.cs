using SiraUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace Hukidashi.Installers
{
    public class HukidashiGameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<HukidashiController>().FromNewComponentOnNewGameObject(nameof(HukidashiController)).AsSingle().NonLazy();
        }
    }
}
