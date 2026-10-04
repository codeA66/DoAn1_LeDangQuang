using Unity.Cinemachine;
using UnityEngine;

namespace LuHanh.Core
{
    /// <summary>
    /// Gan Player (tim theo tag) lam Tracking Target cho CinemachineCamera luc bat dau, de camera
    /// nam trong prefab LevelEssentials van theo dung nhan vat cua tung man ma khong can keo tay.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraFollowPlayer : MonoBehaviour
    {
        void Awake()
        {
            var cam = GetComponent<CinemachineCamera>();
            if (cam.Target.TrackingTarget) return;
            var player = GameObject.FindWithTag("Player");
            if (!player) return;
            cam.Target.TrackingTarget = player.transform;
            // Dat camera ngay vao cho nhan vat, khong troi tu vi tri cu toi.
            cam.ForceCameraPosition(player.transform.position + Vector3.forward * cam.transform.position.z, cam.transform.rotation);
        }
    }
}
