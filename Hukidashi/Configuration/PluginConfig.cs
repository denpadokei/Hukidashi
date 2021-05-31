using System;
using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Hukidashi.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual float HukidashiScale { get; set; } = 0.03f;
        public virtual float HukidashiPosX { get; set; } = 5f;
        public virtual float HukidashiPosY { get; set; } = 0.3f;
        public virtual float HukidashiPosZ { get; set; } = 1f;
        public virtual string OBSSouceName { get; set; } = "txt_jp";
        public virtual string TargetCameraName { get; set; } = "cameraplus.cfg";
        public virtual int ModPort { get; set; } = 4443;
        public virtual int OBSPort { get; set; } = 4444;

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
            this.HukidashiScale = other.HukidashiScale;
            this.HukidashiPosX = other.HukidashiPosX;
            this.HukidashiPosY = other.HukidashiPosY;
            this.HukidashiPosZ = other.HukidashiPosZ;
            this.OBSSouceName = other.OBSSouceName;
            this.TargetCameraName = other.TargetCameraName;
            this.ModPort = other.ModPort;
            this.OBSPort = other.OBSPort;
        }
    }
}
