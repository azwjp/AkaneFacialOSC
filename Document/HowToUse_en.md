# How to use AkaneFacialOSC on VRChat
This application sends the facial and eye-tracking data with OSC.

Some basic knowledge to use animators and blend shapes on Unity for VRChat is required to use this.

```
[Trackers] -> [AkaneFacialOSC] -(OSC)-> [VRChat]
```

## Features
- Sends facial/eye tracker data via OSC
- Aggregates and calculates the data to save the VRCExpressionParameters' memory
- Supports
    - Vive Facial Tracker
    - Vive Pro Eye
    - Pimax Droolon P1

## Steps to run
1. Install SRanipal SDK, and run it before using this app
    - you can find it on the Vive official SDK website 
1. Run this application
    - You might find an alert from "Windows Security Alert"
        - If it shows, **accept to communicate on the private networks**
        - This is because OSC uses a network feature to send the data (this is the specification of OSC)
        - This app does not send beyond the network (and the Internet) actually, only communicate in your computer running this app

## How to set up your avatar on VRChat
### Summary (for experts)
- This app sends the OSC data with `/avatar/parameters/{key}`
- The `{key}` is the text shown in the list of the application next to checkboxes
- Basically, the values are between `[0, 1]`
    - The default (natural) value is 0.5; you can change the range to `[-1, 1]`

### Stops
1. Prepare facial blend shapes that you want to move
    - you can skip this step if you only want to move the eye bones for gaze tracking
1. Add the `{key}`s which you want to move into the `Expression Parameters`
    - Each parameter must be float, which consumes 8 memories
1. Add the key with the same name with the key into the `Animator Parameter`
    - Note that 
        - if you want to move blend shapes, the animator must be in `FX`
        - if you want to move transform, the animator must be in except for the `FX` layer
1. Make animations to move your blend shapes
1. Make a transition on the animator to move your animation
    - Usually, users use `Motion Time` or `BlendTree`

### The sending data
This application sends not only the raw data from the trackers but also some computed data.
This saves the memory of VRCExpressionParameters.

The original data from trackers is separated with left and right.
And they also include exclusive data which will never have value at the same time.
(e.g., `Mouth_Upper_UpRight` (upper lip moved to right) and `Mouth_Upper_UpLeft`  (upper lip moved to right).)

The application combines two values to one to prevent waste memory.
One is bipolar data: one value is below 0.5, the other is over 0.5, 0.5 is the centre.
Another is average: the averages of left and right.

## Keys
### Eye data (raw data and the data calculated with the same way to the SDK)
| Key                   | Default | Min | Max |   | 
| ---------------------- | --- | --- | --- | --- |
| Eye_Left_Blink         | 0   | Natural | Closed left eye | Left eye's blinking |
| Eye_Left_Wide          | 0   | Natural | Widely opened left eye |  |
| Eye_Left_Right         | 0   | Natural | Tighted right side of the left eye to look to the right | Only when the pupil is further to the right from the centre |
| Eye_Left_Left          | 0   | Natural | Tighted left side of the left eye to look to the left | Only when the pupil is further to the left from the centre |
| Eye_Left_Up            | 0   | Natural | Risen left eyelid to look up | The same value with Eye_Left_Wide. Like vrc.lookingup|
| Eye_Left_Down          | 0   | Natural | Droopy eyelids to look down | Only when the pupil is further to the down from the centre like vrc.lookingdown |
| Eye_Right_Blink        | 0   | Natural | Closed right eye | Right eye's blinking  |
| Eye_Right_Wide         | 0   | Natural | Widely opened right eye |   |
| Eye_Right_Right        | 0   | Natural | Tighted right side of the right eye to look to the right | Only when the pupil is further to the right from the centre  |
| Eye_Right_Left         | 0   | Natural | Tighted left side of the right eye to look to the left | Only when the pupil is further to the left from the centre  |
| Eye_Right_Up           | 0   | Natural | Risen right eyelid to look up | The same value with Eye_Right_Wide. Like vrc.lookingup |
| Eye_Right_Down         | 0   | Natural | Droopy eyelids to look down | Only when the pupil is further to the down from the centre like vrc.lookingdown  |
| Eye_Left_Frown         | 0   | Natural | Frown eye, tighten the centre of the eyebrows | |
| Eye_Right_Frown        | 0   | Natural | Frown eye, tighten the centre of the eyebrows | |
| Eye_Left_Squeeze       | 0   | Natural | Squeezed eye, dropped eyebrow | |
| Eye_Right_Squeeze      | 0   | Natural | Squeezed eye, dropped eyebrow | |

### Gaze (Computed by the application)
| Key                   | Default | Min | Max |   | 
| ---------------------- | --- | --- | --- | --- |
| Gaze_Left_Vertical    | 0.5 (0) | Left eye looking down | Left eye looking up | |
| Gaze_Left_Horizontal  | 0.5 (0) | Left eye looking left | Left eye looking right | |
| Gaze_Right_Vertical   | 0.5 (0) | Right eye looking down | Right eye looking up | |
| Gaze_Right_Horizontal | 0.5 (0) | Right eye looking left | Right eye looking right | |
| Gaze_Vertical         | 0.5 (0) | Both eyes looking down | Both eyes looking up | The average of both eyes |
| Gaze_Horizontal       | 0.5 (0) | Both eyes looking left | Both eyes looking right | The average of both eyes |

### Face (raw data)
| Key                   | Default | Min | Max |   | 
| ---------------------- | --- | --- | --- | --- |
| Jaw_Right              | 0 | Natural | Jaw moved to right | If Jaw_Left and this have the same value, the jaw will be at the original position |
| Jaw_Left               | 0 | Natural | Jaw moved to left | If Jaw_Right and this have the same value, the jaw will be at the original position |
| Jaw_Forward            | 0 | Natural | Jaw moved to front | |
| Jaw_Open               | 0 | Natural | Opened jaw | |
| Mouth_Ape_Shape        | 0 | Natural | Opened jaw with closed mouth |  |
| Mouth_Upper_Right      | 0 | Natural | Upper lip moved to the right with closed mouth | If Mouth_Upper_Left and this have the same value, the lip will be at the original position |
| Mouth_Upper_Left       | 0 | Natural | Upper lip moved to left with closed mouth | If Mouth_Upper_Right and this have the same value, the lip will be at the original position |
| Mouth_Lower_Right      | 0 | Natural | Lower lip moved to the right with closed mouth | If Mouth_Lower_Left and this have the same value, the lip will be at the original position |
| Mouth_Lower_Left       | 0 | Natural | Lower lip moved to left with closed mouth | If Mouth_Lower_Right and this have the same value, the lip will be at the original position |
| Mouth_Upper_Overturn   | 0 | Natural | Upper lid stuck out with closed mouth |  |
| Mouth_Lower_Overturn   | 0 | Natural | Lower lid stuck out with closed mouth |  |
| Mouth_Pout             | 0 | Natural | Pouty mouth with closed | Possibly you could use the shape for vrc.v_ou instead, but it has a possibility of unintended movement because it should be with closed mouth |
| Mouth_Smile_Right      | 0 | Natural | Right corner of mouth brought up with closed mouth |  |
| Mouth_Smile_Left       | 0 | Natural | Left corner of mouth brought up with closed mouth |  |
| Mouth_Sad_Right        | 0 | Natural | Right corner of mouth brought down with closed mouth |  |
| Mouth_Sad_Left         | 0 | Natural | Left corner of mouth brought down with closed mouth |  |
| Cheek_Puff_Right       | 0 | Natural | Puffed right cheek |  |
| Cheek_Puff_Left        | 0 | Natural | Puffed left cheek |  |
| Cheek_Suck             | 0 | Natural | Suck both sides of cheek |  |
| Mouth_Upper_UpRight    | 0 | Natural | Risen right side of the upper lip with closed jar |  |
| Mouth_Upper_UpLeft     | 0 | Natural | Risen left side of the upper lip with closed jar |  |
| Mouth_Lower_DownRight  | 0 | Natural | Risen right side of the lower lip with closed jar |  |
| Mouth_Lower_DownLeft   | 0 | Natural | Risen left side of the lower lip with closed jar |  |
| Mouth_Upper_Inside     | 0 | Natural | Upper lip inside the mouth like biting |  |
| Mouth_Lower_Inside     | 0 | Natural | Lower lip inside the mouth like biting |  |
| Mouth_Lower_Overlay    | 0 | Natural | Lower lip overlays the upper lip | No signal for upper lip from the tracker |
| Tongue_LongStep1       | 0 | Natural | Tongue stuck out | like `th` sound. Tongue won't get out when closed mouth |
| Tongue_LongStep2       | 0 | Natural | Tongue stuck out widely | Completely stuck out tongue |
| Tongue_Down            | 0 | Natural | Tip of tongue moved to down | |
| Tongue_Up              | 0 | Natural | Tip of tongue moved to up | Like `r` sound |
| Tongue_Right           | 0 | Natural | Tongue moved to right |  |
| Tongue_Left            | 0 | Natural | Tongue moved to left |  |
| Tongue_Roll            | 0 | Natural | Rolled tongue |  |
| Tongue_UpLeft_Morph    | 0 | Natural | Tip of tongue bent to upper left |  |
| Tongue_UpRight_Morph   | 0 | Natural | Tip of tongue bent to upper right |  |
| Tongue_DownLeft_Morph  | 0 | Natural | Tip of tongue bent to lower left |  |
| Tongue_DownRight_Morph | 0 | Natural | Tip of tongue bent to lower right |  |

### Face (Computed by the application)
| Key                   | Default | Min | Max |   | 
| ---------------------- | --- | --- | --- | --- |
| Jaw_Left_Right              | 0.5 (0) | Jaw moved to left | Jaw moved to right | Calculated from Jaw_Left and Jaw_Right |
| Mouth_Sad_Smile_Right       | 0.5 (0) | Right corner of mouth brought down with closed mouth | Right corner of mouth brought up with closed mouth | Calculated from Mouth_Sad_Right and Mouth_Smile_Right |
| Mouth_Sad_Smile_Left        | 0.5 (0) | Left corner of mouth brought down with closed mouth | Left corner of mouth brought up with closed mouth  | Calculated from Mouth_Sad_Left and Mouth_Smile_Left |
| Mouth_Smile                 | 0       | Natural | Both corners of mouth brought up with closed mouth | Average of Mouth_Smile_Left and Mouth_Smile_Right |
| Mouth_Sad                   | 0       | Natural | Both corners of mouth brought down with closed mouth | Average of Mouth_Sad_Left and Mouth_Sad_Right |
| Mouth_Sad_Smile             | 0.5 (0) | Both corners of mouth brought down with closed mouth | Both corners of mouth brought up with closed mouth | Calculated from Mouth_Sad and Mouth_Smile |
| Mouth_Upper_Left_Right      | 0.5 (0) | Upper lip moved to left with closed mouth | Upper lip moved to the right with closed mouth | Calculated from Mouth_Upper_Left and Mouth_Upper_Right |
| Mouth_Lower_Left_Right      | 0.5 (0) | Lower lip moved to left with closed mouth | Lower lip moved to the right with closed mouth | Calculated from Mouth_Lower_Left and Mouth_Lower_Right |
| Mouth_Left_Right            | 0.5 (0) | Lip moved to left with closed mouth | Lip moved to the right with closed mouth | Average of Mouth_Upper_Left_Right and Mouth_Lower_Left_Right |
| Mouth_Upper_Inside_Overturn | 0.5 (0) | Upper lip inside the mouth like biting | Upper lid stuck out with closed mouth | Calculated from Mouth_Upper_Inside and Mouth_Upper_Overturn |
| Mouth_Lower_Inside_Overturn | 0.5 (0) | Lower lip inside the mouth like biting | Lower lid stuck out with closed mouth | Calculated from Mouth_Lower_Inside and Mouth_Lower_Overturn |
| Cheek_Puff                  | 0       | Natural | Puffed cheek | Average of Cheek_Puff_Left and Cheek_Puff_Right |
| Cheek_Suck_Puff             | 0.5 (0) | Suck both sides of cheek | Puffed cheek | Calculated from Cheek_Suck and Cheek_Puff |
| Mouth_Upper_Up              | 0       | Natural | Risen right side of the upper lip with closed jar | Average of Mouth_Upper_UpLeft and Mouth_Upper_UpRight |
| Mouth_Lower_Down            | 0       | Natural | Risen right side of the lower lip with closed jar | Average of Mouth_Lower_DownLeft and Mouth_Lower_DownRight |
| Tongue_Left_Right           | 0.5 (0) | Tongue moved to left | Tongue moved to right | Calculated from Tongue_Left and Tongue_Right |
| Tongue_Down_Up              | 0.5 (0) | Tip of tongue moved to down | Tip of tongue moved to up | Calculated from Tongue_Down and Tongue_Up |