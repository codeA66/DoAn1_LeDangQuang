using System.Collections.Generic;
using UnityEngine;

namespace LuHanh.Level
{
    /// <summary>
    /// Danh sach man theo thu tu choi. Cong dich chuyen tra danh sach nay de biet man ke tiep;
    /// them man moi chi can them mot dong (va them scene vao Build Profiles).
    /// </summary>
    [CreateAssetMenu(menuName = "LuHanh/Level List", fileName = "LevelList")]
    public class LevelList : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            [Tooltip("Ten scene (dung ten file, khong co .unity). Scene phai co trong Build Profiles.")]
            public string sceneName;
            [Tooltip("Ma man hien tren the chuyen man, vd 1-1.")]
            public string code;
            [Tooltip("Ten man hien duoi ma man.")]
            public string title;
            [Tooltip("Man co boss: cong an cho toi khi ha boss.")]
            public bool hasBoss;
        }

        public List<Entry> levels = new();
        [Tooltip("Scene khi da qua man cuoi cung.")]
        public string completeScene = "LevelComplete";

        [Header("The ten man khi chuyen man")]
        [Tooltip("Font ma man (vd 1-2).")]
        public Font codeFont;
        [Tooltip("Font ten man.")]
        public Font titleFont;
        [Tooltip("Material lo tron dong/mo man hinh (shader LuHanh/UIIris).")]
        public Material irisMaterial;

        public int IndexOf(string sceneName) => levels.FindIndex(e => e.sceneName == sceneName);

        public Entry Get(int index) => index >= 0 && index < levels.Count ? levels[index] : null;

        /// <summary>Man ngay sau <paramref name="sceneName"/>, null neu la man cuoi.</summary>
        public Entry Next(string sceneName) => Get(IndexOf(sceneName) + 1);
    }
}
