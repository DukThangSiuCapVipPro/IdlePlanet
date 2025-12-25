using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PopupSystem
{
    public class PopupManager : MonoBehaviour
    {
        #region Popup Events

        public delegate void PopupEvent(BasePopup popup);
        public event PopupEvent EvtPopupOpen;
        public event PopupEvent EvtPopupClose;
        public Action EventAllPopupClose;
        public event PopupEvent EvtTouchOverlay;

        #endregion
        public Canvas canvas;
        public Canvas effectCanvas;
        public bool usingDefaultTransparent = true;
        public BasePopup[] prefabs;
        public Image transparent;
        private Transform mTransparentTrans;
        public Stack<BasePopup> popupStacks = new Stack<BasePopup>();
        public Transform parent;
        private int defaultSortingOrder;
        private static PopupManager mInstance;
        private Queue<BasePopup> popupQueue = new Queue<BasePopup>();
        public bool hasPopupShowing;
        BasePopup currentPopup;
        public static PopupManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    Debug.LogError("Popup Manager is null!");
                    mInstance = LoadResource<PopupManager>("PopupManager");
                }

                return mInstance;
            }
        }

        public static bool Exits => mInstance;

        void Awake()
        {
            if (!InitializeSingleton())
            {
                return;
            }

            InitializeComponents();
        }

        private bool InitializeSingleton()
        {
            if (mInstance != null && mInstance != this)
            {
                Destroy(gameObject);
                return false;
            }

            mInstance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }

        private void InitializeComponents()
        {
            if (transparent != null)
            {
                mTransparentTrans = transparent.transform;
            }

            if (canvas != null)
            {
                defaultSortingOrder = canvas.sortingOrder;
            }
        }

        private void Start()
        {
            EvtPopupClose += HandlePopupClose;
            hasPopupShowing = false;
        }

        private void Update()
        {
            if (canvas.worldCamera && effectCanvas.worldCamera) return;
            var main = Camera.main;
            canvas.worldCamera = main;
            effectCanvas.worldCamera = main;
        }

        private void OnDestroy()
        {
            EvtPopupClose -= HandlePopupClose;
        }

        public static T CreateNewInstance<T>()
        {
            T result = Instance.CheckInstancePopupPrebab<T>();
            return result;
        }

        public T CheckInstancePopupPrebab<T>()
        {
            System.Type type = typeof(T);
            GameObject go = null;
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (IsOfType<T>(prefabs[i]))
                {
                    go = (GameObject)Instantiate(prefabs[i].gameObject, parent);
                    break;
                }
            }

            T result = go.GetComponent<T>();
            return result;
        }

        private bool IsOfType<T>(object value)
        {
            return value is T;
        }

        public void ChangeTransparentOrder(Transform topPopupTransform, bool active)
        {
            if (active)
            {
                mTransparentTrans.SetSiblingIndex(topPopupTransform.GetSiblingIndex() - 1);
                if (usingDefaultTransparent)
                {
                    ShowFade();
                }
                else
                {
                    HideFade();
                }
                hasPopupShowing = true;
            }
            else
            {
                if (parent.childCount >= 2)
                {
                    mTransparentTrans.SetSiblingIndex(parent.childCount - 2);
                    hasPopupShowing = true;
                    currentPopup = parent.GetChild(parent.childCount - 1).GetComponent<BasePopup>();
                    if (currentPopup != null)
                        currentPopup.OnTop();
                }
                else
                {
                    HideFade();
                    hasPopupShowing = false;
                }
            }
        }

        public PopupManager Preload()
        {
            return mInstance;
        }

        public bool SequenceHidePopup()
        {
            if (popupStacks.Count > 0)
                popupStacks.Peek().Hide();
            else
            {
                HideFade();
                hasPopupShowing = false;
            }

            return (popupStacks.Count > 0);
        }

        public void CloseAllPopup()
        {
            while (popupStacks.Count > 0)
            {
                BasePopup popup = popupStacks.Pop();
                if (popup != null)
                {
                    popup.ForceHide();
                }
            }
            foreach (Transform child in parent)
            {
                if (child.gameObject != transparent.gameObject)
                    Destroy(child.gameObject);
            }
            hasPopupShowing = false;
            ResetOrder();
            HideFade();
        }

        public static T LoadResource<T>(string name)
        {
            GameObject go = (GameObject)GameObject.Instantiate(Resources.Load(name));
            go.name = $"[{name}]";
            DontDestroyOnLoad(go);
            return go.GetComponent<T>();
        }

        public void SetSortingOrder(int order)
        {
            canvas.sortingOrder = order;
        }

        public void ResetOrder()
        {
            canvas.sortingOrder = defaultSortingOrder;
        }

        public void OderPopup(BasePopup popup)
        {
            if (!hasPopupShowing)
            {
                popup.ActivePopup();
            }
            else
            {
                popup.gameObject.SetActive(false);
                popupQueue.Enqueue(popup);
            }
        }

        public void OnClickOverlay()
        {
            EvtTouchOverlay?.Invoke(popupStacks.Peek());
        }

        public bool GetHasPopUp()
        {
            return hasPopupShowing;
        }

        public BasePopup GetCurrentPopup()
        {
            return currentPopup;
        }

        public int TopPopupIndex()
        {
            return popupStacks.Count + 1;
        }

        #region Event Methods

        public void OnPopupOpen(BasePopup popup)
        {
            currentPopup = popup;
            if (currentPopup != null)
                currentPopup.OnTop();
            EvtPopupOpen?.Invoke(popup);
        }

        public void OnPopupClose(BasePopup popup)
        {
            if (popupStacks.Count == 0)
            {
                hasPopupShowing = false;
                EventAllPopupClose?.Invoke();
            }
            EvtPopupClose?.Invoke(popup);
        }

        #endregion

        #region Handle Events

        private void HandlePopupClose(BasePopup popup)
        {
            if (popupStacks.Count == 0 && popupQueue.Count > 0)
            {
                BasePopup nextPopup = popupQueue.Dequeue();
                nextPopup.gameObject.SetActive(true);
                nextPopup.ActivePopup();
            }
        }

        #endregion

        #region Tween

        public void ShowFade()
        {
            transparent.gameObject.SetActive(true);
            //transparent.DOFade(transparentAmount, fadeTweenTime).SetEase(fadeInTweenType);
        }

        public void HideFade()
        {
            //transparent.DOFade(0, fadeTweenTime).SetEase(fadeOutTweenType).OnComplete(() =>
            //{
            //    transparent.gameObject.SetActive(false);
            //});
            transparent.gameObject.SetActive(false);
        }

        public void DisableFadeBackground()
        {
            transparent.gameObject.SetActive(false);
        }

        public void EnableFadeBackground()
        {
            transparent.gameObject.SetActive(true);
        }

        #endregion
    }
}