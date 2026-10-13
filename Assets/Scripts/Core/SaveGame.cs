using System;
using System.IO;
using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>Du lieu luu giua cac lan choi: checkpoint cuoi va ky nang da mo.</summary>
    [Serializable]
    public class SaveData
    {
        [Tooltip("Ten scene co checkpoint ngoi gan nhat (rong = chua ngoi lan nao).")]
        public string checkpointScene;
        public Vector2 checkpointPosition;
        public bool wallCling;
        public bool orbShot;
    }

    /// <summary>
    /// Luu game ra mot file JSON trong persistentDataPath. Doc mot lan khi can, ghi khi goi <see cref="Save"/>
    /// (ngoi checkpoint, nhat ky nang). Shadow van luu bang PlayerPrefs nhu cu.
    /// </summary>
    public static class SaveGame
    {
        static SaveData data;

        static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static SaveData Data
        {
            get
            {
                if (data == null) Load();
                return data;
            }
        }

        public static bool HasCheckpointIn(string sceneName) =>
            !string.IsNullOrEmpty(Data.checkpointScene) && Data.checkpointScene == sceneName;

        public static void Save()
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true)); }
            catch (Exception e) { Debug.LogWarning("Khong ghi duoc file luu: " + e.Message); }
        }

        /// <summary>Xoa file luu (choi lai tu dau).</summary>
        public static void Delete()
        {
            data = new SaveData();
            if (File.Exists(FilePath)) File.Delete(FilePath);
        }

        static void Load()
        {
            data = null;
            try
            {
                if (File.Exists(FilePath)) data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            }
            catch (Exception e) { Debug.LogWarning("File luu hong, tao moi: " + e.Message); }
            data ??= new SaveData();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => data = null;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LuHanh/Save/Xoa file luu (choi lai tu dau)")]
        static void DeleteMenu()
        {
            Delete();
            Debug.Log("Da xoa file luu: " + FilePath);
        }
#endif
    }
}
