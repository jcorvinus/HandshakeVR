# Leap SteamVR Skeleton
	  
## What is this?
This is a compatibility system that allows VR developers to target both Leap Motion and SteamVR skeletal hand tracking, as well as use Leap Motion's incredibly robust Interaction Engine with SteamVR controllers. All in a single workflow.

## Why would I want to use this?
If you want:
- More precise, finger-level interactions than the SteamVR Interaction, VRTK, or other interaction systems (currently) provide.
- Physical UI controls instead of laser pointers and button presses.
- To be able to swap back and forth between Leap optical hand tracking and SteamVR skeletal tracking (Knuckles, Touch, etc.) at *run time*, automatically.
- If you want to minimise use fo SteamVR's action system. With this, you can have grabbing, squeezing, and UI interactions without needing to touch the SteamVR action system. It just uses the pose, skeleton, and grab actions, and comes pre-configured.
- If you want to give your users the option of upgrading to higher quality finger tracking than they'll get from the Knuckles controller, with no changes needed to their software. (Seriously people start writing more Leap compatible software)
- If you're a Interaction Engine developer and want the ability to hold objects behind you, well outside of the Leap Motion's regular FOV.

## Great, how do I start using it?
This project is currently set up for Unity version 2017.3.1f1. Get this version if you're having problems getting it working with your preferred version of unity. After that, The first thing you'll want to do is clone the repository. Then, get the following unity packages:

SteamVR:
https://github.com/ValveSoftware/steamvr_unity_plugin/releases/tag/RC3

Leap Core Assets & Interaction Engine:
https://developer.leapmotion.com/unity/#5436356

Once you've downloaded the packages, open the project in Unity. Import the asset packages. You will get a compile error about Provider being read-only. At the moment, we need to make one small change to the leap motion assets to get things working properly. In HandUtils.cs, after line 82, add the following
      set
      {
        s_provider = value;
      }

# Exploring the project's capabilities	  
Now to start testing and exploring the project's capabilities. You'll want the following hardware:
- Leap Motion controller (Just get one of these, they're inexpesnive and super useful.)
- Knuckles EV2 (If you can get them - Oculus touch controllers might work in a pinch. Vive wands will not perform well.)

Technically neither of these are necessary but it's extraordinarily difficult to test your code and assumptions without them.

Open the 'Basic Interaction' scene. Ensure your leap motion is connected, and make sure that all SteamVR controllers are off. Hit the play button and use the leap optical tracking to play with the objects a bit. Then, when you're satisfied, turn on your Knuckles controllers and after a moment the hand data feed will switch and you'll see the slim VR glove model. The new hand model should work much like you expect. Do note that to pick anything up you'll need to touch one of the thumb buttons (like the trackpad, thumbstick, or face buttons). It won't work if you just try using the grip. You should be able to grab, poke, slap, punch, and throw all of the interactive objects (that aren't anchored by a joint) just like you would with the leap hands.

Go head and open up any of the other scenes and try them out as well. They're adapted versions of the original Leap Motion Interaction Engine samples, with the exception of the 'SteamVRTest' scene, which is a minimal implementation example.

# Setting up a new scene
- Create a new scene
- Delete the main camera
- Locate the /Assets/_LeapControllerCompatibility/Prefabs/LeapRig prefab
	- Drag it into the scene
- Locate the /Assets/_LeapControllerCompatibility/Prefabs/Interaction Manager prefab
- Expand both the LeapRig object and the Interaction Manager object.
- Select the LeapRig/Custom Provider/ gameobject
- Drag the left and right interaction hand objects into the 'left interaction hand' and 'right interaction hand' properties on the CustomProvider behavior
- save the scene
- enter play mode

# FAQ:
Q: How do I develop for Interaction Engine?
A: Head on over to the Interaction Engine documentation: https://leapmotion.github.io/UnityModules/interaction-engine.html

Q: Why does releasing an object sometimes cause it to pop upwards?
A: I'm not 100% sure but I think it has to do with the colliders of the last few fingers still penetrating the object when the 'grab' is released. This should be fixable by changing the collision behavior. I do plan on fixing this.

Q: Why is the grabbing fiddly for me?
A: The thumb and index finger are the most important parts of a grab in interaction engine. Make sure to grab objects with those fingers. At some point I'll create customn poses that make this easier, same with pinching.

Q: Why do you switch to the SteamVR thin glove model when in SteamVR mode despite the retargeting being valid for both models?
A: The leap hand model rigged to the SteamVR skeleton is horrifying.
