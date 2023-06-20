using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Primer.Editor
{
    /// <summary>
    ///   This class adds the necessary attributes to the members of IHierarchyManipulator
    ///   so that they look consistently in the inspector.
    /// </summary>
    [UsedImplicitly]
    public class HierarchyManipulatorProcessor : OdinAttributeProcessor<IHierarchyManipulator>
    {
        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
        {
            return member.Name
                is nameof(IHierarchyManipulator.UpdateChildren)
                or nameof(IHierarchyManipulator.RegenerateChildren);
        }

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name) {
                case nameof(IHierarchyManipulator.UpdateChildren):
                    UpdateChildrenAttributes(attributes);
                    break;

                case nameof(IHierarchyManipulator.RegenerateChildren):
                    RegenerateChildrenAttributes(attributes);
                    break;
            }
        }

        private static void UpdateChildrenAttributes(ICollection<Attribute> attributes)
        {
            attributes.Add(new PropertySpaceAttribute());
            attributes.Add(new PropertyOrderAttribute(1000));
            attributes.Add(new ButtonGroupAttribute("Children manipulation"));
            attributes.Add(new ButtonAttribute(ButtonSizes.Medium) { Icon = SdfIconType.ArrowRepeat });
        }

        private static void RegenerateChildrenAttributes(ICollection<Attribute> attributes)
        {
            attributes.Add(new PropertyOrderAttribute(1001));
            attributes.Add(new ButtonGroupAttribute("Children manipulation"));
            attributes.Add(new ButtonAttribute(ButtonSizes.Medium) { Icon = SdfIconType.Trash });
        }
    }
}
