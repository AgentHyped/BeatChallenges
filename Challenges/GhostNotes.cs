using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DataPuller.Data;

namespace BeatChallenges.Challenges
{
    public class GhostNotes : MonoBehaviour, IChallenge
    {
        public string Name => "Ghost Notes";
        public bool IsCompleted { get; private set; } = false;
        public bool HasFailed { get; private set; } = false;
        public Action OnCompleted { get; set; }

        private float duration = 10f;
        private float timer = 0f;
        private bool challengeActive = false;
        private TextMeshProUGUI Text;
        public static bool IsGhostModeActive { get; private set; } = false;
        private Coroutine _ghostNotesCoroutine;
        private Coroutine _uiRoutine;
        private bool isPaused = false;

        public void StartChallenge()
        {
            if (challengeActive) return;

            challengeActive = true;
            timer = duration;
            IsCompleted = false;
            HasFailed = false;
            IsGhostModeActive = true;

            if (_ghostNotesCoroutine != null)
                StopCoroutine(_ghostNotesCoroutine);

            _ghostNotesCoroutine = StartCoroutine(GhostNotesRoutine());
            _uiRoutine = StartCoroutine(Routine());
        }

        public void StopChallenge()
        {
            StopAllCoroutines();
            CleanupUI();
            RemoveGhostNotes();
            IsGhostModeActive = false;
            challengeActive = false;
            IsCompleted = true;
        }

        public void PauseChallenge() => isPaused = true;
        public void ResumeChallenge() => isPaused = false;

        private IEnumerator Routine()
        {
            UI();

            while (timer > 0f && !HasFailed)
            {
                if (isPaused)
                {
                    yield return null;
                    continue;
                }

                timer -= Time.deltaTime;

                if (MapData.Instance.LevelFailed)
                {
                    HasFailed = true;
                    Text.text = "❌ Ghost notes challenge failed!";
                    Debug.Log("[BeatChallenges] Challenge failed while completing ghost notes");

                    yield return new WaitForSeconds(2);
                    break;
                }

                Text.text = $"👻 GHOST NOTES! Stay sharp for {timer:F1}s";
                yield return null;
            }

            if (!HasFailed)
            {
                IsCompleted = true;
                Text.text = "✅ Ghost Notes survived!";
                Debug.Log("[BeatChallenges] Challenge completed successfully!");
                yield return new WaitForSeconds(2);
            }

            CleanupUI();
            RemoveGhostNotes();
            IsGhostModeActive = false;
            challengeActive = false;
        }

        private void CleanupUI()
        {
            if (Text != null && Text.transform != null && Text.transform.parent != null)
                Destroy(Text.transform.parent.gameObject);

            Text = null;
        }

        private IEnumerator GhostNotesRoutine()
        {
            Debug.Log("[BeatChallenges] Starting ghost notes effect.");

            while (timer > 0f && !HasFailed)
            {
                if (!isPaused)
                    ApplyGhostNotes();

                yield return new WaitForSeconds(0.2f);
            }

            Debug.Log("[BeatChallenges] Ghost notes effect ended.");
        }

        private void ApplyGhostNotes()
        {
            foreach (var note in FindObjectsOfType<NoteController>())
            {
                var meshRenderers = note.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var renderer in meshRenderers)
                {
                    string name = renderer.gameObject.name.ToLower();
                    if (name.Contains("arrow")) continue;

                    Material material = renderer.material;
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = 0.05f;
                        material.color = color;
                    }
                    else
                    {
                        renderer.enabled = false;
                    }
                }
            }
        }

        private void RemoveGhostNotes()
        {
            Debug.Log("[BeatChallenges] Restoring all notes to visible.");
            foreach (var note in FindObjectsOfType<NoteController>())
            {
                var meshRenderer = note.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer != null)
                {
                    Material material = meshRenderer.material;
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = 1f;
                        material.color = color;
                    }
                }
            }
        }

        private void UI()
        {
            Vector3 uiPosition = new Vector3(0f, 2.6f, 6f);

            GameObject canvasObj = new("NoteComboCanvas") { transform = { position = uiPosition } };

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(canvasObj.transform);

            Text = textObj.AddComponent<TextMeshProUGUI>();
            Text.alignment = TextAlignmentOptions.Center;
            Text.fontSize = 0.3f;
            Text.rectTransform.sizeDelta = new Vector2(600, 100);
            Text.rectTransform.localPosition = Vector3.zero;
        }
    }
}
