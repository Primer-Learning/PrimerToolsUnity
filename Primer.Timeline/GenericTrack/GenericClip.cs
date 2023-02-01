using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public class GenericClip : PrimerClip<GenericBehaviour>
    {
        public override ClipCaps clipCaps => ClipCaps.Extrapolation;

        // TODO: Move GenericBehaviour branching logic to here
        // private ScrubbableShit scrubbable = new();
        // private TriggerableShit triggerable = new();
        // // private SequentialShit sequential = new();
        //
        // [Space(32)]
        // [HideLabel]
        // [EnumToggleButtons]
        // [OnValueChanged(nameof(OnKindChanged))]
        // public Kind kind = Kind.Scrubbable;
        // public enum Kind { Scrubbable, Trigger, Sequence }
        //
        // private void OnKindChanged()
        // {
        //     template = kind switch {
        //         Kind.Scrubbable => scrubbable,
        //         Kind.Trigger => triggerable,
        //         // Kind.Sequence => sequential,
        //         // _ => throw new ArgumentOutOfRangeException(),
        //     };
        // }
        //
        // public void Execute(float time) => template.Execute(time);
    }
}
