using System;
using System.Reflection;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
    [UsedImplicitly]
    public class CopyToClipboardDrawer : MethodDrawer
    {
        protected override bool CanDrawMethodProperty(InspectorProperty property)
        {
            return property.GetAttribute<CopyToClipboardAttribute>() is not null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            const SdfIconType ICON = SdfIconType.Clipboard;
            const float SIZE = (float)ButtonSizes.Medium;

            var attribute = Property.GetAttribute<CopyToClipboardAttribute>();
            var text = attribute.label ?? label.text.Replace("To", "Copy");
            var rect = OdinBullshitToGetRect(text, SIZE);

            if (SirenixEditorGUI.SDFIconButton(rect, label, ICON))
                Invoke();
        }

        private void Invoke()
        {
            // try {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();

                var memberInfo = Property.Info.GetMemberInfo() as MethodInfo;
                var delegateInfo = Property.Info.GetMethodDelegate();
                var methodInfo = memberInfo ?? delegateInfo.Method;

                if (methodInfo.IsGenericMethodDefinition) {
                    Debug.LogError("Cannot invoke a generic method definition.");
                    return;
                }

                if (methodInfo.ReturnType != typeof(string)) {
                    Debug.LogError($"Cannot [CopyCode] on a method that does not return a string: {methodInfo.GetFullName()}");
                    return;
                }

                var result = memberInfo is null
                    ? InvokeDelegate(delegateInfo)
                    : InvokeMethodInfo(methodInfo, Property.Parent);

                CopyToClipboard(result);
            // }
            // finally {
            //     GUIHelper.RequestRepaint();
            // }
        }

        private static object InvokeDelegate(Delegate delegateInfo)
        {
            try {
                return delegateInfo.DynamicInvoke();
            }
            catch (Exception ex) {
                HandleException(ex);
                return null;
            }
        }

        private static object InvokeMethodInfo(MethodInfo methodInfo, InspectorProperty parent)
        {
            if (parent == null && !methodInfo.IsStatic)
                return null;

            try {
                return methodInfo.IsStatic
                    ? methodInfo.Invoke(null, Array.Empty<object>())
                    : methodInfo.Invoke(parent, Array.Empty<object>());
            }
            catch (Exception ex) {
                HandleException(ex);
                return null;
            }
        }

        private static void HandleException(Exception ex)
        {
            if (ex.IsExitGUIException())
                throw ex.AsExitGUIException();

            Debug.LogException(ex);
        }

        private static void CopyToClipboard(object result)
        {
            if (result is not string str) {
                Debug.LogError($"Cannot copy to clipboard, result is not a string: {result}");
                return;
            }

            GUIUtility.systemCopyBuffer = str;
        }

        private static Rect OdinBullshitToGetRect(string text, float buttonHeight)
        {
            throw new NotImplementedException();

            // FIXME: Thanks Odin for breaking this

            // SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(
            //     text,
            //     null,
            //     hasIcon: true,
            //     buttonHeight
            //     out var num2,
            //     out var num3,
            //     out var num4,
            //     out var minWidth
            // );
            //
            // var rect = GUILayoutUtility.GetRect(0.0f, buttonHeight, GUILayout.MinWidth(minWidth));
            // return EditorGUI.IndentedRect(rect);
        }
    }
}
