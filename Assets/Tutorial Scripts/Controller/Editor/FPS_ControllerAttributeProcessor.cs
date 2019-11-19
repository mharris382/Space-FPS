using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace SA
{
    public class FPS_ControllerAttributeProcessor : Sirenix.OdinInspector.Editor.OdinAttributeProcessor<FPS_Controller>
    {
        static string[] movesettings = new string[]
        {
            "velocityDownSpeed",
            "movementSpeed",
            "torqueSpeed",
            "rotationSpeed",
            "movementDownWeapon",
            "aimSpeed",
            "normalMovementSpeed"
        };
        static string[] dependencies = new string[]
        {
            "tiltTransform",
            "gunParent",
            "pivotTransform"
        };
        static string[] general = new string[]
        {
            "isLocal"
        };
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            if (movesettings.Contains(member.Name))
            {
                attributes.Add(new BoxGroupAttribute("Move Settings", order: 2));
            }
            else if (dependencies.Contains(member.Name))
            {
                attributes.Add(new FoldoutGroupAttribute("Dependencies",false, order:1));
                attributes.Add(new RequiredAttribute());
            }
            else if (general.Contains(member.Name))
            {
                attributes.Add(new FoldoutGroupAttribute("general", true, order: 0)); 
            }
            else
            {
                attributes.Add(new BoxGroupAttribute("Gun Settings", order:3));
            }


            base.ProcessChildMemberAttributes(parentProperty, member, attributes);
        }
    }
}
