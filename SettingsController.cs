using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using UnityEngine;
using System.Collections;

namespace BeatChallenges
{
    public class SettingsController
    {
        private bool _enabled = Plugin.GeneratedPluginConfig.Enabled;

        [UIValue("Enabled")]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;

                _enabled = value;
                Plugin.GeneratedPluginConfig.Enabled = value;
                Plugin.Log.Info($"Mod enabled state changed: {value}");

                if (!value && ChallengeManager.Instance != null)
                    ChallengeManager.Instance.CleanupChallenges();
            }
        }

        public void InitSafely()
        {
            GameObject obj = new("BeatChallenges.SettingsDelay");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.AddComponent<BSMLInitWaiter>().settingsController = this;
        }

        public void Register()
        {
            Plugin.Log.Info("Registering settings menu...");
            BSMLSettings.Instance.AddSettingsMenu("Beat Challenges", "BeatChallenges.UI.SettingsView.bsml", this);
        }

        private class BSMLInitWaiter : MonoBehaviour
        {
            public SettingsController settingsController;

            private IEnumerator Start()
            {
                yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().Length > 0);
                settingsController.Register();
                Destroy(gameObject);
            }
        }
    }
}