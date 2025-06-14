using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DataPuller.Data;
using System.Reflection;

namespace BeatChallenges.Challenges
{
    public class ComboRush : MonoBehaviour, IChallenge
    {
        public string Name => "Combo Rush";
        public bool IsCompleted { get; private set; }
        public bool HasFailed { get; private set; }
        public Action OnCompleted { get; set; }

        private TextMeshProUGUI _comboText;
        private bool _isPaused = false;

        private float _duration = 15f;
        private float _elapsed = 0f;
        private bool _hasEverReached50 = false;

        public void StartChallenge()
        {
            ShowComboUI();
            StartCoroutine(ComboCheckRoutine());
        }

        public void StopChallenge()
        {
            StopAllCoroutines();
            CleanupUI();
        }

        public void PauseChallenge() => _isPaused = true;
        public void ResumeChallenge() => _isPaused = false;

        private void ShowComboUI()
        {
            Vector3 uiPosition = new Vector3(0f, 2.6f, 6f);

            GameObject canvasObj = new("NoteComboCanvas") { transform = { position = uiPosition } };

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject textObj = new("ComboText");
            textObj.transform.SetParent(canvasObj.transform);
            _comboText = textObj.AddComponent<TextMeshProUGUI>();
            _comboText.alignment = TextAlignmentOptions.Center;
            _comboText.fontSize = 0.3f;
            _comboText.rectTransform.sizeDelta = new Vector2(600, 100);
            _comboText.rectTransform.localPosition = Vector3.zero;
        }

        private IEnumerator ComboCheckRoutine()
        {
            while (_elapsed < _duration && !IsCompleted && !HasFailed)
            {
                if (_isPaused)
                {
                    yield return null;
                    continue;
                }

                _elapsed += Time.deltaTime;
                int combo = LiveData.Instance.Combo;

                if (combo >= 50)
                {
                    _hasEverReached50 = true;
                    _comboText.text = $"🔥 Maintain 50 Combo! ({_elapsed:0.0}s)";
                }
                else
                {
                    _hasEverReached50 = false;
                    _comboText.text = $"💥 COMBO RUSH! Reach 50 combo ({combo}/50) - {_elapsed:0.0}s left";
                }

                if (MapData.Instance.LevelFailed)
                {
                    HasFailed = true;
                    yield return new WaitForSeconds(2);
                    break;
                }

                yield return null;
            }

            if (_hasEverReached50)
            {
                IsCompleted = true;
                _comboText.text = "✅ Combo Master! You held strong!";
                Debug.Log("[BeatChallenges] Challenge completed successfully!");
                yield return new WaitForSeconds(2);
            }
            else
            {
                HasFailed = true;
                _comboText.text = "❌ Challenge failed: Combo too weak!";
                Debug.Log("[BeatChallenges] Challenge failed while completing combo rush");

                ForceFailLevel();

                yield return new WaitForSeconds(2);
            }

            CleanupUI();
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

        private void CleanupUI()
        {
            if (_comboText?.transform?.parent != null)
                Destroy(_comboText.transform.parent.gameObject);

            _comboText = null;
        }
    }
}
