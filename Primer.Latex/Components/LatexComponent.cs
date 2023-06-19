using UnityEngine;

namespace Primer.Latex
{
    [ExecuteAlways]
    [AddComponentMenu("Primer / LaTeX")]
    public partial class LatexComponent : MonoBehaviour
    {
        public const string PREFAB_NAME = "LaTeX";

        [SerializeField] private Transform activeDisplay;

        public void SetActiveDisplay(Transform root)
        {
            if (root.parent != transform) {
                Debug.LogError("LatexComponent's active display must be a direct child of the LatexComponent");
                return;
            }

            if (activeDisplay == root) return;

            foreach (var child in transform.GetChildren()) {
                child.SetActive(child == root);
            }

            activeDisplay = root;
        }


        [CopyCode]
        public string ToCode()
        {
            return $".AddLatex(@\"{latex}\", \"{name}\")";
        }
    }
}
