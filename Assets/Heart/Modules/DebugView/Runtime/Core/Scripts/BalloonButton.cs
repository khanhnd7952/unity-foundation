using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public class BalloonButton : MonoBehaviour
    {
        [SerializeField] private RectTransform balloonRectTrans;
        [SerializeField] private Button balloonButton;
        [SerializeField] private Button backdropButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float animationDuration = 0.25f;
        [SerializeField] private Ease animationType = Ease.InCirc;
        [SerializeField] private Text balloonText;

        private Canvas _canvas;
        private Action _clicked;
        private bool _isInitialized;
        private RectTransform _rectTrans;

        public bool IsAnimating { get; private set; }

        private void Awake()
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnEnable()
        {
            balloonButton.onClick.AddListener(OnBalloonButtonClicked);
            backdropButton.onClick.AddListener(OnBackdropButtonClicked);
        }

        private void OnDisable()
        {
            balloonButton.onClick.RemoveListener(OnBalloonButtonClicked);
            backdropButton.onClick.RemoveListener(OnBackdropButtonClicked);
        }

        public void Initialize(Canvas canvas)
        {
            _rectTrans = (RectTransform) transform;
            _canvas = canvas;
            _isInitialized = true;
        }

        private void OnBalloonButtonClicked()
        {
            if (IsAnimating)
                return;

            _clicked?.Invoke();
            Hide();
        }

        private void OnBackdropButtonClicked()
        {
            if (IsAnimating)
                return;

            Hide();
        }

        public void Show(RectTransform targetRectTrans, string text, Action clicked)
        {
            Assert.IsTrue(_isInitialized);

            var worldCorners = new Vector3[4];
            targetRectTrans.GetWorldCorners(worldCorners);
            var worldPos = worldCorners[1] + (worldCorners[2] - worldCorners[1]) * 0.5f;
            var canvasCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTrans, screenPoint, canvasCamera, out var localPosition);
            balloonRectTrans.anchoredPosition = localPosition;

            balloonText.text = text;
            _clicked = clicked;
            ShowRoutine();
        }

        public void Hide()
        {
            _clicked = null;
            HideRoutine();
        }

        private void ShowRoutine()
        {
            if (IsAnimating) throw new InvalidOperationException($"{GetType().Name} is already animating.");

            IsAnimating = true;
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            Tween.Alpha(canvasGroup, 1, animationDuration, animationType)
                .OnComplete(() =>
                {
                    canvasGroup.alpha = 1.0f;
                    canvasGroup.interactable = true;
                    IsAnimating = false;
                });
        }

        private void HideRoutine()
        {
            if (IsAnimating) throw new InvalidOperationException($"{GetType().Name} is already animating.");

            IsAnimating = true;
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            Tween.Alpha(canvasGroup, 0, animationDuration, animationType)
                .OnComplete(() =>
                {
                    canvasGroup.alpha = 0.0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    IsAnimating = false;
                });
        }
    }
}