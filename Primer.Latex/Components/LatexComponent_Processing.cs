using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Latex
{
    public partial class LatexComponent
    {
        internal readonly LatexProcessor processor = LatexProcessor.GetInstance();

        public bool isProcessing => processor.state == LatexProcessingState.Processing;

        #region public LatexExpression expression;
        [SerializeField, HideInInspector]
        private LatexExpression _expression;
        public Action onExpressionChange;

        public LatexExpression expression {
            get => _expression;
            private set {
                Debug.Log("WTF");
                _expression = value;
                UpdateCharacters();
                onExpressionChange?.Invoke();
            }
        }
        #endregion

        #region public string latex;
        [SerializeField, HideInInspector]
        internal string _latex;

#if UNITY_EDITOR
        private bool isIdle => !isProcessing && !isCancelling && !hasError;
        private bool isCancelling => processor.state == LatexProcessingState.Cancelled && isProcessing;
        private bool hasError => processor.renderError != null;

        [Title("LaTeX Processing")]
        [InfoBox("Ok", VisibleIf = nameof(isIdle))]
        [InfoBox("Processing...", InfoMessageType.Warning, VisibleIf = nameof(isProcessing))]
        [InfoBox("Cancelling...", InfoMessageType.Warning, VisibleIf = nameof(isCancelling))]
        [InfoBox("@processor.renderError.Message", InfoMessageType.Error, VisibleIf = nameof(hasError))]
        // Attributes above are applied to `latex` below
#endif

        [ShowInInspector]
        [HideLabel]
        [MultiLineProperty(10)]
        public string latex
        {
            get => _latex;
            set => config = new LatexInput(value, _headers);
        }
        #endregion

        #region public List<string> headers;
        [SerializeField]
        [HideInInspector]
        internal List<string> _headers = LatexInput.GetDefaultHeaders();

        [ShowInInspector]
        [Tooltip(@"These will be inserted into the LaTeX template before \begin{document}.")]
        public List<string> headers
        {
            get => _headers;
            set => config = new LatexInput(_latex, value);
        }
        #endregion

        public LatexInput config {
            get => new(_latex, _headers);
            set => Process(value).Forget();
        }


        public UniTask Process(int input) => Process($"${input}$");
        public UniTask Process(float input) => Process($"${input}$");
        public UniTask Process(string input) => Process(new LatexInput(input, headers));

        public async UniTask Process(LatexInput input)
        {
            _latex = input.code;
            _headers = input.headers;

            var request = Evaluate(input);
            var newExpression = await PrimerTimeline.RegisterOperation(request);

            if (newExpression is not null && !expression.IsSame(newExpression))
                expression = newExpression;
        }

        private async UniTask<LatexExpression> Evaluate(LatexInput input)
        {
            if (string.IsNullOrWhiteSpace(input.code))
                return new LatexExpression(input);

            try {
                return await processor.Process(input);
            }
            catch (OperationCanceledException) {
                Debug.LogWarning($"Removing queued LaTeX execution: {input.code}");
                return null;
            }
        }
    }
}
