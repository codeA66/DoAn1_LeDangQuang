using System.Collections;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// San sap: nguoi choi dung len thi nut dan trong <see cref="crumbleTime"/> giay, vo roi, mat va cham;
    /// sau <see cref="respawnTime"/> giay hien lai. Collider dac tren object goc (layer Ground).
    /// </summary>
    public class CrumblePlatform : MonoBehaviour
    {
        [SerializeField] SpriteRenderer visual;
        [SerializeField] Sprite[] stateFrames;      // nguyen, nut, nut nang
        [SerializeField] Sprite[] breakFrames;
        [SerializeField] float crumbleTime = 0.6f;
        [SerializeField] float respawnTime = 3f;
        [SerializeField] float fps = 12f;
        [SerializeField] float jitter = 0.04f;

        Collider2D solid;
        bool busy;
        Vector3 baseLocal;

        void Awake()
        {
            solid = GetComponent<Collider2D>();
            if (visual) baseLocal = visual.transform.localPosition;
        }

        void OnCollisionEnter2D(Collision2D c)
        {
            if (busy || !c.collider.CompareTag("Player")) return;
            if (c.collider.bounds.min.y < solid.bounds.max.y - 0.2f) return;
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            busy = true;
            int n = stateFrames != null ? stateFrames.Length : 0;
            for (float t = 0f; t < crumbleTime; t += Time.deltaTime)
            {
                if (visual)
                {
                    if (n > 0) visual.sprite = stateFrames[Mathf.Min(n - 1, Mathf.FloorToInt(t / crumbleTime * n))];
                    visual.transform.localPosition = baseLocal + (Vector3)(Random.insideUnitCircle * jitter);
                }
                yield return null;
            }
            solid.enabled = false;
            if (visual)
            {
                visual.transform.localPosition = baseLocal;
                if (breakFrames != null)
                    foreach (var f in breakFrames) { visual.sprite = f; yield return new WaitForSeconds(1f / fps); }
                visual.enabled = false;
            }
            yield return new WaitForSeconds(respawnTime);
            solid.enabled = true;
            if (visual)
            {
                visual.enabled = true;
                if (n > 0) visual.sprite = stateFrames[0];
            }
            busy = false;
        }
    }
}
