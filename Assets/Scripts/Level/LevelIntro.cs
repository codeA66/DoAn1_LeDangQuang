using UnityEngine;
using UnityEngine.SceneManagement;

namespace LuHanh.Level
{
    /// <summary>
    /// Mo dau man khi vao thang man (bam Play, mo game): the ten man + cong den mo ra cho nhan vat roi ra.
    /// Den man bang cong thi SceneTransition da lo hieu ung nay, nen tu bo qua.
    /// Dat trong LevelEssentials de man nao cung co.
    /// </summary>
    public class LevelIntro : MonoBehaviour
    {
        [SerializeField] LevelList levels;
        [Tooltip("Prefab cong (LevelPortal) dung lam cong den.")]
        [SerializeField] LevelPortal portalPrefab;
        [Tooltip("Tat khi dang thu man trong Editor cho nhanh.")]
        [SerializeField] bool playOnStart = true;

        void Start()
        {
            if (!playOnStart || SceneTransition.IsBusy || !levels || !portalPrefab) return;
            var entry = levels.Get(levels.IndexOf(SceneManager.GetActiveScene().name));
            var arrival = Instantiate(portalPrefab);
            arrival.MarkAsArrival();
            SceneTransition.PlayIntro(entry?.code, entry?.title, levels, arrival);
        }
    }
}
