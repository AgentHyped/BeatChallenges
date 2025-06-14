using System.Collections;
using UnityEngine;
using BeatChallenges.Challenges;
using DataPuller.Data;

namespace BeatChallenges
{
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance { get; private set; }

        private readonly List<Type> challengeTypes =
        [
            typeof(NoMisses),
            typeof(SpeedUp),
            typeof(GhostNotes),
            typeof(SlowDown),
            typeof(ComboRush),
        ];

        private bool levelActive = false;
        private float levelStartTime = -1f;
        private bool isPaused = false;
        private bool wasPaused = false;
        private IChallenge activeChallenge = null;
        private Coroutine challengeLoopCoroutine = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            if (MapData.Instance != null)
                MapData.Instance.OnUpdate += HandleMapUpdate;
        }

        void OnDisable()
        {
            if (MapData.Instance != null)
                MapData.Instance.OnUpdate -= HandleMapUpdate;
        }

        private void HandleMapUpdate(string newHash)
        {
            if (!Plugin.GeneratedPluginConfig.Enabled)
                return;

            var md = MapData.Instance;

            if (!levelActive && md.InLevel)
            {
                Debug.Log("[BeatChallenges] Level started.");
                levelActive = true;
                levelStartTime = Time.time;
                challengeLoopCoroutine = StartCoroutine(ChallengeLoop());
            }
            else if (levelActive && !md.InLevel)
            {
                Debug.Log("[BeatChallenges] Level ended.");
                levelActive = false;
                StopCoroutine(challengeLoopCoroutine);
                CleanupChallenges();
            }
        }

        void Update()
        {
            if (!Plugin.GeneratedPluginConfig.Enabled || !levelActive)
                return;

            bool currentlyPaused = MapData.Instance.LevelPaused;

            if (currentlyPaused && !wasPaused)
            {
                isPaused = true;
                PauseChallenge();
            }
            else if (!currentlyPaused && wasPaused)
            {
                isPaused = false;
                ResumeChallenge();
            }

            wasPaused = currentlyPaused;
        }

        private void PauseChallenge()
        {
            Debug.Log("[BeatChallenges] Pausing active challenge.");
            activeChallenge?.PauseChallenge();
        }

        private void ResumeChallenge()
        {
            Debug.Log("[BeatChallenges] Resuming active challenge.");
            activeChallenge?.ResumeChallenge();
        }

        private IEnumerator ChallengeLoop()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 15f));

            while (levelActive)
            {
                if (!isPaused && activeChallenge == null)
                {
                    float elapsed = LiveData.Instance?.TimeElapsed ?? 0f;
                    float duration = MapData.Instance?.Duration ?? 0f;

                    float remainingTime = duration - elapsed;

                    if (remainingTime > 15f)
                    {
                        StartRandomChallenge();
                    }
                    else
                    {
                        Debug.Log("[BeatChallenges] Not starting challenge: less than 15 seconds remaining.");
                    }
                }

                yield return new WaitForSeconds(UnityEngine.Random.Range(15f, 20f));
            }
        }

        private void StartRandomChallenge()
        {
            if (!Plugin.GeneratedPluginConfig.Enabled) return;

            int index = UnityEngine.Random.Range(0, challengeTypes.Count);
            Type selected = challengeTypes[index];

            Debug.Log($"[BeatChallenges] Starting new challenge: {selected.Name}");

            Component comp = gameObject.AddComponent(selected);

            if (comp is IChallenge challenge)
            {
                activeChallenge = challenge;
                challenge.StartChallenge();

                StartCoroutine(WaitForChallengeEnd(challenge));
            }
            else
            {
                Debug.LogError("Added component does not implement IChallenge!");
            }
        }

        private IEnumerator WaitForChallengeEnd(IChallenge challenge)
        {
            while (!challenge.IsCompleted && !challenge.HasFailed)
                yield return null;

            yield return new WaitForSeconds(2.2f);

            Destroy(challenge as Component);
            activeChallenge = null;
        }

        public void CleanupChallenges()
        {
            StopAllCoroutines();
            foreach (var c in GetComponents<IChallenge>())
                Destroy(c as Component);

            activeChallenge = null;
        }
    }
}