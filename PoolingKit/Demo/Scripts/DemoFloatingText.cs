// Assets/Yalza_Core/Pooling/Demo/Scripts/DemoFloatingText.cs

using UnityEngine;
using UnityEngine.UI;
using Yalza_Core.PoolingKit.Runtime.Core;

namespace Yalza_Core.PoolingKit.Demo.Scripts
{
    public class DemoFloatingText : MonoBehaviour, IPoolable
    {
        public float moveUpDistance = 1.5f;
        public float duration = 1f;

        private Text _text;              // nếu dùng TMP thì đổi sang TextMeshProUGUI
        private CanvasGroup _canvasGroup;

        private Vector3 _startWorldPos;
        private float _timer;

        private static Camera _mainCam;

        private void Awake()
        {
            _text = GetComponent<Text>();
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (_mainCam == null)
            {
                _mainCam = Camera.main;
            }
        }

        public void OnSpawned()
        {
            _timer = 0f;
            _canvasGroup.alpha = 1f;
        }

        public void OnDeSpawned()
        {
            throw new System.NotImplementedException();
        }


        private void Update()
        {
            _timer += Time.deltaTime;
            float t = _timer / duration;

            // Move up
            var worldPos = _startWorldPos + Vector3.up * moveUpDistance * t;
            var screenPos = _mainCam.WorldToScreenPoint(worldPos);
            transform.position = screenPos;

            // Fade
            _canvasGroup.alpha = 1f - t;

            if (_timer >= duration)
            {
                GetComponent<PooledObject>().Despawn();
            }
        }

        public void SetWorldStart(Vector3 worldPos)
        {
            _startWorldPos = worldPos;
        }

        public void SetText(string msg)
        {
            if (_text != null)
                _text.text = msg;
        }

        // Helper static cho demo
        public static void SpawnText(Vector3 worldPos, string msg)
        {
            var go = PoolManager.Spawn("floating_text", Vector3.zero, Quaternion.identity);
            if (go == null) return;

            var ft = go.GetComponent<DemoFloatingText>();
            ft.SetWorldStart(worldPos);
            ft.SetText(msg);
        }
    }
}
