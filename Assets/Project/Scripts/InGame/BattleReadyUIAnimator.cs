using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectFirst.InGame
{
    /// <summary>
    /// Battle Ready ?遺얇늺??UI ?醫딅빍筌롫뗄???띾궢 ???????곕굡獄쏄퉮???온?귐뗫???덈뼄.
    /// </summary>
    public class BattleReadyUIAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float slotHoverScale = 1.1f;
        [SerializeField] private float slotAnimationDuration = 0.2f;
        [SerializeField] private float popupFadeDuration = 0.3f;

        [Header("Audio")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip removeSound;

        private AudioSource _audioSource;
        private BattleReadyManager _battleReadyManager;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _battleReadyManager = GetComponent<BattleReadyManager>();
        }

        private void OnEnable()
        {
            SetupSlotAnimations();
            SetupPopupAnimations();
        }

        private void SetupSlotAnimations()
        {
            if (_battleReadyManager == null) return;

            var slotElementsField = typeof(BattleReadyManager).GetField("_slotElements",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (slotElementsField == null) return;

            var slotElements = slotElementsField.GetValue(_battleReadyManager) as VisualElement[];
            if (slotElements == null) return;

            for (int i = 0; i < slotElements.Length; i++)
            {
                if (slotElements[i] != null)
                    SetupSlotHoverAnimation(slotElements[i], i);
            }
        }

        private void SetupSlotHoverAnimation(VisualElement slot, int slotIndex)
        {
            slot.RegisterCallback<PointerEnterEvent>(_ =>
            {
                PlayHoverSound();
                SetScale(slot, slotHoverScale);
            });

            slot.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                SetScale(slot, 1f);
            });

            slot.RegisterCallback<ClickEvent>(_ =>
            {
                PlayClickAnimation(slot);
                PlaySelectSound();
            });
        }

        private void SetupPopupAnimations()
        {
            if (_battleReadyManager == null) return;

            var charSelectPopupField = typeof(BattleReadyManager).GetField("_charSelectPopup",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (charSelectPopupField == null) return;

            var popup = charSelectPopupField.GetValue(_battleReadyManager) as VisualElement;
            if (popup == null) return;

            popup.style.opacity = 0f;
            popup.style.display = DisplayStyle.None;
        }

        public void ShowPopup(VisualElement popup)
        {
            if (popup == null) return;
            StartCoroutine(ShowPopupWithAnimation(popup));
        }

        private IEnumerator ShowPopupWithAnimation(VisualElement popup)
        {
            popup.style.display = DisplayStyle.Flex;
            popup.style.opacity = 0f;

            float elapsed = 0f;
            while (elapsed < popupFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                popup.style.opacity = Mathf.Clamp01(elapsed / popupFadeDuration);
                yield return null;
            }

            popup.style.opacity = 1f;
        }

        public void HidePopupWithAnimation(VisualElement popup)
        {
            if (popup == null) return;
            StartCoroutine(HidePopupDelayed(popup));
        }

        private IEnumerator HidePopupDelayed(VisualElement popup)
        {
            float startOpacity = popup.resolvedStyle.opacity;
            float elapsed = 0f;

            while (elapsed < popupFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / popupFadeDuration);
                popup.style.opacity = Mathf.Lerp(startOpacity, 0f, t);
                yield return null;
            }

            popup.style.opacity = 0f;
            popup.style.display = DisplayStyle.None;
        }

        private void PlayClickAnimation(VisualElement element)
        {
            if (element == null) return;
            StartCoroutine(ClickAnimationCoroutine(element));
        }

        private IEnumerator ClickAnimationCoroutine(VisualElement element)
        {
            SetScale(element, 0.95f);
            yield return new WaitForSeconds(slotAnimationDuration * 0.5f);
            SetScale(element, 1f);
        }

        public void PlayCharacterSelectAnimation(VisualElement characterCard)
        {
            if (characterCard == null) return;
            StartCoroutine(CharacterSelectAnimationCoroutine(characterCard));
        }

        private IEnumerator CharacterSelectAnimationCoroutine(VisualElement card)
        {
            SetScale(card, 1.05f);
            yield return new WaitForSeconds(0.15f);
            SetScale(card, 1f);
            PlaySelectSound();
        }

        public void PlayCharacterRemoveAnimation(VisualElement slot)
        {
            if (slot == null) return;
            StartCoroutine(CharacterRemoveAnimationCoroutine(slot));
        }

        private IEnumerator CharacterRemoveAnimationCoroutine(VisualElement slot)
        {
            slot.style.opacity = 0.3f;
            SetScale(slot, 0.9f);

            PlayRemoveSound();

            yield return new WaitForSeconds(0.2f);

            slot.style.opacity = 1f;
            SetScale(slot, 1f);
        }

        private void PlayHoverSound()
        {
            if (hoverSound != null && _audioSource != null)
                _audioSource.PlayOneShot(hoverSound);
        }

        private void PlaySelectSound()
        {
            if (selectSound != null && _audioSource != null)
                _audioSource.PlayOneShot(selectSound);
        }

        private void PlayRemoveSound()
        {
            if (removeSound != null && _audioSource != null)
                _audioSource.PlayOneShot(removeSound);
        }

        public void PlayBattleStartAnimation(Button battleStartBtn)
        {
            if (battleStartBtn == null) return;
            StartCoroutine(BattleStartAnimationCoroutine(battleStartBtn));
        }

        private IEnumerator BattleStartAnimationCoroutine(Button btn)
        {
            SetScale(btn, 1.1f);
            yield return new WaitForSeconds(0.2f);
            SetScale(btn, 1f);
        }

        private static void SetScale(VisualElement element, float scale)
        {
            if (element == null) return;
            element.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }
    }
}
