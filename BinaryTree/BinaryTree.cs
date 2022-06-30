using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BinaryTree : PrimerObject
{
    public BTNode root;

    public void SetLabelProps(float labelBuffer = -1, float labelLerp = -1, int labelAlign = -1)
    {
        SetSubtreeLabelProps(root, labelBuffer, labelLerp, labelAlign);
    }

    public void SetSubtreeLabelProps(BTNode root, float labelBuffer = -1, float labelLerp = -1, int labelAlign = -1)
    {
        if (root == null)
            return;
        if (labelBuffer != -1)
            root.labelBuffer = labelBuffer;
        if (labelLerp != -1)
            root.labelLerp = labelLerp;
        if (labelAlign != -1)
            root.labelAlign = (TMPro.TextAlignmentOptions)labelAlign;
        if (root.label != null && root.label.isActiveAndEnabled)
            root.PlaceLabel();
        SetSubtreeLabelProps(root.Left, labelBuffer, labelLerp, labelAlign);
        SetSubtreeLabelProps(root.Right, labelBuffer, labelLerp, labelAlign);
    }

    public void Display()
    {
        StartCoroutine(display());
    }

    IEnumerator display()
    {
        yield return displaySubtree(root);
    }

    public void DisplaySubtree(BTNode n)
    {
        StartCoroutine(displaySubtree(n));
    }

    public void DisplayToLevel(int level)
    {
        StartCoroutine(displayToLevel(level));
    }

    IEnumerator displayToLevel(int level)
    {
        for (int i = 0; i < level; i++)
        {
            yield return displayLevel(i);
        }
    }

    public void DisplayLevel(int level)
    {
        StartCoroutine(displayLevel(level));
    }

    IEnumerator displayLevel(int level)
    {
        foreach (BTNode n in GetLevel(level))
        {
            n.Display();
        }
        yield return new WaitForSeconds(0.5f);
    }

    public List<BTNode> GetLevel(int level, BTNode n = null, int current = 0)
    {
        if (n == null) n = root;
        List<BTNode> nodes = new List<BTNode>();
        if (current == level)
        {
            nodes.Add(n);
        } else
        {
            if (n.Left != null)
                nodes.AddRange(GetLevel(level, n.Left, current + 1));
            if (n.Right != null)
                nodes.AddRange(GetLevel(level, n.Right, current + 1));
        }
        return nodes;
    }

    IEnumerator displaySubtree(BTNode n)
    {
        n.Display();
        yield return new WaitForSeconds(0.5f);
        if (n.Left != null)
        {
            DisplaySubtree(n.Left);
        }
        if (n.Right != null)
        {
            DisplaySubtree(n.Right);
        }
        yield return new WaitForSeconds(0.5f);
    }

    public void Hide(float totalDuration = 1)
    {
        float durationPerLevel = totalDuration / GetLevelCount();
        StartCoroutine(hide(durationPerLevel));
    }

    IEnumerator hide(float durationPerLevel)
    {
        yield return hideSubtree(root, durationPerLevel);
    }

    public void HideSubtree(BTNode n, float totalDuration = 1)
    {
        float durationPerLevel = totalDuration / GetLevelCount();
        StartCoroutine(hideSubtree(n, durationPerLevel));
    }

    IEnumerator hideSubtree(BTNode n, float durationPerLevel)
    {
        Coroutine l = null, r = null;
        if (n.Left != null)
        {
            l = StartCoroutine(hideSubtree(n.Left, durationPerLevel));
        }
        if (n.Right != null)
        {
            r = StartCoroutine(hideSubtree(n.Right, durationPerLevel));
        }
        yield return l;
        yield return r;
        n.Hide(durationPerLevel);
        yield return new WaitForSeconds(durationPerLevel);
    }

    // Calls arrow.SetColor on all arrows from root to specified node
    public void SetPathColor(BTNode n, Color color)
    {
        BTNode curr = n;
        while (curr != null)
        {
            if (curr.arrow != null)
            {
                curr.arrow.SetColor(color);
            }
            curr = curr.Parent;
        }
    }
    // Calls arrow.ChangeColor (which starts an animation coroutine) on all arrows from root to specified node
    public void AnimatePathColor(BTNode n, Color color, float duration = 0.5f) {
        BTNode curr = n;
        while (curr != null)
        {
            if (curr.arrow != null)
            {
                curr.arrow.ChangeColor(color, duration: duration);
            }
            curr = curr.Parent;
        }
    }
    // Convenience method to animate a path color then back to its original color
    public void HighlightPath(BTNode n, Color color, float totalDuration = 1, float changeDuration = 0.5f) {
        if (totalDuration < 2 * changeDuration) {
            Debug.LogError("totalDuration less than twice changeDuration. Not enough time to animate path highlighting");
        }
        StartCoroutine(highlightPath(n, color, totalDuration, changeDuration));
    }
    IEnumerator highlightPath(BTNode n, Color color, float totalDuration = 1, float changeDuration = 0.5f) {
        Color initialColor = n.arrow.Color;
        AnimatePathColor(n, color, duration: changeDuration);
        yield return new WaitForSeconds(totalDuration - 2 * changeDuration);
        AnimatePathColor(n, initialColor, duration: changeDuration);
    }

    public void UpdateNodes(int height = -1)
    {
        updateNode(root, 0, height);
    }

    void updateNode(BTNode node, int direction = 0, int height = -1)
    {
        if (direction != 0)
        {
            Vector3 newPos = CalcNodePos(node, direction, height, true);
            node.intendedPos = newPos;
            node.MoveTo(newPos);
            if (node.arrow != null)
            {
                if (node.arrow.isActiveAndEnabled)
                    node.arrow.AnimateFromTo(node.Parent.intendedPos, newPos, endBuffer: node.buffer, startBuffer: node.Parent.buffer);
                else
                    node.arrow.SetFromTo(node.Parent.intendedPos, newPos, node.buffer, node.Parent.buffer);
                if (node.label != null && node.label.isActiveAndEnabled)
                {
                    // Debug.Log("arstoien");
                    node.UpdateLabel(intended: true);
                }
                // else
                    node.PlaceLabel();
            } else
            {
                node.arrow = Instantiate(SceneManager.instance.primerArrowPrefab);
                node.arrow.SetWidth(4f);
                node.arrow.SetFromTo(node.Parent.transform.position, node.transform.position, node.buffer, node.Parent.buffer);
                node.arrow.transform.parent = node.transform;
                node.arrow.gameObject.SetActive(false);
                node.PlaceLabel();
            }
        }
        if (height > 0)
        {
            if (node.Left != null)
                updateNode(node.Left, -1, height - 1);
            if (node.Right != null)
                updateNode(node.Right, 1, height - 1);
        }
    }

    public void PlaceNodes(int height = -1)
    {
        PlaceNode(root, rec: true, height: height);
    }
    public void UpdateLabels(BTNode node)
    {
        if (node != root) {
            node.UpdateLabel(intended: false);
        }
        if (node.Left != null) {
            UpdateLabels(node.Left);
        }
        if (node.Right != null) {
            UpdateLabels(node.Right);
        }
    }
    private void PlaceNode(BTNode node, int direction = 0, bool rec = false, int height = -1)
    {
        if (node == null) return;
        if (direction != 0)
        {
            node.transform.position = CalcNodePos(node, direction, height);
            if (node.arrow == null)
            {
                node.arrow = Instantiate(SceneManager.instance.primerArrowPrefab);
                node.arrow.SetWidth(4f);
                node.arrow.transform.parent = this.transform;
                node.arrow.gameObject.SetActive(false);
                node.PlaceArrow();
                node.PlaceLabel();
            }
        }
        if (rec)
        {
            PlaceNode(node.Left, -1, true, height - 1);
            PlaceNode(node.Right, 1, true, height - 1);
        }
    }

    private Vector3 CalcNodePos(BTNode node, int direction, int height = -1, bool local = false)
    {
        if (!local)
            return node.Parent.transform.position + node.yOffset + direction * node.xOffset * (1 + node.SpaceNeeded(-direction, height));
        return node.Parent.intendedPos + node.yOffset + direction * node.xOffset * (1 + node.SpaceNeeded(-direction, height));
    }
    public void SetRootNodeText(string newText) {
        root.transform.GetComponentsInChildren<TextMeshPro>()[0].text = newText;
    }

    public void Label(PrimerText prefab)
    {
        LabelSubtree(prefab, root);
    }

    public void LabelSubtree(PrimerText prefab, BTNode n)
    {
        if (n == null) return;
        n.Label(prefab);
        LabelSubtree(prefab, n.Left);
        LabelSubtree(prefab, n.Right);
    }

    public static BTNode coin(BinaryTree tree, BTNode coinPrefab, bool heads = true)
    {
        BTNode node = Instantiate(coinPrefab);
        if (heads)
        {
            node.transform.Find("100 coin").Rotate(new Vector3(0, 180, 0));
        }
        else
        {
            node.transform.Find("100 coin").Rotate(new Vector3(0, 0, 180));
        }
        node.transform.parent = tree.transform;
        return node;
    }

    public static BinaryTree DecisionTree(int height, BinaryTree treePrefab, BTNode rootPrefab, BTNode coinPrefab) {
        Debug.LogWarning("Constructing a binary tree by passing a prefab is deprecated. Ignoring the prefab.");
        return DecisionTree(height, rootPrefab, coinPrefab);
    }
    public static BinaryTree DecisionTree(int height, BTNode rootPrefab, BTNode coinPrefab,
        PrimerText headsLabel = null, PrimerText tailsLabel = null)
    {
        BinaryTree tree = new GameObject().AddComponent<BinaryTree>();
        tree.gameObject.name = "Binary tree";
        tree.root = Instantiate(rootPrefab);
        tree.root.transform.parent = tree.transform;
        if (height > 0)
        {
            tree.root.Left = DecisionSubtree(tree, coinPrefab, height - 1, false, headsLabel, tailsLabel);
            tree.root.Right = DecisionSubtree(tree, coinPrefab, height - 1, true, headsLabel, tailsLabel);
        }
        return tree;
    }

    public static BTNode DecisionSubtree(BinaryTree tree, BTNode coinPrefab, int height, bool heads = true,
        PrimerText headsLabel = null, PrimerText tailsLabel = null)
    {
        BTNode root = coin(tree, coinPrefab, heads);
        if (headsLabel != null && heads)
        {
            root.Label(headsLabel);
        } else if (tailsLabel != null && !heads)
        {
            root.Label(tailsLabel);
        }
        if (height > 0)
        {
            root.Left = DecisionSubtree(tree, coinPrefab, height - 1, false, headsLabel, tailsLabel);
            root.Right = DecisionSubtree(tree, coinPrefab, height - 1, true, headsLabel, tailsLabel);
        }
        return root;
    }
    public void ShowBranchLabels(string leftLabel, string rightLabel, float delay, int maxLevel = -1) {
        StartCoroutine(showBranchLabels(root, "", leftLabel, rightLabel, delay, maxLevel));
    }
    IEnumerator showBranchLabels(BTNode node, string labelVal, string leftLabel, string rightLabel, float delay, int maxLevel) {
        if (node != root) {
            node.Label(Resources.Load<PrimerText>("text"));
            node.PlaceLabel();
            node.label.tmpro.text = labelVal;
            node.label.gameObject.SetActive(true);
            node.label.ScaleUpFromZero();
            yield return new WaitForSeconds(delay);
        }
        if (maxLevel != 0) {
            if (node.Left != null) {
                StartCoroutine(showBranchLabels(node.Left, leftLabel, leftLabel, rightLabel, delay, maxLevel - 1));
            }
            if (node.Right != null) {
                StartCoroutine(showBranchLabels(node.Right, rightLabel, leftLabel, rightLabel, delay, maxLevel - 1));
            }
        }
    }
    public void HideAllLabels(float delay) {
        StartCoroutine(hideAllLabels(root, delay));
    }
    IEnumerator hideAllLabels(BTNode node, float delay) {
        if (node != root && node.label != null) {
            node.label.ScaleDownToZero();
            yield return new WaitForSeconds(delay);
        }
        if (node.Left != null) {
            StartCoroutine(hideAllLabels(node.Left, delay));
        }
        if (node.Right != null) {
            StartCoroutine(hideAllLabels(node.Right, delay));
        }
    }

    public int GetLevelCount() {
        // Lazy but all I need right now, assumes tree is full
        BTNode node = root;
        int levelCount = 1;
        while (node.Left != null) {
            levelCount++;
            node = node.Left;
        }
        return levelCount;
    }
}
