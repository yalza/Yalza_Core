// Assets/Yalza_Core/Pooling/Demo/Scripts/PoolingDemoUI.cs

using UnityEngine;
using UnityEngine.UI;
using Yalza_Core.PoolingKit.Runtime.Core;

namespace Yalza_Core.PoolingKit.Demo.Scripts
{
    public class PoolingDemoUI : MonoBehaviour
    {
        public Text bulletText;
        public Text explosionText;
        public Text floatingTextText;

        private void Update()
        {
            UpdatePoolLabel("bullet", bulletText);
            UpdatePoolLabel("explosion", explosionText);
            UpdatePoolLabel("floating_text", floatingTextText);
        }

        private void UpdatePoolLabel(string key, Text label)
        {
            if (label == null) return;

            var pool = PoolManager.Instance.GetPool(key);
            if (pool == null)
            {
                label.text = $"{key}: (no pool)";
                return;
            }

            label.text =
                $"{key}: Active {pool.CountActive} / Inactive {pool.CountInactive} (Total {pool.CountAll})";
        }
    }
}
