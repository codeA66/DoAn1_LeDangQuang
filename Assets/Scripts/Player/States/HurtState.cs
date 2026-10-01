using UnityEngine;

namespace LuHanh.Player.States
{
    /// <summary>
    /// Bi danh trung: bat nguoc ra xa nguon sat thuong, mat dieu khien mot khoang ngan,
    /// van chiu trong luc. Het gio thi FSM tu chuyen sang Fall/Idle/Run.
    /// </summary>
    public class HurtState : PlayerStateBase
    {
        public HurtState(PlayerContext context) : base(context) { }

        Vector2 launchVelocity;
        float duration;
        float timer;

        public bool Finished => timer <= 0f;

        /// <summary>Goi truoc khi vao trang thai: van toc bat ra va thoi gian mat dieu khien.</summary>
        public void Setup(Vector2 velocity, float seconds)
        {
            launchVelocity = velocity;
            duration = seconds;
        }

        public override void Enter()
        {
            timer = duration;
            Ctx.Body.linearVelocity = launchVelocity;
            Ctx.CancelJumpHold();
            // Bo Cloaked khong co clip bi thuong - dung tam clip roi.
            Ctx.Animation.Play(PlayerAnim.Fall);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            timer -= fixedDeltaTime;
            Ctx.ApplyGravity(fixedDeltaTime);
        }
    }
}
