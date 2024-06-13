using System;
using System.Collections.Generic;
using Coffee.UIEffects;
using Pancake.Apex;
using Pancake.Localization;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class SettingView : View, IRecyclerViewCellProvider, IRecyclerViewDataProvider
    {
        [SerializeField] private LocaleTextComponent localeTextVersion;
        [SerializeField] private Button buttonMusic;
        [SerializeField] private Button buttonSound;
        [SerializeField] private Button buttonVibrate;
        [SerializeField] private Button buttonUpdate;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonCredit;
        [SerializeField] private Button buttonBackupData;
        [SerializeField] private Button buttonRestoreData;

        [SerializeField] private UIEffect musicUIEffect;
        [SerializeField] private UIEffect soundUIEffect;
        [SerializeField] private UIEffect vibrateUIEffect;

        [Header("Language")] [SerializeField] private GameObject languageElementPrefab;
        [SerializeField] private RectTransform languagePopup;
        [SerializeField] private UIButton buttonSelectLanguage;
        [SerializeField] private TextMeshProUGUI textNameLanguageSelected;
        [SerializeField] private LanguageData langData;

        [Space] [SerializeField] private FloatVariable musicVolume;
        [SerializeField] private FloatVariable sfxVolume;
        [SerializeField] private ScriptableEventAudioHandle eventPauseMusic;
        [SerializeField] private ScriptableEventAudioHandle eventResumeMusic;
        [SerializeField] private ScriptableEventNoParam eventStopAllSfx;
        [SerializeField, PopupPickup] private string popupCredit;
        [SerializeField, PopupPickup] private string popupBackupData;
        [SerializeField, PopupPickup] private string popupRestoreData;
#if UNITY_IOS
        [SerializeField] private Button buttonRestore;
        [SerializeField] private Pancake.IAP.ScriptableEventIAPNoParam restorePurchaseEvent;
#endif

        private Language _selectedLang;
        private RectTransform _languageScrollerRT;
        private bool _firstTimeActiveLanguage;
        private PopupContainer _popupContainer;
        private RecyclerView _recyclerView;
        private readonly List<LanguageElementCellModel> _datas = new();

        private RecyclerView RecyclerView
        {
            get
            {
                if (_recyclerView == null)
                {
                    _recyclerView = languagePopup.GetComponentInChildren<RecyclerView>();
                    _recyclerView.DataCount = 0;
                    _recyclerView.CellProvider = this;
                    _recyclerView.DataProvider = this;
                }

                return _recyclerView;
            }
        }

        protected override UniTask Initialize()
        {
            // Add padding for the safe area.
            float canvasScaleFactor = GetComponentInParent<Canvas>().scaleFactor;
            RecyclerView.AfterPadding += (int) (Screen.safeArea.y / canvasScaleFactor);

            _datas.Clear();
            foreach (var key in langData.Keys)
            {
                _datas.Add(new LanguageElementCellModel
                {
                    Lang = key,
                    LocaleText = langData[key],
                    IsSelected = IsElementSelected,
                    OnSelectAction = OnButtonElementClicked,
                    OnHideLanuageaAction = InternalHideSelectLanguage
                });
                _recyclerView.DataCount++;
            }

            _languageScrollerRT = RecyclerView.GetComponent<RectTransform>();
            buttonMusic.onClick.AddListener(OnButtonMusicPressed);
            buttonSound.onClick.AddListener(OnButtonSoundPressed);
            buttonVibrate.onClick.AddListener(OnButtonVibratePressed);
            buttonUpdate.onClick.AddListener(OnButtonUpdatePressed);
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonCredit.onClick.AddListener(OnButtonCreditPressed);
            buttonBackupData.onClick.AddListener(OnButtonBackupDataPressed);
            buttonRestoreData.onClick.AddListener(OnButtonRestoreDataPressed);
            buttonSelectLanguage.onClick.AddListener(OnButtonSelectLanguagePressed);
            _selectedLang = Locale.CurrentLanguage;
            textNameLanguageSelected.text = _selectedLang.Name;
#if UNITY_IOS
            buttonRestore.onClick.AddListener(OnButtonRestorePressed);
#endif
            _popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
            Refresh();
            return UniTask.CompletedTask;
        }

        private void OnButtonRestoreDataPressed() { _popupContainer.Push<RestoreDataPopup>(popupRestoreData, true); }

        private void OnButtonBackupDataPressed() { _popupContainer.Push<BackupDataPopup>(popupBackupData, true); }

        private void OnButtonSelectLanguagePressed()
        {
            if (buttonSelectLanguage.AffectObject.localEulerAngles.z.Equals(0))
            {
                Tween.LocalRotation(buttonSelectLanguage.AffectObject.transform, Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 90), 0.3f);
                InternalShowSelectLanguage();

                if (!_firstTimeActiveLanguage)
                {
                    _firstTimeActiveLanguage = true;
                    RecyclerView.Reload();
                }
            }
            else
            {
                Tween.LocalRotation(buttonSelectLanguage.AffectObject.transform, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f);
                InternalHideSelectLanguage();
            }
        }

        private void OnButtonCreditPressed() { _popupContainer.Push<CreditPopup>(popupCredit, true); }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void OnButtonSoundPressed()
        {
            bool state = sfxVolume.Value.Approximately(1);
            state = !state;
            if (state)
            {
                sfxVolume.Value = 1;
            }
            else
            {
                sfxVolume.Value = 0;
                eventStopAllSfx.Raise();
            }

            RefreshSoundState(state);
        }

        private void OnButtonVibratePressed()
        {
            bool state = Vibration.EnableVibration;
            state = !state;
            Vibration.EnableVibration = state;
            RefreshVibrateState(state);
        }

        private void OnButtonMusicPressed()
        {
            bool state = musicVolume.Value.Approximately(1);
            state = !state;
            if (state)
            {
                musicVolume.Value = 1;
                eventResumeMusic.Raise(AudioHandle.invalid);
            }
            else
            {
                musicVolume.Value = 0;
                eventPauseMusic.Raise(AudioHandle.invalid);
            }

            RefreshMusicState(state);
        }

        private void OnButtonUpdatePressed() { C.GotoStore(); }

#if UNITY_IOS
        private void OnButtonRestorePressed()
        {
            restorePurchaseEvent?.Raise();
        }
#endif

        private void Refresh()
        {
            localeTextVersion.UpdateArgs($"{Application.version}");
            bool vibrateState = Vibration.EnableVibration;

            RefreshMusicState(musicVolume.Value.Approximately(1));
            RefreshSoundState(sfxVolume.Value.Approximately(1));
            RefreshVibrateState(vibrateState);
        }

        private void RefreshVibrateState(bool vibrateState) { vibrateUIEffect.effectMode = vibrateState ? EffectMode.None : EffectMode.Grayscale; }

        private void RefreshSoundState(bool soundState) { soundUIEffect.effectMode = soundState ? EffectMode.None : EffectMode.Grayscale; }

        private void RefreshMusicState(bool musicState) { musicUIEffect.effectMode = musicState ? EffectMode.None : EffectMode.Grayscale; }

        private void OnButtonElementClicked(Language lang)
        {
            _selectedLang = lang;
            Locale.CurrentLanguage = _selectedLang;
            RecyclerView.RefreshData();
            textNameLanguageSelected.text = lang.Name;
        }

        private bool IsElementSelected(Language lang) { return _selectedLang.Code.Equals(lang.Code); }

        private void InternalShowSelectLanguage()
        {
            _languageScrollerRT.pivot = new Vector2(0.5f, 1f);
            languagePopup.gameObject.SetActive(true);
            languagePopup.SetSizeDeltaY(74f);
            RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(false);
            Tween.UISizeDelta(languagePopup, new Vector2(languagePopup.sizeDelta.x, 666f), 0.5f)
                .OnComplete(() =>
                {
                    RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(true);
                    _languageScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                });
        }

        private void InternalHideSelectLanguage()
        {
            _languageScrollerRT.pivot = new Vector2(0.5f, 1f);
            RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(false);

            Tween.UISizeDelta(languagePopup, new Vector2(languagePopup.sizeDelta.x, 74f), 0.5f)
                .OnComplete(() =>
                {
                    languagePopup.gameObject.SetActive(false);
                    RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(true);
                    _languageScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                });
            Tween.LocalRotation(buttonSelectLanguage.AffectObject.transform, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f);
        }

        GameObject IRecyclerViewCellProvider.GetCell(int dataIndex) { return languageElementPrefab.Request(); }

        void IRecyclerViewCellProvider.ReleaseCell(GameObject cell) { cell.Return(); }

        void IRecyclerViewDataProvider.SetupCell(int dataIndex, GameObject cell) { cell.GetComponent<ICell>().Setup(_datas[dataIndex]); }
    }
}