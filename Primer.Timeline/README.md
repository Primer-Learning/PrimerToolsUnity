# Primer.Timeline

This package provides a layer on top of `Unity.Timeline` in an attempt to simplify it's usage and make it closer to a video editor.

For that it provides two primary utilities: `Scrubbable` and `TriggeredBehaviour`.

It also provides a facade on top of `Unity.Timeline`, that includes

- `PrimerTrack`
- `PrimerClip&lt;TPlayable&gt;`
- `PrimerPlayable`
- `PrimerPlayable&lt;TTrackBind&gt;`
- `PrimerMarker`

And the more complex `PrimerMixer`.

## Utilities

### Scrubbable

Due to Unity's limitations it's an abstract class but it can be also represented as an interface

```csharp
interface Scrubbable {
  Transform target { get; set; }
  void Prepare();
  void Cleanup();
  void Update(float t);
}
```

It represents a function that receives a number from 0 to 1, this function will be called from a clip that we can move anywhere on the timeline and modifies a GameObject (or more) as needed.

Scrubbables **MUST** save the state they are going to modify on `Prepare()` and restore it on `Cleanup()`.

#### Usage

1. Define your behaviour by creating a class that extends `Scrubbable`, for example:

    ```csharp
    class MoveTo : Scrubbable {
        Vector3 initialPosition;
        public Vector3 destination = Vector3.zero;

        public override void Prepare() =>
            initialPosition = target.localPosition;
   
        public override void Cleanup() =>
            target.localPosition = initialPosition;

        public override void Update(float t) =>
            target.localPosition = Vector3.Lerp(initialPosition, destination, t);
    }
    ```

2. Drag any GameObject to the timeline and create a `GenericTrack`.
3. Right click on the track and create a `GenericClip`
4. Click on the clip and select your scrubbable under the "Animation" section
5. In "Animation extrapolation" select if the state should be extended before / after the clip ends.
6. You can now scrubble the timeline over the clip and see it live in edit mode

It's recommended to add a `public EaseMode easing;` field to the Scrubbable and use `easing.Apply(t)` instead of `t` directly to give more control over the animation.

### TriggeredBehaviour

This class represents a behaviour that can't be scrubbed because it defines set of actions that will be executed sequentially. In other words it's behaviour is not defined declaratively but in an imperative manner.

In this case a function will be invoked and it will start an execution that will take several frames:

```csharp
void MyImperativeMethod() {
    await arrow.ScaleUpFromZero();
    // the animation is already over, this is waiting one second
    // after previous animation ends
    await Seconds(1);
    // in this case we run two tasks in parallel
    // execution will continue after both are finished
    await Parallel(
      blob.WaveLookingAt(otherBlob),
      otherBlob.WaveLookingAt(blob)
    );
    await Milliseconds(100);
}
```

The methods inside a `TriggeredBehaviour` are executed via `TriggerMarker`s in the timeline.

These animations we `await` will only execute if `Application.isPlaying` is true, otherwise (in edit mode) they should apply the final state and return immediately. This ensures we can execute them in edit mode where the user can move start and stop the timeline at any rate without having to wait for the complete animation to run.


## Facade

### PrimerTrack

Base class for clip tracks, it enables the features listed in the following classes.

### PrimerClip&lt;TPlayable&gt;

Base class for clips, it contains a `TPlayable template` property that represents the playable this clip is meant to create.

### PrimerPlayable

Provides virtual `Start()` and `Stop()` methods that are much simpler to understand than the various `PlayableBehaviour` virtual methods.

`ProcessFrame` must be overriden and `base.ProcessFrame()` invoked inside.

It can implement some utility interfaces:

- `IExposedReferenceResolver` in which case the extension method `T Resolve(ExposedReference&lt;T&gt;)` will be added

- `IPropertyModifier` which requires method `void RegisterProperties(IPropertyCollector)`, notice that `IPropertyCollector` has a new extension method `AddProperties(Component, params string[])` that will print all available properties in case of an error.

### PrimerPlayable&lt;TTrackBind&gt;

Extension of `PrimerPlayable` that includes `TTrackBind trackTarget` property containing the Component is bound to the track of this playable, this is set before `Start()` is invoked so it's safe to use from `Start()`, `Stop()` and after invoking `base.ProcessFrame()`

### PrimerMarker

It provides utility checkboxes for common marker behaviours like:

- `retroactive`: whether the marker should be executed if the timeline is played after this marker's position 
- `emitOnce`: prevent this marker to be executed twice
- `emitInEditor`: whether this marker should be executed in edit mode

### PrimerMixer

A mixer will collect the states of multiple clips - and the initial state of the object - and merge (mix) them into a single state. This is required when clips allow `ClipCaps.Blending`.

Examples of this can be found in `Primer.Graph`.

`PrimerMixer` expects you to implement two abstract methods:

#### CreateCollector()

The `collector` not a Unity concept, is the entity in charge of collecting the states from the clips. This was added because following Unity's approach this behaviour had to be re-implemented every time we create a mixer.

With the signature `IMixerCollector&lt;TData&gt; CreateCollector()`, this method should provide a collector for the mixer, some collectors are provided by `Primer.Timeline` but more can be created:

- `MixerCollector`: Most common usage, it will just collect the states from the different clips. There are two ways to use this:
  - `new MixerColector&lt;TBehaviour&gt;()`
  - `new MixerCollector&lt;TBehaviour, TData&gt;(behaviour =&gt; behaviour.getDataSomewhow())`.
- `CollectorWithDirection`: same as `MixerCollector` but it will add a `isReverse` property (needs better naming) that will be true if we're at the end slope of a clip, this is used in graphs where the "ease in" animation is different from the "ease out"
- `NoopCollector`: Nothing will be collected, use this collector when the mixer doesn't care about the clips

#### Frame(collector)

This is where the mixer will merge the states inside the collector into a single state and apply it to `trackTarget`.
