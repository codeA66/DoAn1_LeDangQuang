using LuHanh.Core;
using LuHanh.Core.FSM;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Quai chui dat (Giun da), kieu Goam/Dirtcarver: ngam duoi dat (an, khong danh duoc) bam theo nguoi choi
    /// de lai vet bui -> toi noi thi bui bung bao truoc -> troi len can -> lo than (danh duoc) -> chui xuong.
    /// </summary>
    public class BurrowEnemyController : BuriedEnemyBase
    {
        [Tooltip("Bui khi ngam / bao truoc (Emit, World space).")]
        [SerializeField] ParticleSystem dust;
        [SerializeField] int trailDustCount = 3;
        [SerializeField] int telegraphDustCount = 26;

        ActionState lurk, hunt, telegraph, bite, exposed, burrow;
        Light2D[] lights;
        Collider2D[] colliders;
        float groundY, gravity, dustTimer;
        bool hidden;

        protected override IState InitialState => lurk;

        protected override void Awake()
        {
            lights = GetComponentsInChildren<Light2D>(true);
            colliders = GetComponentsInChildren<Collider2D>(true);
            gravity = GetComponent<Rigidbody2D>().gravityScale;
            groundY = transform.position.y;
            base.Awake();
        }

        bool groundKnown;

        /// <summary>
        /// Do mat dat that duoi cho dat quai. Luc Awake collider cua Tilemap co the chua san sang nen
        /// do lai o moi buoc vat ly cho toi khi trung.
        /// </summary>
        void EnsureGround()
        {
            if (groundKnown) return;
            var hit = Physics2D.Raycast(new Vector2(body.position.x, StartPosition.y + 0.5f), Vector2.down, 3f, config.groundLayers);
            if (!hit.collider) return;
            groundY = hit.point.y;
            groundKnown = true;
        }

        // Dang ngam thi khong bi danh trung (moi collider da tat), chi khung lai khi dang lo than.
        protected override bool CanStagger() => !hidden;

        protected override void BuildStates(StateMachine machine)
        {
            lurk = new ActionState(enter: () => SetHidden(true), fixedTick: _ => HoldUnderground());
            hunt = new ActionState(enter: () => SetHidden(true), tick: TrailDust, fixedTick: HuntStep);
            telegraph = new ActionState(
                enter: () =>
                {
                    HoldUnderground();
                    EmitDust(telegraphDustCount);
                    CameraShake.Kick(0.12f, config.emergeTelegraph);
                },
                tick: dt => { if (telegraph.Elapsed > config.emergeTelegraph * 0.5f) TrailDust(dt); },
                fixedTick: _ => HoldUnderground());
            bite = new ActionState(
                enter: () => { SetHidden(false); FacePlayer(); PlayEmerge(); SetAttackHitbox(true); EmitDust(telegraphDustCount / 2); },
                fixedTick: _ => Stop(),
                exit: () => SetAttackHitbox(false));
            exposed = new ActionState(enter: PlayIdle, fixedTick: _ => Stop());
            burrow = new ActionState(enter: () => { PlayBurrow(); EmitDust(8); }, fixedTick: _ => Stop());

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, exposed, () => HurtFinished);
            machine.AddTransition(lurk, hunt, () => PlayerInRange(config.detectRange));
            machine.AddTransition(hunt, telegraph, () => Mathf.Abs(PlayerDx) <= config.emergeDistance || hunt.Elapsed >= config.maxHuntTime);
            machine.AddTransition(hunt, lurk, () => !PlayerInRange(config.detectRange * 1.4f));
            machine.AddTransition(telegraph, bite, () => telegraph.Elapsed >= config.emergeTelegraph);
            machine.AddTransition(bite, exposed, () => bite.Elapsed >= config.biteTime);
            machine.AddTransition(exposed, burrow, () => exposed.Elapsed >= config.exposedTime);
            machine.AddTransition(burrow, hunt, () => burrow.Elapsed >= config.burrowTime && PlayerInRange(config.detectRange));
            machine.AddTransition(burrow, lurk, () => burrow.Elapsed >= config.burrowTime);
        }

        /// <summary>Ngam: an hinh/den, tat va cham va trong luc, giu y o mat dat.</summary>
        void SetHidden(bool on)
        {
            hidden = on;
            if (sprite) sprite.enabled = !on;
            if (flash && on) flash.enabled = false;
            foreach (var l in lights) if (l) l.enabled = !on;
            // Tat ca collider (than + Hitbox): neu chi tat than, don chem van trung Hitbox va tru mau khi dang ngam.
            foreach (var c in colliders) if (c) c.enabled = !on;
            body.gravityScale = on ? 0f : gravity;
            body.linearVelocity = Vector2.zero;
            if (on) body.position = new Vector2(body.position.x, groundY);
            else ReapplyIgnorePlayer(); // bat lai collider lam mat IgnoreCollision
        }

        void HoldUnderground()
        {
            EnsureGround();
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(body.position.x, groundY);
        }

        void HuntStep(float dt)
        {
            EnsureGround();
            float dir = Mathf.Sign(PlayerDx);
            float nextX = body.position.x + dir * config.undergroundSpeed * dt;
            // Chi di duoi phan dat lien: phia truoc khong con dat (ho / mep) thi dung cho.
            bool groundAhead = Physics2D.Raycast(new Vector2(nextX + dir * 0.5f, groundY + 0.3f), Vector2.down, 0.8f, config.groundLayers);
            Face(dir);
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(groundAhead ? nextX : body.position.x, groundY);
        }

        void TrailDust(float dt)
        {
            dustTimer -= dt;
            if (dustTimer > 0f) return;
            dustTimer = 0.1f;
            EmitDust(trailDustCount);
        }

        void EmitDust(int count)
        {
            if (!dust) return;
            dust.transform.position = new Vector3(body.position.x, groundY, 0f);
            dust.Emit(count);
        }

        protected override void OnResetState()
        {
            base.OnResetState();
            groundKnown = false;
            SetHidden(true);
        }
    }
}
