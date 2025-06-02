using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Arena.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Image fadeOutImage;
        public static UIManager Instance;
        
        [SerializeField]
        private TMP_Text myPointsLabel;
        
        [SerializeField]
        private TMP_Text opponentsPointsLabel;

        [SerializeField] private Camera uiCamera;

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            Instance = null;
        }

        public void FadeOut(Action onFinished)
        {
            StartCoroutine(FadeOutCoroutine(onFinished));
        }

        public void FadeIn(Action onFinished)
        {
            StartCoroutine(FadeInCoroutine(onFinished));
        }

        public void LoserAnimation()
        {
            StartCoroutine(EnvironmentToWhiteCoroutine());
        }


        
        public void SetPoints(string myName, int myPoints, string opponentsName, int opponentsPoints)
        {
            myPointsLabel.text = myName + "\n" + myPoints;
            opponentsPointsLabel.text = opponentsName + "\n" + opponentsPoints;
        }

        public void ShowPoints()
        {
            myPointsLabel.enabled = true;
            opponentsPointsLabel.enabled = true;
        }

        private void HidePoints()
        {
            myPointsLabel.enabled = false;
            opponentsPointsLabel.enabled = false;
        }

        private IEnumerator FadeOutCoroutine(Action onFinished)
        {
            for (float t = 0; t < 3; t += Time.deltaTime)
            {
                fadeOutImage.color = Color.Lerp(new Color(0,0,0,0), Color.black,t/3);
                yield return null;
            }

            RenderSettings.fogColor = Color.black;
            RenderSettings.skybox.color = Color.black;
            uiCamera.enabled = true;
            ShowPoints();
            onFinished();
        }
        
        private IEnumerator FadeInCoroutine(Action onFinished)
        {
            for (float t = 0; t < 3; t += Time.deltaTime)
            {
                fadeOutImage.color = Color.Lerp(Color.black, new Color(0,0,0,0),t/3);
                yield return null;
            }
            uiCamera.enabled = false;
            HidePoints();
            onFinished();
        }
        
        private IEnumerator EnvironmentToWhiteCoroutine()
        {
            for (var timeLeft = 1f; timeLeft > 0; timeLeft -= Time.deltaTime)
            {
                var lerpedColor = Color.Lerp(Color.black, Color.white, 1 - timeLeft);
                RenderSettings.fogColor = lerpedColor;
                RenderSettings.skybox.color = lerpedColor;
                yield return null;
            }
        }
    }
}
