# PrimerUnity
Graph and scene direction tools used for Primer YouTube videos

## Use options
If you want to play with the tool and potentially contribute, clone the repo. If you want to use the tool in another project, it's probably easiest to just download the package in the Releases folder and import it into your project.

## How it works

### Using a game engine to make videos
Unity is a game engine. It operates [on a loop](https://docs.unity3d.com/Manual/ExecutionOrder.html) that processes one frame at a time. 

To output these frames to image files that can be stitched into a video file, I use the Unity Recorder package. If you clone the repo, it's already included in the Unity project. To add it to your own project, you can find it in the package manager (Window > Package Manager). To see it, you have to include preview packages under advanced settings. Once the package is installed, you can use it by going to Window > General > Recorder > Recorder Window. [This video](https://www.youtube.com/watch?v=VqW-Fg5VafQ) helped me get going with it.

In a game, the framerate is often flexible based on the amount of processing that needs to go into each frame. For videos, though, we need the framerate to be constant. We can do this by setting `Time.captureFrameRate = 60` (or whatever framerate you want). With this set, each frame loop of the game engine will move "game time" forward by 1/60 of a second, regardless of how much time went by in the real world while the frame was processing. This turns out to be pretty important when saving frames with the recorder, since at high resolutions, it can easily take close to if not more than 1/60 seconds to render and save the image. It also makes it so your code can produce smooth videos even if it's too slow to run in realtime.

### PrimerObject class
The PrimerObject class inherits from Unity's MonoBehaviour class and adds methods for animating parameters with easing. PrimerObject is the parent class of more specific classes like PrimerArrow, and it's what you should use as a parent class for any custom script you make for Unity GameObject you want to animate with this tool.

### Director class
The Director class also inherits from MonoBehavior. It sets up the scene camera and lighting, references some prefabs, and defines some methods for setting up animation timings. To script a scene, you can make a script defining a subclass of Director that sets timings for SceneBlocks. 

For example, the Unity scene "SampleDirectedScene" has a gameObject named "Scene Director" with a "SampleSceneDirector" component attached to it. SampleSceneDirector inherits from Director and defines the specific animations for the scene.

### Coroutines
This tool makes extensive (excessive?) use of coroutines. The frame loop usually waits for all methods to finish before rendering the frame and moving to the next iteration in the loop. If you write a method to animate the motion of an object, it will appear to just teleport to its final destination, since all the code is executed between frames, in an instant of game time.

[Coroutines](https://docs.unity3d.com/Manual/Coroutines.html) are different. They use yield statements to pause execution until the next iteration of the frame loop, letting you write a method that executes as game time passes. The most basic yield statement is `yield return null;`, which is the same as saying "stop executing here, let the frame update, and then continue on". The methods in PrimerObject mainly puts `yield return null` statements inside while loops that are set to run for a certain duration in game time. There are other kinds of yield statements. `yield return new WaitForSeconds(x);` will wait `x` (a float value) seconds of game time before continuing the coroutine. The Director class defines a custom yield statement `yield return new WaitUntilSceneTime(x)` that waits until the specified absolute game time in seconds. You can also yield based on the completion of another coroutine.

## General Unity learning
If you're just getting started with Unity, I found Unity's free [Create with Code](https://learn.unity.com/course/create-with-code) course to be helpful. If you're experienced with coding, some parts might feel a little slow, but you can skip through it as you like. Unity has a lot more free resources.

The programming language is C#. There's [a course on Codecademy](https://www.codecademy.com/learn/learn-c-sharp).

[Sebastian Lague](https://www.youtube.com/user/Cercopithecan) also has a super helpful channel for learning Unity and Blender.

## Simulations
I haven't yet included simulations in this repo. I hope to do so before too long. (How long? I don't know.)

## Contributions
I've never worked on code with a team, and I originally made this for personal use. I tried to clean it up to be usable for others, but you'll still likely see some things that make you go "but... why?". Contributions to make it faster, more robust, easier to use, or better documented are all very welcome.

## Licence
MIT License.
