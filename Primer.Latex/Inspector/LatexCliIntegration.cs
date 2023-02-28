using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Primer.Latex
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("LaTeX Processing")]
    internal class LatexCliIntegration
    {
        [SerializeField]
        [HideInInspector]
        internal string _latex;

#if UNITY_EDITOR
        private bool isIdle => !isProcessing && !isCancelling && !hasError;
        private bool isCancelling => processor.state == LatexProcessingState.Cancelled && isProcessing;
        private bool isProcessing => processor.state == LatexProcessingState.Processing;
        private bool hasError => processor.renderError != null;

        [InfoBox("Ok", VisibleIf = nameof(isIdle))]
        [InfoBox("Processing...", InfoMessageType.Warning, VisibleIf = nameof(isProcessing))]
        [InfoBox("Cancelling...", InfoMessageType.Warning, VisibleIf = nameof(isCancelling))]
        [InfoBox("@processor.renderError.Message", InfoMessageType.Error, VisibleIf = nameof(hasError))]
#endif

        [ShowInInspector]
        [HideLabel]
        [MultiLineProperty(10)]
        public string latex
        {
            get => _latex;
            set {
                _latex = value;
                Refresh();
            }
        }

        [SerializeField]
        [HideInInspector]
        internal List<string> _headers = LatexInput.GetDefaultHeaders();

        [ShowInInspector]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        public List<string> headers
        {
            get => _headers;
            set {
                _headers = value;
                Refresh();
            }
        }

        internal readonly LatexProcessor processor = LatexProcessor.GetInstance();

        [NonSerialized]
        internal LatexExpression expression;

        public Action onChange;

        public LatexInput config {
            get => new(_latex, _headers);
            set => Process(value).Forget();
        }

        public async void Refresh()
        {
            var newExpression = await Evaluate(config);
            ApplyExpression(newExpression);
        }

        public async UniTask Process(LatexInput input)
        {
            _latex = input.code;
            _headers = input.headers;
            var newExpression = await Evaluate(input);
            ApplyExpression(newExpression);
        }

        private async UniTask<LatexExpression> Evaluate(LatexInput input)
        {
            if (string.IsNullOrWhiteSpace(input.code))
                return new LatexExpression();

            try {
                return await processor.Process(input);
            }
            catch (OperationCanceledException) {
                Debug.LogWarning($"Removing queued LaTeX execution: {input.code}");
                return null;
            }
        }

        private bool ApplyExpression(LatexExpression newExpression)
        {
            if (newExpression is null || expression is not null && expression.IsSame(newExpression))
                return false;

            expression = newExpression;
            onChange?.Invoke();
// #if UNITY_EDITOR
//             GUIHelper.RequestRepaint();
// #endif
            return true;
        }
    }
}
