using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UITemplateMaker
{
    public sealed class UITemplateMakerWindow : EditorWindow
    {
        public enum VisualMode { ColorPalette, SpriteImage }
        public enum GroupAlignment { Left, Center, Right }
        [SerializeField] private GroupAlignment alignment = GroupAlignment.Center;
        [SerializeField] private VisualMode visualMode;
        [SerializeField] private Sprite defaultSprite, activeSprite;
        [SerializeField] private bool useSpriteTransition;
        [SerializeField] private UnityEngine.UI.ColorBlock transitionColors = UnityEngine.UI.ColorBlock.defaultColorBlock;
        [SerializeField] private UnityEngine.UI.SpriteState transitionSprites;
        [SerializeField] private Sprite normalTransitionSprite;
        private bool exclusive = true, allowEmpty, vertical;
        private int count = 3, initialSelection;
        [SerializeField] private List<string> buttonLabels = new List<string> { "Option A", "Option B", "Option C" };
        [SerializeField] private List<string> toggleNames = new List<string> { "Toggle 1", "Toggle 2", "Toggle 3" };
        [SerializeField] private bool useButtonLabels = true;
        [SerializeField] private float buttonWidth = 200f, buttonHeight = 52f;
        private Color defaultColor = new Color32(42, 48, 62, 255);
        private Color activeColor = new Color32(39, 118, 173, 255);
        private Vector2 scroll;

        [MenuItem("Tools/UI Template Maker")]
        public static void Open() => GetWindow<UITemplateMakerWindow>("UI Template Maker");

        [MenuItem("GameObject/UI Templates/Toggle Group - Independent", false, 10)]
        private static void Independent(MenuCommand command) => QuickCreate(command, false);

        [MenuItem("GameObject/UI Templates/Toggle Group - Exclusive", false, 11)]
        private static void Exclusive(MenuCommand command) => QuickCreate(command, true);

        private static void QuickCreate(MenuCommand command, bool exclusive)
        {
            Create(command.context as GameObject ?? Selection.activeGameObject, exclusive, false,
                false, 3, new[] { "Option A", "Option B", "Option C" }, exclusive ? 0 : -1,
                new Color32(42, 48, 62, 255), new Color32(39, 118, 173, 255));
        }

        [SerializeField] private bool showLayout = true, showToggles = true, showVisual = true, showTransition;

        private void OnEnable()
        {
            minSize = new Vector2(360, 400);
        }

        private void OnGUI()
        {
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Clamp(position.width * .36f, 120f, 170f);
            try
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.Space(4);
                        EditorGUILayout.LabelField("Toggle Button Group", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField("Create a ready-to-edit uGUI hierarchy.", EditorStyles.miniLabel);
                        EditorGUILayout.Space(4);
                    }
                    using (var view = new EditorGUILayout.ScrollViewScope(scroll))
                    {
                        scroll = view.scrollPosition;
                        using (new EditorGUILayout.VerticalScope())
                        {
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                showLayout = DrawSectionHeader(showLayout, "Layout & Selection");
                                if (showLayout) DrawLayout();
                            }
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                showToggles = DrawSectionHeader(showToggles, "Toggles (" + count + ")");
                                if (showToggles) DrawToggles();
                            }
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                showVisual = DrawSectionHeader(showVisual, "Visual  /  " + (visualMode == VisualMode.ColorPalette ? "Color Palette" : "Sprite Image"));
                                if (showVisual) DrawVisual();
                            }
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                showTransition = DrawSectionHeader(showTransition, "Transition  /  " + (useSpriteTransition ? "Sprite Swap" : "Color Tint"));
                                if (showTransition) DrawTransition();
                            }
                        }
                    }
                    DrawFooter();
                }
            }
            finally { EditorGUIUtility.labelWidth = previousLabelWidth; }
        }

        private static bool DrawSectionHeader(bool expanded, string title)
        {
            // foldoutHeader is intended for the header-group control. Ordinary
            // Foldout applies separate label rendering, leaving a partial backdrop.
            bool result = EditorGUILayout.BeginFoldoutHeaderGroup(expanded, title);
            EditorGUILayout.EndFoldoutHeaderGroup();
            return result;
        }

        private void EnsureEntries()
        {
            // Keep hidden entries when the count is reduced.
            while (buttonLabels.Count < count) buttonLabels.Add("Option " + (buttonLabels.Count + 1));
            while (toggleNames.Count < count) toggleNames.Add("Toggle " + (toggleNames.Count + 1));
        }

        private void DrawLayout()
        {
            EditorGUILayout.Space(6);
            exclusive = EditorGUILayout.Popup("Selection", exclusive ? 1 : 0,
                new[] { "Independent (multi select)", "Exclusive (single select)" }) == 1;
            if (exclusive) allowEmpty = EditorGUILayout.Toggle("Allow Switch Off", allowEmpty);
            vertical = EditorGUILayout.Popup("Direction", vertical ? 1 : 0, new[] { "Horizontal", "Vertical" }) == 1;
            alignment = (GroupAlignment)EditorGUILayout.Popup("Alignment", (int)alignment,
                vertical ? new[] { "Top", "Center", "Bottom" } : new[] { "Left", "Center", "Right" });
            count = EditorGUILayout.IntSlider("Toggle Count", count, 1, 20);
            buttonWidth = PositiveSize(EditorGUILayout.FloatField("Width", buttonWidth), 200f);
            buttonHeight = PositiveSize(EditorGUILayout.FloatField("Height", buttonHeight), 52f);
            EnsureEntries();
            bool canBeEmpty = !exclusive || allowEmpty;
            int offset = canBeEmpty ? 1 : 0;
            var options = new string[count + offset];
            if (canBeEmpty) options[0] = "None";
            for (int i = 0; i < count; i++)
                options[i + offset] = (i + 1) + ". " + (string.IsNullOrWhiteSpace(toggleNames[i]) ? "Toggle " + (i + 1) : toggleNames[i]);
            initialSelection = Mathf.Clamp(initialSelection, canBeEmpty ? -1 : 0, count - 1);
            initialSelection = EditorGUILayout.Popup("Initially Active", initialSelection + offset, options) - offset;
            EditorGUILayout.Space(4);
        }

        private void DrawToggles()
        {
            EditorGUILayout.Space(6);
            useButtonLabels = EditorGUILayout.Toggle("Use Button Labels", useButtonLabels);
            EnsureEntries();
            for (int i = 0; i < count; i++)
            {
                EditorGUILayout.Space(4);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Toggle " + (i + 1), EditorStyles.miniBoldLabel);
                    toggleNames[i] = EditorGUILayout.TextField("Object Name", toggleNames[i]);
                    if (useButtonLabels)
                        buttonLabels[i] = EditorGUILayout.TextField("Label Text", buttonLabels[i]);
                }
            }
            EditorGUILayout.Space(4);
        }

        private void DrawVisual()
        {
            EditorGUILayout.Space(6);
            visualMode = (VisualMode)EditorGUILayout.Popup("Mode", (int)visualMode, new[] { "Color Palette", "Sprite Image" });
            if (visualMode == VisualMode.ColorPalette)
            {
                defaultColor = EditorGUILayout.ColorField("Default Color", defaultColor);
                activeColor = EditorGUILayout.ColorField("Active Color", activeColor);
            }
            else
            {
                defaultSprite = SpriteField("Default Sprite", defaultSprite);
                activeSprite = SpriteField("Active Sprite", activeSprite);
            }
            EditorGUILayout.Space(4);
        }

        private void DrawTransition()
        {
            EditorGUILayout.Space(6);
            useSpriteTransition = EditorGUILayout.Popup("Mode", useSpriteTransition ? 1 : 0, new[] { "Color Tint", "Sprite Swap" }) == 1;
            if (useSpriteTransition)
            {
                normalTransitionSprite = SpriteField("Normal Sprite", normalTransitionSprite);
                transitionSprites.highlightedSprite = SpriteField("Highlighted Sprite", transitionSprites.highlightedSprite);
                transitionSprites.pressedSprite = SpriteField("Pressed Sprite", transitionSprites.pressedSprite);
                transitionSprites.selectedSprite = SpriteField("Selected Sprite", transitionSprites.selectedSprite);
                transitionSprites.disabledSprite = SpriteField("Disabled Sprite", transitionSprites.disabledSprite);
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox("Empty states use the normal sprite. Normal can use Default Sprite from Visual. Selected means focus; Active follows Is On.", MessageType.None);
            }
            else
            {
                transitionColors.normalColor = EditorGUILayout.ColorField("Normal", transitionColors.normalColor);
                transitionColors.highlightedColor = EditorGUILayout.ColorField("Highlighted", transitionColors.highlightedColor);
                transitionColors.pressedColor = EditorGUILayout.ColorField("Pressed", transitionColors.pressedColor);
                transitionColors.selectedColor = EditorGUILayout.ColorField("Selected", transitionColors.selectedColor);
                transitionColors.disabledColor = EditorGUILayout.ColorField("Disabled", transitionColors.disabledColor);
                EditorGUILayout.Space(4);
                transitionColors.colorMultiplier = EditorGUILayout.Slider("Multiplier", transitionColors.colorMultiplier, 1, 5);
                transitionColors.fadeDuration = Mathf.Max(0, EditorGUILayout.FloatField("Fade Duration", transitionColors.fadeDuration));
            }
            EditorGUILayout.Space(4);
        }

        private void DrawFooter()
        {
            // Validate outside foldouts so collapsed settings cannot bypass validation.
            bool missingVisual = visualMode == VisualMode.SpriteImage && (defaultSprite == null || activeSprite == null);
            bool missingNormal = useSpriteTransition && normalTransitionSprite == null && (visualMode != VisualMode.SpriteImage || defaultSprite == null);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (missingVisual) EditorGUILayout.HelpBox("Visual: assign Default Sprite and Active Sprite.", MessageType.Warning);
                if (missingNormal) EditorGUILayout.HelpBox("Transition: assign Normal Sprite or use a Default Sprite in Visual.", MessageType.Warning);
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorGUILayout.HelpBox("Exit Play mode to create a template.", MessageType.Info);
                var selected = Selection.activeGameObject;
                string destination = selected != null && selected.transform is RectTransform && selected.GetComponentInParent<Canvas>() != null
                    ? selected.name : "Canvas (reuse or create)";
                EditorGUILayout.LabelField(new GUIContent("Parent: " + destination, destination), EditorStyles.miniLabel);
                using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || missingVisual || missingNormal))
                    if (GUILayout.Button("Create Toggle Button Group", GUILayout.Height(34)))
                    {
                        EnsureEntries();
                        Create(Selection.activeGameObject, exclusive, allowEmpty, vertical, count,
                            buttonLabels.ToArray(), initialSelection, defaultColor, activeColor,
                            toggleNames.ToArray(), useButtonLabels, buttonWidth, buttonHeight,
                            visualMode, defaultSprite, activeSprite, useSpriteTransition,
                            transitionColors, transitionSprites, normalTransitionSprite, alignment);
                    }
            }
        }

        public static GameObject Create(GameObject selected, bool exclusive, bool allowEmpty,
            bool vertical, int count, string[] labels, int initial, Color idle, Color active,
            string[] names = null, bool useLabels = true, float width = 200f, float height = 52f,
            VisualMode visualMode = VisualMode.ColorPalette, Sprite defaultSprite = null, Sprite activeSprite = null,
            bool spriteTransition = false, UnityEngine.UI.ColorBlock? colors = null,
            UnityEngine.UI.SpriteState? sprites = null, Sprite normalSprite = null,
            GroupAlignment alignment = GroupAlignment.Center)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Create templates in Edit mode.");
            if (visualMode == VisualMode.SpriteImage && (defaultSprite == null || activeSprite == null))
                throw new ArgumentException("Assign both Default Sprite and Active Sprite.");
            if (spriteTransition && normalSprite == null && (visualMode != VisualMode.SpriteImage || defaultSprite == null))
                throw new ArgumentException("Assign a Normal Sprite for Sprite Swap.");
            TMP_FontAsset font = null;
            if (useLabels)
            {
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                if (font == null) font = TMP_Settings.defaultFontAsset;
                if (font == null)
                    throw new InvalidOperationException("A TMP font is required. Import TMP Essential Resources before creating a template.");
            }
            count = Mathf.Clamp(count, 1, 20);
            width = PositiveSize(width, 200f);
            height = PositiveSize(height, 52f);
            initial = Mathf.Clamp(initial, exclusive && !allowEmpty ? 0 : -1, count - 1);
            Undo.IncrementCurrentGroup();
            int undo = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Toggle Button Group");
            try
            {
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                Transform parent = selected != null && selected.transform is RectTransform && selected.GetComponentInParent<Canvas>() != null ? selected.transform : null;
                if (stage != null && parent == null)
                    throw new InvalidOperationException("Select an object under a Canvas in this prefab, or create the template in a scene.");
                if (parent == null)
                {
                    // Reuse only an active root Canvas in the active scene.
                    foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                        if (canvas.isRootCanvas && canvas.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                        { parent = canvas.transform; break; }
                }
                if (parent == null)
                {
                    GameObject canvas = NewUI("Canvas", null, typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
                    canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = .5f;
                    parent = canvas.transform;
                }
                Canvas owner = parent.GetComponentInParent<Canvas>().rootCanvas;
                if (owner.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                    Undo.AddComponent<UnityEngine.UI.GraphicRaycaster>(owner.gameObject);
                if (stage == null) EnsureEventSystem(parent.gameObject.scene);

                GameObject root = NewUI(exclusive ? "ToggleGroup Exclusive" : "ToggleGroup Independent", parent);
                GameObjectUtility.EnsureUniqueNameForSibling(root);
                var rect = (RectTransform)root.transform;
                float horizontalAnchor = alignment == GroupAlignment.Left ? 0f : alignment == GroupAlignment.Right ? 1f : .5f;
                Vector2 groupAnchor = vertical ? new Vector2(.5f, 1f - horizontalAnchor) : new Vector2(horizontalAnchor, .5f);
                rect.anchorMin = rect.anchorMax = groupAnchor;
                rect.pivot = groupAnchor;
                rect.sizeDelta = vertical ? new Vector2(width, count * height + (count - 1) * 12) : new Vector2(count * width + (count - 1) * 12, height);
                var fitter = root.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                fitter.horizontalFit = vertical ? UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained : UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = vertical ? UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize : UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
                var layout = vertical
                    ? (UnityEngine.UI.HorizontalOrVerticalLayoutGroup)root.AddComponent<UnityEngine.UI.VerticalLayoutGroup>()
                    : root.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                layout.spacing = 12;
                layout.childAlignment = vertical
                    ? (alignment == GroupAlignment.Left ? TextAnchor.UpperCenter
                        : alignment == GroupAlignment.Right ? TextAnchor.LowerCenter : TextAnchor.MiddleCenter)
                    : (alignment == GroupAlignment.Left ? TextAnchor.MiddleLeft
                        : alignment == GroupAlignment.Right ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter);
                layout.childControlWidth = layout.childControlHeight = false;
                layout.childForceExpandWidth = layout.childForceExpandHeight = false;
                UnityEngine.UI.ToggleGroup group = null;
                if (exclusive)
                {
                    group = root.AddComponent<UnityEngine.UI.ToggleGroup>();
                    group.allowSwitchOff = allowEmpty;
                }
                for (int i = 0; i < count; i++)
                {
                    string label = labels != null && i < labels.Length && !string.IsNullOrWhiteSpace(labels[i]) ? labels[i].Trim() : "Option " + (i + 1);
                    string toggleName = names != null && i < names.Length && !string.IsNullOrWhiteSpace(names[i]) ? names[i].Trim() : "Toggle " + (i + 1);
                    GameObject button = NewUI(toggleName, root.transform, typeof(UnityEngine.UI.Toggle), typeof(UnityEngine.UI.LayoutElement));
                    ((RectTransform)button.transform).sizeDelta = new Vector2(width, height);
                    var size = button.GetComponent<UnityEngine.UI.LayoutElement>();
                    size.minWidth = size.preferredWidth = width;
                    size.minHeight = size.preferredHeight = height;
                    GameObject background = NewUI("Background", button.transform, typeof(UnityEngine.UI.Image));
                    Stretch((RectTransform)background.transform);
                    var image = background.GetComponent<UnityEngine.UI.Image>();
                    ConfigureImage(image, visualMode, defaultSprite, idle);
                    if (spriteTransition)
                    {
                        image.sprite = normalSprite != null ? normalSprite : defaultSprite;
                        image.color = Color.white;
                        image.type = image.sprite.border != Vector4.zero ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
                    }
                    GameObject indicator = NewUI("Active", background.transform, typeof(UnityEngine.UI.Image));
                    Stretch((RectTransform)indicator.transform);
                    var activeImage = indicator.GetComponent<UnityEngine.UI.Image>();
                    ConfigureImage(activeImage, visualMode, activeSprite, active);
                    activeImage.raycastTarget = false;
                    if (useLabels)
                    {
                        GameObject text = NewUI("Label", button.transform, typeof(TextMeshProUGUI));
                        Stretch((RectTransform)text.transform, 12);
                        var tmp = text.GetComponent<TextMeshProUGUI>();
                        tmp.font = font;
                        tmp.text = label;
                        tmp.fontSize = 20;
                        tmp.color = Color.white;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.raycastTarget = false;
                    }
                    var toggle = button.GetComponent<UnityEngine.UI.Toggle>();
                    toggle.targetGraphic = image;
                    toggle.transition = spriteTransition ? UnityEngine.UI.Selectable.Transition.SpriteSwap : UnityEngine.UI.Selectable.Transition.ColorTint;
                    toggle.colors = colors ?? UnityEngine.UI.ColorBlock.defaultColorBlock;
                    toggle.spriteState = sprites ?? new UnityEngine.UI.SpriteState();
                    toggle.graphic = activeImage;
                    toggle.toggleTransition = UnityEngine.UI.Toggle.ToggleTransition.None;
                    toggle.SetIsOnWithoutNotify(i == initial);
                    toggle.group = group;
                }
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                Selection.activeGameObject = root;
                EditorGUIUtility.PingObject(root);
                EditorSceneManager.MarkSceneDirty(root.scene);
                return root;
            }
            catch
            {
                Undo.RevertAllDownToGroup(undo);
                throw;
            }
            finally { Undo.CollapseUndoOperations(undo); }
        }

        private static Sprite SpriteField(string label, Sprite sprite)
            => (Sprite)EditorGUILayout.ObjectField(label, sprite, typeof(Sprite), false);

        private static void ConfigureImage(UnityEngine.UI.Image image, VisualMode mode, Sprite sprite, Color color)
        {
            image.color = mode == VisualMode.SpriteImage ? Color.white : color;
            image.sprite = mode == VisualMode.SpriteImage ? sprite : null;
            image.type = image.sprite != null && image.sprite.border != Vector4.zero
                ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        }

        private static float PositiveSize(float value, float fallback)
            => float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Max(1f, value);

        private static GameObject NewUI(string name, Transform parent, params Type[] components)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            if (parent != null) obj.layer = parent.gameObject.layer;
            else obj.layer = LayerMask.NameToLayer("UI");
            foreach (Type component in components) obj.AddComponent(component);
            Undo.RegisterCreatedObjectUndo(obj, "Create UI template");
            return obj;
        }

        private static void Stretch(RectTransform rect, float inset = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * inset;
            rect.offsetMax = Vector2.one * -inset;
        }

        private static void EnsureEventSystem(UnityEngine.SceneManagement.Scene scene)
        {
            // An EventSystem in another loaded scene already handles this UI too.
            if (UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None).Length > 0) return;
            GameObject events = new GameObject("EventSystem", typeof(EventSystem));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(events, scene);
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
            Undo.RegisterCreatedObjectUndo(events, "Create EventSystem");
        }
    }
}
