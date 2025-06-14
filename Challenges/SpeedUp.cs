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
        
        private AudioTimeSyncController _audioTimeSync;
        private AudioSource _audioSource;
        private float _originalPitch;
        private float _originalTimeScale;
        private float _speedMultiplier;
        private float _duration;
        private object _njsProviderInstance;
        private float _originalNjsValue;
        private FieldInfo _jumpSpeedFieldInfo;
        private FieldInfo _timeScaleField;
        private TextMeshProUGUI _speedUpText;
        private bool isPaused = false;

        private void Awake()
        {
            _audioTimeSync = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
            if (_audioTimeSync == null)
            {
                Debug.LogError("[BeatChallenges] Missing AudioTimeSyncController");
                Destroy(this);
                return;
            }

            _audioSource = _audioTimeSync.GetComponent<AudioSource>();
            _originalPitch = _audioSource.pitch;

            _timeScaleField = typeof(AudioTimeSyncController).GetField("_timeScale", BindingFlags.Instance | BindingFlags.NonPublic);
            _originalTimeScale = (float)_timeScaleField.GetValue(_audioTimeSync);

            _njsProviderInstance = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .FirstOrDefault(x => x.GetType().Name == "NoteJumpValueProvider");

            if (_njsProviderInstance != null)
            {
                var type = _njsProviderInstance.GetType();
                _jumpSpeedFieldInfo = type.GetField("jumpSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (_jumpSpeedFieldInfo != null)
                    _originalNjsValue = (float)_jumpSpeedFieldInfo.GetValue(_njsProviderInstance);
                else
                    _njsProviderInstance = null;
            }
        }

        public void StartChallenge()
        {
            if (_audioTimeSync == null || _timeScaleField == null)
                return;

            _speedMultiplier = UnityEngine.Random.Range(1.05f, 1.20f);
            _duration = 10f;

            _audioSource.pitch = _speedMultiplier;
            _timeScaleField.SetValue(_audioTimeSync, _speedMultiplier);

            if (_njsProviderInstance != null)
            {
                _jumpSpeedFieldInfo.SetValue(_njsProviderInstance, _originalNjsValue * _speedMultiplier);
            }

            ShowSpeedUpUI();

            StartCoroutine(SpeedUpRoutine());
        }

        public void StopChallenge()
        {
            StopAllCoroutines();
        }

        public void PauseChallenge() => isPaused = true;
        public void ResumeChallenge() => isPaused = false;

        private void ShowSpeedUpUI()
        {
            Vector3 uiPosition = new Vector3(0f, 2.6f, 6f);

            GameObject canvasObj = new("SpeedUpCanvas") { transform = { position = uiPosition } };

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject textObj = new GameObject("SpeedUpText");
            textObj.transform.SetParent(canvasObj.transform);

            _speedUpText = textObj.AddComponent<TextMeshProUGUI>();
            _speedUpText.alignment = TextAlignmentOptions.Center;
            _speedUpText.fontSize = 0.3f;
            _speedUpText.rectTransform.sizeDelta = new Vector2(600, 100);
            _speedUpText.rectTransform.localPosition = Vector3.zero;
        }

        private IEnumerator SpeedUpRoutine()
        {
            float timeLeft = _duration;

            while (timeLeft > 0f && !HasFailed)
            {
                if (isPaused)
                {
                    yield return null;
                    continue;
                }

                timeLeft -= Time.deltaTime;

                if (MapData.Instance.LevelFailed)
                {
                    HasFailed = true;
                    _speedUpText.text = "❌ Too fast for you? Challenge failed.";
                    Debug.Log("[BeatChallenges] Challenge failed while completing speed up");

                    yield return new WaitForSeconds(2);
                    break;
                }

                _speedUpText.text = $"💨 SPEED UP! + {(_speedMultiplier - 1f) * 100f:0}% FOR {timeLeft:0.0}s";
                yield return null;
            }

            if (!HasFailed)
            {
                IsCompleted = true;
                RevertSpeed();
                _speedUpText.text = "✅ Speed survived! Nice reflexes!";
                Debug.Log("[BeatChallenges] Challenge completed successfully!");
                yield return new WaitForSeconds(2);
            }

            CleanupUI();
        }

        private void CleanupUI()
        {
            if (_speedUpText != null && _speedUpText.transform != null && _speedUpText.transform.parent != null)
                Destroy(_speedUpText.transform.parent.gameObject);

            _speedUpText = null;
        }

        private void RevertSpeed()
        {
            _audioSource.pitch = _originalPitch;
            _timeScaleField.SetValue(_audioTimeSync, _originalTimeScale);

            if (_njsProviderInstance != null)
                _jumpSpeedFieldInfo.SetValue(_njsProviderInstance, _originalNjsValue);
        }
    }
}
