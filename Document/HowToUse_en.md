# How to use AkaneFacialOSC on VRChat
This application send the facial and eye tracking data with OSC.

Some basic knowledge to use animators and blendshapes on Unity for VRChat is required to use this.

```
[Trackers] -> [AkaneFacialOSC] -(OSC)-> [VRChat]
```

## Steps to run
1. Install SRanipal SDK, and run it before use this app
    - you can find it on the Vive offifial SDK web site 
1. Run this application
    - You might find an alert from "Windows Security Alert"
        - If it shows, **accept to communicate on the private networks**
        - This is because OSC uses a network feature to send the data (this is the specification of OSC)
        - This app does not send beyond the network (and the Internet) actually, only communicate in your computer running this app

## How to setup your avatar on VRChat
### Summary (for exparts)
- This app send the OSC data with `/avatar/parameters/{key}`
- The `{key}` is the text shown in the list of the application next to checkboxes

### Stops
1. Prepare facial blendshapes which you want to move
    - you can skip this step if you only want to move the eye bones
1. Add the values with the same name with the key into the `Animator Parameter`
    - Note that 
        - if you want to move blendshapes, the animator must be in `FX`
        - if you want to move transform, the animator must be in except for the `FX` layer
1. Add the values also into the `Expression Parameters`
1. Make a transition on the animator to move your animation

## Features
- Send ORC data Using the Vive Facial Tracker and eye-tracker on Vive Pro Eye (experimental)
