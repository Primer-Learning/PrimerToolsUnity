using System.Linq;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

public class Home : MonoBehaviour
{
    private Transform hinge => transform.Find("hinge");
    [SerializeField, ShowInInspector] private Transform _rock;

    public Transform rock
    {
        get
        {
            if (_rock != null) return _rock;
            _rock = hinge.Find("rock 11");
            return _rock;
        }
    }
    
    private FruitTree[] _treesByDistance;
    public FruitTree[] treesByDistance => _treesByDistance ?? OrderTreesByDistance();

    public FruitTree[] OrderTreesByDistance()
    {
        var trees = transform.parent.parent.Find("Trees").GetComponentsInChildren<FruitTree>();
        _treesByDistance = trees.OrderBy(tree => (tree.transform.position - transform.position).sqrMagnitude).ToArray();
        return treesByDistance;
    }
    public Tween Open(float angle = 90)
    {
        return TweenHingeAngle(-angle);
    }
    public Tween Close()
    {
        return TweenHingeAngle(0);
    }

    private Tween TweenHingeAngle(float angle)
    { 
        return hinge.RotateTo(new Vector3(0, 0, angle));
    }
}