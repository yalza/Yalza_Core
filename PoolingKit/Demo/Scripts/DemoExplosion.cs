using UnityEngine;
using Yalza_Core.PoolingKit.Runtime.Core;

public class DemoExplosion : MonoBehaviour, IPoolable
{
    public float duration = 1f;

    private float _timer;
    private Vector3 _startScale;

    private void Awake()
    {
        _startScale = transform.localScale;
    }

    public void OnSpawned()
    {
        _timer = 0f;
        transform.localScale = _startScale * 0.5f;
    }

    public void OnDeSpawned()
    {
        throw new System.NotImplementedException();
    }


    private void Update()
    {
        _timer += Time.deltaTime;
        float t = _timer / duration;
        transform.localScale = Vector3.Lerp(_startScale * 0.5f, _startScale * 1.5f, t);

        if (_timer >= duration)
        {
            GetComponent<PooledObject>().Despawn();
        }
    }
}
