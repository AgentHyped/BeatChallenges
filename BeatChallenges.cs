using IPA;
using IPA.Config;
using IPA.Config.Stores;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace BeatChallenges
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static PluginConfig GeneratedPluginConfig { get; private set; }
        internal static IPALogger Log { get; private set; }
        private SettingsController _settingsController;

        [Init]
        public void Init(IPALogger logger, Config config)
        {
            Log = logger;
            GeneratedPluginConfig = config.Generated<PluginConfig>();
            Log.Info("BeatChallenges is firing up!");
        }

        private GameObject _managerObject;

        [OnEnable]
        public void OnEnable()
        {
            _settingsController = new SettingsController();
            _settingsController.InitSafely();

            if (!GeneratedPluginConfig.Enabled)
            {
                Log.Info("BeatChallenges is disabled via ingame settings.");
                return;
            }

            Log.Info("BeatChallenges is running!");

            if (_managerObject == null)
            {
                _managerObject = new GameObject("ChallengeManager");
                GameObject.DontDestroyOnLoad(_managerObject);
                _managerObject.AddComponent<ChallengeManager>();
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            Log.Info("BeatChallenges powering down.");
        }
    }
}
