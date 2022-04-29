using Hukidashi.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace Hukidashi
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }


        #region BSIPA Config
        [Init]
        public void InitWithConfig(IPALogger logger, Zenjector zenjector, Config conf)
        {
            Instance = this;
            Log = logger;
            Log.Info("Hukidashi initialized.");
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            zenjector.Install<HukidashiAppInstaller>(Location.App);
            zenjector.Install<HukidashiMenuInstaller>(Location.Menu | Location.Player);
            Log.Debug("Config loaded");
        }
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            Log.Debug("OnApplicationStart");
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");

        }
    }
}
