using LuHanh.Core;
using LuHanh.Level;
using LuHanh.Player;
using LuHanh.Upgrades;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LuHanh.UI
{
    /// <summary>
    /// Bang nang cap o tho ren (dung bang code). Mo tu <see cref="BlacksmithNpc"/> (onAccepted -> <see cref="Open"/>),
    /// dong thi goi <see cref="BlacksmithNpc.EndTalk"/>. Game dung trong luc mo (<see cref="HitStop.Hold"/>), UI dem
    /// thoi gian thuc. Len/xuong chon, E / Space / J / Enter mua (cung bo phim xac nhan voi ChoiceDialog), Esc dong.
    /// Danh sach lay tu UpgradeCatalog cua UpgradeSystem.
    /// </summary>
    public class UpgradeShopUI : MonoBehaviour
    {
        [SerializeField] BlacksmithNpc npc;
        [SerializeField] Font font;
        [SerializeField] string title = "Upgrades";
        [Header("Manh ban do ban them (the gioi kieu Hollow Knight)")]
        [SerializeField] LuHanh.World.WorldMapData map;
        [Tooltip("Id manh ban do ban o tho ren nay (vd Foundry.2). Gia lay tu WorldMapData.")]
        [SerializeField] string[] mapPiecesForSale = new string[0];

        [Header("Mau")]
        [SerializeField] Color dimColor = new(0f, 0f, 0f, 0.6f);
        [SerializeField] Color panelColor = new(0.047f, 0.039f, 0.078f, 0.95f);   // #0C0A14
        [SerializeField] Color borderColor = new(0.95f, 0.96f, 0.97f, 0.85f);     // #F2F4F8
        [SerializeField] Color textColor = new(0.95f, 0.96f, 0.97f);
        [SerializeField] Color mutedColor = new(0.55f, 0.55f, 0.62f);
        [SerializeField] Color accentColor = new(0.72f, 1f, 0.94f);               // #B8FFF0 (co loi)
        [SerializeField] Color warnColor = new(0.92f, 0.4f, 0.28f);               // #EA6547

        UpgradeDefinition[] items;
        readonly System.Collections.Generic.List<LuHanh.World.WorldMapData.Piece> pieces = new();
        GameObject root;
        Text[] rowTexts;
        Text detailName, detailDesc, detailLevel, detailNow, detailNext, detailCost, shardsText, messageText;
        Image detailIcon;
        int selected;
        bool open, waitRelease;
        float lastMoveSign, messageTimer, punchTimer;
        InputAction move, closeAction;
        InputAction[] buys = new InputAction[0];

        public bool IsOpen => open;

        void Start()
        {
            var sys = FindAnyObjectByType<UpgradeSystem>();
            items = sys && sys.Catalog ? sys.Catalog.upgrades : null;
            items ??= new UpgradeDefinition[0];
            if (map)
                foreach (var id in mapPiecesForSale)
                {
                    var piece = map.GetPiece(id);
                    if (piece != null) pieces.Add(piece);
                    else Debug.LogWarning($"UpgradeShopUI: khong co manh ban do '{id}' trong WorldMapData.", this);
                }
            if (!npc) npc = GetComponent<BlacksmithNpc>();
            var p = GameObject.FindWithTag("Player");
            var input = p ? p.GetComponent<PlayerInput>() : null;
            if (input)
            {
                move = input.actions.FindAction("Move");
                buys = new[]
                {
                    input.actions.FindAction("Interact"), input.actions.FindAction("Jump"),
                    input.actions.FindAction("Attack"), input.actions.FindAction("UI/Submit")
                };
                closeAction = input.actions.FindAction("UI/Cancel");
            }
            Build();
        }

        void OnDisable()
        {
            if (open) Close();
        }

        /// <summary>Mo bang (goi tu BlacksmithNpc.onAccepted).</summary>
        public void Open()
        {
            if (open) return;
            if (Count == 0)
            {
                Debug.LogWarning("UpgradeShopUI: khong co nang cap (thieu UpgradeSystem / UpgradeCatalog).", this);
                if (npc) npc.EndTalk();
                return;
            }
            open = true;
            selected = 0;
            waitRelease = true;   // phim vua bam o khung thoai: doi nha ra
            lastMoveSign = 0f;
            messageTimer = 0f;
            // Esc / Enter thuoc map UI: map Player dang bat khong tu bat cac action nay.
            closeAction?.Enable();
            foreach (var a in buys) if (a != null && !a.enabled) a.Enable();
            root.SetActive(true);
            HitStop.Hold();
            Refresh();
        }

        void Close()
        {
            open = false;
            root.SetActive(false);
            HitStop.Unhold();
            if (npc) npc.EndTalk();
        }

        void Update()
        {
            if (!open) return;
            float dt = Time.unscaledDeltaTime;
            if (messageTimer > 0f) { messageTimer -= dt; if (messageTimer <= 0f) messageText.text = ""; }
            if (punchTimer > 0f)
            {
                punchTimer -= dt;
                float k = Mathf.Max(0f, punchTimer) / 0.18f;
                rowTexts[selected].transform.localScale = Vector3.one * (1f + 0.12f * k);
            }

            bool held = IsHeld(closeAction);
            foreach (var a in buys) if (IsHeld(a)) held = true;
            if (waitRelease) { if (!held) waitRelease = false; else return; }

            if (Pressed(closeAction)) { Close(); return; }

            float y = move != null ? move.ReadValue<Vector2>().y : 0f;
            float sign = Mathf.Abs(y) > 0.5f ? Mathf.Sign(y) : 0f;
            if (sign != 0f && sign != lastMoveSign)
            {
                rowTexts[selected].transform.localScale = Vector3.one;
                selected = (selected - (int)sign + Count) % Count; // len = giam chi so
                Refresh();
            }
            lastMoveSign = sign;

            foreach (var a in buys) if (Pressed(a)) { TryBuy(); break; }
        }

        static bool Pressed(InputAction a) => a != null && a.WasPressedThisFrame();
        static bool IsHeld(InputAction a) => a != null && a.IsPressed();

        /// <summary>So dong trong bang: nang cap truoc, manh ban do sau.</summary>
        int Count => items.Length + pieces.Count;
        LuHanh.World.WorldMapData.Piece PieceAt(int i) => i >= items.Length ? pieces[i - items.Length] : null;

        void TryBuyPiece(LuHanh.World.WorldMapData.Piece piece)
        {
            if (LuHanh.World.WorldMapData.HasPiece(piece.id))
            {
                Sfx.Play(SfxId.UpgradeDenied);
                ShowMessage("Already owned", mutedColor);
            }
            else if (!ShardWallet.TrySpend(piece.price))
            {
                Sfx.Play(SfxId.UpgradeDenied);
                ShowMessage("Not enough Shards", warnColor);
            }
            else
            {
                LuHanh.World.WorldMapData.GivePiece(piece.id);
                Sfx.Play(SfxId.UpgradeBuy);
                ShowMessage(piece.displayName + " added to your map!", accentColor);
                punchTimer = 0.18f;
            }
            Refresh();
        }

        void TryBuy()
        {
            var piece = PieceAt(selected);
            if (piece != null) { TryBuyPiece(piece); return; }
            var def = items[selected];
            var r = PlayerUpgrades.TryBuy(def);
            switch (r)
            {
                case PlayerUpgrades.BuyResult.Ok:
                    Sfx.Play(SfxId.UpgradeBuy);
                    ShowMessage(def.displayName + " upgraded!", accentColor);
                    punchTimer = 0.18f;
                    break;
                case PlayerUpgrades.BuyResult.NotEnoughShards:
                    Sfx.Play(SfxId.UpgradeDenied);
                    ShowMessage("Not enough Shards", warnColor);
                    break;
                case PlayerUpgrades.BuyResult.Maxed:
                    Sfx.Play(SfxId.UpgradeDenied);
                    ShowMessage("Already at max level", mutedColor);
                    break;
            }
            Refresh();
        }

        void ShowMessage(string text, Color color)
        {
            messageText.text = text;
            messageText.color = color;
            messageTimer = 1.6f;
        }

        void Refresh()
        {
            for (int i = 0; i < rowTexts.Length; i++)
            {
                var row = rowTexts[i];
                var rowPiece = i < Count ? PieceAt(i) : null;
                bool has = rowPiece != null || (i < items.Length && items[i]);
                row.gameObject.SetActive(has);
                if (!has) continue;
                if (rowPiece != null)
                    row.text = (i == selected ? "> " : "   ") + rowPiece.displayName + "   " +
                               (LuHanh.World.WorldMapData.HasPiece(rowPiece.id) ? "●" : "○");
                else
                {
                    var def = items[i];
                    int lv = PlayerUpgrades.Level(def);
                    row.text = (i == selected ? "> " : "   ") + def.displayName + "   " + Pips(lv, def.MaxLevel);
                }
                row.color = i == selected ? accentColor : textColor;
            }

            var piece = PieceAt(selected);
            if (piece != null) { RefreshPiece(piece); return; }
            var d = items[selected];
            int level = PlayerUpgrades.Level(d);
            bool maxed = PlayerUpgrades.IsMaxed(d);
            int cost = PlayerUpgrades.NextCost(d);
            detailName.text = d.displayName;
            detailDesc.text = d.description;
            detailLevel.text = "Level " + level + " / " + d.MaxLevel;
            detailNow.text = "Now:  " + EffectText(d, level);
            detailNext.text = maxed ? "Next:  -" : "Next:  " + EffectText(d, level + 1);
            if (maxed) { detailCost.text = "MAX"; detailCost.color = mutedColor; }
            else
            {
                detailCost.text = "Cost:  " + cost + " Shards";
                detailCost.color = ShardWallet.Count >= cost ? accentColor : warnColor;
            }
            shardsText.text = "Shards:  " + ShardWallet.Count;
            detailIcon.enabled = d.icon;
            detailIcon.sprite = d.icon;
        }

        void RefreshPiece(LuHanh.World.WorldMapData.Piece p)
        {
            bool owned = LuHanh.World.WorldMapData.HasPiece(p.id);
            detailName.text = p.displayName;
            detailDesc.text = p.description;
            detailLevel.text = "Map";
            detailNow.text = owned ? "Owned" : "Not owned";
            detailNext.text = "";
            if (owned) { detailCost.text = "OWNED"; detailCost.color = mutedColor; }
            else
            {
                detailCost.text = "Cost:  " + p.price + " Shards";
                detailCost.color = ShardWallet.Count >= p.price ? accentColor : warnColor;
            }
            shardsText.text = "Shards:  " + ShardWallet.Count;
            detailIcon.enabled = p.icon;
            detailIcon.sprite = p.icon;
        }

        static string Pips(int level, int max)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < max; i++) sb.Append(i < level ? '●' : '○'); // ● ○
            return sb.ToString();
        }

        /// <summary>Tong hieu ung cua <paramref name="level"/> cap, dang de doc (vd "+2 damage", "-28% attack recovery").</summary>
        static string EffectText(UpgradeDefinition d, int level)
        {
            if (level <= 0) return "none";
            string what = d.stat switch
            {
                PlayerStat.AttackDamage => "damage",
                PlayerStat.AttackCooldown => "attack recovery",
                PlayerStat.MaxMasks => "max masks",
                _ => d.stat.ToString()
            };
            if (d.mode == UpgradeDefinition.Mode.Add) return "+" + (d.valuePerLevel * level).ToString("0.##") + " " + what;
            float pct = (Mathf.Pow(d.valuePerLevel, level) - 1f) * 100f;
            return (pct >= 0f ? "+" : "") + pct.ToString("0") + "% " + what;
        }

        // ---------- Dung giao dien ----------

        void Build()
        {
            root = new GameObject("UpgradeShop", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            UIBuild.SetupOverlay(root, 70);

            var dim = MakeImage("Dim", root.transform, dimColor, Vector2.zero);
            var drt = dim.rectTransform; drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one; drt.sizeDelta = Vector2.zero;

            var border = MakeImage("Border", root.transform, borderColor, new Vector2(1240f, 660f));
            var panel = MakeImage("Panel", border.transform, panelColor, new Vector2(1232f, 652f));
            var pt = panel.transform;

            var titleText = Label("Title", pt, 52, accentColor, new Vector2(0f, 270f), new Vector2(1100f, 70f), TextAnchor.MiddleCenter);
            titleText.supportRichText = true;
            titleText.text = UIBuild.SmallCaps(title, 64);

            rowTexts = new Text[6];
            for (int i = 0; i < rowTexts.Length; i++)
                rowTexts[i] = Label("Row" + i, pt, 38, textColor, new Vector2(-320f, 160f - i * 70f), new Vector2(520f, 60f), TextAnchor.MiddleLeft);

            var divider = MakeImage("Divider", pt, new Color(borderColor.r, borderColor.g, borderColor.b, 0.25f), new Vector2(2f, 440f));
            divider.rectTransform.anchoredPosition = new Vector2(-30f, -10f);

            detailIcon = MakeImage("Icon", pt, Color.white, new Vector2(96f, 96f));
            detailIcon.rectTransform.anchoredPosition = new Vector2(70f, 150f);
            detailName = Label("Name", pt, 44, textColor, new Vector2(340f, 170f), new Vector2(460f, 60f), TextAnchor.MiddleLeft);
            detailLevel = Label("Level", pt, 32, mutedColor, new Vector2(340f, 120f), new Vector2(460f, 44f), TextAnchor.MiddleLeft);
            detailDesc = Label("Desc", pt, 32, textColor, new Vector2(290f, 40f), new Vector2(560f, 100f), TextAnchor.UpperLeft);
            detailDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
            detailNow = Label("Now", pt, 34, textColor, new Vector2(290f, -50f), new Vector2(560f, 44f), TextAnchor.MiddleLeft);
            detailNext = Label("Next", pt, 34, accentColor, new Vector2(290f, -100f), new Vector2(560f, 44f), TextAnchor.MiddleLeft);
            detailCost = Label("Cost", pt, 38, accentColor, new Vector2(290f, -170f), new Vector2(560f, 50f), TextAnchor.MiddleLeft);
            shardsText = Label("Shards", pt, 34, textColor, new Vector2(290f, -220f), new Vector2(560f, 44f), TextAnchor.MiddleLeft);

            messageText = Label("Message", pt, 34, accentColor, new Vector2(0f, -275f), new Vector2(1100f, 44f), TextAnchor.MiddleCenter);
            var hint = Label("Hint", root.transform, 28, mutedColor, new Vector2(0f, -370f), new Vector2(1240f, 40f), TextAnchor.MiddleCenter);
            hint.text = "Up / Down  -  Select        J / Enter  -  Buy        Esc  -  Leave";

            root.SetActive(false);
        }

        Text Label(string name, Transform parent, int size, Color color, Vector2 pos, Vector2 box, TextAnchor anchor)
        {
            var t = UIBuild.MakeText(name, parent, font, size, color);
            t.alignment = anchor;
            t.rectTransform.sizeDelta = box;
            t.rectTransform.anchoredPosition = pos;
            return t;
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
    }
}
