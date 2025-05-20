using System;
using System.Collections;
using UnityEngine;

public class CollectibleCoin : MonoBehaviour
{
    
    [SerializeField]
    private float spinningSpeed;

    [SerializeField] private float collectDuration;
    [SerializeField] private float collectSpinFactor;

    private bool _pickedUp;

    void Update()
    {
        if (_pickedUp) return;
        transform.Rotate(Vector3.up, Time.deltaTime * spinningSpeed);
    }


    private void OnTriggerEnter(Collider other)
    {
        _pickedUp = true;
        OnCollected();
    }

    private void OnCollected()
    {
        StartCoroutine(CollectTween());
    }

    private IEnumerator CollectTween()
    {
        for (var timeLeft = collectDuration; timeLeft > 0; timeLeft -= Time.deltaTime)
        {
            transform.localScale = Vector3.one * timeLeft / collectDuration;
            transform.Rotate(Vector3.up, Time.deltaTime * spinningSpeed * collectSpinFactor);
            yield return null;
        }
    }
}
