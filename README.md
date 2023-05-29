
# FPS Controller for Unity

This package contains a simple FPS Controller for your games in Unity ! Did you happen to want a controller but the one provided by Unity it's just too complex or it just has too many modules for you project ? Then this package is for you.



## Installation

Just download the package and import it into your project with Unity.
## Usage

Just drag the prefab from ```Assets/FPSController/Core/Prefabs``` to your scene and start tweaking the controller configuration to your liking.


## Modules Explained

Here, I'll try to briefly explain the different modules that appear in the Unity inspector (although they're pretty self explanatory and all of them are explained in the tooltips)

## Can Sprint

Module that manages if the controller can press the selected key _(Default LShift)_ to sprint.

## Can Jump

Module that manages if the controller can press the selected key _(Default Spacebar)_ to jump.

## Can Crouch

Module that manages if the controller can press the selected key _(Default LControl)_ to crouch. This also reduces the player hitbox and checks with a raycast before standing up.

## Headbob Enabled

Module that manages if the controller will have a headbob while moving, speed and amount of the headbob while walking/sprinting/crouching are available to tweak.

## Will Slide on Slopes

Module that manages the controller's behaviour when standing on a slope. The angle _(in degress)_ of the slope is defined in the _CharacterController_ component of the _GameObject_

## Can Zoom

Module that manages if the controller is able to get in/out of a simple Zoom mode with the selected key _(Default Right Mouse Click)_

## Can Interact

Module that manages if the controller is able to interact with objects that implement the _Abstract_ class _'Interactable'_ with the selected key _(Default Left Mouse Click)_

## Use Footsteps

Module that manages if the controller should play sounds while walking to simulate footsteps. This module will throw a warning since in comes enabled by default but no audio clips are provided _(Sorry I'm lazy, lol)_, documentation for it is in the code.

## Use Health System

Module that manages if the controller should be able to: Have health, take damage and regenerate the missing health.

## Use Stamina System

Module that manages if the controller will use a stamina system when sprinting aswell as regenerate the missing stamina.