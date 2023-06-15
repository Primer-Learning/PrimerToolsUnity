public enum PoissonDiscOverflowMode {
    None,
    // Squeeze mode doesn't seem to properly respect the new min distance
    // Current guess for why is that the new grid boxes not only divide but 
    // also shift when the dimensions are odd.
    Squeeze,
    Force
}
