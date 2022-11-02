using System.Threading;
using System.Threading.Tasks;
using Primer;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TextMeshPro))]
public class PrimerText2 : PrimerBehaviour
{
    public bool billboard;
    public bool flipped;
    public Vector3 orientation = Vector3.forward;

    #region Aliases
    TextMeshPro meshCache;
    TextMeshPro mesh => meshCache ? meshCache : meshCache = GetComponent<TextMeshPro>();

    Camera cameraCache;
    Camera mainCamera => cameraCache ? cameraCache : cameraCache = Camera.main;

    public string text
    {
        get => mesh.text;
        set => mesh.text = value;
    }

    public TextAlignmentOptions alignment
    {
        get => mesh.alignment;
        set => mesh.alignment = value;
    }

    public Color Color
    {
        get => mesh.color;
        set => mesh.color = value;
    }
    #endregion

    float value;
    public float Value
    {
        get => value;
        set {
            this.value = value;
            mesh.text = Presentation.FormatNumber(value);
        }
    }

    void Update() {
        if (!billboard) {
            transform.LookAt(transform.position + orientation);
            return;
        }

        transform.rotation = Quaternion.LookRotation(
            transform.position - mainCamera.transform.position,
            transform.parent.up
        );

        if (flipped) {
            transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    public async void TweenColor(CancellationToken ct, Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        // If component is already destroyed, skip
        if (!this) return;

        await foreach (var color in PrimerAnimation.Tween(ct , Color, newColor, duration, ease)) {
            if (!ct.IsCancellationRequested) {
                Color = color;
            }
        }
    }

    public async Task tempColorChange(CancellationToken ct, Color newColor, float duration, float attack, float decay, EaseMode ease) {
        var originalColor = Color;
        var delay = (duration - attack - decay) * 1000;
        TweenColor(ct, newColor, attack, ease);
        await Task.Delay((int)delay, ct);
        TweenColor(ct, originalColor, decay, ease);
    }
}
