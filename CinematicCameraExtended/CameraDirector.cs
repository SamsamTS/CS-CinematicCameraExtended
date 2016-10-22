using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CinematicCameraExtended
{
    public class CameraDirector : MonoBehaviour
    {
        public GameObject movieUI;

        public CameraPath cameraPath;

        public Camera uiCamera;

        public static EventSystem eventSystem;

        private void Start()
        {
            base.StartCoroutine(this.ConstructUI());
        }

        private void OnDestroy()
        {
            if (this.movieUI)
            {
                UnityEngine.Object.Destroy(this.movieUI);
                this.movieUI = null;
            }
            if (CameraDirector.eventSystem)
            {
                UnityEngine.Object.Destroy(CameraDirector.eventSystem.gameObject);
                CameraDirector.eventSystem = null;
            }
        }

        private void Update()
        {
            LoadingManager arg_05_0 = Singleton<LoadingManager>.instance;
            if (this.movieUI)
            {
                this.movieUI.SetActive(!this.uiCamera.enabled && !this.cameraPath.playBack);
            }
        }

        public static string FindAssetBundlePath()
        {
            string result = "Unable to find asset bundle path.";
            string strB = "409073879";
            PluginManager instance = Singleton<PluginManager>.instance;
            foreach (PluginManager.PluginInfo current in instance.GetPluginsInfo())
            {
                if (current.name.CompareTo(strB) == 0)
                {
                    result = "file:///" + current.modPath + "/";
                }
            }
            return result;
        }

        private System.Collections.IEnumerator ConstructUI()
        {
            string text = CameraDirector.FindAssetBundlePath();
            WWW wWW = new WWW(text + "moviecameraui");
            yield return wWW;
            AssetBundle assetBundle = wWW.assetBundle;
            if (assetBundle)
            {
                AssetBundleRequest assetBundleRequest = assetBundle.LoadAllAssetsAsync<Sprite>();
                yield return assetBundleRequest;
                if (assetBundleRequest.allAssets.Length == 2)
                {
                    Sprite uiImage = assetBundleRequest.allAssets[0] as Sprite;
                    Sprite knobImage = assetBundleRequest.allAssets[1] as Sprite;
                    Font[] array = Resources.FindObjectsOfTypeAll<Font>();
                    Font font = array[array.Length - 1];
                    Font[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        Font font2 = array2[i];
                        if (font2.name.CompareTo("OpenSans-Regular") == 0)
                        {
                            font = font2;
                        }
                    }
                    this.uiCamera = UIView.GetAView().uiCamera;
                    if (this.uiCamera)
                    {
                        this.cameraPath = CameraDirector.CreateCameraPath();
                        Canvas canvas = CameraDirector.CreateCanvas();
                        this.movieUI = canvas.gameObject;
                        CameraDirector.CreateUIEventSystem();
                        CameraDirector.CreateButtons(canvas.transform, this.cameraPath, uiImage, knobImage, font);
                    }
                    else
                    {
                        Debug.Log("CameraDirector: failed to localte ui camera.");
                    }
                }
                else
                {
                    Debug.Log("CameraDirector: failed to load assets from asset bundle at path: " + text);
                }
            }
            else
            {
                Debug.Log("CameraDirector: failed to load asset bundle from path: " + text);
            }
            yield break;
        }

        public static CameraPath CreateCameraPath()
        {
            GameObject gameObject = new GameObject("CinematicCameraExtended");
            CameraPath cameraPath = gameObject.AddComponent<CameraPath>();
            Transform transform = GameObject.FindGameObjectWithTag("MainCamera").transform;
            cameraPath.cameraTransform = transform;
            return cameraPath;
        }

        public static Canvas CreateCanvas()
        {
            GameObject gameObject = new GameObject("MovieCameraUI");
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.gameObject.layer = 5;
            canvas.renderMode = 0;
            CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void CreateUIEventSystem()
        {
            CameraDirector.eventSystem = null;
            CameraDirector.eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (!CameraDirector.eventSystem)
            {
                Debug.Log("CinematicCameraExtended: Creating UI event system.");
                GameObject gameObject = new GameObject("MovieCameraEventSystem");
                CameraDirector.eventSystem = gameObject.AddComponent<EventSystem>();
                gameObject.AddComponent<StandaloneInputModule>();
                gameObject.AddComponent<TouchInputModule>();
                return;
            }
            Debug.Log("CinematicCameraExtended: existing UI event system found, trying to use existing (this may cause issues).");
        }

        public static void CreateButtons(Transform canvas, CameraPath cameraPath, Sprite uiImage, Sprite knobImage, Font font)
        {
            CameraDirector.CreateButton("AddKnot", canvas, uiImage, font, "+", new Vector2(-169f, -128f), new Vector2(20f, 14f), Vector2.one, Vector2.one);
            CameraDirector.CreateButton("Play", canvas, uiImage, font, ">", new Vector2(-25f, -128f), new Vector2(20f, 14f), Vector2.one, Vector2.one);
            Transform parent = CameraDirector.CreatePanel(canvas, uiImage);
            CameraDirector.CreateSlider(canvas, uiImage, knobImage);
            Transform transform = CameraDirector.CreateListItem("Position 0", parent, uiImage, font);
            transform.gameObject.SetActive(false);
            CameraDirector.PopulateWithComponents(canvas.gameObject, cameraPath, transform.gameObject);
        }

        public static void ClearFocus()
        {
            CameraDirector.eventSystem.SetSelectedGameObject(null);
        }

        public static GameObject SelectedUIGameObject()
        {
            return CameraDirector.eventSystem.currentSelectedGameObject;
        }

        public static Button CreateButton(string name, Transform parent, Sprite sprite, Font font, string labelText, Vector2 position, Vector2 size, Vector2 anchorMax, Vector2 anchorMin)
        {
            Rect rect = default(Rect);
            GameObject gameObject = new GameObject(name);
            gameObject.layer = 5;
            gameObject.transform.SetParent(parent);
            gameObject.AddComponent<CanvasRenderer>();
            Image image = gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            Button result = gameObject.AddComponent<Button>();
            rect.Set(-17f, -128f, 20f, 14f);
            RectTransform component = gameObject.GetComponent<RectTransform>();
            component.anchoredPosition = position;
            component.sizeDelta = size;
            component.anchorMax = anchorMax;
            component.anchorMin = anchorMin;
            GameObject gameObject2 = new GameObject("Text");
            gameObject2.layer = 5;
            gameObject2.transform.SetParent(gameObject.transform);
            gameObject2.AddComponent<CanvasRenderer>();
            Text text = gameObject2.AddComponent<Text>();
            text.text = labelText;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;
            text.fontSize = 8;
            component = text.GetComponent<RectTransform>();
            component.anchorMax = Vector2.one;
            component.anchorMin = Vector2.zero;
            component.anchoredPosition = Vector2.zero;
            component.sizeDelta = Vector2.zero;
            return result;
        }

        public static Transform CreatePanel(Transform canvas, Sprite uiImage)
        {
            GameObject gameObject = new GameObject("Panel");
            gameObject.layer = 5;
            gameObject.transform.SetParent(canvas);
            gameObject.AddComponent<CanvasRenderer>();
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.offsetMax = new Vector2(-15f, -138f);
            rectTransform.offsetMin = new Vector2(621f, -355f);
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            return gameObject.transform;
        }

        public static void CreateSlider(Transform canvas, Sprite uiImage, Sprite knobImage)
        {
            GameObject gameObject = new GameObject("TimelineSlider");
            gameObject.layer = 5;
            gameObject.transform.SetParent(canvas);
            Slider slider = gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(303f, -128f);
            rectTransform.sizeDelta = new Vector2(111f, 14f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            GameObject gameObject2 = new GameObject("Background");
            gameObject2.transform.SetParent(gameObject.transform);
            gameObject2.layer = 5;
            gameObject2.AddComponent<CanvasRenderer>();
            Image image = gameObject2.AddComponent<Image>();
            image.sprite = uiImage;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            rectTransform = gameObject2.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0.75f);
            rectTransform.anchorMin = new Vector2(0f, 0.25f);
            GameObject gameObject3 = new GameObject("Fill Area");
            gameObject3.transform.SetParent(gameObject.transform);
            gameObject3.layer = 5;
            rectTransform = gameObject3.AddComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1f, 0.75f);
            rectTransform.anchorMin = new Vector2(0f, 0.25f);
            rectTransform.offsetMin = new Vector2(5f, 0f);
            rectTransform.offsetMax = new Vector2(-15f, 0f);
            GameObject gameObject4 = new GameObject("Fill");
            gameObject4.transform.SetParent(gameObject3.transform);
            gameObject4.layer = 5;
            gameObject4.AddComponent<CanvasRenderer>();
            image = gameObject4.AddComponent<Image>();
            image.sprite = uiImage;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            rectTransform = gameObject4.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(10f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            GameObject gameObject5 = new GameObject("Handle Slide Area");
            gameObject5.transform.SetParent(gameObject.transform);
            gameObject5.layer = 5;
            rectTransform = gameObject5.AddComponent<RectTransform>();
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.offsetMin = new Vector2(10f, 0f);
            rectTransform.offsetMax = new Vector2(-10f, 0f);
            GameObject gameObject6 = new GameObject("Handle");
            gameObject6.transform.SetParent(gameObject5.transform);
            gameObject6.layer = 5;
            gameObject6.AddComponent<CanvasRenderer>();
            image = gameObject6.AddComponent<Image>();
            image.sprite = knobImage;
            image.type = Image.Type.Simple;
            rectTransform = gameObject6.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(20f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            slider.fillRect = gameObject4.GetComponent<RectTransform>();
            slider.handleRect = gameObject6.GetComponent<RectTransform>();
            slider.targetGraphic = gameObject6.GetComponent<Image>();
        }

        public static Transform CreateListItem(string name, Transform parent, Sprite sprite, Font font)
        {
            GameObject gameObject = new GameObject("ListItem");
            gameObject.layer = 5;
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(0f, -17f);
            rectTransform.offsetMax = new Vector2(0f, 0f);
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = Vector2.one;
            CameraDirector.CreateButton("Name", gameObject.transform, sprite, font, name, new Vector2(23f, -9f), new Vector2(44f, 14f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CameraDirector.CreateButton("Easing", gameObject.transform, sprite, font, "InOut", new Vector2(62f, -9f), new Vector2(34f, 14f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CameraDirector.CreateButton("Set", gameObject.transform, sprite, font, "O", new Vector2(-30f, -9f), new Vector2(20f, 14f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            CameraDirector.CreateButton("Remove", gameObject.transform, sprite, font, "X", new Vector2(-10f, -9f), new Vector2(20f, 14f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            CameraDirector.CreateInputField("Duration", "2", gameObject.transform, font, sprite, new Vector2(91f, -9f), new Vector2(22f, 14f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            CameraDirector.CreateInputField("Delay", "0", gameObject.transform, font, sprite, new Vector2(113f, -9f), new Vector2(22f, 14f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            return gameObject.transform;
        }

        public static InputField CreateInputField(string name, string value, Transform parent, Font font, Sprite uiImage, Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.layer = 5;
            gameObject.transform.SetParent(parent);
            gameObject.AddComponent<CanvasRenderer>();
            Image image = gameObject.AddComponent<Image>();
            image.sprite = uiImage;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            InputField inputField = gameObject.AddComponent<InputField>();
            inputField.targetGraphic = image;
            inputField.text = value;
            inputField.contentType = InputField.ContentType.DecimalNumber;
            RectTransform component = gameObject.GetComponent<RectTransform>();
            component.anchoredPosition = position;
            component.sizeDelta = size;
            component.anchorMax = anchorMax;
            component.anchorMin = anchorMin;
            GameObject gameObject2 = new GameObject("Text");
            gameObject2.layer = 5;
            gameObject2.transform.SetParent(gameObject.transform);
            gameObject2.AddComponent<CanvasRenderer>();
            Text text = gameObject2.AddComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = 8;
            text.alignment = TextAnchor.UpperCenter;
            inputField.textComponent = text;
            component = gameObject2.GetComponent<RectTransform>();
            component.anchoredPosition = new Vector2(0f, -3f);
            component.sizeDelta = new Vector2(0f, 0f);
            component.anchorMax = Vector2.one;
            component.anchorMin = Vector2.zero;
            return inputField;
        }

        public static void PopulateWithComponents(GameObject movieUI, CameraPath cameraPath, GameObject listItemPrefab)
        {
            AddKnotButton addKnotButton = movieUI.AddComponent<AddKnotButton>();
            addKnotButton.cameraTransform = cameraPath.cameraTransform;
            addKnotButton.button = CameraDirector.FindChildByName(movieUI.transform, "AddKnot").GetComponent<Button>();
            addKnotButton.path = cameraPath;
            addKnotButton.panel = CameraDirector.FindChildByName(movieUI.transform, "Panel");
            addKnotButton.listItemPrefab = listItemPrefab.transform;
            PlayButton playButton = movieUI.AddComponent<PlayButton>();
            playButton.button = CameraDirector.FindChildByName(movieUI.transform, "Play").GetComponent<Button>();
            playButton.path = cameraPath;
            TimeLineSlider timeLineSlider = movieUI.AddComponent<TimeLineSlider>();
            timeLineSlider.slider = CameraDirector.FindChildByName(movieUI.transform, "TimelineSlider").GetComponent<Slider>();
            timeLineSlider.cameraPath = cameraPath;
            addKnotButton.timelineSlider = timeLineSlider.slider;
        }

        public static Transform FindChildByName(Transform parent, string name)
        {
            Transform result = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.CompareTo(name) == 0)
                {
                    result = child;
                    break;
                }
            }
            return result;
        }
    }
}
