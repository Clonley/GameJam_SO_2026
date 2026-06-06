using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeVelocity
{
    public class EV_TabsController : MonoBehaviour
    {
        public TMP_FontAsset normalFont;
        public TMP_FontAsset selectFont;
        public ButtonNav[] buttonNavs;
        public bool debugButtonTransforms = false;

        [System.Serializable]
        public class ButtonNav
        {
            public Button button;
            public Selectable selectOnUp;
            public Selectable selectOnDown;
            public Selectable selectOnRight;
            public Selectable selectOnLeft;
            public UITweening tweeningTarget;
        }
        private int activeTab = 0;

        void Awake()
        {
            if (debugButtonTransforms) ToggleLayoutGroups(false);
        }

        private void OnEnable()
        {
            StartCoroutine(DisableLayoutGroups());
            Refresh();
        }

        private void OnDisable()
        {
            if (debugButtonTransforms)
            {
                ToggleLayoutGroups(true);
            }
        }

        IEnumerator Start()
        {
            yield return null;

            if(debugButtonTransforms) ToggleLayoutGroups(false);

            Refresh();
        }

        private void Refresh()
        {
            for (int i = 0; i < buttonNavs.Length; i++)
            {
                SetNavTarget(buttonNavs[i].button, 0);
                if (i == 0)
                {
                    buttonNavs[i].button.GetComponentInChildren<TextMeshProUGUI>().font = selectFont;

                    if (buttonNavs[i].tweeningTarget == null) continue;
                    buttonNavs[i].tweeningTarget.TargetObject.SetActive(true);
                    buttonNavs[i].tweeningTarget.TargetObject.GetComponent<CanvasGroup>().alpha = 1;
                    buttonNavs[i].tweeningTarget.isOpen = true;
                }
                else
                {
                    buttonNavs[i].button.GetComponentInChildren<TextMeshProUGUI>().font = normalFont;

                    if (buttonNavs[i].tweeningTarget == null) continue;
                    buttonNavs[i].tweeningTarget.TargetObject.SetActive(false);
                    buttonNavs[i].tweeningTarget.TargetObject.GetComponent<CanvasGroup>().alpha = 0;
                    buttonNavs[i].tweeningTarget.isOpen = false;
                }
                buttonNavs[i].button.GetComponentInChildren<TextMeshProUGUI>().ForceMeshUpdate();
            }
        }

        IEnumerator DisableLayoutGroups()
        {
            yield return null;
            if (debugButtonTransforms) ToggleLayoutGroups(false);
        }

        private void ToggleLayoutGroups(bool enable)
        {
            var contentFitter = GetComponent<ContentSizeFitter>();
            if (contentFitter != null) contentFitter.enabled = enable;

            var vertical = GetComponent<VerticalLayoutGroup>();
            if (vertical != null) vertical.enabled = enable;

            var horizontal = GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null) horizontal.enabled = enable;
        }

        public void SwitchTabs(int index)
        {
            int previousTab = activeTab;

            for (int i = 0; i < buttonNavs.Length; i++)
            {
                SetNavTarget(buttonNavs[i].button, index);
                if (i == index)
                {
                    buttonNavs[i].button.GetComponentInChildren<TextMeshProUGUI>().font = selectFont;

                    if (buttonNavs[i].tweeningTarget == null) continue;

                    if (previousTab <= index)
                        buttonNavs[i].tweeningTarget.TweenIn();
                    else
                        buttonNavs[i].tweeningTarget.InverseTweenIn();
                }
                else
                {
                    buttonNavs[i].button.GetComponentInChildren<TextMeshProUGUI>().font = normalFont;

                    if (buttonNavs[i].tweeningTarget == null) continue;

                    if (i > index)
                        buttonNavs[i].tweeningTarget.TweenOut();
                    else
                        buttonNavs[i].tweeningTarget.InverseTweenOut();
                }
            }
            activeTab = index;
        }


        private void SetNavTarget(Button modifiedButton, int activeButtonIdx)
        {
            var nav = modifiedButton.navigation;

            if (buttonNavs[activeButtonIdx].selectOnUp != null)
            {
                nav.selectOnUp = buttonNavs[activeButtonIdx].selectOnUp;
            }
            if (buttonNavs[activeButtonIdx].selectOnDown != null)
            {
                nav.selectOnDown = buttonNavs[activeButtonIdx].selectOnDown;
            }
            if (buttonNavs[activeButtonIdx].selectOnRight != null)
            {
                nav.selectOnRight = buttonNavs[activeButtonIdx].selectOnRight;
            }
            if (buttonNavs[activeButtonIdx].selectOnLeft != null)
            {
                nav.selectOnLeft = buttonNavs[activeButtonIdx].selectOnLeft;
            }
            modifiedButton.navigation = nav;
        }
    }   
}
