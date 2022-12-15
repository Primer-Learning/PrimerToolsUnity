using System;
using Primer.Animation;
using UnityEngine;

namespace Primer.Graph
{
    public class Tic2 : PrimerBehaviour
    {
        public float value;

        public string label
        {
            get => text.text;
            set => text.text = value;
        }

        PrimerText2 text;

        public void Initialize(PrimerText2 primerTextPrefab, TicData data, float distance) {
            if (text is not null) {
                throw new Exception($"Tic {value} has already been initialized");
            }

            text = Instantiate(primerTextPrefab, transform);
            var textTransform = text.transform;

            textTransform.localPosition = new Vector3(0f, -distance, 0f);
            textTransform.localScale = Vector3.Scale(
                textTransform.localScale,
                transform.parent.localScale
            );

            value = data.value;
            label = data.label;
        }
    }
}
