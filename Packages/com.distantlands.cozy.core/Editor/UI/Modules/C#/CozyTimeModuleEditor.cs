using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using DistantLands.Cozy.Data;

namespace DistantLands.Cozy.EditorScripts
{
    [CustomEditor(typeof(CozyTimeModule))]
    public class CozyTimeModuleEditor : CozyModuleEditor
    {

        CozyTimeModule timeModule;
        public override ModuleCategory Category => ModuleCategory.time;
        public override string ModuleTitle => "Time";
        public override string ModuleSubtitle => "Time Management Module";
        public override string ModuleTooltip => "Setup time settings, simple calendars, and manage current settings.";
        public VisualElement ProfileContainer => root.Q<VisualElement>("profile-container");
        public VisualElement Container => root.Q<VisualElement>("current-settings-container");
        VisualElement root;

        void OnEnable()
        {
            if (!target)
                return;

            timeModule = (CozyTimeModule)target;
        }

        public override Button DisplayWidget()
        {
            Button widget = LargeWidget();
            Label status = widget.Q<Label>("dynamic-status");
            status.text = timeModule.currentTime;
            VisualElement lowerContainer = widget.Q<VisualElement>("lower-container");

            lowerContainer.Add(new Label()
            {
                text = $"Currently it is {timeModule.currentTime.ToString()}"
            });
            Label dayYearLabel = new Label()
            {
                text = $"Day {timeModule.currentDay} of year {timeModule.currentYear}"
            };
            lowerContainer.Add(dayYearLabel);

            return widget;

        }

        public override VisualElement DisplayUI()
        {
            root = new VisualElement();

            VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.distantlands.cozy.core/Editor/UI/Modules/UXML/time-module-editor.uxml"
            );

            asset.CloneTree(root);

            CozyProfileField<PerennialProfile> profile = new CozyProfileField<PerennialProfile>(serializedObject.FindProperty("perennialProfile"));
            ProfileContainer.Add(profile);

            PropertyField dayPercentage = new PropertyField();
            dayPercentage.BindProperty(serializedObject.FindProperty("currentTime"));
            dayPercentage.label = "Time";
            Container.Add(dayPercentage);

            SliderInt currentDay = new SliderInt("Day", 0, timeModule.DaysPerYear, SliderDirection.Horizontal, 0);
            currentDay.showInputField = true;
            currentDay.AddToClassList("unity-base-field__aligned");
            currentDay.BindProperty(serializedObject.FindProperty("currentDay"));
            Container.Add(currentDay);

            PropertyField currentYear = new PropertyField();
            currentYear.BindProperty(serializedObject.FindProperty("currentYear"));
            Container.Add(currentYear);

            InspectorElement inspector = new InspectorElement(timeModule.perennialProfile);
            inspector.AddToClassList("p-0");
            root.Add(inspector);
            inspector.RegisterCallback<PointerMoveEvent>((PointerMoveEvent) =>
            {
                currentDay.highValue = timeModule.DaysPerYear;
            });

            return root;

        }

        public void SetTime(object time)
        {
            timeModule.currentTime = (float)time;
        }

        public override void AddContextMenuItems(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Set Time to Morning"), false, SetTime, 0.25f);
            menu.AddItem(new GUIContent("Set Time to Day"), false, SetTime, 0.5f);
            menu.AddItem(new GUIContent("Set Time to Evening"), false, SetTime, 0.75f);
            menu.AddItem(new GUIContent("Set Time to Night"), false, SetTime, 0f);
            menu.AddSeparator("");
        }

        public override void OpenDocumentationURL()
        {
            Application.OpenURL("https://distant-lands.gitbook.io/cozy-stylized-weather-documentation/how-it-works/modules/time-module");
        }


    }
}