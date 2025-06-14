using System.Collections;
using System.Reflection;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DataPuller.Data;

namespace BeatChallenges.Challenges
{
    public class SlowDown : MonoBehaviour, IChallenge
    {
        public string Name => "Slow Down";
        public bool IsCompleted { get; private set; }
        public bool HasFailed { get; private set; }
        public Action OnCompleted { get; set; }

        private AudioTimeSyncController _audioTimeSync;
        private AudioSource _audioSource;
        private float _originalPitch;
        private float _originalTimeScale;
        private float _slowMultiplier;
        private float _duration;
        private object _njsProviderInstance;
        private float _originalNjsValue;
        private FieldInfo _jumpSpeedFieldInfo;
        private FieldInfo _timeScaleField;
        private TextMeshProUGUI _slowDownText;
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

            _slowMultiplier = 0.7f;
            _duration = 2f;

            _audioSource.pitch = _slowMultiplier;
            _timeScaleField.SetValue(_audioTimeSync, _slowMultiplier);

            if (_njsProviderInstance != null)
                _jumpSpeedFieldInfo.SetValue(_njsProviderInstance, _originalNjsValue * _slowMultiplier);

            ShowSlowDownUI();
            StartCoroutine(SlowDownRoutine());
        }

        public void StopChallenge()
        {
            StopAllCoroutines();
        }

        public void PauseChallenge() => isPaused = true;
        public void ResumeChallenge() => isPaused = false;

        private void ShowSlowDownUI()
        {
            Vector3 uiPosition = new Vector3(0f, 2.6f, 6f);

            GameObject canvasObj = new("SlowDownCanvas") { transform = { position = uiPosition } };

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject textObj = new GameObject("SlowDownText");
            textObj.transform.SetParent(canvasObj.transform);

            _slowDownText = textObj.AddComponent<TextMeshProUGUI>();
            _slowDownText.alignment = TextAlignmentOptions.Center;
            _slowDownText.fontSize = 0.3f;
            _slowDownText.rectTransform.sizeDelta = new Vector2(600, 100);
            _slowDownText.rectTransform.localPosition = Vector3.zero;
        }

        private IEnumerator SlowDownRoutine()
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
                    Debug.Log("[BeatChallenges] Challenge failed while completing slow down");

                    yield return new WaitForSeconds(2);
                    break;
                }

                _slowDownText.text = $"🐌 SLOW DOWN -30% FOR {timeLeft:0.0}s";
                yield return null;
            }

            if (!HasFailed)
            {
                IsCompleted = true;
                RevertSpeed();
            }

            CleanupUI();
        }

        private void RevertSpeed()
        {
            _audioSource.pitch = _originalPitch;
            _timeScaleField.SetValue(_audioTimeSync, _originalTimeScale);

            if (_njsProviderInstance != null)
                _jumpSpeedFieldInfo.SetValue(_njsProviderInstance, _originalNjsValue);
        }

        private void CleanupUI()
        {
            if (_slowDownText != null && _slowDownText.transform != null && _slowDownText.transform.parent != null)
                Destroy(_slowDownText.transform.parent.gameObject);

            _slowDownText = null;
        }
    }
}
