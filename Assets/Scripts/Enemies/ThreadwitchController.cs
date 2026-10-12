using System.Collections.Generic;
using LuHanh.Combat;
using LuHanh.Core.FSM;
using LuHanh.Level;
using UnityEngine;

namespace LuHanh.Enemies
{
    /// <summary>
    /// Phu thuy re (Threadwitch): khong tu danh, cham vao khong mat mau. Long lung giu khoang cach voi nguoi choi,
    /// noi soi re do (<see cref="ThreadLink"/>) toi toi da 2 quai cung phong - quai duoc noi hanh dong nhanh hon 30%, sat thuong x2 (EnemyBase.AddBuff).
    /// Noi mat 1 giay (soi do sang dan - bao truoc). Nguoi choi chem dut day thi no giat lui, choang 1,5 giay,
    /// 3 giay sau moi noi lai. Chet thi moi day dut.
    /// </summary>
    public class ThreadwitchController : BuriedEnemyBase
    {
        [Header("Bay")]
        [SerializeField] float acceleration = 12f;
        [Tooltip("Giu khoang cach ngang voi nguoi choi trong khoang nay.")]
        [SerializeField] float keepDistanceMin = 5f;
        [SerializeField] float keepDistanceMax = 7f;
        [SerializeField] float idleRadius = 2f;

        [Header("Noi day")]
        [SerializeField] float linkRange = 8f;
        [Tooltip("Day bi keo dai qua khoang nay thi tu dut (khong choang).")]
        [SerializeField] float linkBreakRange = 12f;
        [SerializeField] int maxLinks = 2;
        [SerializeField] float bindTime = 1f;
        [SerializeField] float recoilTime = 1.5f;
        [SerializeField] float relinkDelay = 3f;
        [SerializeField] Sprite threadSprite;
        [Tooltip("Lua toa quanh quai duoc buff (Prefabs/Fx/BuffFlame).")]
        [SerializeField] ParticleSystem buffFxPrefab;
        [SerializeField] float threadThickness = 0.15f;
        [Tooltip("Diem soi day bat ra (dau ngon tay), tinh tu chan theo huong dang nhin.")]
        [SerializeField] Vector2 handOffset = new Vector2(0.4f, 1.1f);

        [Header("Chet")]
        [SerializeField] float dieTime = 0.67f;

        ActionState drift, bind, recoil, dead;
        readonly List<ThreadLink> links = new List<ThreadLink>();
        Transform hand;
        Room ownRoom;
        EnemyBase bindTarget;
        Vector2 idleTarget;
        float relinkTimer;

        static readonly int FloatHash = Animator.StringToHash("float");
        static readonly int BindHash = Animator.StringToHash("bind");
        static readonly int RecoilHash = Animator.StringToHash("recoil");
        static readonly int HurtHash = Animator.StringToHash("hurt");
        static readonly int DieHash = Animator.StringToHash("die");

        protected override IState InitialState => drift;

        protected override void Awake()
        {
            hand = new GameObject("Hand").transform;
            hand.SetParent(transform, false);
            hand.localPosition = handOffset;
            base.Awake();
            body.gravityScale = 0f;
        }

        protected override void Start()
        {
            base.Start();
            ownRoom = Room.At(StartPosition);
        }

        void OnDisable() => ReleaseAll();

        protected override void BuildStates(StateMachine machine)
        {
            drift = new ActionState(
                enter: () => { PlayAnimHash(FloatHash); idleTarget = StartPosition; },
                tick: Tick,
                fixedTick: Drift);
            bind = new ActionState(
                enter: () => { PlayAnimHash(BindHash); PlayWindupFx(true, bindTime); if (bindTarget) Face(bindTarget.transform.position.x - transform.position.x); },
                tick: Tick,
                fixedTick: dt => Brake(dt),
                exit: () =>
                {
                    if (bind.Elapsed >= bindTime && IsLinkable(bindTarget))
                        links.Add(ThreadLink.Create(this, hand, bindTarget, threadSprite, threadThickness, sprite ? sprite.sortingOrder - 1 : 0, buffFxPrefab));
                    bindTarget = null;
                });
            recoil = new ActionState(
                enter: () => { PlayAnimHash(RecoilHash); SetVelocity(new Vector2(-FacingSign * 3f, 0f)); },
                tick: Tick,
                fixedTick: dt => Brake(dt));
            dead = new ActionState(enter: () => { PlayWindupFx(false); PlayAnimHash(DieHash); });

            machine.AddAnyTransition(hurt, () => ShouldStagger);
            machine.AddTransition(hurt, drift, () => HurtFinished);
            machine.AddTransition(drift, bind, FindBindTarget);
            machine.AddTransition(bind, drift, () => bind.Elapsed >= bindTime || !IsLinkable(bindTarget));
            machine.AddTransition(recoil, drift, () => recoil.Elapsed >= recoilTime);
        }

        void Tick(float dt)
        {
            relinkTimer -= dt;
            // Quai duoc noi chet / roi phong / qua xa thi go day (khong choang).
            for (int i = links.Count - 1; i >= 0; i--)
            {
                var t = links[i].Target;
                if (t && t.gameObject.activeInHierarchy &&
                    ((Vector2)(t.transform.position - transform.position)).sqrMagnitude <= linkBreakRange * linkBreakRange) continue;
                links[i].Release();
                links.RemoveAt(i);
            }
        }

        void Drift(float dt)
        {
            Vector2 target;
            if (Player && Player.gameObject.activeInHierarchy && InHomeRoom(Player.position))
            {
                // Dung o phia ben kia cac quai duoc noi (tron sau dong doi), cach nguoi choi 5-7 o.
                float side = Mathf.Sign(transform.position.x - Player.position.x);
                if (links.Count > 0) side = Mathf.Sign(links[0].Target.transform.position.x - Player.position.x);
                if (side == 0f) side = 1f;
                float dist = Mathf.Clamp(Mathf.Abs(PlayerDx), keepDistanceMin, keepDistanceMax);
                target = new Vector2(Player.position.x + side * dist, StartPosition.y);
                Face(-side);
            }
            else
            {
                if (Vector2.Distance(body.position, idleTarget) < 0.3f)
                    idleTarget = (Vector2)StartPosition + Random.insideUnitCircle * idleRadius;
                target = idleTarget;
            }
            Vector2 to = target - body.position;
            Vector2 desired = Vector2.ClampMagnitude(to * 2f, config.moveSpeed);
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, desired, acceleration * dt);
        }

        void Brake(float dt) => body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, Vector2.zero, acceleration * dt);

        bool InHomeRoom(Vector2 p) => !ownRoom || Room.At(p) == ownRoom;

        bool IsLinkable(EnemyBase e) =>
            e && e != this && e.gameObject.activeInHierarchy && !(e is ThreadwitchController) &&
            InHomeRoom(e.transform.position) &&
            ((Vector2)(e.transform.position - transform.position)).sqrMagnitude <= linkRange * linkRange;

        bool FindBindTarget()
        {
            if (links.Count >= maxLinks || relinkTimer > 0f) return false;
            EnemyBase best = null;
            float bestSqr = float.MaxValue;
            foreach (var e in FindObjectsByType<EnemyBase>(FindObjectsInactive.Exclude))
            {
                if (!IsLinkable(e) || links.Exists(l => l.Target == e)) continue;
                float d = ((Vector2)(e.transform.position - transform.position)).sqrMagnitude;
                if (d >= bestSqr) continue;
                best = e;
                bestSqr = d;
            }
            bindTarget = best;
            return best;
        }

        /// <summary>Nguoi choi chem dut mot soi day.</summary>
        public void OnLinkCut(ThreadLink link)
        {
            if (!links.Remove(link)) return;
            link.Release();
            relinkTimer = relinkDelay;
            if (Machine.Current != dead) Machine.SetState(recoil);
        }

        void ReleaseAll()
        {
            foreach (var l in links) if (l) l.Release();
            links.Clear();
        }

        protected override void PlayHurtAnim() => PlayAnimHash(HurtHash);

        // Bay: bi day lui theo ca hai truc.
        protected override void OnHurt(HitInfo hit) => body.linearVelocity = hit.direction * hit.knockback;

        protected override bool OnDeath(HitInfo hit)
        {
            ReleaseAll();
            Machine.SetState(dead);
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.gravityScale = 1f;
            CancelInvoke();
            Invoke(nameof(Vanish), dieTime);
            return true;
        }

        void Vanish() => gameObject.SetActive(false);

        protected override void OnResetState()
        {
            CancelInvoke();
            ReleaseAll();
            body.gravityScale = 0f;
            foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = true;
            ReapplyIgnorePlayer();
            relinkTimer = 0f;
            base.OnResetState();
        }
    }
}
