using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulator : PrimerObject
{
    // SimulationManager simManager = null;
    // CycleType cycleType = CycleType.Continuous;
    public enum CycleType {
        Continuous,
        Phased
    }
    internal virtual void Go() {
        //Override in subclass
    }
}