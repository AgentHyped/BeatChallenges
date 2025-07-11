using System.Collections;
using System.Reflection;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DataPuller.Data;

namespace BeatChallenges.Challenges
{
    public class SpeedUp : MonoBehaviour, IChallenge
    {
        public string Name => "Speed Up";
        public bool IsCompleted { get; private set; }
        public bool HasFailed { get; private set; }
        public Action OnCompleted { get; set; }

        private AudioTimeSyncController _audioSync;
        private AudioSource _audioSource;
        private float _originalPitch;
        private float _originalTimeScale;
        private float _speedMultiplier;
        private float _countdown = 12f;

        private object _njsProvider;
        private float _originalNjs;
        private FieldInfo _jumpSpeedField;
        private FieldInfo _timeScaleField;

        private TextMeshProUGUI _challengeText;
        private bool _isPaused = false;

        private void Awake()
        {
            _audioSync = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
            if (_audioSync == null)
            {
                Debug.LogError("[BeatChallenges] Missing AudioTimeSyncController");
                Destroy(this);
                return;
            }

            _audioSource = _audioSync.GetComponent<AudioSource>();
            _originalPitch = _audioSource.pitch;

            _timeScaleField = typeof(AudioTimeSyncController).GetField("_timeScale", BindingFlags.Instance | BindingFlags.NonPublic);
            _originalTimeScale = (float)_timeScaleField.GetValue(_audioSync);

            _njsProvider = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .FirstOrDefault(x => x.GetType().Name == "NoteJumpValueProvider");

            if (_njsProvider != null)
            {
                var type = _njsProvider.GetType();
                _jumpSpeedField = type.GetField("jumpSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (_jumpSpeedField != null)
                    _originalNjs = (float)_jumpSpeedField.GetValue(_njsProvider);
                else
                    _njsProvider = null;
            }
        }

        public void StartChallenge()
        {
            if (_audioSync == null || _timeScaleField == null) return;

            _speedMultiplier = UnityEngine.Random.Range(1.15f, 1.20f);

            _audioSource.pitch = _speedMultiplier;
            _timeScaleField.SetValue(_audioSync, _speedMultiplier);

            if (_njsProvider != null)
                _jumpSpeedField.SetValue(_njsProvider, _originalNjs * _speedMultiplier);

            ShowUI();
            StartCoroutine(SpeedUpRoutine());
        }

        public void StopChallenge() => StopAllCoroutines();
        public void PauseChallenge() => _isPaused = true;
        public void ResumeChallenge() => _isPaused = false;

        private void ShowUI()
        {
            var canvasObj = new GameObject("SpeedUpCanvas") { transform = { position = new Vector3(0f, 2.6f, 6f) } };
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            canvasObj.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
            canvasObj.AddComponent<GraphicRaycaster>();

            var textObj = new GameObject("SpeedUpText");
            textObj.transform.SetParent(canvasObj.transform);

            _challengeText = textObj.AddComponent<TextMeshProUGUI>();
            _challengeText.alignment = TextAlignmentOptions.Center;
            _challengeText.fontSize = 0.3f;
            _challengeText.rectTransform.sizeDelta = new Vector2(600, 100);
            _challengeText.rectTransform.localPosition = Vector3.zero;
        }

        private IEnumerator SpeedUpRoutine()
        {
            while (_countdown > 0f && !HasFailed)
            {
                if (_isPaused)
                {
                    yield return null;
                    continue;
                }

                _countdown -= Time.deltaTime;

                if (MapData.Instance.LevelFailed)
                {
                    HasFailed = true;
                    _challengeText.text = "❌ Too fast for you? Challenge failed.";
                    Debug.Log("[BeatChallenges] Challenge failed during speed up.");
                    yield return new WaitForSeconds(2);
                    break;
                }

                _challengeText.text = $"💨 SPEED UP! + {(_speedMultiplier - 1f) * 100f:0}% FOR {_countdown:0.0}s";
                yield return null;
            }

            if (!HasFailed)
            {
                IsCompleted = true;
                RevertSpeed();
                _challengeText.text = "✅ Speed survived! Nice reflexes!";
                Debug.Log("[BeatChallenges] Challenge completed successfully!");
                yield return new WaitForSeconds(2);
            }

            CleanupUI();
        }

        private void CleanupUI()
        {
            if (_challengeText?.transform?.parent != null)
                Destroy(_challengeText.transform.parent.gameObject);

            _challengeText = null;
        }

        private void RevertSpeed()
        {
            _audioSource.pitch = _originalPitch;
            _timeScaleField.SetValue(_audioSync, _originalTimeScale);

            if (_njsProvider != null)
                _jumpSpeedField.SetValue(_njsProvider, _originalNjs);
        }
    }
}
