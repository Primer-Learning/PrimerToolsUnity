# PrimerUnity
Hey there!

This is the tool used to make visualizations and animated simulations for [Primer YouTube videos](https://www.youtube.com/channel/UCKzJFdi57J53Vr_BkTfN3uQ).

I hope this is usable for you. I can't guarantee I'll be able to help everyone with it, but there is [a Primer Discord community](https://discord.gg/NbruaNW) that I hope will become a place for people to give and receive help.

## Why this is public
For now, at least, I'm the only one who uses this code. 

Other use cases I can think of are:
- Primer viewers who want to tinker with sims from videos
- Other aspiring or established video creators using the tool to create videos of their own
- Coding students (in formal or informal settings) using it as a way to get started
- Academic researchers and teachers using it to animate their models or create 3D visualizations

To be useful for any of that, though, it's going to take more than me working on it. I'm not a software engineer, gamedev, or artist by training, so I'm guessing there's a lot of low-hanging fruit for anyone who knows what they are doing. And, well. I've been working on this mostly by myself, so I'm sure it could be made more user-friendly, too.

## Getting started
This code only works inside of a Unity project, so you'll need to [download Unity](https://unity.com/) create a project. It's free for personal use. This project uses Unity 2020.3.20f1, but you're probably fine if you have a different recent version.

If you're just planning to use it, your best bet is probably to download Primer.unitypackage from the root directory and import that into your Unity project. Another option is to download the whole repo, unzip it, and drop it into the Assets folder of your Unity project.

If you're planning to contribute to the tool, I recommend setting it up as a submodule inside your Unity project's repo. Or, you can just clone this repo into your Assets folder if you don't want to use git for your Unity project.

This repo uses git-LFS to handle some .blend files, so cloning might not work if you don't have that configured. So either [get that set up](https://git-lfs.github.com/) or just dowload the repo as a zip file. If you run into other issues, [this might be helpful](https://thoughtbot.com/blog/how-to-git-with-unity).

Once you get it open, there are three sample scenes. 
- SimpleDirectedScene just moves a spherical creature around a plane. It's meant to show how basics of the `Director` and `PrimerObject` classes, which are probably the most important two. More on them below.
- SampleGraphScene shows some features of the graph tool. There are also bar graphs, but I haven't gotten around to putting that in a sample scene.
- CoinFlipSimBaseScene uses classes that inherit from the `Simulator` and `SimulationManager` classes to have a bunch of sphere creatures flip coins.

If all goes well, you should be able to open the project, pick a scene, and hit play. Then, you can modify the scripts or make your own.

If you want to record your scene to a sequence of pngs, you can do that by clicking on the Director game object in the Heirarchy window, then using the dropdown in the Inspector window to set the DirectorMode to "Recording".

### General Unity learning
If you're just getting started with Unity, I found Unity's free [Create with Code](https://learn.unity.com/course/create-with-code) course to be helpful. If you're experienced with coding, some parts might feel a little slow, but you can skip through it as you like. Unity also has a lot more free resources.

The programming language is C#. There's a course on <a target='new' href="https://click.linksynergy.com/fs-bin/click?id=1myhZO82FrY&offerid=781062.49&type=3&subid=0&LSNSUBSITE=TEST">Codecademy</a>, which I used when getting up to speed with Unity. You can learn C# for free, but if you choose to pay for their pro features, using this link helps support Primer.

[Sebastian Lague](https://www.youtube.com/user/Cercopithecan) also has a super helpful channel for learning Unity and Blender.

## How it works

### The PrimerObject class
The PrimerObject class inherits from Unity's base class for game object scripts, [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html), and adds methods for animating parameters with easing. PrimerObject is the parent class of more specific classes like PrimerArrow, and it's what you should use as a parent class for any custom script you make for Unity game object you want to animate with this tool. For any game object you put into Unity, you can attach a PrimerObject component, which will let you call methods like `MoveTo` or `ScaleUpFromZero`. Classes that inherit from `PrimerObject` can also animate their C# properties (not fields) using `AnimateValue`.

### The Director class
The Director class also inherits from MonoBehaviour. It sets up the scene camera and lighting, references some prefabs, and defines some methods for setting up animation timings. To script a scene, you can make a script defining a subclass of Director that sets timings for SceneBlocks. 

For example, the Unity scene "SimpleDirectedScene" has a game object named "Director" with a "SimpleSceneDirector" component attached to it. SampleSceneDirector inherits from Director and defines the specific animations for the scene.

### Using a game engine to make videos
Unity is a game engine. It operates [on a loop](https://docs.unity3d.com/Manual/ExecutionOrder.html) that processes one frame at a time.

In a game, the framerate is often flexible based on the amount of processing that needs to go into each frame. The engine keeps the "game time" in sync with real time, letting the rate of frames vary. In video formats, though, "game time" between frames needs to be constant, or else the speed of our eventual video will be warped. 

We can do this by setting `Time.captureFramerate = 60` (or whatever framerate you want). With this set, each frame loop of the game engine will move "game time" forward by 1/60 of a second, regardless of how much time went by in the real world while the frame was processing. This effectively slows "game time" down so the frames can stay in sync (or, it can even speed up the game if you have a beefy machine and a simple scene). This turns out to be pretty important when saving frames with the recorder, since at high resolutions, it can easily take close to if not more than 1/60 seconds just to save the image. It also makes it so your code can produce smooth videos even if it's slow. You probably don't need to worry about all this since the Director class takes care of it when you set it to Record mode. But if you want to produce a video with a different framerate, now you know how.

When you eventually make a video from your frames, your video editor of choice should be able to import an image sequence. Unity has an official recorder asset, which can output video directly, but it doesn't let you control it from code, so I rolled my own.

### Coroutines
This tool makes extensive (excessive?) use of coroutines. The frame loop usually waits for all methods to finish before rendering the frame and moving to the next iteration in the loop. If you write a method to animate the motion of an object, it will appear to just teleport to its final destination, since all the code is executed between frames, in an instant of game time.

[Coroutines](https://docs.unity3d.com/Manual/Coroutines.html) are different. They use yield statements to pause execution until the next iteration of the frame loop, letting you write a method that executes as game time passes. The most basic yield statement is `yield return null;`, which is the same as saying "stop executing here, let the frame update, and then continue on". The methods in PrimerObject mainly puts `yield return null` statements inside while loops that are set to run for a certain duration in game time. There are other kinds of yield statements. `yield return new WaitForSeconds(x);` will wait `x` (a float value) seconds of game time before continuing the coroutine. The Director class defines a custom yield statement `yield return new WaitUntilSceneTime(x)` that waits until the specified absolute game time in seconds. You can also yield based on the completion of another coroutine.

### Simulations
The only simulations in this repo so far are coin flipping and die rolling. Since I'm learning as I go, the tool has changed quite a lot over time, and my biology and econ sims aren't compatible with the current version (and are extreme spaghetti code that I'm not going to update, nor do I think it would be morally correct to inflict them on anyone as they are). But the basic identity of the tool feels pretty stable to me now, and I plan to build future sims in a way that will allow them to be shared publically.

## Licence
MIT License
