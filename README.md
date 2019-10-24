# Live Record

- Live record is a Unity Debugging tool, designed to help developers and testers constantly catch and record bugs.
- It works by constantly keeping the last 600 frames in memory and saving the frames in memory to the persistent storage of the device with a click of a button.
- It captures the gameplay screen every 3 frames so the recording is in 20 frames per second for a 60 fps game.
- It allows users to record the previous 30 seconds from the time they press the record button, so it is practically a time machine.
- It is a debugging tool and is not suitable for recording high quality video.

## How to use

1) Create an empty game object.
2) Add the LiveRecord script on top of the empty game object.
3) Configure the LiveRecord object by modifiying the parameters.
    - **Image Count:** Number of images/frames stored in the memory.
    - **Scale Ratio:** The downscale ratio of the recorded frames.
    - **Jpeg Quality:** Quality of the Jpegs stored in the persistant storage.
    - **Start Delay:** How much the tool will wait before starting the recording.
    - **Capture Count:** Number of captures that will be kept on the persistent storage, oldest capture will be deleted if the amount is passed.
