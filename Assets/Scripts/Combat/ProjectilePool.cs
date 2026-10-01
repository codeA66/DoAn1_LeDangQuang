using System.Collections.Generic;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Pool dan dung chung: moi prefab dan mot pool, tao luc can lan dau va tao san vai vien.
    /// Ban dan chi bat/tat object co san -> khong cap phat, khong rac, khong giat GC.
    /// Kem mot particle "vo" dung chung: vien dan vo thi dich toi cho do va Emit.
    ///
    /// Cach dung: <c>ProjectilePool.For(prefab, burstPrefab).Spawn(vi tri, van toc)</c>.
    /// Nguoi choi hoi sinh thi moi vien dang bay bi thu ve.
    /// </summary>
    public class ProjectilePool : MonoBehaviour, IResettable
    {
        static readonly Dictionary<Projectile, ProjectilePool> Pools = new();
        static Transform root;

        Projectile prefab;
        ParticleSystem burst;
        int burstCount = 8;
        readonly Stack<Projectile> free = new();
        readonly List<Projectile> active = new();

        /// <summary>Lay pool cua prefab; chua co thi tao moi va tao san <paramref name="prewarm"/> vien.</summary>
        public static ProjectilePool For(Projectile prefab, ParticleSystem burstPrefab = null, int prewarm = 6, int burstParticles = 8)
        {
            if (Pools.TryGetValue(prefab, out var existing) && existing) return existing;

            if (!root) root = new GameObject("Pools").transform;
            var pool = new GameObject($"Pool_{prefab.name}").AddComponent<ProjectilePool>();
            pool.transform.SetParent(root, false);
            pool.prefab = prefab;
            pool.burstCount = burstParticles;
            if (burstPrefab) pool.burst = Instantiate(burstPrefab, pool.transform);
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
            if (playBurst && burst)
            {
                burst.transform.position = p.transform.position;
                burst.Emit(burstCount);
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
