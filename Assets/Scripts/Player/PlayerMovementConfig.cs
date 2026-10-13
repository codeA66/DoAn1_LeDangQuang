using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Toan bo so lieu chinh tay cua chuyen dong. Tach ra ScriptableObject de designer
    /// chinh duoc ma khong dung vao code, va de co nhieu bo so cho cac trang thai khac nhau
    /// (binh thuong / Ban Hoa Quy / duoi nuoc...).
    ///
    /// Gia tri mac dinh lay theo Player.cs cua Celeste, doi don vi: Celeste 8 pixel = 1 o,
    /// o day 1 unit = 1 o, nen moi so pixel deu chia 8.
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Player Movement Config", fileName = "PlayerMovementConfig")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [Header("Chay")]
        [Tooltip("Toc do chay toi da, unit/giay. Celeste: MaxRun = 90px.")]
        public float maxRun = 11.25f;
        [Tooltip("Tang toc, va giam toc ve 0 khi tha phim, unit/giay^2. Celeste: RunAccel = 1000px.")]
        public float runAccel = 125f;
        [Tooltip("Giam toc khi dang nhanh hon maxRun ma van giu cung huong - khong bi chan cung. Celeste: RunReduce = 400px.")]
        public float runReduce = 50f;
        [Tooltip("He so gia toc khi o tren khong. Celeste: AirMult = 0.65.")]
        [Range(0f, 1f)] public float airMult = 0.65f;

        [Header("Nhay")]
        [Tooltip("Van toc bat len, unit/giay. Celeste: JumpSpeed = 105px.")]
        public float jumpSpeed = 13.125f;
        [Tooltip("Van toc ngang cong them luc bat nhay theo huong dang giu. Celeste: JumpHBoost = 40px.")]
        public float jumpHBoost = 5f;
        [Tooltip("Giu phim nhay thi giu nguyen van toc bat len toi da bao lau. Celeste: VarJumpTime = 0.2.")]
        public float varJumpTime = 0.2f;

        [Header("Trong luc")]
        [Tooltip("Unit/giay^2. Celeste: Gravity = 900px.")]
        public float gravity = 112.5f;
        [Tooltip("Gan dinh (|van toc doc| nho hon nguong nay) ma con giu nhay thi trong luc chi con mot nua. Celeste: HalfGravThreshold = 40px.")]
        public float halfGravThreshold = 5f;
        [Tooltip("Toc do roi toi da. Celeste: MaxFall = 160px.")]
        public float maxFall = 20f;
        [Tooltip("Toc do roi toi da khi giu phim xuong. Celeste: FastMaxFall = 240px.")]
        public float fastMaxFall = 30f;
        [Tooltip("Toc do doi qua lai giua maxFall va fastMaxFall. Celeste: FastMaxAccel = 300px.")]
        public float fastMaxAccel = 37.5f;

        [Header("Dash")]
        [Tooltip("Celeste: DashSpeed = 240px.")]
        public float dashSpeed = 30f;
        [Tooltip("Van toc con lai khi dash ket thuc (tru dash cheo xuong / xuong). Celeste: EndDashSpeed = 160px.")]
        public float endDashSpeed = 20f;
        [Tooltip("Dash huong len thi van toc doc cuoi nhan them he so nay. Celeste: EndDashUpMult = 0.75.")]
        [Range(0f, 1f)] public float endDashUpMult = 0.75f;
        [Tooltip("Thoi gian mot cu dash. Celeste: DashTime = 0.15.")]
        public float dashTime = 0.15f;
        [Tooltip("Khoang nghi toi thieu giua hai cu dash. Celeste: DashCooldown = 0.2.")]
        public float dashCooldown = 0.2f;
        [Tooltip("Vua dash xong chua duoc hoi dash ngay du dang cham dat. Celeste: DashRefillCooldown = 0.1.")]
        public float dashRefillCooldown = 0.1f;
        [Tooltip("So lan dash, hoi lai khi cham dat. Celeste: 1.")]
        [Min(0)] public int maxDashes = 1;

        [Header("Do dung sai - phan lam game feel de chiu")]
        [Tooltip("Roi khoi mep roi van nhay duoc trong bao lau. Celeste: JumpGraceTime = 0.1.")]
        public float coyoteTime = 0.1f;
        [Tooltip("Bam nhay / dash truoc bao lau thi van tinh. Celeste: 0.08.")]
        public float inputBuffer = 0.08f;
        [Tooltip("Dau vuong mep tran trong khoang nay thi tu truot sang ben de di tiep. Celeste: 4px.")]
        public float cornerCorrection = 0.5f;
        [Tooltip("Buoc thu khi tim cho truot. Celeste thu tung pixel = 1/8 unit.")]
        public float cornerCorrectionStep = 0.125f;

        [Header("Bam tuong (ky nang mo khoa)")]
        [Tooltip("Toc do truot xuong toi da khi bam tuong (Hollow Knight truot cham).")]
        public float wallSlideSpeed = 3f;
        [Tooltip("Van toc ngang khi dap tuong bat ra.")]
        public float wallJumpHSpeed = 9f;
        [Tooltip("Van toc len khi dap tuong (ty le so voi jumpSpeed).")]
        [Range(0.5f, 1.2f)] public float wallJumpVRatio = 0.95f;
        [Tooltip("Sau khi dap tuong, bo qua phim trai/phai trong bao lau de khong dinh lai tuong ngay.")]
        public float wallJumpLockTime = 0.16f;
        [Tooltip("Khoang do tuong canh hai ben collider.")]
        public float wallCheckDistance = 0.08f;

        [Header("Huong nhin")]
        [Tooltip("Ban ve goc cua nhan vat quay sang phai hay trai. " +
                 "Bo sprite hien tai (stand-Sheet, run-Sheet...) quay TRAI nen de tat.")]
        public bool spriteFacesRight = false;

        [Header("Do cham dat")]
        public LayerMask groundLayers = 0;
        [Tooltip("Tam vong do, tinh tu goc toa do nhan vat.")]
        public Vector2 groundProbeOffset = new Vector2(0f, -0.92f);
        [Tooltip("Ban kinh vong do. Hoi nho hon nua be ngang collider mot chut.")]
        public float groundProbeRadius = 0.24f;
    }
}
