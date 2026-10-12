using System.Collections.Generic;
using LuHanh.Combat;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Ban sao bong cua nguoi choi do Echoling tha ra: di lai dung duong nguoi choi da di (moi buoc vat ly mot mau),
    /// cung tu the (dung / chay / nhay / chem). Cham vao mat mau; doan nguoi choi chem thi bat them vung chem.
    /// Khong co IDamageable nen don chem di xuyen qua. Het duong thi tat.
    /// </summary>
    public class EchoGhost : MonoBehaviour
    {
        public enum Pose { Stand, Run, Jump, Slash }

        public struct Sample
        {
            public Vector2 position;
            public Pose pose;
            public bool facingRight;
        }

        [Tooltip("Vung chem (object con, ContactDamage). Chi bat o cac mau nguoi choi dang chem.")]
        [SerializeField] ContactDamage slashHitbox;

        static readonly int[] PoseHashes =
        {
            Animator.StringToHash("stand"), Animator.StringToHash("run"),
            Animator.StringToHash("jump"), Animator.StringToHash("slash")
        };

        readonly List<Sample> samples = new List<Sample>();
        Animator animator;
        int index;
        int currentHash;

        public bool Playing => gameObject.activeSelf;

        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            gameObject.SetActive(false);
        }

        public void Play(List<Sample> path)
        {
            if (path.Count == 0) return;
            samples.Clear();
            samples.AddRange(path);
            index = 0;
            currentHash = 0;
            transform.SetParent(null, true);
            gameObject.SetActive(true);
            Apply(samples[0]);
        }

        public void Stop() => gameObject.SetActive(false);

        void FixedUpdate()
        {
            index++;
            if (index >= samples.Count) { Stop(); return; }
            Apply(samples[index]);
        }

        void Apply(Sample s)
        {
            transform.position = new Vector3(s.position.x, s.position.y, transform.position.z);
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (s.facingRight ? 1f : -1f);
            transform.localScale = scale;
            if (slashHitbox) slashHitbox.enabled = s.pose == Pose.Slash;

            int hash = PoseHashes[(int)s.pose];
            if (hash == currentHash || !animator) return;
            currentHash = hash;
            animator.Play(hash, 0, 0f);
        }
    }
}
