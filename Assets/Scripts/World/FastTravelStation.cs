using System.Collections.Generic;
using LuHanh.Core;
using LuHanh.Player;
using LuHanh.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LuHanh.World
{
    /// <summary>
    /// Tram di chuyen nhanh kieu Stag cua Hollow Knight. Dung gan, bam E: lan dau mo tram (mien phi, luu co
    /// "Station/[ma phong]"), roi hien bang cac tram da mo -> chon -> WorldLoader.TravelToStation.
    /// Danh sach tram lay tu WorldMapData (phong loai Station). Game dung trong luc mo bang (HitStop.Hold).
    /// Ten tram = ten vung cua phong do + " Station". Moi phong toi da mot tram.
    /// </summary>
    public class FastTravelStation : MonoBehaviour
    {
        public const string Flag = "Station/";

        [SerializeField] WorldMapData map;
        [SerializeField] Font font;
        [Tooltip("Khoang cach ngang toi tram de bam E.")]
        [SerializeField] float useRange = 1.8f;
        [Tooltip("Cho dat chan nguoi choi khi den tram nay, tinh tu goc tram.")]
        [SerializeField] Vector2 arrivalOffset = new(1.5f, 0f);

        [Header("Mau")]
        [SerializeField] Color dimColor = new(0f, 0f, 0f, 0.6f);
        [SerializeField] Color panelColor = new(0.047f, 0.039f, 0.078f, 0.95f);   // #0C0A14
        [SerializeField] Color borderColor = new(0.95f, 0.96f, 0.97f, 0.85f);     // #F2F4F8
        [SerializeField] Color textColor = new(0.95f, 0.96f, 0.97f);
        [SerializeField] Color mutedColor = new(0.55f, 0.55f, 0.62f);
        [SerializeField] Color accentColor = new(0.72f, 1f, 0.94f);               // #B8FFF0

        public Vector2 ArrivalPoint => (Vector2)transform.position + arrivalOffset;
        string RoomId => gameObject.scene.name;

        PlayerController controller;
        MonoBehaviour[] blockWhileOpen;
        InputAction interact, move, cancel;
        InputAction[] confirms = new InputAction[0];
        CanvasGroup prompt;
        GameObject menu;
        Text[] rows;
        Text title, message;
        readonly List<string> targets = new();
        int selected;
        bool open, waitRelease;
        float lastMoveSign, messageTimer;

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p)
            {
                controller = p.GetComponent<PlayerController>();
                blockWhileOpen = new MonoBehaviour[]
                {
                    controller, p.GetComponent<PlayerAttack>(), p.GetComponent<PlayerOrbShot>(),
                    p.GetComponent<PlayerShadow>(), p.GetComponent<PlayerFocus>()
                };
                var input = p.GetComponent<PlayerInput>();
                if (input)
                {
                    interact = input.actions.FindAction("Interact");
                    move = input.actions.FindAction("Move");
                    cancel = input.actions.FindAction("UI/Cancel");
                    confirms = new[]
                    {
                        interact, input.actions.FindAction("Jump"), input.actions.FindAction("Attack"),
                        input.actions.FindAction("UI/Submit")
                    };
                }
            }
            Build();
        }

        void OnDisable()
        {
            if (open) Close();
        }

        void Update()
        {
            if (!controller) return;
            if (open) { UpdateMenu(); return; }
            if (messageTimer > 0f) { messageTimer -= Time.unscaledDeltaTime; if (messageTimer <= 0f) message.text = ""; }

            bool near = controller.enabled && controller.IsGrounded && !WorldLoader.IsBusy &&
                        Mathf.Abs(controller.transform.position.x - transform.position.x) <= useRange &&
                        Mathf.Abs(controller.transform.position.y - transform.position.y) <= 1.5f;
            if (prompt) prompt.alpha = Mathf.MoveTowards(prompt.alpha, near ? 1f : 0f, Time.deltaTime * 6f);
            if (near && interact != null && interact.WasPressedThisFrame()) Use();
        }

        void Use()
        {
            if (!WorldState.Has(Flag + RoomId))
            {
                WorldState.Set(Flag + RoomId);
                Sfx.Play(SfxId.Bench);
                ShowMessage("Station unlocked");
            }
            targets.Clear();
            if (map)
                foreach (var r in map.rooms)
                    if (r.kind == WorldMapData.RoomKind.Station && r.id != RoomId && WorldState.Has(Flag + r.id))
                        targets.Add(r.id);
            if (targets.Count == 0)
            {
                if (messageTimer <= 0f) ShowMessage("No other stations unlocked");
                return;
            }
            Open();
        }

        void Open()
        {
            open = true;
            selected = 0;
            waitRelease = true;
            lastMoveSign = 0f;
            if (prompt) prompt.alpha = 0f;
            foreach (var b in blockWhileOpen) if (b) b.enabled = false;
            cancel?.Enable();
            foreach (var a in confirms) if (a != null && !a.enabled) a.Enable();
            menu.SetActive(true);
            HitStop.Hold();
            Refresh();
        }

        void Close()
        {
            open = false;
            menu.SetActive(false);
            HitStop.Unhold();
            foreach (var b in blockWhileOpen) if (b) b.enabled = true;
            if (controller) controller.ClearBufferedInput();
        }

        void UpdateMenu()
        {
            bool held = cancel != null && cancel.IsPressed();
            foreach (var a in confirms) if (a != null && a.IsPressed()) held = true;
            if (waitRelease) { if (!held) waitRelease = false; else return; }

            if (cancel != null && cancel.WasPressedThisFrame()) { Close(); return; }

            float y = move != null ? move.ReadValue<Vector2>().y : 0f;
            float sign = Mathf.Abs(y) > 0.5f ? Mathf.Sign(y) : 0f;
            if (sign != 0f && sign != lastMoveSign)
            {
                selected = (selected - (int)sign + targets.Count) % targets.Count;
                Refresh();
            }
            lastMoveSign = sign;

            foreach (var a in confirms)
            {
                if (a == null || !a.WasPressedThisFrame()) continue;
                string target = targets[selected];
                Close();
                Sfx.Play(SfxId.Elevator);
                if (WorldLoader.Instance) WorldLoader.Instance.TravelToStation(target);
                return;
            }
        }

        string StationName(string roomId)
        {
            var room = map ? map.GetRoom(roomId) : null;
            var zone = room != null ? map.GetZone(room.zone) : null;
            return (zone != null ? zone.title : roomId) + " Station";
        }

        void Refresh()
        {
            for (int i = 0; i < rows.Length; i++)
            {
                bool has = i < targets.Count;
                rows[i].gameObject.SetActive(has);
                if (!has) continue;
                rows[i].text = (i == selected ? "> " : "   ") + StationName(targets[i]);
                rows[i].color = i == selected ? accentColor : textColor;
            }
        }

        void ShowMessage(string text)
        {
            message.text = text;
            messageTimer = 1.8f;
        }

        // ---------- Giao dien ----------

        void Build()
        {
            var promptGo = new GameObject("StationPrompt", typeof(CanvasGroup));
            promptGo.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(promptGo, 40);
            prompt = promptGo.GetComponent<CanvasGroup>();
            prompt.alpha = 0f;
            prompt.blocksRaycasts = false;
            var pt = UIBuild.MakeText("Text", promptGo.transform, font, 34, accentColor);
            pt.text = "E  -  Travel";
            pt.rectTransform.anchorMin = pt.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            pt.rectTransform.anchoredPosition = new Vector2(0f, 140f);
            pt.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);

            var msgGo = new GameObject("StationMessage");
            msgGo.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(msgGo, 41);
            message = UIBuild.MakeText("Text", msgGo.transform, font, 34, accentColor);
            message.rectTransform.anchorMin = message.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            message.rectTransform.anchoredPosition = new Vector2(0f, 200f);
            message.gameObject.AddComponent<Outline>().effectColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);
            message.text = "";

            menu = new GameObject("StationMenu", typeof(RectTransform));
            menu.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(menu, 70);
            var dim = MakeImage("Dim", menu.transform, dimColor, Vector2.zero);
            dim.rectTransform.anchorMin = Vector2.zero; dim.rectTransform.anchorMax = Vector2.one;
            var border = MakeImage("Border", menu.transform, borderColor, new Vector2(820f, 520f));
            var panel = MakeImage("Panel", border.transform, panelColor, new Vector2(812f, 512f));
            title = UIBuild.MakeText("Title", panel.transform, font, 52, accentColor);
            title.supportRichText = true;
            title.text = UIBuild.SmallCaps("Travel", 64);
            title.rectTransform.anchoredPosition = new Vector2(0f, 190f);
            rows = new Text[6];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = UIBuild.MakeText("Row" + i, panel.transform, font, 38, textColor);
                rows[i].alignment = TextAnchor.MiddleLeft;
                rows[i].rectTransform.sizeDelta = new Vector2(680f, 60f);
                rows[i].rectTransform.anchoredPosition = new Vector2(0f, 90f - i * 64f);
            }
            var hint = UIBuild.MakeText("Hint", menu.transform, font, 28, mutedColor);
            hint.text = "Up / Down  -  Select        J / Enter  -  Travel        Esc  -  Leave";
            hint.rectTransform.anchoredPosition = new Vector2(0f, -300f);
            menu.SetActive(false);
        }

        static Image MakeImage(string name, Transform parent, Color color, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            img.rectTransform.sizeDelta = size;
            return img;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.29f, 0.5f, 0.76f, 0.9f);
            Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(useRange * 2f, 2f, 0f));
            Gizmos.DrawWireSphere(ArrivalPoint, 0.25f);
        }
    }
}
