using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static QuickStackStore.QSSConfig;

namespace QuickStackStore
{
    internal class ControllerButtonHintHelper
    {
        internal static Sprite circleButtonSprite;
        internal static Sprite rectButtonSprite;

        internal static void AddControllerTooltipToTrashCan(Button button, Transform parent)
        {
            var uiGamePad = InventoryGui.instance.m_takeAllButton.GetComponent<UIGamePad>();

            var controllerHint = Object.Instantiate(uiGamePad.m_hint, parent);
            var uiGamePadNew = button.gameObject.AddComponent<UIGamePad>();
            uiGamePadNew.m_hint = controllerHint;

            InventoryGui.instance.StartCoroutine(WaitAFrameToSetupControllerHint(uiGamePadNew));
        }

        private static IEnumerator WaitAFrameToSetupControllerHint(UIGamePad uiGamePad)
        {
            yield return null;

            if (uiGamePad == null)
            {
                yield break;
            }

            if (!uiGamePad.m_hint)
            {
                yield break;
            }

            ControllerButtonHintHelper.SetupControllerHint(uiGamePad, KeybindChecker.joyTrash);
        }

        internal static void FixTakeAllButtonControllerHint(InventoryGui instance)
        {
            var uiGamePad = instance.m_takeAllButton.GetComponent<UIGamePad>();

            bool shouldShowHint = !ControllerConfig.RemoveControllerButtonHintFromTakeAllButton.Value;

            uiGamePad.enabled = shouldShowHint;

            var toMoveUp = uiGamePad.m_hint.gameObject;

            if (!shouldShowHint)
            {
                toMoveUp.SetActive(false);
            }
            else
            {
                toMoveUp.SetActive(true);

                //toMoveUp.GetComponentInChildren<Text>().text = Localization.instance.Translate(KeybindChecker.keyPrefix + "LStick");

                var canvas = toMoveUp.AddComponent<Canvas>();
                toMoveUp.AddComponent<GraphicRaycaster>();

                instance.StartCoroutine(DelayedOverrideSorting(canvas));
            }
        }

        private static IEnumerator DelayedOverrideSorting(Canvas canvas)
        {
            yield return null;

            while (canvas != null && !canvas.isActiveAndEnabled)
            {
                yield return null;
            }

            if (canvas == null)
            {
                yield break;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = 1;
        }

        internal static IEnumerator WaitAFrameToSetupControllerHint(Button button, string joyHint)
        {
            yield return null;

            if (button == null)
            {
                yield break;
            }

            var uiGamePad = button.GetComponent<UIGamePad>();

            if (!uiGamePad)
            {
                yield break;
            }

            if (!uiGamePad.m_hint)
            {
                yield break;
            }

            SetupControllerHint(uiGamePad, joyHint);
        }

        internal static void SetupControllerHint(UIGamePad uiGamePad, string joyHint)
        {
            var hint = uiGamePad.m_hint;
            uiGamePad.m_zinputKey = null;

            if (ControllerConfig.UseHardcodedControllerSupport.Value)
            {
                if (joyHint != KeybindChecker.joyStoreAll)
                {
                    var image = hint.GetComponent<Image>();
                    var rect = hint.GetComponent<RectTransform>();

                    if (joyHint == KeybindChecker.joySort)
                    {
                        var height = rect.sizeDelta.y + 4;
                        rect.sizeDelta = new Vector2(height, height);
                        image.sprite = circleButtonSprite;
                    }
                    else
                    {
                        image.sprite = rectButtonSprite;
                    }

                    float grey = 150f / 255f;
                    image.color = new Color(grey, grey, grey, 1f);
                    hint.transform.GetChild(0).GetComponent<Image>().enabled = false;
                }

                hint.gameObject.SetActive(true);
                hint.GetComponentInChildren<Text>().text = Localization.instance.Translate(KeybindChecker.joyTranslationPrefix + joyHint);
            }
            else
            {
                uiGamePad.enabled = false;
                hint.gameObject.SetActive(false);
            }
        }
    }
}