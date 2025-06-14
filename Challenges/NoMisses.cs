using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using DataPuller.Data;
using System.Reflection;

namespace BeatChallenges.Challenges
{
    public class NoMisses : MonoBehaviour, IChallenge
    {
        public string Name => "Sudden Death";
        public bool IsCompleted { get; private set; }
        public bool HasFailed { get; private set; }
        public Action OnCompleted { get; set; }

        private TextMeshProUGUI challengeText;
        private float countdown = 12f;
        private int initialMissCount;
        private bool isPaused = false;

        public void StartChallenge()
        {
            IsCompleted = false;
            HasFailed = false;
            initialMissCount = LiveData.Instance.Misses;

            if (LiveData.Instance == null)
            {
                Debug.LogError("[BeatChallenges] PlayerData.Instance is null!");
                return;
            }
            StartCoroutine(Countdown());
        }

        public void StopChallenge()
        {
            StopAllCoroutines();
        }

        public void PauseChallenge() => isPaused = true;
        public void ResumeChallenge() => isPaused = false;

        private IEnumerator Countdown()
        {
            ShowUI();

            while (countdown > 0f && !HasFailed)
            {
                if (isPaused)
                {
                    yield return null;
                    continue;
                }

                countdown -= Time.deltaTime;

                if (LiveData.Instance.Misses > initialMissCount)
                {
                    HasFailed = true;
                    challengeText.text = "💀 You missed...";
                    Debug.Log("[BeatChallenges] Challenge failed due to a missed note.");

                    ForceFailLevel();

                    yield return new WaitForSeconds(2);
                    break;
                }

                challengeText.text = $"🩸 SUDDEN DEATH! Dont miss for {countdown:F1}s";
                yield return null;
            }

            if (!HasFailed)
            {
                IsCompleted = true;
                challengeText.text = "✅ Survived Sudden Death!";
                Debug.Log("[BeatChallenges] Challenge completed successfully!");
                yield return new WaitForSeconds(2);
            }

            CleanupUI();
        }

        private void CleanupUI()
        {
            if (challengeText != null && challengeText.transform != null && challengeText.transform.parent != null)
                Destroy(challengeText.transform.parent.gameObject);

            challengeText = null;
        }

        private void ShowUI()
        {
            Vector3 uiPosition = new Vector3(0f, 2.6f, 6f);

            GameObject canvasObj = new("ChallengeCanvas") { transform = { position = uiPosition } };

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject textObj = new GameObject("ChallengeText");
            textObj.transform.SetParent(canvasObj.transform);

            challengeText = textObj.AddComponent<TextMeshProUGUI>();
            challengeText.alignment = TextAlignmentOptions.Center;
            challengeText.fontSize = 0.3f;
            challengeText.text = $"{Name}";

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.localPosition = Vector3.zero;
            textRect.sizeDelta = new Vector2(600, 100);
        }

        private void ForceFailLevel()
        {
            var failController = Resources.FindObjectsOfTypeAll<StandardLevelFailedController>().FirstOrDefault();
            if (failController == null)
            {
                Debug.LogError("[BeatChallenges] Could not find StandardLevelFailedController instance.");
                return;
            }

            var failMethod = typeof(StandardLevelFailedController).GetMethod("HandleLevelFailed", BindingFlags.Instance | BindingFlags.NonPublic);
            if (failMethod == null)
            {
                Debug.LogError("[BeatChallenges] Could not find HandleLevelFailed method.");
                return;
            }

            failMethod.Invoke(failController, null);
            Debug.Log("[BeatChallenges] Level fail triggered via HandleLevelFailed.");
        }
    }
}