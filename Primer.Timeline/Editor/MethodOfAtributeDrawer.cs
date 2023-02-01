using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    public class MethodOfAttributeDrawer : OdinAttributeDrawer<MethodOfAttribute>
    {
        private static readonly string[] ignoreMethods = {
            "IsInvoking",
            "CancelInvoke",
            "StopAllCoroutines",
            "GetComponent",
            "GetComponentInChildren",
            "GetComponentsInChildren",
            "GetComponentInParent",
            "GetComponentsInParent",
            "GetComponents",
            "GetInstanceID",
            "GetHashCode",
            "ToString",
            "GetType",
            "Equals",
        };

        private ValueResolver<object> targetResolver;
        private InspectorProperty methodName;


        protected override void Initialize()
        {
            if (Property.ValueEntry.TypeOfValue != typeof(MethodInvocation)) {
                throw new Exception(
                    $"{nameof(MethodOfAttribute)} can only be used on {nameof(MethodInvocation)} properties"
                );
            }

            targetResolver = ValueResolver.Get<object>(Property, Attribute.sourceProp);
            methodName = Property.Children["methodName"];
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (targetResolver is null) {
                EditorGUILayout.LabelField(label, $"This property needs to be of type {nameof(MethodInvocation)}");
                return;
            }

            ValueResolver.DrawErrors(targetResolver);

            var target = targetResolver.GetValue();

            if (target is null)
                throw new Exception($"Source for {nameof(MethodOfAttribute)} is null");

            var methods = Attribute.Filter(GetMethodsOf(target.GetType()));
            var options = methods.Select(x => x.Name).ToArray();

            EditorGUI.BeginDisabledGroup(options.Length == 1);
            var labels = methods.Select(x => x.Print()).ToArray();
            var selected = (string)methodName.ValueEntry.WeakSmartValue;
            var index = Mathf.Clamp(Array.IndexOf(options, selected), 0, options.Length);
            var newIndex = SirenixEditorFields.Dropdown(label, index, labels);
            methodName.ValueEntry.WeakSmartValue = options[newIndex];
            EditorGUI.EndDisabledGroup();
        }

        private static MethodInfo[] GetMethodsOf(Type targetType)
        {
            return targetType.GetMethods()
                .Where(x => !x.Name.StartsWith("get_") && !x.Name.StartsWith("set_") && !ignoreMethods.Contains(x.Name))
                .ToArray();
        }
    }
}
