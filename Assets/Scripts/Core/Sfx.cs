using UnityEngine;

namespace LuHanh.Core
{
    public enum SfxId
    {
        Swing, HitEnemy, EnemyDeath, PlayerHurt, Jump, Land, Dash, WallCling,
        OrbCast, OrbImpact, Bench, Heal, SkillPickup, ShardPickup, WallCollapse, GateSlam, Lever, Elevator
    }

    /// <summary>
    /// Phat am thanh hieu ung: goi <see cref="Play"/> o bat ky dau. Dat trong LevelEssentials/GameSystems;
    /// tao san mot nhom AudioSource 2D dung xoay vong, moi tieng co nhieu clip (chon ngau nhien), am luong
    /// va do lech cao do ngau nhien de khong nhai lai y het.
    /// </summary>
    public class Sfx : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public SfxId id;
            public AudioClip[] clips;
            [Range(0f, 1f)] public float volume = 0.8f;
            [Tooltip("Do lech cao do ngau nhien (+/-).")]
            [Range(0f, 0.5f)] public float pitchJitter = 0.08f;
            [Tooltip("Khoang cach toi thieu giua 2 lan phat (giay) - tranh don tieng khi trung nhieu muc tieu.")]
            public float minInterval = 0.03f;
        }

        static Sfx instance;

        [SerializeField] Entry[] entries;
        [Range(0f, 1f)] [SerializeField] float masterVolume = 1f;
        [Min(1)] [SerializeField] int voices = 12;

        AudioSource[] sources;
        int next;
        Entry[] byId;
        float[] lastPlayed;

        void Awake()
        {
            instance = this;
            sources = new AudioSource[voices];
            for (int i = 0; i < voices; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                sources[i] = s;
            }
            int n = System.Enum.GetValues(typeof(SfxId)).Length;
            byId = new Entry[n];
            lastPlayed = new float[n];
            if (entries != null) foreach (var e in entries) if (e != null) byId[(int)e.id] = e;
        }

        void OnDestroy() { if (instance == this) instance = null; }

        /// <summary>Phat tieng <paramref name="id"/> (khong co clip thi bo qua). <paramref name="volumeScale"/> nhan them am luong.</summary>
        public static void Play(SfxId id, float volumeScale = 1f)
        {
            if (instance) instance.PlayInternal(id, volumeScale);
        }

        void PlayInternal(SfxId id, float volumeScale)
        {
            var e = byId[(int)id];
            if (e == null || e.clips == null || e.clips.Length == 0) return;
            float now = Time.unscaledTime;
            if (now - lastPlayed[(int)id] < e.minInterval) return;
            lastPlayed[(int)id] = now;
            var clip = e.clips[Random.Range(0, e.clips.Length)];
            if (!clip) return;
            var s = sources[next];
            next = (next + 1) % sources.Length;
            s.pitch = 1f + Random.Range(-e.pitchJitter, e.pitchJitter);
            s.PlayOneShot(clip, e.volume * volumeScale * masterVolume);
        }
    }
}
