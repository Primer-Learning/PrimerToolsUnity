using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class LatexComponent : MonoBehaviour
    {
        [SerializeReference] private LatexCliIntegration integration = new();
        [SerializeReference] private LatexGroups groups = new();
        [SerializeReference] private new LatexRenderer renderer = new();

        [Title("Events")]
        public UnityEvent<LatexExpression> onChange = new();

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
            if (isEnabled) {
                await integration.Process(input);
                return;
            }

            OnEnable();
            await integration.Process(input);
            OnDisable();
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

        public UniTask TweenColor(Color newColor, Tweener anim = null, CancellationToken ct = default)
        {
            return renderer.TweenColor(newColor, anim, ct);
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

        public async UniTask<LatexScrubbable> TransitionTo(LatexComponent to, IEnumerable<TransitionType> transitions, Tweener anim = null,
            CancellationToken ct = default)
        {
            var scrubbable = CreateTransition(to, transitions.ToArray());
            await scrubbable.Play(anim, ct);
            return scrubbable;
        }


        #region Internals
        internal bool isEnabled = false;

        internal void OnEnable()
        {
            if (isEnabled)
                return;

            isEnabled = true;
            groups.onChange += EmitOnChange;
            integration.onChange += EmitOnChange;

            groups.expression = integration.expression;
            renderer.transform = transform;
            renderer.latexGroups = groups;

            integration.Refresh();
        }

        internal void OnDisable()
        {
            if (!isEnabled)
                return;

            isEnabled = false;
            groups.onChange -= EmitOnChange;
            integration.onChange -= EmitOnChange;
        }

        private void OnDrawGizmos()
        {
            renderer.DrawGizmos();
        }

        private void EmitOnChange()
        {
            groups.expression = integration.expression;
            renderer.UpdateChildren();
            onChange?.Invoke(expression);
        }

        internal static Material defaultMaterial;
        private void Reset()
        {
            defaultMaterial ??= AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");

            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (renderer.material is null || presets.All(preset => preset.excludedProperties.Contains("material"))) {
                renderer.material = defaultMaterial;
            }
        }

        #endregion
    }
}
