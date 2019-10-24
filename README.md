# Live Record

- The LiveRecord is a Unity Debugging tool, designed to help developers and testers constantly catch and record bugs.
- It works by constantly keeping the last 600 frames in memory and saving the frames in memory to the persistent storage of the device with a click of a button.
- It captures the gameplay screen every 3 frames so the recording is in 20 frames per second for a 60 fps game.
- It allows users to record the previous 30 seconds from the time they press the record button, so it is practically a time machine.
- It is a debugging tool and is not suitable for recording high-quality footage.

## How to use

1) Import the LiveRecord unity package.
2) Move the LiveRecord prefab to the scene.
3) Configure the LiveRecord object by modifying the parameters.
    - **Image Count:** Number of images/frames stored in the memory.
    - **Scale Ratio:** The downscale ratio of the recorded frames.
    - **Jpeg Quality:** Quality of the Jpegs stored in persistent storage.
    - **Auto Start:** Whether the recording should start automatically inside the awake method or manually by calling the StartRecording() method.
    - **Start Delay:** How much the tool will wait before starting the recording.
    - **Capture Count:** Number of captures that will be kept on the persistent storage, the oldest capture will be deleted if the amount is passed.
4) Add the cameras you want to be rendered in the footage to the Cameras array.

![Live Record Inspector](https://github.com/ahmetayrnc/live_record/blob/master/images/live_record_inspector.png)

5) Move the SaveRecordingButton prefab to the scene, below a canvas.
6) Drag the LiveRecord object in the scene to the SaveRecordingButton script.

![Live Record Save Button](https://github.com/ahmetayrnc/live_record/blob/master/images/save_button_inspector.png)

7) Press the button while the game is running to save the last 30 seconds.

![Live Record Button in Action](https://github.com/ahmetayrnc/live_record/blob/master/images/live_record_gameplay.png)

### How to replay the saved footage
- The footage is recorded as Jpegs to the persistent data path.
- Unity Doc Link: https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
- The footage can be converted to mp4 using the Python script in the repo.
- The footage can also be replayed inside the game using the Playback Scene provided in the package.

### Using the Playback Scene
- The number of available footage is shown on the bottom left corner.
- Enter the desired number of the footage to the input field on the bottom left corner.
- Then press the Load button.
- After the footage is loaded, the buttons and the slider can be used to control the playback.

![Live Record Button in Action](https://github.com/ahmetayrnc/live_record/blob/master/images/live_record_playback.png)

### Notes:
Public methods:
- **StartRecording():** Starts the background recording process.
- **StopRecording():** Stops the background recording process.
- **SaveCapture(...):** Saves the last 30 seconds to persistent storage.
