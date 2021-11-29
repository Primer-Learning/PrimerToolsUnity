# PrimerUnity
Hey there!

This is the tool used to make visualizations and animated simulations for [Primer YouTube videos](https://www.youtube.com/channel/UCKzJFdi57J53Vr_BkTfN3uQ).

If you want to give or receive help, there is [a Primer Discord community](https://discord.gg/NbruaNW). If you have other inquiries, you can email tools@primerlearning.org (but I will ignore help requests there; go to the discord for that).

## What this is hopefully useful for
For now, at least, I'm the only one who uses this code.

Other use cases I can think of are:

- Primer viewers who want to tinker with sims from videos

- Other aspiring or established video creators using the tool to create videos of their own

- Coding students (in formal or informal settings) using it for applied projects

- Academic researchers and teachers using it to animate their models or create 3D visualizations

To be useful for any of that, though, it's going to take more than me working on it. I'm not a software engineer, gamedev, or artist by training. If happen to know what you're doing on any of those fronts, you'll discover pretty quickly that I've taken some silly approaches along the way. I'm very open to suggestions or contributions of any sort, big or small.

## Getting started

This code only works inside of a Unity project, so you'll need to [download Unity](https://unity.com/) create a project. It's free for personal use. This project uses Unity 2020.3.20f1, but you're probably fine if you have a different recent version.

If you're just planning to use it, your best bet is probably to download Primer.unitypackage from the root directory and import that into your Unity project. Another option is to download the whole repo, unzip it, and drop it into the Assets folder of your Unity project.

If you think you might contribute to the tool, I recommend setting it up as a submodule inside your Unity project's repo. Or, you can just clone this repo into your Assets folder if you don't want to use git for your Unity project.

This repo uses git-LFS (Large File Storage) to handle some .blend files, so cloning might not work if you don't have that configured. So either [get that set up](https://git-lfs.github.com/) or just download the repo as a zip file. 

If you run into other issues when setting up, [this might be helpful](https://thoughtbot.com/blog/how-to-git-with-unity).

Once you get it open, there are four sample scenes. I'd recommend going through them in the order below. They also have some comments to help explain what they are doing.

- **SimpleDirectedScene** just moves a blob around a plane. It's meant to show how basics of the `Director` and `PrimerObject` classes, which are the most important two. More on them below.
- **CoinFlipSimBaseScene** uses classes that inherit from the `Simulator` and `SimulationManager` classes to have a bunch of sphere creatures (and one blob) flip coins.
- **SampleGraphScene** shows some features of the graph tool. There are also bar graphs, but they are shown in the dice sample scene.
- **DiceGameWithGraphScene** is a recreation of a scene from [the video on Hamilton's rule](https://youtu.be/iLX_r_WPrIw?t=114). It's the most complex sample scene, meant to show a simulation connected to a graph, which is basically the core of most video.

If all goes well, you should be able to open the project, pick a scene, and hit play. Then, you can modify the scripts or make your own. I'm excited to see what you make.

If you want to record your scene to a sequence of pngs, you can do that by clicking on the Director game object in the Hierarchy window, then using the dropdown in the Inspector window to set the Mode to "Constant Frame Rate", checking the "Record on play" box, then hitting play. By default, it will create a "png" folder in the root folder of your unity project and save the pngs there.

#### General Unity learning
If you're just getting started with Unity, I found Unity's free [Create with Code](https://learn.unity.com/course/create-with-code) course to be helpful. If you're experienced with coding, some parts might feel a little slow, but you can skip through it as you like. Unity has a lot more free resources, and there are many useful youtube channels and forums.

The programming language is C#. There's a course on [Codecademy](https://www.codecademy.com/learn/learn-c-sharp) which I used when getting up to speed with Unity.

## Documentation so far
There isn't real documentation yet, but here's an overview of the most fundamental pieces to hopefully get you going.
#### The `PrimerObject` class
The `PrimerObject` class inherits from Unity's base class for game object scripts, [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html), and adds methods for animating parameters with easing. `PrimerObject` is the parent class of more specific classes like PrimerArrow, and it's what you should use as a parent class for any custom script you make for Unity game object you want to animate with this tool. For any game object you put into Unity, you can attach a `PrimerObject` component, which will let you call methods like `MoveTo` or `ScaleUpFromZero`. Classes that inherit from `PrimerObject` can also animate their C# properties (not fields) using `AnimateValue`.

#### The `Director` class
The `Director` class sets up the scene camera and lighting and defines some methods for setting up animation timings. To script a scene, you can make a script defining a subclass of `Director` that sets timings for `SceneBlock`s. `Directors` are usually going to have game objects and their components stored as variables, then manipulate and/or call those objects' methods in different parts of the scene.

For example, the Unity scene "SimpleDirectedScene" has a game object named "Director" with a `SimpleSceneDirector` component attached to it. `SimpleSceneDirector` inherits from `Director` and defines the specific animations for the scene.

#### Context around using a game engine to make videos
Unity is a game engine. It operates [on a loop](https://docs.unity3d.com/Manual/ExecutionOrder.html) that processes one frame at a time.

In a game, the framerate is often flexible based on the amount of processing that needs to go into each frame. The engine keeps the "game time" in sync with real time, letting the rate of frames vary. The "Normal" director mode acts this way.

In video formats, though, "game time" between frames needs to be constant, or else the speed of our eventual video will be warped. We keep the frame rate constant by setting `Time.captureFramerate`, which happens in "Constant Frame Rate" director mode. With this set to 60, for example, each frame loop of the game engine will move game time forward by 1/60 of a second, regardless of how much time went by in the real world while the frame was processing. This effectively slows game time down so the frames can stay in sync (or, it can even speed up the game if you have a beefy machine and a simple scene). This turns out to be pretty important when saving frames with the recorder, since at high resolutions, it can easily take close to if not more than 1/60 seconds just to save the image. It also makes it so your code can produce smooth videos even if it's slow. 

When you eventually make a video from your frames, your video editor of choice should be able to import an image sequence. If you need other formats, you can use Unity's official recorder asset (...or add the feature to this tool). It doesn't let you start and stop recordings (or anything else) via code, though. 

#### Coroutines
This tool makes extensive (excessive?) use of coroutines. The frame loop usually waits for all methods to finish before rendering the frame and moving to the next iteration in the loop. If you write a method to animate the motion of an object, it will appear to just teleport to its final destination, since all the code is executed between frames, in an instant of game time.
  
[Coroutines](https://docs.unity3d.com/Manual/Coroutines.html) are different. They use yield statements to pause execution until the next iteration of the frame loop, letting you write a method that executes as game time passes. The most basic yield statement is `yield return null;`, which is the same as saying "stop executing here, let the frame update, and then continue on". The methods in PrimerObject mainly puts `yield return null` statements inside while loops that are set to run for a certain duration in game time. There are other kinds of yield statements. `yield return new WaitForSeconds(x);` will wait `x` (a float value) seconds of game time before continuing the coroutine. The Director class defines a custom yield statement `yield return new WaitUntilSceneTime(x)` that waits until the specified scene time (managed by the director, but usually the same as game time) in seconds. You can also yield based on the completion of another coroutine. `CoinFlipScene` has an example of this.
## Simulations
The only simulations in this repo so far are coin flipping and die rolling. Since I'm learning as I go, the tool has changed quite a lot over time, and my biology and econ sims aren't compatible with the current version (and are extreme spaghetti code that I'm not going to update, nor do I think it would be morally correct to inflict them on anyone as they are). But the basic identity of the tool feels pretty stable to me now, and I plan to build future sims in a way that will allow them to be shared publicly.

## License summary
The code is under an MIT license.

For the blob and blob-related assets, rights are reserved. I included them in the repo since people seem to like them, and I want the tool to be interesting to as many people as possible. So feel free to use them for personal projects, but if you're going to publish anything, don't use them. You're better off using your own style anyway. For organizational purposes, I have put these assets in a folder labeled "Rights reserved", but if a blob-related asset somehow ends up elsewhere, rights are still reserved.
