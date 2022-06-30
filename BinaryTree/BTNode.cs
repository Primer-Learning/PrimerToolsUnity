using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTNode : PrimerObject
{
    public Vector3 yOffset = new Vector3(0f, -2f, 0f);
    public Vector3 xOffset = new Vector3(1f, 0f, 0f);
    public float buffer = 1.75f;
    public float labelBuffer = 2f;
    public float labelLerp = 0.5f;
    public TMPro.TextAlignmentOptions labelAlign = TMPro.TextAlignmentOptions.Center;
    private BTNode left = null, right = null;
    public PrimerArrow arrow = null;
    public BTNode Parent = null;
    public Vector3 intendedPos;
    public PrimerText label;
    private void OnValidate()
    {
        PlaceArrow();
        if (left != null) {
            left.PlaceArrow();
        }
        if (right != null) {
            right.PlaceArrow();
        }
        if (label != null)
        {
            PlaceLabel();
            label.tmpro.alignment = labelAlign;
        }
    }

    public void PlaceArrow()
    {
        if (arrow != null)
        {
            Transform parent = arrow.transform.parent;
            arrow.transform.parent = null;
            float scale = transform.localScale.x * transform.parent.localScale.x;
            arrow.SetFromTo(Parent.transform.position, transform.position, buffer * scale, Parent.buffer * scale);
            arrow.transform.SetParent(parent);
            PlaceLabel();
        }
    }

    public void Label(PrimerText prefab)
    {
        label = Instantiate(prefab);
        label.transform.SetParent(transform.parent);
        label.tmpro.alignment = labelAlign;
        label.gameObject.SetActive(false);
    }

    public void PlaceLabel()
    {
        if (label != null)
        {
            label.transform.localPosition = CalcLabelPos();
            // Debug.Log(label.transform.position);
            label.tmpro.alignment = labelAlign;
        }
    }

    public void UpdateLabel(bool intended = false)
    {
        if (label != null)
            label.MoveTo(CalcLabelPos(intended));
    }

    private Vector3 CalcLabelPos(bool intended = false)
    {
        if (arrow != null)
        {
            Vector3 diff = transform.localPosition - Parent.transform.localPosition;
            diff.Normalize();
            diff = diff.x > 0 ? new Vector3(-diff.y, diff.x) : new Vector3(diff.y, -diff.x);
            // float scale = transform.localScale.x * transform.parent.localScale.x;
            if (intended)
                return Vector3.Lerp(intendedPos, Parent.intendedPos, labelLerp) + diff * labelBuffer;
            return Vector3.Lerp(transform.localPosition, Parent.transform.localPosition, labelLerp) + diff * labelBuffer;
            // // Vector3 diff = transform.position - Parent.transform.position;
            // // diff.Normalize();
            // // diff = diff.x > 0 ? new Vector3(-diff.y, diff.x) : new Vector3(diff.y, -diff.x);
            // // float scale = transform.localScale.x * transform.parent.localScale.x;
            // if (intended)
            //     return Vector3.Lerp(intendedPos, Parent.intendedPos, labelLerp);
            // return Vector3.Lerp(transform.localPosition, Parent.transform.localPosition, labelLerp);
        }
        return transform.localPosition;
    }
    private void setChild(BTNode value, int direction)
    {
        gameObject.SetActive(true);
        if (value != null)
        {
            value.Parent = this;
            //value.arrow.SetFromTo(this.transform.position, value.transform.position, 0.75f);
            //Debug.Log(this.transform.position);
            //Debug.Log(value.transform.position);
            //Debug.Log("");
            try
            {
                value.transform.Find("100 coin").gameObject.GetComponent<Renderer>().enabled = false;
            } catch { };
        }
    }

    public int SpaceNeeded(int direction, int height = -1)
    {
        BTNode next = null;
        if (direction == -1) next = Left;
        else if (direction == 1) next = Right;
        if (next == null || height == 0) return 0;
        return 1 + next.SpaceNeeded(-1, height - 1) + next.SpaceNeeded(1, height - 1);
    }

    public BTNode Left
    {
        get { return left; }
        set
        {
            left = value;
            setChild(value, -1);
        }
    }

    public BTNode SetLeft(BTNode left)
    {
        Left = left;
        return this;
    }

    public BTNode Right
    {
        get { return right; }
        set
        {
            right = value;
            setChild(value, 1);
        }
    }

    public BTNode SetRight(BTNode right)
    {
        Right = right;
        return this;
    }

    public void Display()
    {
        StartCoroutine(display());
    }

    IEnumerator display()
    {
        if (arrow != null)
        {
            arrow.gameObject.SetActive(true);
            arrow.ScaleUpFromZero();
        }
        if (label != null)
        {
            label.gameObject.SetActive(true);
            label.ScaleUpFromZero();
        }
        try
        {
            transform.Find("100 coin").gameObject.GetComponent<Renderer>().enabled = true;
        }
        catch { };
        ScaleUpFromZero();
        yield return new WaitForSeconds(0.5f);
    }

    public void Hide(float duration = 0.5f)
    {
        StartCoroutine(hide(duration));
    }

    IEnumerator hide(float delay = 0.5f)
    {
        ScaleDownToZero();
        if (arrow != null && arrow.isActiveAndEnabled)
        {
            arrow.ScaleDownToZero();
        }
        if (label != null && label.isActiveAndEnabled)
        {
            label.ScaleDownToZero();
        }
        // This is needed because the BinaryTree hide methods yield on this coroutine
        yield return new WaitForSeconds(delay);
    }
}
