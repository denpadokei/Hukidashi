using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Hukidashi.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual float MenuHukidashiScale { get; set; } = 0.03f;
        public virtual float MenuHukidashiPosX { get; set; } = 5f;
        public virtual float MenuHukidashiPosY { get; set; } = 0.3f;
        public virtual float MenuHukidashiPosZ { get; set; } = 1f;
        public virtual string MenuTargetCameraName { get; set; } = "cameraplus.cfg";
        public virtual float GameHukidashiScale { get; set; } = 0.03f;
        public virtual float GameHukidashiPosX { get; set; } = 5f;
        public virtual float GameHukidashiPosY { get; set; } = 0.3f;
        public virtual float GameHukidashiPosZ { get; set; } = 1f;
        public virtual string GameTargetCameraName { get; set; } = "cameraplus.cfg";

        public virtual int ModPort { get; set; } = 4443;

        public event Action<PluginConfig> OnChanged;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
            this.OnChanged?.Invoke(this);
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
            this.MenuHukidashiScale = other.MenuHukidashiScale;
            this.MenuHukidashiPosX = other.MenuHukidashiPosX;
            this.MenuHukidashiPosY = other.MenuHukidashiPosY;
            this.MenuHukidashiPosZ = other.MenuHukidashiPosZ;
            this.MenuTargetCameraName = other.MenuTargetCameraName;

            this.GameHukidashiScale = other.GameHukidashiScale;
            this.GameHukidashiPosX = other.GameHukidashiPosX;
            this.GameHukidashiPosY = other.GameHukidashiPosY;
            this.GameHukidashiPosZ = other.GameHukidashiPosZ;
            this.GameTargetCameraName = other.GameTargetCameraName;

            this.ModPort = other.ModPort;
        }
    }
}
