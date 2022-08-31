using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PrimerText : PrimerObject
{
    public bool billboard;
    public TextMeshPro tmpro;
    public bool flipped;

    double animateableNumber = 0;
    public double AnimateableNumber {
        get{ return animateableNumber; }
        set{
            animateableNumber = value; 
            tmpro.text = FormatDouble(value);
        }
    }
    public Color Color {
        get { return this.tmpro.color; }
        set { this.tmpro.color = value; }
    }
    protected override void Awake() {
        base.Awake();
        //This script assumes it's on a prefab with a TextMeshPro component
        tmpro = GetComponent<TextMeshPro>();
    }
    void Update()
    {
        if (billboard) {
            transform.rotation = Quaternion.LookRotation(
                transform.position - Camera.main.transform.position, 
                transform.parent.up
            ) * (flipped ? Quaternion.Euler(0, 180, 0) : Quaternion.identity);
        }
    }
    // Making this a method in case I want to add different modes
    string FormatDouble(double numToFormat) {
        // return numToFormat.ToString("{0:n0}");
        return string.Format("{0:n0}", numToFormat);
    }
    public void AddToNumber(float numAdded) {
        AnimateableNumber += numAdded;
    }
    public override void ChangeColor(Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        AnimateValue<Color>("Color", newColor, duration: duration, ease: ease);
    }
    public override IEnumerator tempColorChange(Color newColor, float duration, float attack, float decay, EaseMode ease)
    {
        Color oldColor = this.Color;
        ChangeColor(newColor, duration: attack, ease: ease);
        yield return new WaitForSeconds(duration - attack - decay);
        ChangeColor(oldColor, duration: decay, ease: ease);
    }
}
