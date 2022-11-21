using System;
using Primer;
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

            text.transform.localPosition = new Vector3(0f, -distance, 0f);
            text.transform.localScale = Vector3.Scale(
                text.transform.localScale,
                transform.parent.localScale
            );

            value = data.value;
            label = data.label;
        }
    }
}
