using System.Collections;
using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

public class PrimerShapesLine : PrimerObject
{
    [SerializeField] internal Shapes.Line line = null;
    internal float animatedOffsetSpeed = 0;

    void Update() {
        line.DashOffset += animatedOffsetSpeed;
    }
    public Vector3 Start {
        get {
            return line.Start;
        }
        set {
            line.Start = value;
        }
    }
    public Vector3 End {
        get {
            return line.End;
        }
        set {
            line.End = value;
        }
    }
    private float _thickness;
    public float Thickness {
        get { return _thickness; }
        set {
            _thickness = value;
            line.Thickness = value;
        }
    }
    protected override void Awake() {
        base.Awake();
        if (line == null) {
            line = gameObject.AddComponent<Shapes.Line>();
        }
        if (gameObject.name == "New Game Object") {
            gameObject.name = "Line";
        }
    }
    internal void MovePoints(Vector3 newStart, Vector3 newEnd, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        this.AnimateValue<Vector3>("Start", newStart, duration: duration, ease: ease);
        this.AnimateValue<Vector3>("End", newEnd, duration: duration, ease: ease);
    }
    internal void DrawFromStart(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        Vector3 actualEnd = End;
        End = Start;
        MovePoints(Start, actualEnd, duration: duration, ease: ease);
    }
    internal void AnimateThickness(float newThickness, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        this.AnimateValue<float>("Thickness", newThickness, duration: duration, ease: ease);
    }
}
