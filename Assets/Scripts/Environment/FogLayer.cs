using UnityEngine;

namespace LuHanh.Environment
{
    /// <summary>
    /// Mot dai suong mu cuon ngang vo tan, do mo tho nhe theo thoi gian.
    /// Gan vao mot GameObject rong; script tu tao cac tile sprite ben trong.
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class FogLayer : MonoBehaviour
    {
        [Header("Hinh anh")]
        public Sprite fogSprite;
        public Color tint = new Color(0.65f, 0.55f, 0.95f, 0.18f);
        public string sortingLayer = "Background";
        public int sortingOrder = 5;

        [Header("Chuyen dong")]
        [Tooltip("Toc do cuon ngang (unit/giay). Am = cuon sang trai.")]
        public float scrollSpeed = -0.35f;
        [Tooltip("Nhap nho theo chieu doc.")]
        public float driftAmplitude = 0.25f;
        public float driftSpeed = 0.15f;

        [Header("Do mo")]
        [Tooltip("Bien do tho cua alpha, cong/tru vao alpha cua tint.")]
        public float breathAmplitude = 0.06f;
        public float breathSpeed = 0.25f;

        [Header("Kich thuoc")]
        [Tooltip("So tile phu ngang. Tang len neu camera rong.")]
        public int tileCount = 4;
        public float scaleMultiplier = 1f;

        readonly System.Collections.Generic.List<SpriteRenderer> tiles = new();
        float tileWidth;
        float scrollOffset;
        float baseY;

        void Start()
        {
            if (fogSprite == null)
            {
                Debug.LogWarning($"{name}: chua gan fogSprite, FogLayer se khong chay.", this);
                enabled = false;
                return;
            }

            baseY = transform.position.y;
            BuildTiles();
        }

        void BuildTiles()
        {
            tileWidth = fogSprite.bounds.size.x * scaleMultiplier;

            for (int i = 0; i < tileCount; i++)
            {
                var go = new GameObject($"FogTile_{i}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(i * tileWidth, 0f, 0f);
                go.transform.localScale = Vector3.one * scaleMultiplier;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = fogSprite;
                sr.color = tint;
                sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = sortingOrder + i;
                tiles.Add(sr);
            }
        }

        void Update()
        {
            if (tiles.Count == 0 || tileWidth <= 0.001f) return;

            float t = Time.time;
            scrollOffset += scrollSpeed * Time.deltaTime;
            // Giu offset trong mot chu ky de khong mat do chinh xac float sau thoi gian dai.
            scrollOffset = Mathf.Repeat(scrollOffset, tileWidth);

            float drift = Mathf.Sin(t * driftSpeed) * driftAmplitude;
            transform.position = new Vector3(transform.position.x, baseY + drift, transform.position.z);

            for (int i = 0; i < tiles.Count; i++)
            {
                var tr = tiles[i].transform;
                tr.localPosition = new Vector3(scrollOffset + i * tileWidth - tileWidth, 0f, 0f);

                // Moi tile tho lech pha -> dai suong khong phang mot mau.
                float breath = Mathf.Sin(t * breathSpeed + i * 1.7f) * breathAmplitude;
                var c = tint;
                c.a = Mathf.Max(0f, tint.a + breath);
                tiles[i].color = c;
            }
        }
    }
}
