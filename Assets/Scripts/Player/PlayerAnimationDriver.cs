using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>Cac clip ma FSM co the yeu cau phat.</summary>
    public enum PlayerAnim
    {
        Idle,
        Walk,
        Jump,
        Fall,
        Dash
    }

    /// <summary>
    /// Phat animation KHONG co do tre.
    ///
    /// Co tinh khong dung parameter + transition cua Animator Controller: moi transition
    /// deu keo theo thoi gian blend va co the co Exit Time, nghia la animation luon chay
    /// sau logic vai frame. Thay vao do goi thang Animator.Play(hash, layer, 0f) - clip
    /// doi ngay trong frame trang thai doi.
    /// </summary>
    [System.Serializable]
    public class PlayerAnimationDriver
    {
        [Tooltip("Ten state trong Animator Controller, khong phai ten file clip.")]
        public string idleState = "player_idle";
        public string walkState = "player_walk";
        public string jumpState = "player_jump";
        [Tooltip("Chua co clip roi rieng thi de trung voi jump.")]
        public string fallState = "player_jump";
        [Tooltip("Chua co clip dash rieng thi de trung voi jump.")]
        public string dashState = "player_jump";

        Animator animator;
        int idleHash, walkHash, jumpHash, fallHash, dashHash;
        PlayerAnim? currentAnim;

        public void Initialise(Animator animator)
        {
            this.animator = animator;
            currentAnim = null;

            idleHash = Resolve(idleState);
            walkHash = Resolve(walkState);
            jumpHash = Resolve(jumpState);
            fallHash = Resolve(fallState);
            dashHash = Resolve(dashState);
        }

        /// <summary>Doi clip neu khac clip dang phat. Goi lai cung gia tri thi khong lam gi.</summary>
        public void Play(PlayerAnim anim)
        {
            if (animator == null || currentAnim == anim) return;

            int hash = HashOf(anim);
            if (hash == 0) return;

            currentAnim = anim;
            animator.Play(hash, 0, 0f);   // normalizedTime 0, khong blend
        }

        /// <summary>Phat lai tu dau du dang o chinh clip do. Dung cho don danh danh lien tiep.</summary>
        public void Replay(PlayerAnim anim)
        {
            currentAnim = null;
            Play(anim);
        }

        int HashOf(PlayerAnim anim)
        {
            switch (anim)
            {
                case PlayerAnim.Idle: return idleHash;
                case PlayerAnim.Walk: return walkHash;
                case PlayerAnim.Jump: return jumpHash;
                case PlayerAnim.Fall: return fallHash;
                case PlayerAnim.Dash: return dashHash;
                default: return 0;
            }
        }

        /// <summary>Tra ve 0 neu Animator khong co state nay, kem canh bao mot lan.</summary>
        int Resolve(string stateName)
        {
            if (string.IsNullOrEmpty(stateName)) return 0;

            int hash = Animator.StringToHash(stateName);
            if (animator.runtimeAnimatorController == null) return 0;

            if (!animator.HasState(0, hash))
            {
                Debug.LogWarning(
                    $"Animator khong co state '{stateName}'. Kiem tra lai ten trong Animator Controller.",
                    animator);
                return 0;
            }
            return hash;
        }
    }
}
