# ComparingVibrotactileFeedback
Unity Project for the 'Comparing Vibrotactile Feedback Modes for Navigation' paper

Available at: **not yet published**


## To run the Experiment:
- Open the project through the Unity Hub in Unity Editor 2022.3.20f1
- Open the scene 'Main Scene.unity' in Assets/Comp-Vibro-Feedback/Scenes/
- You can change the Participant ID and whether this is the first or second experimental session in the Inspector for the 'Experiment Controller' Object (default settings are Participant 1 & Session 1)
- Make sure the [bHapticsPlayer](https://www.bhaptics.com/) is running in the background and the Tactsuit X40 is connected
- Press the 'Play' icon at the top of the Unity Editor. The game will automatically run in VR if a headset is detected.
- For debugging purposes, the experiment can also run on PC with WASD controls to move, and QE to rotate the character. To enable this, click on the 'XR Interaction Setup' object and enable the 'Player Movement PC' script in the Inspector.
- The experiment will automatically loop through experimental conditions in Latin Square order dependent on the Participant ID set in 'Experiment Controller'
- After every trial (practice excluded), three save file will be created in the 'Streaming Assets' folder. The first file contains the position of the VR camera over time; The second file contains collision timestamps and coordinates; and the third contains the positions of any detected Optitrack markers over time.
- (To enable [Optitrack](https://optitrack.com/), set the object 'Client - Optitrack' to active, and set the attached script settings according to the Optitrack guide.)

## Pictures of Environments
### Mazes

|Variant 1|Variant 2|Variant 3|
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Maze/Variant1.png" height="200">|<img src="Pictures/Environments/Maze/Variant2.png" height="200">|<img src="Pictures/Environments/Maze/Variant3.png" height="200">|
|7.4 meters|7.3 meters|7.3 meters|

|Variant 4|Variant 5|Variant 6| 
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Maze/Variant4.png" height="200">|<img src="Pictures/Environments/Maze/Variant5.png" height="200">|<img src="Pictures/Environments/Maze/Variant6.png" height="200">|
|6.9 meters|7.3 meters|7.3 meters|

|Variant 7|Variant 8|Variant 9| 
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Maze/Variant7.png" height="200">|<img src="Pictures/Environments/Maze/Variant8.png" height="200">|<img src="Pictures/Environments/Maze/Variant9.png" height="200">|
|7.4 meters|7.3 meters|6.9 meters|

### Hallway
|Variant 1|Variant 2|Variant 3|
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Hallway/Variant1.png" height="200">|<img src="Pictures/Environments/Hallway/Variant2.png" height="200">|<img src="Pictures/Environments/Hallway/Variant3.png" height="200">|
|6.9 meters|7.0 meters|6.9 meters|

|Variant 4|Variant 5|Variant 6| 
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Hallway/Variant4.png" height="200">|<img src="Pictures/Environments/Hallway/Variant5.png" height="200">|<img src="Pictures/Environments/Hallway/Variant6.png" height="200">|
|6.9 meters|7.0 meters|7.0 meters|

|Variant 7|Variant 8|Variant 9| 
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Hallway/Variant7.png" height="180">|<img src="Pictures/Environments/Hallway/Variant8.png" height="180">|<img src="Pictures/Environments/Hallway/Variant9.png" height="180">|
|7.0 meters|6.9 meters|7.0 meters|

### Pillars
|Variant 1|Variant 2|Variant 3|
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Pillars/Variant1.png" height="180">|<img src="Pictures/Environments/Pillars/Variant2.png" height="180">|<img src="Pictures/Environments/Pillars/Variant3.png" height="180">|
|6.2 meters|6.1 meters|6.2 meters|

|Variant 4|Variant 5|Variant 6| 
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Pillars/Variant4.png" height="180">|<img src="Pictures/Environments/Pillars/Variant5.png" height="180">|<img src="Pictures/Environments/Pillars/Variant6.png" height="180">|
|6.2 meters|6.2 meters|6.2 meters|

|Variant 7|Variant 8|Variant 9| 
|:-------:|:-------:|:-------:|
|<img src="Pictures/Environments/Pillars/Variant7.png" height="180">|<img src="Pictures/Environments/Pillars/Variant8.png" height="180">|<img src="Pictures/Environments/Pillars/Variant9.png" height="180">|
|6.1 meters|6.2 meters|6.2 meters|

## Project Contents (Assets Folder)
- bHaptics: [The official bHaptics SDK](https://www.bhaptics.com/support/developers/?type=sdk)
- Comp-Vibro-Feedback: Contains all the assets used in actual experiment
  * Materials/Black: Material that makes the 'eyes' of the player avatar black
  * obj-files-vr-lab: Models for the 'base' environment representing the lab in which the experiment took place
  * Prefabs: Unity game-objects re-used throughout the experiment, such as obstacles for the environments
  * Scenes: The main .unity scene in which the experiment takes place
  * Scripts: (see sub-section below)
  * Sound Beeps/Beep1: The .mp3 which played at the start of trials (and when target was reached and when the trial ended)
- Gridbox Prototype Materials: [Materials from the Unity Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/gridbox-prototype-materials-129127?srsltid=AfmBOoqcPSwr5sBgsnd2GTpNhlxB0St-Ofyp_wrep5zygIkb-ATu6SS_) by [SurrealCosmic](https://surrealcosmic.com/) 
- Optitrack: [The official Optitrack Plugin](https://docs.optitrack.com/plugins/optitrack-unity-plugin)
- Plugins: Standard Unity asset for Android builds
- StreamingAssets: Folder in which data is saved during experiment
- TextMesh Pro: [Standard Unity Asset for UI Text](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/index.html)
- Viveport: [Official asset for controlling Unity VR trough HTV Vive](https://developer.vive.com/resources/viveport/sdk/documentation/english/viveport-sdk/integration-viveport-sdk/unity-developers/) (The experiment ran on the [HTV Vive Focus Vision](https://www.vive.com/eu/product/vive-focus-vision/overview/))
- XR, XR Interaction Toolkit, XRI: Standard Unity assets for VR use

### Scripts in Assets/Comp-Vibro-Feedback/Scripts
- ExperimentController.cs: Main script that loops through experimental conditions and enabled/disables environments and feedback accordingly. 
- HapticFeedback.cs: Main script that controls haptic feedback by checking output of the 'Sensor.cs' scripts and activating the haptic vest accordingly.
- OptitrackMoment.cs: class used when saving the optitrack data (for each timestamp have x/y/z coordinates of a specified marker)
- OptitrackPointCollector.cs: Script collects all opti-track markers currently in scene. Saves this as 'OptitrackMoment' classes. This data ended up not being used in analysis, as the headset position sufficed.
- Pathfollowing.cs: Used for the Guidance feedback mode, computes distance and direction between player and path.
- PathObject.cs: stores a path (series of points (x,y) along distance) for each environment variant to use in PathFollowing.cs.
- PlayerMovementPC.cs: Script to allow the player-object to be controlled with keyboard for debugging purposes.
- PlayerTracker.cs: Script that saves the participant's position/orientation throughout experiment.
- Sensor.cs: parent class for specialized 'sensor' scripts to inherit from.
- Sensor_Compass.cs: Sensor used in Guidance feedback, computes feedback in a set direction around the player.
- Sensor_Directed.cs: Sensor used in Hand-held feedback, computes distance to objects pointed at and computes feedback accordingly.
- Sensor_Surround.cs: Sensor used in Surround feedback, scans for obstacles in immediate surroundings and computes feedback accordingly.
- UI_VestActivation.cs: Script that manages a UI representation for activation of the haptic motors of the vest. Used for debugging and for the experiment leader to check if feedback works correct. 
- VRHandheldCollider.cs: Script tracks whether handheld controller is within an obstacle
- VRPlayerBody.cs: Script that has a 'body' (cylinder) follow below (40cm) and behind (8cm) the VR camera. This 'body' is used to measure collisions and position.

### Objects in Main Scene:
- [bhaptics]: From the bHaptics plugin. Required for haptic feedback on the vest.
- ExperimentController: Contains the ExperimentController.cs script. Can be useful for debugging: After starting the experiment, the experimenter van check (and potentially change) the order of conditions and trials in the Inspector for this object. Likewise, the experimenter can track the time of the current trial, and press checkbox-buttons to manually make obstacles or feedback modes (in)visible. 
- HapticFeedback: Contains the HapticFeedback.cs script and the Sensor.cs scripts. In case desired (i.e. for debugging), the experimenter can change settings such as whether collisions should provide feedback, the global vibration intensity, or the length of vibration pulses. 
- Environment: Contains the Lab visual as well as the Environment Types used in the experiment. For debugging, the experimenter can manually dis/enable experimental environments here. 
- Canvas: Shows a UI representation for activation of the haptic motors of the vest. Used for debugging and for the experiment leader to check if feedback works correct. Not visible in VR, thus only seen by the experimenter.
- XR Interaction Setup: Contains all VR requirements and the participant object. 
- Client - Optitrack: Optional, can be enabled if the experimenter wants to track optitrack marker data. Was not used in our analysis.
