using LuHanh.Combat;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Soi re do noi Threadwitch voi mot quai (quai do duoc buff chi so). Object rieng, khong phai con cua
    /// quai nao, de don chem tim thay chinh soi day (IDamageable) chu khong phai quai. Chem trung thi day dut.
    /// Hinh: SpriteRenderer Tiled keo dai Thread.png; collider trigger nam doc soi day.
    /// </summary>
    public class ThreadLink : MonoBehaviour, IDamageable
    {
        ThreadwitchController owner;
        Transform from;
        SpriteRenderer line;
        BoxCollider2D hitArea;
        float thickness;
        ParticleSystem buffFx;

        public EnemyBase Target { get; private set; }

        public static ThreadLink Create(ThreadwitchController owner, Transform from, EnemyBase target, Sprite sprite, float thickness, int sortingOrder, ParticleSystem buffFxPrefab)
        {
            var go = new GameObject("ThreadLink") { layer = owner.gameObject.layer };
            var link = go.AddComponent<ThreadLink>();
            link.owner = owner;
            link.from = from;
            link.Target = target;
            link.thickness = thickness;
            link.line = go.AddComponent<SpriteRenderer>();
            link.line.sprite = sprite;
            link.line.drawMode = SpriteDrawMode.Tiled;
            link.line.sortingOrder = sortingOrder;
            link.hitArea = go.AddComponent<BoxCollider2D>();
            link.hitArea.isTrigger = true;
            target.AddBuff();
            if (buffFxPrefab) link.buffFx = SpawnBuffFx(buffFxPrefab, target);
            link.LateUpdate();
            return link;
        }

        Vector2 TargetPoint
        {
            get
            {
                var c = Target ? Target.GetComponent<Collider2D>() : null;
                return c && c.enabled ? (Vector2)c.bounds.center : (Vector2)Target.transform.position;
            }
        }

        void LateUpdate()
        {
            if (!Target || !from) return;
            Vector2 a = from.position, b = TargetPoint;
            Vector2 d = b - a;
            float len = Mathf.Max(0.01f, d.magnitude);
            transform.position = (a + b) * 0.5f;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            line.size = new Vector2(len, thickness);
            hitArea.size = new Vector2(len, Mathf.Max(thickness, 0.35f));
        }

        /// <summary>Lua toa quanh quai duoc buff: con cua quai, vung phat bang khung collider than.</summary>
        static ParticleSystem SpawnBuffFx(ParticleSystem prefab, EnemyBase target)
        {
            var fx = Instantiate(prefab, target.transform);
            var col = target.GetComponent<Collider2D>();
            if (col && col.enabled)
            {
                var b = col.bounds;
                fx.transform.position = new Vector3(b.center.x, b.center.y, target.transform.position.z);
                var shape = fx.shape;
                shape.scale = new Vector3(b.size.x * 0.9f, b.size.y * 0.9f, 1f);
            }
            fx.transform.localScale = Vector3.one;
            fx.Play();
            return fx;
        }

        public void TakeHit(HitInfo hit) => owner.OnLinkCut(this);

        /// <summary>Go day: quai het buff.</summary>
        public void Release()
        {
            if (Target) Target.RemoveBuff();
            if (buffFx)
            {
                // Ngung phat, hat dang bay tat dan roi tu huy.
                buffFx.transform.SetParent(null, true);
                buffFx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(buffFx.gameObject, buffFx.main.startLifetime.constantMax + 0.1f);
            }
            Target = null;
            Destroy(gameObject);
        }
    }
}
