using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Thong so don chem thuong (phim J). Tach ScriptableObject giong PlayerMovementConfig
    /// de chinh ngay khi dang Play ma khong dung vao code.
    /// Vung chem tinh tu goc toa do nhan vat (o chan); X tinh theo huong dang nhin.
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Player Attack Config", fileName = "PlayerAttackConfig")]
    public class PlayerAttackConfig : ScriptableObject
    {
        [Header("Nhip danh")]
        [Tooltip("Giay toi thieu giua hai nhat chem.")]
        public float cooldown = 0.35f;
        [Tooltip("Vung chem bat trong bao lau sau khi bam.")]
        public float activeTime = 0.1f;
        [Tooltip("Bam J truoc khi het hoi chieu bao lau thi van tinh.")]
        public float inputBuffer = 0.1f;

        [Header("Sat thuong")]
        public int damage = 1;
        [Tooltip("Luc day muc tieu ra, unit/giay.")]
        public float knockback = 6f;
        [Tooltip("Hieu ung loe sang + manh vun khi chem trung (prefab co component HitEffect).")]
        public LuHanh.Effects.HitEffect hitEffectPrefab;

        [Header("Vung chem (tam, kich thuoc)")]
        public Vector2 sideOffset = new Vector2(0.9f, 0.9f);
        public Vector2 sideSize = new Vector2(1.6f, 1.2f);
        public Vector2 upOffset = new Vector2(0f, 2.1f);
        public Vector2 upSize = new Vector2(1.2f, 1.6f);
        public Vector2 downOffset = new Vector2(0f, -0.5f);
        public Vector2 downSize = new Vector2(1.2f, 1.6f);
        [Tooltip("Bat: vung chem lay theo vet chem dang hien (hitArea cua tung bo vet chem), khop hinh. " +
                 "Tat hoac bo vet chem chua do: dung cac hop ben tren.")]
        public bool hitAreaFromSlash = true;
        [Tooltip("Thu nho vung chem so voi hinh vet chem (1 = bang dung hinh).")]
        [Range(0.5f, 1.2f)] public float slashHitShrink = 0.9f;
        [Tooltip("Layer cua nhung thu bi chem duoc.")]
        public LayerMask hitLayers = 0;
        [Tooltip("Ngam ho tro: co muc tieu trong ban kinh nay (unit, tinh tu nguc) thi tu quay mat " +
                 "va chon huong chem ve phia con gan nhat, khong di chuyen nhan vat. 0 = tat.")]
        [Min(0f)] public float aimAssistRadius = 2.5f;

        [Header("Phan luc len nguoi choi")]
        [Tooltip("Chem xuong trung thi bat len voi van toc nay (pogo). 0 = dung jumpSpeed trong PlayerMovementConfig.")]
        public float pogoSpeed = 0f;
        [Tooltip("Chem ngang trung thi nguoi choi bi day lui voi van toc nay.")]
        public float recoilSpeed = 4f;

        [Header("Cam giac luc")]
        [Tooltip("Chem trung thi khung hinh dung lai bao lau (giay thuc). 0 = tat.")]
        public float hitStopTime = 0.05f;

        [Header("Vet chem")]
        [Tooltip("Vet chem hien trong bao lau.")]
        public float slashVisualTime = 0.12f;
        [Tooltip("Vet chem phong to them bao nhieu luc tan.")]
        public float slashGrow = 0.15f;
        [Tooltip("Bat: dat vet chem o giua vung chem (anh ve san huong sang phai, vd slash_0..3). " +
                 "Tat: dat tai slashAnchor va xoay quanh do (anh vong cung quanh tam, vd bo Sugawara).")]
        public bool slashCentredOnHitArea = false;
        [Tooltip("Tam xoay cua vet chem, tinh tu chan nhan vat (thuong la nguc).")]
        public Vector2 slashAnchor = new Vector2(0f, 0.9f);
        [Tooltip("Goc xoay goc de anh thanh vong cung truoc mat, quet tu tren xuong.")]
        public float slashBaseAngle = 90f;
        [Tooltip("Lat ngang anh truoc khi xoay (doi chieu quet).")]
        public bool slashFlipX = true;
        [Tooltip("Day vet chem ra xa nhan vat theo huong chem (unit): ngang / len / xuong. " +
                 "Nen bang khoang da doi tam vung chem de hinh khop cho trung don.")]
        public float slashSidePush = 0f;
        public float slashUpPush = 0f;
        public float slashDownPush = 0f;
        [Tooltip("Chem lai trong khoang nay (giay) thi dung bo vet chem ke tiep (combo). Qua lau thi ve lai bo dau.")]
        public float slashAlternateWindow = 0.6f;

        [Header("Dong tac chem (than nhan vat)")]
        [Tooltip("Co gian than luc vung kiem (x = ngang, y = doc), tu tro ve 1. Ngang / len / xuong.")]
        public Vector2 sideSwingSquash = new Vector2(1.2f, 0.88f);
        public Vector2 upSwingSquash = new Vector2(0.88f, 1.18f);
        public Vector2 downSwingSquash = new Vector2(0.9f, 1.15f);
        [Tooltip("Than nhich ve huong chem bao xa (unit, chi hinh, khong doi vi tri vat ly).")]
        public float swingLunge = 0.12f;
        [Tooltip("Than nghieng theo huong chem bao nhieu do (chem ngang/xuong: chui ve truoc, chem len: ngua ra sau mot nua), tu tro ve.")]
        public float swingLean = 12f;
    }
}
