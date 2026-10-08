using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Toan bo phep tinh van toc ngang, theo cach cua Celeste:
    /// luon tien ve toc do muc tieu voi mot gia toc, tha phim thi muc tieu la 0 (ma sat),
    /// tren khong gia toc bi nhan airMult. Dang nhanh hon maxRun (sau dash, bi day)
    /// ma van giu cung huong thi chi giam tu tu bang runReduce, khong bi chan cung.
    /// </summary>
    public class HorizontalMover
    {
        readonly Rigidbody2D body;
        readonly PlayerMovementConfig config;

        /// <summary>Nhan toc do chay toi da (dang Shadow). 1 = binh thuong.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        public HorizontalMover(Rigidbody2D body, PlayerMovementConfig config)
        {
            this.body = body;
            this.config = config;
        }

        /// <param name="moveX">-1 / 0 / 1 huong nguoi choi muon di.</param>
        /// <param name="grounded">Dung gia toc mat dat hay gia toc tren khong.</param>
        public void Move(int moveX, bool grounded, float deltaTime)
        {
            float current = body.linearVelocity.x;
            float max = config.maxRun * SpeedMultiplier;
            float mult = grounded ? 1f : config.airMult;

            bool overSpeedSameDir = Mathf.Abs(current) > max && (int)Mathf.Sign(current) == moveX;
            float rate = overSpeedSameDir ? config.runReduce : config.runAccel;

            float next = Mathf.MoveTowards(current, max * moveX, rate * mult * deltaTime);
            body.linearVelocity = new Vector2(next, body.linearVelocity.y);
        }

        /// <summary>Dung ngay lap tuc theo phuong ngang. Dung cho choang, tiep dat cung...</summary>
        public void Halt()
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }
    }
}
