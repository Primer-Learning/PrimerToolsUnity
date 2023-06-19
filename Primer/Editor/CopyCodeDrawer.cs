using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
  public class CopyCodeDrawer : OdinAttributeDrawer<CopyCodeAttribute>
  {
    protected override void DrawPropertyLayout(GUIContent label)
    {
      var text = Attribute.label;
      var buttonHeight = (float)Attribute.size;
      var icon = Attribute.icon;

      SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(
        text,
        null,
        hasIcon: true,
        buttonHeight,
        out var num2,
        out var num3,
        out var num4,
        out var minWidth
      );

      var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(0.0f, buttonHeight, GUILayout.MinWidth(minWidth)));

      if (SirenixEditorGUI.SDFIconButton(rect, label, icon))
        Invoke();

      CallNextDrawer(label);
    }

    private void Invoke()
    {
      try {
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

        if (memberInfo is null)
          InvokeDelegate(delegateInfo);
        else
          InvokeMethodInfo(methodInfo);
      }
      finally {
        GUIHelper.ExitGUI(true);
      }

      var code = Property.ValueEntry.WeakSmartValue.ToString();
      GUIUtility.systemCopyBuffer = code;

    }

    private void InvokeDelegate(Delegate delegateInfo)
    {
      try {
        var result = delegateInfo.DynamicInvoke();
        CopyToClipboard(result);
      }
      catch (Exception ex) {
        if (ex.IsExitGUIException())
          throw ex.AsExitGUIException();

        Debug.LogException(ex);
      }
    }

    private void InvokeMethodInfo(MethodInfo methodInfo)
    {
      var parentValues = Property.ParentValues;

      foreach (var (index, obj) in parentValues.WithIndex()) {
        if (obj == null && !methodInfo.IsStatic)
          return;

        try {
          var result = !methodInfo.IsStatic
            ? methodInfo.Invoke(obj, Array.Empty<object>())
            : methodInfo.Invoke(null, Array.Empty<object>());

          CopyToClipboard(result);
        }
        catch (Exception ex) {
          if (ex.IsExitGUIException())
            throw ex.AsExitGUIException();

          Debug.LogException(ex);
        }
      }
    }

    private static void CopyToClipboard(object result)
    {
      if (result is not string str) {
        Debug.LogError("Cannot copy to clipboard, result is not a string!");
        return;
      }

      GUIUtility.systemCopyBuffer = str;
    }
  }
}
