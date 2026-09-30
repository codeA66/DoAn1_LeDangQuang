using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Gom input di chuyen lai mot cho va lo phan dem nut nhay / dash.
    ///
    /// Dem nut la mot nua cua game feel: nguoi choi bam nhay truoc khi cham dat vai
    /// phan tram giay thi van phai an. Nua con lai la coyote time, nam o GroundSensor.
    /// </summary>
    public class MovementInput
    {
        readonly float bufferDuration;

        float lastJumpPressedTime = float.NegativeInfinity;
        bool jumpConsumed = true;

        float lastDashPressedTime = float.NegativeInfinity;
        bool dashConsumed = true;

        public MovementInput(float bufferDuration)
        {
            this.bufferDuration = bufferDuration;
        }

        /// <summary>-1 sang trai, +1 sang phai.</summary>
        public float Horizontal { get; private set; }

        /// <summary>-1 xuong, +1 len. Dung cho huong dash va roi nhanh.</summary>
        public float Vertical { get; private set; }

        /// <summary>Dang giu nut nhay hay khong - dung cho nhay cao thap theo do giu.</summary>
        public bool JumpHeld { get; private set; }

        public bool HasHorizontal => Mathf.Abs(Horizontal) > 0.01f;

        /// <summary>Huong ngang dang giu dang so nguyen -1 / 0 / 1 nhu Celeste.</summary>
        public int MoveX => Horizontal > 0.01f ? 1 : Horizontal < -0.01f ? -1 : 0;

        /// <summary>Huong doc dang giu dang so nguyen -1 / 0 / 1.</summary>
        public int MoveY => Vertical > 0.5f ? 1 : Vertical < -0.5f ? -1 : 0;

        /// <summary>Con mot lenh nhay chua dung va chua het han dem.</summary>
        public bool JumpBuffered => !jumpConsumed && Time.time - lastJumpPressedTime <= bufferDuration;

        /// <summary>Con mot lenh dash chua dung va chua het han dem.</summary>
        public bool DashBuffered => !dashConsumed && Time.time - lastDashPressedTime <= bufferDuration;

        public void SetMove(Vector2 value)
        {
            Horizontal = Mathf.Clamp(value.x, -1f, 1f);
            Vertical = Mathf.Clamp(value.y, -1f, 1f);
        }

        public void PressJump()
        {
            lastJumpPressedTime = Time.time;
            jumpConsumed = false;
            JumpHeld = true;
        }

        public void ReleaseJump() => JumpHeld = false;

        /// <summary>Lay lenh nhay ra khoi dem. Goi khi da thuc su nhay.</summary>
        public void ConsumeJump() => jumpConsumed = true;

        public void PressDash()
        {
            lastDashPressedTime = Time.time;
            dashConsumed = false;
        }

        /// <summary>Lay lenh dash ra khoi dem. Goi khi da thuc su dash.</summary>
        public void ConsumeDash() => dashConsumed = true;
    }
}
