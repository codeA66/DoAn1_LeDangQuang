using System.Collections;
using LuHanh.Core;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Theo doi nguoi choi dang o phong nao (<see cref="Room"/>). Ra khoi phong hien tai, vao phong ke ben:
    /// dung game, camera truot sang phong moi roi choi tiep (kieu Celeste). Di len phong tren duoc day len
    /// mot chut de khong roi lai phong cu. Phong moi khong ke ben, hoac man hinh dang toi (hoi sinh,
    /// chuyen man) thi camera nhay thang toi, khong truot.
    /// Dat trong LevelEssentials; man khong co Room nao thi khong lam gi.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        [Tooltip("Diem kiem tra phong tinh tu chan nhan vat len (unit) - khoang nguc.")]
        [SerializeField] float probeHeight = 0.9f;
        [Tooltip("Di len phong tren ma dang bay len thi van toc doc toi thieu sau khi chuyen. 0 = tat.")]
        [SerializeField] float upwardBoost = 10f;

        Transform player;
        Rigidbody2D playerBody;
        RoomCameraClamp cameraClamp;
        RoomTransitionFx transitionFx;

        public Room Current { get; private set; }
        public bool IsTransitioning { get; private set; }

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p)
            {
                player = p.transform;
                playerBody = p.GetComponent<Rigidbody2D>();
            }
            cameraClamp = FindAnyObjectByType<RoomCameraClamp>();
            transitionFx = FindAnyObjectByType<RoomTransitionFx>();
            if (player) Enter(FindRoom(Probe), skipDamping: true);
        }

        Vector2 Probe => (Vector2)player.position + Vector2.up * probeHeight;

        void Update()
        {
            if (!player || IsTransitioning) return;
            var point = Probe;
            if (Current && Current.Contains(point)) return;

            // Ngoai moi phong (vd dang roi xuong ho duoi day phong): giu phong cu.
            var next = FindRoom(point);
            if (!next || next == Current) return;

            if (!Current || !Current.Touches(next) || ScreenHidden() || !cameraClamp)
                Enter(next, skipDamping: true);
            else
                StartCoroutine(Transition(next, point));
        }

        static bool ScreenHidden() =>
            (RespawnManager.Instance && RespawnManager.Instance.IsRespawning) || SceneTransition.IsBusy;

        static Room FindRoom(Vector2 point)
        {
            var rooms = Room.All;
            for (int i = 0; i < rooms.Count; i++)
                if (rooms[i].Contains(point)) return rooms[i];
            return null;
        }

        void Enter(Room room, bool skipDamping)
        {
            if (!room) return;
            Current = room;
            if (!cameraClamp) return;
            cameraClamp.SnapTo(room.Bounds);
            if (skipDamping) cameraClamp.SkipDamping();
        }

        IEnumerator Transition(Room next, Vector2 point)
        {
            IsTransitioning = true;
            Vector2 direction = ExitDirection(Current.Bounds, point);
            bool goingUp = direction.y > 0f;
            Current = next;

            // Game dung trong luc camera truot; cho toi khi camera bao xong (them chut du phong).
            HitStop.Hold();
            float duration = cameraClamp.TransitionTo(next.Bounds);
            if (transitionFx) transitionFx.Play(direction, duration);
            float timeout = Time.unscaledTime + duration + 0.25f;
            while (cameraClamp.IsTransitioning && Time.unscaledTime < timeout) yield return null;
            HitStop.Unhold();

            if (goingUp && playerBody && playerBody.linearVelocity.y > 0f && upwardBoost > 0f)
            {
                var v = playerBody.linearVelocity;
                v.y = Mathf.Max(v.y, upwardBoost);
                playerBody.linearVelocity = v;
            }
            IsTransitioning = false;
        }

        /// <summary>Huong nhan vat vuot ra khoi phong cu (theo mep da vuot).</summary>
        static Vector2 ExitDirection(Rect from, Vector2 point)
        {
            if (point.x >= from.xMax) return Vector2.right;
            if (point.x < from.xMin) return Vector2.left;
            return point.y >= from.yMax ? Vector2.up : Vector2.down;
        }

        // Bi tat giua luc truot (doi scene...) thi khong de game dung mai.
        void OnDisable()
        {
            if (!IsTransitioning) return;
            IsTransitioning = false;
            HitStop.Unhold();
        }
    }
}
