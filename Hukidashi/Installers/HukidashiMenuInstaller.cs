using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;
using SiraUtil;

namespace Hukidashi.Installers
{
    public class HukidashiMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            this.Container.BindInterfacesAndSelfTo<HukidashiController>().FromNewComponentOnNewGameObject(nameof(HukidashiController)).AsSingle().NonLazy();
        }
    }
}
