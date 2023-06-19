using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Latex
{
    [ExecuteAlways]
    [AddComponentMenu("Primer / LaTeX")]
    public partial class LatexComponent : MonoBehaviour
    {
        public const string PREFAB_NAME = "LaTeX";


        [Button(ButtonSizes.Large)]
        private void CopyCode()
        {
            GUIUtility.systemCopyBuffer = $".Process(@\"{latex}\")";
        }
    }
}
