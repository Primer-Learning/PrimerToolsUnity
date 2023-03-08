using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Events;

namespace Primer.Latex
{
    [ExecuteAlways]
    [AddComponentMenu("Primer / LaTeX")]
    public class LatexComponent : MonoBehaviour, IMeshRendererController
    {
        [SerializeReference] private LatexCliIntegration integration = new();
        [SerializeReference] private LatexGroups groups = new();
        [SerializeReference] private new LatexRenderer renderer = new();

        [Title("Events")]
        public UnityEvent<LatexExpression> onChange = new();

        MeshRenderer[] IMeshRendererController.meshRenderers => GetComponentsInChildren<MeshRenderer>();

        // Processing

        public string latex {
            get => integration.latex;
            set => Process(value);
        }

        public List<string> headers {
            get => integration.headers;
            set => Process(new LatexInput(latex, value)).Forget();
        }

        public LatexInput config {
            get => integration.config;
            set => Process(value).Forget();
        }

        public LatexExpression expression => integration.expression;

        public UniTask Process(int input) => Process($"${input}$");
        public UniTask Process(float input) => Process($"${input}$");
        public UniTask Process(string input) => Process(new LatexInput(input, headers));

        public async UniTask Process(LatexInput input)
        {
            await integration.Process(input);
        }

        // Groups

        public int groupsCount => groups.ranges.Count;

        public void SetGroupIndexes(params int[] indexes)
        {
            groups.SetGroupIndexes(indexes);
        }

        public void SetGroupLengths(params int[] lengths)
        {
            groups.SetGroupLengths(lengths);
        }

        // Rendering

        public Color color {
            get => renderer.color;
            set => renderer.color = value;
        }

        public Material material {
            get => renderer.material;
            set => renderer.material = value;
        }

        public void SetColorInGroups(Color groupColor, params int[] groupIndexes)
        {
            renderer.SetColorInGroups(groupColor, groupIndexes);
        }

        public Tween TweenColorInGroups(Color groupColor, IEnumerable<int> groupIndexes)
        {
            return renderer.TweenColorInGroups(groupColor, groupIndexes);
        }

        // Transitions

        public LatexScrubbable CreateTransition(LatexComponent transitionTo, params TransitionType[] transitions)
        {
            return new LatexScrubbable {
                from = this,
                to = transitionTo,
                groups = transitions,
            };
        }

        public async UniTask<LatexScrubbable> TransitionTo(LatexComponent to, IEnumerable<TransitionType> transitions)
        {
            var scrubbable = CreateTransition(to, transitions.ToArray());
            await scrubbable.Play(anim, ct);
            return scrubbable;
        }


        #region Unity events
        private void Reset()
        {
            ConnectParts();
            PatchMaterial();
        }

        // ConnectParts is critical for the component to function.
        // it's idempotent so it can be called several times without issue.
        // but if it's not called, the component will not function.
        private void Awake() => ConnectParts();
        private void OnEnable() => ConnectParts();
        private void OnDrawGizmos() => renderer.DrawGizmos();
        #endregion


        #region Internals
        public void ConnectParts()
        {
            groups.getExpression = () => integration.expression;
            renderer.transform = transform;
            renderer.latexGroups = groups;

            if (groups.onChange != null && integration.onChange != null)
                return;

            groups.onChange += EmitOnChange;
            integration.onChange += EmitOnChange;

            integration.Refresh();
        }

        // private void DisconnectParts()
        // {
        //     groups.onChange -= EmitOnChange;
        //     integration.onChange -= EmitOnChange;
        // }

        private void EmitOnChange()
        {
            renderer.UpdateChildren();
            onChange?.Invoke(expression);
        }

        private void PatchMaterial()
        {
            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (renderer.material is null || presets.All(preset => preset.excludedProperties.Contains("material"))) {
                renderer.material = IMeshRendererController.defaultMaterial;
            }
        }

        [ButtonGroup("Children group")]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.ArrowRepeat)]
        [ContextMenu("PRIMER > Update children")]
        internal void UpdateChildren()
        {
            renderer.UpdateChildren();
        }

        [ButtonGroup("Children group")]
        [Button(ButtonSizes.Medium, Icon = SdfIconType.Trash)]
        [ContextMenu("PRIMER > Regenerate children")]
        protected virtual void RegenerateChildren()
        {
            if (gameObject.IsPreset())
                return;

            ChildrenDeclaration.Clear(transform);
            UpdateChildren();
        }
        #endregion
    }
}
