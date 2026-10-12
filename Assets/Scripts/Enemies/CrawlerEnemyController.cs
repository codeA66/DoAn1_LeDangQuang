using LuHanh.Core.FSM;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Quai lot duong (Sinh vat re), kieu Crawlid: khong co don danh, chi bo qua lai va quay dau o tuong/mep vuc.
    /// Cham vao moi mat mau; vung sat thuong tu bat lai sau <see cref="contactCooldown"/> giay.
    /// Do kho den tu cho dat no (tren platform hep, canh gai...).
    /// </summary>
    public class CrawlerEnemyController : BuriedEnemyBase
    {
        [Tooltip("Sau khi cham trung nguoi choi, bao lau thi gay sat thuong lai duoc.")]
        [SerializeField] float contactCooldown = 1f;

        ActionState crawl;
        float rearmTimer;

        protected override IState InitialState => crawl;

        protected override void BuildStates(StateMachine machine)
        {
            crawl = new ActionState(enter: () => { PlayWalk(); SetAttackHitbox(true); }, tick: Rearm, fixedTick: _ => PatrolStep());
            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, crawl, () => HurtFinished);
        }

        void Rearm(float dt)
        {
            if (!attackHitbox || attackHitbox.enabled) { rearmTimer = contactCooldown; return; }
            rearmTimer -= dt;
            if (rearmTimer <= 0f) SetAttackHitbox(true);
        }

        protected override void OnResetState()
        {
            base.OnResetState();
            SetAttackHitbox(true);
        }
    }
}
