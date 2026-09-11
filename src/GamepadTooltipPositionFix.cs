using System;
using System.Linq.Expressions;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace GamepadTooltipPositionFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class GamepadTooltipPositionFixPlugin : BaseUnityPlugin
    {
        // Keep the legacy GUID stable so existing BepInEx identity/configuration remains compatible.
        public const string PluginGuid = "nikich.gyk.movegamepadtooltips";
        public const string PluginName = "Gamepad Tooltip Position Fix";
        public const string PluginVersion = "1.3.0";

        private Harmony _harmony;

        private void Awake()
        {
            try
            {
                RuntimeBindings.Initialize();
                _harmony = new Harmony(PluginGuid);
                _harmony.PatchAll();
                Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize {PluginName}: {ex}");
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    internal static class RuntimeBindings
    {
        internal static MethodBase BubbleUpdateMethod { get; private set; }
        internal static Func<bool> ForGamepad { get; private set; }
        internal static Func<object> GuiElementsMe { get; private set; }
        internal static Func<object, Component> Inventory { get; private set; }
        internal static Func<object, Component> TechTree { get; private set; }
        internal static Func<object, object> Widget { get; private set; }
        internal static Func<object, int> WidgetWidth { get; private set; }
        internal static Func<object, int> WidgetHeight { get; private set; }

        internal static void Initialize()
        {
            var baseGuiType = RequireType("BaseGUI");
            var guiElementsType = RequireType("GUIElements");
            var bubbleType = RequireType("WidgetsBubbleGUI");

            BubbleUpdateMethod = AccessTools.Method(bubbleType, "Update")
                ?? throw new MissingMethodException(bubbleType.FullName, "Update");

            ForGamepad = CompileStaticGetter<bool>(baseGuiType, "for_gamepad");
            GuiElementsMe = CompileStaticGetter<object>(guiElementsType, "me");
            Inventory = CompileInstanceGetter<Component>(guiElementsType, "inventory");
            TechTree = CompileInstanceGetter<Component>(guiElementsType, "tech_tree");

            var widgetMember = RequireMember(bubbleType, "widget");
            var widgetType = GetMemberType(widgetMember);
            Widget = CompileInstanceGetter<object>(bubbleType, widgetMember);
            WidgetWidth = CompileInstanceGetter<int>(widgetType, "width");
            WidgetHeight = CompileInstanceGetter<int>(widgetType, "height");
        }

        private static Type RequireType(string name)
        {
            return AccessTools.TypeByName(name)
                ?? throw new TypeLoadException($"Could not find game type '{name}'.");
        }

        private static MemberInfo RequireMember(Type type, string name)
        {
            return (MemberInfo)AccessTools.Property(type, name)
                ?? AccessTools.Field(type, name)
                ?? throw new MissingMemberException(type.FullName, name);
        }

        private static Type GetMemberType(MemberInfo member)
        {
            if (member is PropertyInfo property)
                return property.PropertyType;
            if (member is FieldInfo field)
                return field.FieldType;
            throw new NotSupportedException($"Unsupported member type: {member.MemberType}");
        }

        private static Func<T> CompileStaticGetter<T>(Type type, string name)
        {
            var member = RequireMember(type, name);
            Expression access;

            if (member is PropertyInfo property)
                access = Expression.Property(null, property);
            else if (member is FieldInfo field)
                access = Expression.Field(null, field);
            else
                throw new NotSupportedException();

            return Expression.Lambda<Func<T>>(Expression.Convert(access, typeof(T))).Compile();
        }

        private static Func<object, T> CompileInstanceGetter<T>(Type type, string name)
        {
            return CompileInstanceGetter<T>(type, RequireMember(type, name));
        }

        private static Func<object, T> CompileInstanceGetter<T>(Type type, MemberInfo member)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var typedInstance = Expression.Convert(instance, type);
            Expression access;

            if (member is PropertyInfo property)
                access = Expression.Property(typedInstance, property);
            else if (member is FieldInfo field)
                access = Expression.Field(typedInstance, field);
            else
                throw new NotSupportedException();

            return Expression.Lambda<Func<object, T>>(
                Expression.Convert(access, typeof(T)),
                instance).Compile();
        }
    }

    [HarmonyPatch]
    internal static class WidgetsBubbleGuiUpdatePatch
    {
        // Accepted legacy 1.2.0 visual placement; public 1.3.0 intentionally preserves it.
        private const float LeftEdge = -586f;
        private const float BottomEdge = -312f;

        private static MethodBase TargetMethod()
        {
            return RuntimeBindings.BubbleUpdateMethod;
        }

        [HarmonyPostfix]
        private static void Postfix(object __instance)
        {
            if (!RuntimeBindings.ForGamepad())
                return;

            var gui = RuntimeBindings.GuiElementsMe();
            if (gui == null)
                return;

            var inventory = RuntimeBindings.Inventory(gui);
            var techTree = RuntimeBindings.TechTree(gui);
            var inventoryOpen = inventory != null && inventory.gameObject.activeInHierarchy;
            var techTreeOpen = techTree != null && techTree.gameObject.activeInHierarchy;

            if (!inventoryOpen && !techTreeOpen)
                return;

            var component = __instance as Component;
            var widget = RuntimeBindings.Widget(__instance);
            if (component == null || widget == null)
                return;

            component.transform.localPosition = new Vector3(
                LeftEdge + RuntimeBindings.WidgetWidth(widget) / 2f,
                BottomEdge + RuntimeBindings.WidgetHeight(widget) / 2f,
                0f);
        }
    }
}
