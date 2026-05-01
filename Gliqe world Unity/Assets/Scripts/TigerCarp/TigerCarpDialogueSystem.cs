using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GliqeWorld.TigerCarp
{
    /// <summary>
    /// Event-driven dialogue queue for the Tiger-Carp companion.
    /// Fires when the player enters zone trigger volumes or performs specific actions.
    /// Renders text as diegetic world-space UI near the carp — not a HUD element.
    /// </summary>
    public class TigerCarpDialogueSystem : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [SerializeField] private TigerCarpController carp;
        [SerializeField] private TextMeshPro dialogueText;
        [SerializeField] private float lineDisplayDuration = 3f;
        [SerializeField] private float fadeDuration = 0.4f;

        // ── State ────────────────────────────────────────────────────────────────

        private readonly Queue<DialogueEntry> _queue = new();
        private bool _speaking;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Adds a dialogue entry to the queue.</summary>
        public void Enqueue(DialogueEntry entry)
        {
            if (entry == null) return;
            _queue.Enqueue(entry);
            if (!_speaking)
                StartCoroutine(ProcessQueue());
        }

        // ── Coroutines ───────────────────────────────────────────────────────────

        private IEnumerator ProcessQueue()
        {
            _speaking = true;

            while (_queue.Count > 0)
            {
                DialogueEntry entry = _queue.Dequeue();
                yield return new WaitForSeconds(entry.delay);

                carp?.TriggerExpression(entry.expression);

                foreach (string line in entry.lines)
                {
                    yield return StartCoroutine(ShowLine(line));
                    yield return new WaitForSeconds(lineDisplayDuration);
                    yield return StartCoroutine(FadeOut());
                }
            }

            _speaking = false;
        }

        private IEnumerator ShowLine(string line)
        {
            if (dialogueText == null) yield break;

            dialogueText.text = line;
            dialogueText.gameObject.SetActive(true);

            Color c = dialogueText.color;
            c.a = 0f;
            dialogueText.color = c;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Clamp01(t / fadeDuration);
                dialogueText.color = c;
                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            if (dialogueText == null) yield break;

            Color c = dialogueText.color;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Clamp01(1f - t / fadeDuration);
                dialogueText.color = c;
                yield return null;
            }
            dialogueText.gameObject.SetActive(false);
        }
    }
}
