using UnityEngine;

namespace LuHanh.Player.States
{
    /// <summary>
    /// Dash 8 huong kieu Celeste: van toc co dinh, khong trong luc trong DashTime.
    /// Ket thuc thi con lai EndDashSpeed theo huong dash (tru khi dash xuong),
    /// dash len thi van toc doc cuoi bi giam them EndDashUpMult.
    /// </summary>
    public class DashState : PlayerStateBase
    {
        float timer;
        Vector2 direction;

        public DashState(PlayerContext context) : base(context) { }

        /// <summary>Het thoi gian dash - FSM doc de chuyen sang trang thai tiep theo.</summary>
        public bool Finished => timer <= 0f;

        public override void Enter()
        {
            direction = Ctx.DashDirection;
            Ctx.BeginDash();
            timer = Ctx.Config.dashTime;

            Ctx.Body.linearVelocity = direction * Ctx.Config.dashSpeed;
            Ctx.Facing.FaceTowards(direction.x);
            Ctx.Animation.Play(PlayerAnim.Dash);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (Finished) return;

            Ctx.TryCornerCorrection(fixedDeltaTime);

            timer -= fixedDeltaTime;
            if (Finished) EndDash();
        }

        void EndDash()
        {
            // Dash xuong (co thanh phan y am) thi giu nguyen toc do dang co.
            if (direction.y < 0f) return;

            var v = direction * Ctx.Config.endDashSpeed;
            if (v.y > 0f) v.y *= Ctx.Config.endDashUpMult;
            Ctx.Body.linearVelocity = v;
        }
    }
}
