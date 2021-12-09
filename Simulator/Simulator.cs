using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : PrimerObject
{
    CycleType cycleType = CycleType.Continuous;
    public enum CycleType {
        Continuous,
        Phased
    }
    internal virtual void Go() {
        //Override in subclass
    }
}