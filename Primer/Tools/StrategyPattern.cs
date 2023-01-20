using System;

namespace Primer
{
    /// <summary>
    /// Extend this class to create a type that will be shown as a dropdown in Unity's inspector.
    /// This dropdown will allow you to choose between all subtypes of your class.
    /// </summary>
    /// <example>
    /// abstract class SomeNumber : StrategyPattern {
    ///     public abstract int GetSomeNumber();
    /// }
    /// class NumberOne : SomeNumber {
    ///     public override int GetSomeNumber() => 1;
    /// }
    /// class NumberTwo : SomeNumber {
    ///     public override int GetSomeNumber() => 2;
    /// }
    /// class NumberThree : SomeNumber {
    ///     public override int GetSomeNumber() => 3;
    /// }
    ///
    /// class MyBehaviour : MonoBehaviour {
    ///     // This will become a dropdown in Unity's inspector with NumberOne, NumberTwo and NumberThree
    ///     [SerializeReference] SomeNumber strategy = new NumberOne();
    ///     void Update() => Debug.Log(strategy.GetSomeNumber());
    /// }
    /// </example>
    [Obsolete("Odin inspector implements this by default, this class isn't needed anymore")]
    public class StrategyPattern
    {
    }
}
