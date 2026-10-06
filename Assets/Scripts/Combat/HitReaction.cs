using LuHanh.Player;
using UnityEngine;

namespace LuHanh.Combat
{
    /// <summary>
    /// Phan ung cua nguoi choi khi trung mot don (day lui, chop sang, khung hinh...).
    /// Moi loai quai tro toi mot asset rieng qua <see cref="HitInfo.reaction"/>; muon hieu ung dac biet
    /// (doc, chay, choang...) thi viet lop con va override <see cref="Apply"/>.
    /// Duoc goi sau khi da tru mau va nguoi choi chua chet.
    /// </summary>
    public abstract class HitReaction : ScriptableObject
    {
        public abstract void Apply(PlayerHealth target, HitInfo hit);
    }
}
