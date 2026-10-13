using System.Collections;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Thang may kieu Hollow Knight: be (Rigidbody2D Kinematic, layer Ground) chay doc giua hai diem dung.
    /// Goi bang <see cref="ElevatorSwitch"/> (can tren be = doi chieu, can goi o moi dau = goi be ve dau do).
    /// Nguoi choi dung tren be duoc dich theo be moi buoc vat ly nen khong bi rot / rung khi be chay.
    /// </summary>
    public class Elevator : MonoBehaviour
    {
        [Tooltip("Be thang (Rigidbody2D Kinematic + collider layer Ground).")]
        [SerializeField] Rigidbody2D platform;
        [Tooltip("Toa do Y the gioi cua goc be o diem dung duoi / tren.")]
        [SerializeField] float bottomY;
        [SerializeField] float topY;
        [SerializeField] bool startAtTop;
        [SerializeField] float speed = 7f;
        [Tooltip("Giay cho tu luc gat toi luc be chay.")]
        [SerializeField] float startDelay = 0.35f;
        [Tooltip("Vung kiem tra nguoi choi dung tren be (tinh tu goc be).")]
        [SerializeField] Vector2 riderBoxSize = new Vector2(5.4f, 0.5f);
        [SerializeField] Vector2 riderBoxOffset = new Vector2(0f, 0.55f);
        [Header("Cabin")]
        [Tooltip("Cua chan 2 ben: chi bat trong luc thang chay (nguoi choi khong ra duoc giua chung).")]
        [SerializeField] Collider2D[] sideGates;
        [Tooltip("Hinh cua chan (hien khi chay). De trong = khong co hinh.")]
        [SerializeField] GameObject gateVisual;
        [Tooltip("Hinh den cabin (hien khi chay).")]
        [SerializeField] GameObject runLights;
        [Header("Danh quai luc chay (kieu Nine Sols)")]
        [Tooltip("Prefab quai tha ra moi lan thang chay (de trong = khong co).")]
        [SerializeField] GameObject rideEnemyPrefab;
        [Tooltip("Vi tri tha, tinh tu be thang luc bat dau chay.")]
        [SerializeField] Vector2[] rideSpawnOffsets = { new Vector2(-2f, 6f), new Vector2(2f, 9f) };
        [SerializeField] bool spawnOnlyGoingUp = true;
        [SerializeField] float startShake = 0.1f;
        [SerializeField] float stopShake = 0.18f;

        readonly Collider2D[] hits = new Collider2D[8];
        bool atTop, moving;

        public bool IsMoving => moving;
        public bool AtTop => atTop;

        void Awake()
        {
            atTop = startAtTop;
            if (platform) platform.position = new Vector2(platform.position.x, atTop ? topY : bottomY);
            SetClosed(false);
        }

        void SetClosed(bool closed)
        {
            if (sideGates != null) foreach (var g in sideGates) if (g) g.enabled = closed;
            if (gateVisual) gateVisual.SetActive(closed);
            if (runLights) runLights.SetActive(closed);
        }

        /// <summary>Doi chieu (can tren be).</summary>
        public void Toggle() => MoveTo(!atTop);

        /// <summary>Goi be ve dau tren / duoi (can goi o moi dau). Be dang o do roi thi thoi.</summary>
        public void CallTo(bool top)
        {
            if (atTop != top) MoveTo(top);
        }

        void MoveTo(bool top)
        {
            if (moving || !platform) return;
            StartCoroutine(Run(top));
        }

        IEnumerator Run(bool top)
        {
            moving = true;
            SetClosed(true);
            if (startShake > 0f) CameraShake.Kick(startShake, 0.2f);
            yield return new WaitForSeconds(startDelay);
            if (rideEnemyPrefab && (top || !spawnOnlyGoingUp)) SpawnRideEnemies();
            float target = top ? topY : bottomY;
            var wait = new WaitForFixedUpdate();
            Rigidbody2D rider = null;
            float riderOffset = 0f;
            while (!Mathf.Approximately(platform.position.y, target))
            {
                yield return wait;
                float ny = Mathf.MoveTowards(platform.position.y, target, speed * Time.fixedDeltaTime);
                // Ghi khoang cach nguoi choi - be luc moi buoc len; nhay len (van toc doc duong) thi tha ra.
                var found = FindRider();
                if (found != rider)
                {
                    rider = found;
                    if (rider) riderOffset = rider.position.y - platform.position.y;
                }
                if (rider && rider.linearVelocity.y > 0.5f) rider = null;
                // Dat be va nguoi choi cung luc (khong dung MovePosition) de nguoi choi khong ho khoi be
                // trong mot buoc -> bo do dat khong bao "dang roi", animation dung yen.
                platform.position = new Vector2(platform.position.x, ny);
                if (rider)
                {
                    rider.position = new Vector2(rider.position.x, ny + riderOffset);
                    var v = rider.linearVelocity;
                    v.y = 0f;
                    rider.linearVelocity = v;
                }
            }
            atTop = top;
            moving = false;
            SetClosed(false);
            if (stopShake > 0f) CameraShake.Kick(stopShake, 0.2f);
        }

        readonly System.Collections.Generic.List<GameObject> spawned = new();

        /// <summary>Tha quai luc thang chay; dot truoc con con nao song thi khong tha them.</summary>
        void SpawnRideEnemies()
        {
            spawned.RemoveAll(e => !e || !e.activeInHierarchy);
            if (spawned.Count > 0) return;
            foreach (var o in rideSpawnOffsets)
                spawned.Add(Instantiate(rideEnemyPrefab, platform.position + o, Quaternion.identity));
        }

        Rigidbody2D FindRider()
        {
            var filter = new ContactFilter2D { useTriggers = false };
            int n = Physics2D.OverlapBox(platform.position + riderBoxOffset, riderBoxSize, 0f, filter, hits);
            for (int i = 0; i < n; i++)
            {
                var rb = hits[i].attachedRigidbody;
                if (rb && rb != platform && rb.CompareTag("Player")) return rb;
            }
            return null;
        }

        void OnDrawGizmosSelected()
        {
            if (!platform) return;
            Gizmos.color = new Color(0.27f, 0.86f, 0.77f, 0.8f);
            var x = platform.transform.position.x;
            Gizmos.DrawLine(new Vector3(x, bottomY), new Vector3(x, topY));
            Gizmos.DrawWireCube((Vector3)(platform.position + riderBoxOffset), riderBoxSize);
        }
    }
}
