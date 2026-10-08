using UnityEngine;

namespace LuHanh.Effects
{
    /// <summary>Khoi lua pixel lien mach, dung mesh/material tao san va property block tai su dung.</summary>
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class CinderHuskFlameVisual : MonoBehaviour
    {
        public enum Kind { Fireball, Core, Blast }
        [Tooltip("Cau lua bay, loi than truy duoi hoac vu no.")]
        [SerializeField] Kind kind;
        [Tooltip("Ban kinh hinh theo Unity units; duong kinh bang chieu cao Player.")]
        [SerializeField, Min(0.05f)] float radius = 0.9205f;
        [Tooltip("Do dai vu no, giay; shader dung tien trinh nay thay vi thoi gian khong scale.")]
        [SerializeField, Min(0.1f)] float burstDuration = 0.65f;
        MeshRenderer meshRenderer;
        MaterialPropertyBlock properties;
        float phase, progress, direction = 1f, warning = 1f;
        bool bursting;
        float mode = -1f;
        static readonly int ModeId = Shader.PropertyToID("_Mode");
        static readonly int PhaseId = Shader.PropertyToID("_Phase");
        static readonly int ProgressId = Shader.PropertyToID("_Progress");
        static readonly int DirectionId = Shader.PropertyToID("_Direction");
        static readonly int WarningId = Shader.PropertyToID("_Warning");

        void EnsureReady()
        {
            if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
            if (properties == null) properties = new MaterialPropertyBlock();
        }
        void OnEnable()
        {
            phase = progress = 0f;
            mode = (float)kind;
            EnsureReady();
            meshRenderer.enabled = kind != Kind.Blast;
            Apply();
        }
        void Update()
        {
            phase += Time.deltaTime;
            if (bursting)
            {
                progress = Mathf.Clamp01(phase / burstDuration);
                if (progress >= 1f) { bursting = false; meshRenderer.enabled = false; }
            }
            Apply();
        }
        public void SetDirection(float sign)
        {
            mode = (float)kind;
            bursting = false;
            direction = sign >= 0f ? 1f : -1f;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * radius;
            Apply();
        }
        public void SetWarning(bool bright) { warning = bright ? 1f : 0.22f; Apply(); }
        public void PlayBlast(float blastRadius)
        {
            EnsureReady();
            phase = progress = 0f;
            radius = blastRadius;
            transform.localScale = Vector3.one * radius;
            bursting = true;
            meshRenderer.enabled = true;
            Apply();
        }
        public void PlayWallImpact(Vector2 normal, float size, float duration)
        {
            EnsureReady();
            phase = progress = 0f;
            mode = 3f;
            direction = 1f;
            burstDuration = duration;
            transform.localScale = Vector3.one * size;
            transform.localRotation = Quaternion.FromToRotation(Vector3.right, normal);
            bursting = true;
            meshRenderer.enabled = true;
            Apply();
        }
        public void ClearBlast()
        {
            bursting = false;
            if (kind == Kind.Blast) { EnsureReady(); meshRenderer.enabled = false; }
        }
        public void Preview(float time, float burstProgress = 0f)
        {
            EnsureReady();
            phase = time; progress = burstProgress;
            meshRenderer.enabled = true;
            Apply();
        }
        void Apply()
        {
            EnsureReady();
            if (mode < 0f) mode = (float)kind;
            properties.SetFloat(ModeId, mode);
            properties.SetFloat(PhaseId, phase);
            properties.SetFloat(ProgressId, progress);
            properties.SetFloat(DirectionId, direction);
            properties.SetFloat(WarningId, warning);
            meshRenderer.SetPropertyBlock(properties);
        }
    }
}
