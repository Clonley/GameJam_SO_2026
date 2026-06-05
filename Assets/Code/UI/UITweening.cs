using NUnit.Framework.Internal;
using System.Collections;
using TMPro;
using UnityEngine;

namespace EscapeVelocity
{
    public class UITweening : MonoBehaviour
    {
        public GameObject TargetObject;

        [Header("Transformation Settings")]
        public Vector2 PositionTransformation = Vector2.zero;
        public Vector2 ScaleTransformation = Vector2.one;
        public float RotationTransformation = 0f;
        public float FadeAlpha = 0f;

        [Header("Animation Settings")]
        public float Duration = 0.5f;
        public AnimationCurve EasingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector2 initialPosition;
        private Vector2 initialScale;
        private Vector3 initialRotation;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        public bool isOpen;

        void Start()
        {
            InitializeVariables();
            isOpen = TargetObject.activeSelf;
        }

        private void OnValidate()
        {
            InitializeVariables();
        }

        public void TweenIn()
        {
            if (isOpen == false)
            {
                StopAllCoroutines();
                StartCoroutine(Tween(PositionTransformation+initialPosition, ScaleTransformation, RotationTransformation, initialPosition, initialScale, initialRotation.z, 1, false));
                isOpen = true;
            }
        }
        public void TweenOut()
        {
            if (isOpen == true)
            {
                StopAllCoroutines();
                StartCoroutine(Tween(initialPosition, initialScale, initialRotation.z, PositionTransformation + initialPosition, ScaleTransformation, RotationTransformation, FadeAlpha, true));
                isOpen = false;
            }
        }

        public void InverseTweenIn()
        {
            if (isOpen == false)
            {
                StopAllCoroutines();
                StartCoroutine(Tween(-PositionTransformation + initialPosition, ScaleTransformation, RotationTransformation, initialPosition, initialScale, initialRotation.z, 1, false));
                isOpen = true;
            }
        }
        public void InverseTweenOut()
        {
            if (isOpen == true)
            {
                StopAllCoroutines();
                StartCoroutine(Tween(initialPosition, initialScale, initialRotation.z, -PositionTransformation + initialPosition, ScaleTransformation, RotationTransformation, FadeAlpha, true));
                isOpen = false;
            }
        }

        private void InitializeVariables()
        {
            if (TargetObject == null)
            {
                Debug.LogError("TargetObject is not assigned in UITweening.");
                return;
            }

            rectTransform = TargetObject.GetComponent<RectTransform>();
            canvasGroup = TargetObject.GetComponent<CanvasGroup>();

            initialPosition = rectTransform.localPosition;
            initialScale = rectTransform.localScale;
            initialRotation = rectTransform.localEulerAngles;
        }

        IEnumerator Tween(Vector2 initialPosition, Vector2 initialScale, float initialRotation, Vector2 PositionTransformation, Vector2 targetScale, float targetRotation,float targetAlpha, bool invert)
        {
            canvasGroup.interactable = false;
            TargetObject.SetActive(true);
            float i = 0f;
            float t = 0f;
            Vector2 targetPosition = PositionTransformation;

            while (i<Duration)
            {
                i += Time.deltaTime;
                t = EasingCurve.Evaluate(i / Duration);

                rectTransform.localPosition = Vector2.Lerp(initialPosition, targetPosition, t);
                rectTransform.localScale = Vector2.Lerp(initialScale, targetScale, t);
                rectTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(0, 0, initialRotation)), Quaternion.Euler(new Vector3(0, 0, targetRotation)), t);
                canvasGroup.alpha = Mathf.Lerp(invert?1:0, targetAlpha, t);

                yield return null;
            }
            rectTransform.localPosition = targetPosition;
            rectTransform.localScale = targetScale;
            rectTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, targetRotation));
            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = !invert;
            TargetObject.SetActive(!invert);
        }

        public void Initialize(float targetAlpha, bool invert)
        {
            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = !invert;
            TargetObject.SetActive(!invert);
        }
    }
}
