# PrimerUnity
Graph and scene direction tools used for [Primer YouTube videos](https://www.youtube.com/channel/UCKzJFdi57J53Vr_BkTfN3uQ).

[Tell me](https://docs.google.com/forms/d/1j39Rj-XF4y5sXLiYLkC-t5MdLOMu_JvDfJK4TGLlQTU/edit) about your experience with this tool.

## Getting started
This is a Unity project. You'll need to download Unity separately. The version this project uses is [Unity 2019.2.8f1](https://unity3d.com/unity/whats-new/2019.2.8)

If you think you might want to contribute code, clone the repo. This repo uses git-LFS to handle some .blend files, so cloning might not work if you don't have that configured. So either [get that set up](https://git-lfs.github.com/) or just dowload the repo as a zip file. If you run into other issues, [this might be helpful](https://thoughtbot.com/blog/how-to-git-with-unity).

If you don't think you'll contribute code, it's probably easiest to just download the unity package in the [Releases](https://github.com/Helpsypoo/PrimerUnity/blob/master/Releases/primerTools.unitypackage) folder and import that into a new or existing project.

Once you get it open, there are two sample scenes. SampleDirectedScene moves a spherical creature around a plane, and SampleGraphScene shows some features of the graph tool. If all goes well, you should be able to open the project, pick a scene, and hit play. Then, you can modify the scene director scripts (more on that below) or make your own.

## How it works

### Using a game engine to make videos
Unity is a game engine. It operates [on a loop](https://docs.unity3d.com/Manual/ExecutionOrder.html) that processes one frame at a time. 

To output these frames to image files that can be stitched into a video file, I use the Unity Recorder package. If you clone the repo, it's already included in the Unity project. To add it to your own project, you can find it in the package manager (Window > Package Manager). To see it, you have to include preview packages under advanced settings. Once the package is installed, you can use it by going to Window > General > Recorder > Recorder Window. [This video](https://www.youtube.com/watch?v=VqW-Fg5VafQ) helped me get going with it.

In a game, the framerate is often flexible based on the amount of processing that needs to go into each frame. The engine keeps the "game time" in sync with real time, letting the rate of frames vary. In video formats, though, "game time" between frames needs to be constant, or else the speed of our eventual video will be warped. 

We can do this by setting `Time.captureFramerate = 60` (or whatever framerate you want). With this set, each frame loop of the game engine will move "game time" forward by 1/60 of a second, regardless of how much time went by in the real world while the frame was processing. This effectively slows "game time" down so the frames can stay in sync. This turns out to be pretty important when saving frames with the recorder, since at high resolutions, it can easily take close to if not more than 1/60 seconds just to save the image. It also makes it so your code can produce smooth videos even if it's too slow to run in realtime.

`Time.captureFramerate` is already set in the Director class (more on that below). Why tell you all this if it's already taken care of? Well, the relationship between frames, "game time", and realtime is pretty crucial to understanding how this works. And if you want to produce a video with a different framerate, now you know how.

When you eventually make a video from your frames, your video editor of choice should be able to import an image sequence. The recorder package can also produce video files directly, which might work for you, but I like to produce each frame individually to have the most control. This can use a lot of space on your hard drive, though, so experiment with what works best for you.

### Coroutines
This tool makes extensive (excessive?) use of coroutines. The frame loop usually waits for all methods to finish before rendering the frame and moving to the next iteration in the loop. If you write a method to animate the motion of an object, it will appear to just teleport to its final destination, since all the code is executed between frames, in an instant of game time.

[Coroutines](https://docs.unity3d.com/Manual/Coroutines.html) are different. They use yield statements to pause execution until the next iteration of the frame loop, letting you write a method that executes as game time passes. The most basic yield statement is `yield return null;`, which is the same as saying "stop executing here, let the frame update, and then continue on". The methods in PrimerObject mainly puts `yield return null` statements inside while loops that are set to run for a certain duration in game time. There are other kinds of yield statements. `yield return new WaitForSeconds(x);` will wait `x` (a float value) seconds of game time before continuing the coroutine. The Director class defines a custom yield statement `yield return new WaitUntilSceneTime(x)` that waits until the specified absolute game time in seconds. You can also yield based on the completion of another coroutine.

### The PrimerObject class
The PrimerObject class inherits from Unity's base class, [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html), and adds methods for animating parameters with easing. PrimerObject is the parent class of more specific classes like PrimerArrow, and it's what you should use as a parent class for any custom script you make for Unity game object you want to animate with this tool.

### The Director class
The Director class also inherits from MonoBehaviour. It sets up the scene camera and lighting, references some prefabs, and defines some methods for setting up animation timings. To script a scene, you can make a script defining a subclass of Director that sets timings for SceneBlocks. 

For example, the Unity scene "SampleDirectedScene" has a game object named "Scene Director" with a "SampleSceneDirector" component attached to it. SampleSceneDirector inherits from Director and defines the specific animations for the scene.

## General Unity learning
If you're just getting started with Unity, I found Unity's free [Create with Code](https://learn.unity.com/course/create-with-code) course to be helpful. If you're experienced with coding, some parts might feel a little slow, but you can skip through it as you like. Unity also has a lot more free resources.

The programming language is C#. There's a course on <a target='new' href="https://click.linksynergy.com/fs-bin/click?id=1myhZO82FrY&offerid=781062.49&type=3&subid=0&LSNSUBSITE=TEST">Codecademy</a>, which I used when getting up to speed with Unity. You can learn C# for free, but if you choose to pay for their pro features, using this link helps support Primer.

[Sebastian Lague](https://www.youtube.com/user/Cercopithecan) also has a super helpful channel for learning Unity and Blender.

## Simulations
I haven't yet included simulations in this repo. I hope to do so before too long. (How long? I don't know.)

## Contributions
I've never worked on code with a team, and I originally made this for personal use. I tried to clean it up to be usable for others, but you'll still likely see some things that make you go "but... why?". Contributions to make it faster, more robust, easier to use, or better documented are all very welcome.

Made with help from [Sam Van Cise](https://vancisesam.com/).

## Licence
MIT License.
