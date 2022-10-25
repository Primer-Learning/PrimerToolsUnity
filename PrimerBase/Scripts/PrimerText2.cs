using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Primer;

[ExecuteInEditMode]
[RequireComponent(typeof(TextMeshPro))]
public class PrimerText2 : PrimerBehaviour
{
    public bool billboard;
    public bool flipped;
    public Transform invertRotation;
    Quaternion lastRotationInverted;

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
            mesh.text = FormatDouble(value);
        }
    }

    void Update() {
        if (invertRotation && lastRotationInverted != invertRotation.rotation) {
            lastRotationInverted = invertRotation.rotation;
            transform.localRotation = Quaternion.Inverse(invertRotation.rotation);
        }

        if (!billboard) return;

        transform.rotation = Quaternion.LookRotation(
            transform.position - mainCamera.transform.position,
            transform.parent.up
        );

        if (flipped) {
            transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    public async void TweenColor(Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        await foreach (var color in tween(Color, newColor, duration, ease)) {
            Color = color;
        }
    }

    public async Task tempColorChange(Color newColor, float duration, float attack, float decay, EaseMode ease) {
        var originalColor = Color;
        var delay = (duration - attack - decay) * 1000;
        TweenColor(newColor, attack, ease);
        await Task.Delay((int)delay);
        TweenColor(originalColor, decay, ease);
    }

    static string FormatDouble(float numToFormat) {
        return $"{numToFormat:n0}";
    }
}
