using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Latex
{
    [ExecuteAlways]
    [AddComponentMenu("Primer / LaTeX")]
    public partial class LatexComponent : MonoBehaviour
    {
        public const string PREFAB_NAME = "LaTeX";

        #region public LatexExpression expression;
        private LatexExpression _expression;

        public LatexExpression expression {
            get => _expression;
            private set {
                _expression = value;
                UpdateChildren();
                onExpressionChange?.Invoke();
            }
        }
        #endregion

        public Action onExpressionChange;


        [SerializeField]
        [ChildGameObjectsOnly]
        private Transform activeDisplay;

        public void ResetActiveDisplay()
        {
            SetActiveDisplay(charactersContainer);
        }

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


        [CopyToClipboard]
        public string ToCode()
        {
            return  $".AddLatex(@\"{latex}\", \"{name}\")";
        }
    }
}
