using System.Collections.Generic;
using LuHanh.Core;
using LuHanh.Effects;
using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Pool dan dung chung: moi prefab dan mot pool, tao luc can lan dau va tao san vai vien.
    /// Ban dan chi bat/tat object co san -> khong cap phat, khong rac, khong giat GC.
    /// Kem vai ban hieu ung no (FrameAnimation) dung xoay vong: vien dan vo thi lay ban ke tiep,
    /// dich toi cho do va Play (nhieu vien vo gan nhau van moi vien mot vu no).
    ///
    /// Cach dung: <c>ProjectilePool.For(prefab, burstPrefab).Spawn(vi tri, van toc)</c>.
    /// Nguoi choi hoi sinh thi moi vien dang bay bi thu ve.
    /// </summary>
    public class ProjectilePool : MonoBehaviour, IResettable
    {
        static readonly Dictionary<Projectile, ProjectilePool> Pools = new();
        static Transform root;

        Projectile prefab;
        FrameAnimation[] bursts;
        int nextBurst;
        readonly Stack<Projectile> free = new();
        readonly List<Projectile> active = new();

        /// <summary>Lay pool cua prefab; chua co thi tao moi va tao san <paramref name="prewarm"/> vien.</summary>
        public static ProjectilePool For(Projectile prefab, FrameAnimation burstPrefab = null, int prewarm = 6, int burstCopies = 3)
        {
            if (Pools.TryGetValue(prefab, out var existing) && existing) return existing;

            if (!root) root = new GameObject("Pools").transform;
            var pool = new GameObject($"Pool_{prefab.name}").AddComponent<ProjectilePool>();
            pool.transform.SetParent(root, false);
            pool.prefab = prefab;
            if (burstPrefab)
            {
                pool.bursts = new FrameAnimation[Mathf.Max(1, burstCopies)];
                for (int i = 0; i < pool.bursts.Length; i++) pool.bursts[i] = Instantiate(burstPrefab, pool.transform);
            }
            for (int i = 0; i < prewarm; i++) pool.free.Push(pool.Create());

            Pools[prefab] = pool;
            RespawnManager.Register(pool);
            return pool;
        }

        Projectile Create()
        {
            var p = Instantiate(prefab, transform);
            p.gameObject.SetActive(false);
            return p;
        }

        public Projectile Spawn(Vector2 position, Vector2 velocity)
        {
            var p = free.Count > 0 ? free.Pop() : Create();
            p.gameObject.SetActive(true);
            p.Launch(this, position, velocity);
            active.Add(p);
            return p;
        }

        public void Return(Projectile p, bool playBurst)
        {
            if (!p.gameObject.activeSelf) return;
            if (playBurst && bursts != null)
            {
                var b = bursts[nextBurst];
                nextBurst = (nextBurst + 1) % bursts.Length;
                b.transform.position = p.transform.position;
                b.Play();
            }
            p.gameObject.SetActive(false);
            active.Remove(p);
            free.Push(p);
        }

        /// <summary>Nguoi choi hoi sinh: thu ve moi vien dang bay.</summary>
        public void ResetState()
        {
            for (int i = active.Count - 1; i >= 0; i--) Return(active[i], false);
        }

        void OnDestroy()
        {
            RespawnManager.Unregister(this);
            if (prefab && Pools.TryGetValue(prefab, out var p) && p == this) Pools.Remove(prefab);
        }
    }
}
