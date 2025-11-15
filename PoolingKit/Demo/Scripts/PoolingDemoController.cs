// Assets/Yalza_Core/Pooling/Demo/Scripts/PoolingDemoController.cs

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Yalza_Core.PoolingKit.Runtime.Core;

namespace Yalza_Core.PoolingKit.Demo.Scripts
{
    public class PoolingDemoController : MonoBehaviour
    {
        public float spawnHeight = 1f;
        public float autoSpawnInterval = 0.2f;

        private bool _autoSpawn;
        private Coroutine _autoSpawnRoutine;
        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            if (IsPointerOverUI())
                return;
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Left click - try despawn clicked bullet");
                TryDespawnClickedBullet();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space pressed - spawn 10 bullets");
                SpawnMultiple(10);
            }
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Spawn 1 bullet tại vị trí chuột đang trỏ trên mặt đất / collider.
        /// </summary>
        private void SpawnBulletAtMouse()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 1000f))
                return;

            var pos = hit.point + Vector3.up * spawnHeight;

            // Cho bullet bắn từ camera về điểm click cho dễ thấy
            var dir = (hit.point - _cam.transform.position).normalized;
            var rot = Quaternion.LookRotation(dir);

            PoolManager.Spawn("bullet", pos, rot);
        }

        /// <summary>
        /// Nếu click trúng bullet → Despawn bullet đó về pool.
        /// </summary>
        private void TryDespawnClickedBullet()
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 1000f))
                return;

            // Lấy component DemoBullet trên collider (hoặc cha của collider)
            var bullet = hit.collider.GetComponentInParent<DemoBullet>();
            if (bullet == null)
                return;

            var pooled = bullet.GetComponent<PooledObject>();
            if (pooled != null)
            {
                pooled.Despawn();
            }
            else
            {
                // Trường hợp lỡ quên PooledObject (demo thôi, thực tế luôn có)
                Destroy(bullet.gameObject);
            }
        }

        /// <summary>
        /// Spawn nhiều bullet random để show pool.
        /// </summary>
        public void SpawnMultiple(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var x = Random.Range(-8f, 8f);
                var z = Random.Range(-8f, 8f);
                var pos = new Vector3(x, spawnHeight, z);
                var dir = (Vector3.zero - pos).normalized;
                var rot = Quaternion.LookRotation(dir);

                PoolManager.Spawn("bullet", pos, rot);
            }
        }

        public void SetAutoSpawn(bool enabled)
        {
            _autoSpawn = enabled;
            if (_autoSpawn)
            {
                if (_autoSpawnRoutine == null)
                    _autoSpawnRoutine = StartCoroutine(AutoSpawnLoop());
            }
            else
            {
                if (_autoSpawnRoutine != null)
                {
                    StopCoroutine(_autoSpawnRoutine);
                    _autoSpawnRoutine = null;
                }
            }
        }

        private IEnumerator AutoSpawnLoop()
        {
            while (_autoSpawn)
            {
                SpawnMultiple(3);
                yield return new WaitForSeconds(autoSpawnInterval);
            }
        }

        public void ClearAll()
        {
            PoolManager.Instance.ReleaseAllOf("bullet");
            PoolManager.Instance.ReleaseAllOf("explosion");
            PoolManager.Instance.ReleaseAllOf("floating_text");
        }
    }
}
