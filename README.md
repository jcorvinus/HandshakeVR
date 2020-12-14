# HandshakeVR
	  
## What is this?
Handshake VR is a compatibility system that allows VR developers to target Ultraleap, SteamVR, Oculus, and hopefully soon Hololens 2 hand tracking, as well as use Ultraleap's incredibly satisfying and easy-to-work-with Interaction Engine with hand tracking from all of the aforementioned sources. All in a single workflow.

## Why would I want to use this?
[![Demo video](https://github.com/jcorvinus/LeapSteamVRSkeleton/blob/master/Docs/thumbnail.jpg)](http://www.youtube.com/watch?v=ghk21xVKhT0)

If you want:
- More precise, finger-level interactions than many other interaction systems (currently) provide.
- Physical UI controls instead of laser pointers and button presses.
- To be able to swap back and forth between Ultraleap optical hand tracking and controller tracking (Knuckles, Touch, etc.) at *run time*, automatically.
- If you want to minimise use of SteamVR's action system. With this, you can have grabbing, squeezing, and UI interactions without needing to touch the SteamVR action system. It just uses the pose, skeleton, and grab actions, and comes pre-configured.
- If you want to give your users the option of upgrading to higher quality finger tracking than they'll get from the Knuckles controller, with no changes needed to their software. (**Seriously people start writing more Leap compatible software**)
- If you're a Interaction Engine developer and want the ability to hold objects behind you, well outside of the Leap Motion's regular FOV.

## How does it work?
The main principle, is that Handshake ingests hand tracking and controller data from various sources and converts them to the hand data format used by Ultraleap.

![Data Flow](https://github.com/jcorvinus/HandshakeVR/blob/master/Docs/HandFlowSimple.png)

Data flow starts at the bottom and goes up. This is a simplified diagram, a more detailed entity component diagram is coming soon.

## Great, how do I start using it?
This project is currently set up for Unity version 2018.4.11f1. Get this version if you're having problems getting it working with your preferred version of unity. After that, The first thing you'll want to do is clone the repository. Then, get the following unity packages:

SteamVR:
https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647

Leap Core Assets & Interaction Engine. Version 4.6.0:
https://developer.leapmotion.com/unity/#5436356

Oculus Unity Integration:
https://developer.oculus.com/downloads/package/unity-integration/

(if it asks you to upgrade, select 'yes')

Leap Graphic Renderer (Optional):
You can import Leap Motion's Graphic Renderer asset package. There is an example scene that shows how to create curved UI elements. You will need to add the symbol 'LeapGraphicRenderer' for the glow effect to work, but otherwise, you can just import the Graphic Renderer module.

![define symbol location](https://github.com/jcorvinus/LeapSteamVRSkeleton/blob/master/Docs/scripting%20define%20symbols.png "Define symbols")

Once you've downloaded the packages, open the project in Unity. Import the asset packages. You will get a compile error about Provider being read-only. At the moment, we need to make one small change to the leap motion assets to get things working properly. In HandUtils.cs, after line 82, add the following
```
	set
	{
		s_provider = value;
	}
```

# Exploring the project's capabilities	  
Now to start testing and exploring the project's capabilities. You'll want the following hardware:
- Leap Motion controller (Just get one of these, they're inexpesnive and super useful.)
- Valve Index Controllers (any revision post EV2), Oculus Touch, or Vive Controllers (In order of compatibility quality). If you have something else that supports SteamVR Skeletal Input (like say, your own custom hardware), that might work too. In fact, let me know if it does or does not.
- Oculus Quest (2)
- Oculus Rift (S). Note that these only support hand-held controllers and not full skeletal finger tracking.

Technically none of these are necessary but it's extraordinarily difficult to test your code and assumptions without them. I recommend using hardware.

Open the 'Basic Interaction' scene. Ensure your leap motion is connected, and make sure that all SteamVR controllers are off. Hit the play button and use the leap optical tracking to play with the objects a bit. Then, when you're satisfied, turn on your Knuckles controllers and after a moment the hand data feed will switch and you'll see the slim VR glove model. The new hand model should work much like you expect. You should be able to grab, poke, slap, punch, and throw all of the interactive objects (that aren't anchored by a joint) just like you would with the leap hands.

Go head and open up any of the other scenes and try them out as well. They're adapted versions of the original Leap Motion Interaction Engine samples, with the exception of the 'SteamVRTest' scene, which is a minimal implementation example.

# Setting up a new scene
- Create a new scene
- Delete the main camera
- Locate the /Assets/HandshakeVR/Prefabs/LeapRig prefab
	- Drag it into the scene
- Locate the /Assets/HandshakeVR/Prefabs/Interaction Manager prefab
	- Drag it into the scene
- save the scene
- enter play mode

# FAQ:
Q: How do I develop for Interaction Engine?
A: Head on over to the Interaction Engine documentation: https://leapmotion.github.io/UnityModules/interaction-engine.html
