using System.Linq;
using UnityEngine;

public class Home : MonoBehaviour
{
    private FruitTree[] _treesByDistance;
    public FruitTree[] treesByDistance => _treesByDistance ?? OrderTreesByDistance();

    public FruitTree[] OrderTreesByDistance()
    {
        var trees = transform.parent.GetComponentsInChildren<FruitTree>();
        _treesByDistance = trees.OrderBy(tree => (tree.transform.position - transform.position).sqrMagnitude).ToArray();
        return treesByDistance;
    }
}