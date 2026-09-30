using UnityEngine;

namespace LuHanh.Player
{
    /// <summary>
    /// Quay mat nhan vat theo huong di.
    ///
    /// Lat bang localScale.x chu khong bang SpriteRenderer.flipX: cach nay lat theo
    /// ca cac object con (vu khi, diem ban dan, hieu ung), con flipX chi lat rieng sprite.
    /// </summary>
    public class FacingController
    {
        readonly Transform target;
        readonly float baseScaleX;

        /// <summary>
        /// Sprite goc ve nhan vat quay sang phai hay trai. Bo sprite hien tai quay TRAI,
        /// nghia la scale.x duong = dang nhin trai - khong the gia dinh duong la phai.
        /// </summary>
        readonly bool artFacesRight;

        public FacingController(Transform target, bool artFacesRight)
        {
            this.target = target;
            this.artFacesRight = artFacesRight;

            baseScaleX = Mathf.Abs(target.localScale.x);

            bool scalePositive = target.localScale.x >= 0f;
            FacingRight = scalePositive == artFacesRight;
        }

        public bool FacingRight { get; private set; }

        /// <summary>+1 nhin phai, -1 nhin trai. Gia tri gan 0 thi giu nguyen huong cu.</summary>
        public void FaceTowards(float direction)
        {
            if (Mathf.Abs(direction) < 0.01f) return;

            bool right = direction > 0f;
            if (right == FacingRight) return;

            FacingRight = right;

            // Chi de scale duong khi huong muon nhin trung voi huong cua ban ve goc.
            var s = target.localScale;
            s.x = (right == artFacesRight) ? baseScaleX : -baseScaleX;
            target.localScale = s;
        }
    }
}
