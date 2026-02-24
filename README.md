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
<p float="left">
  <img src="Pictures/Environments/Maze/Variant1.png" height="200">
  <img src="Pictures/Environments/Maze/Variant2.png" height="200">
  <img src="Pictures/Environments/Maze/Variant3.png" height="200">
</p><p float="left">
  <img src="Pictures/Environments/Maze/Variant4.png" height="200">
  <img src="Pictures/Environments/Maze/Variant5.png" height="200">
  <img src="Pictures/Environments/Maze/Variant6.png" height="200">
</p><p float="left">
  <img src="Pictures/Environments/Maze/Variant7.png" height="200">
  <img src="Pictures/Environments/Maze/Variant8.png" height="200">
  <img src="Pictures/Environments/Maze/Variant9.png" height="200">
</p>

### Hallway
<p float="left">
  <img src="Pictures/Environments/Hallway/Variant1.png" height="200">
  <img src="Pictures/Environments/Hallway/Variant2.png" height="200">
  <img src="Pictures/Environments/Hallway/Variant3.png" height="200">
</p><p float="left">
  <img src="Pictures/Environments/Hallway/Variant4.png" height="200">
  <img src="Pictures/Environments/Hallway/Variant5.png" height="200">
  <img src="Pictures/Environments/Hallway/Variant6.png" height="200">
</p><p float="left">
  <img src="Pictures/Environments/Hallway/Variant7.png" height="200">
  <img src="Pictures/Environments/Hallway/Variant8.png" height="200">
  <img src="Pictures/Environments/Hallway/Variant9.png" height="200">
</p>

### Pillars
<p float="left">
  <img src="Pictures/Environments/Pillars/Variant1.png" height="180">
  <img src="Pictures/Environments/Pillars/Variant2.png" height="180">
  <img src="Pictures/Environments/Pillars/Variant3.png" height="180">
</p><p float="left">
  <img src="Pictures/Environments/Pillars/Variant4.png" height="180">
  <img src="Pictures/Environments/Pillars/Variant5.png" height="180">
  <img src="Pictures/Environments/Pillars/Variant6.png" height="180">
</p><p float="left">
  <img src="Pictures/Environments/Pillars/Variant7.png" height="180">
  <img src="Pictures/Environments/Pillars/Variant8.png" height="180">
  <img src="Pictures/Environments/Pillars/Variant9.png" height="180">
</p>


## Project Contents (Assets Folder)
- bHaptics: [The official bHaptics SDK](https://www.bhaptics.com/support/developers/?type=sdk)
- Comp-Vibro-Feedback: Contains all the assets used in actual experiment
  * Materials/Black: Material that makes the 'eyes' of the player avatar black
  * obj-files-vr-lab: Models for the 'base' environment representing the lab in which the experiment took place
  * Prefabs: Unity game-objects re-used throughout the experiment, such as obstacles for the environments
  * Scenes: The main .unity scene in which the experiment takes place
  * Scripts (see sub-section below)
  * Sound beeps/Beep1: The .mp3 which played at the start of trials (and when target was reached and when the trial ended)
- Gridbox Prototype Materials: [Materials from the Unity Asset Store](https://assetstore.unity.com/packages/2d/textures-materials/gridbox-prototype-materials-129127?srsltid=AfmBOoqcPSwr5sBgsnd2GTpNhlxB0St-Ofyp_wrep5zygIkb-ATu6SS_)
- Optitrack: [The official Optitrack Plugin](https://docs.optitrack.com/plugins/optitrack-unity-plugin)
- Plugins: Standard Unity asset for Android builds
- StreamingAssets: Folder in which data is saved during experiment
- TextMesh Pro: [Standard Unity Asset for UI Text](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/index.html)
- Viveport: [Official asset for controlling Unity VR trough HTV Vive](https://developer.vive.com/resources/viveport/sdk/documentation/english/viveport-sdk/integration-viveport-sdk/unity-developers/) (The experiment ran on the [HTV Vive Focus Vision](https://www.vive.com/eu/product/vive-focus-vision/overview/))
- XR, XR Interaction Toolkit, XRI: Standard Unity assets for VR use

### Scripts in Assets/Comp-Vibro-Feedback/Scripts
- ExperimentController.cs:
- HapticFeedback.cs:
- OptitrackMoment.cs: 
- OptitrackPointCollector.cs: 
- Pathfollowing.cs:
- PathObject.cs:
- PlayerMovementPC.cs:
- PlayerTracker.cs:
- Sensor.cs:
- Sensor_Compass.cs:
- Sensor_Directed.cs:
- Sensor_Surround.cs:
- UI_VestActivation.cs:
- VRHandheldCollider.cs: 
- VRPlayerBody.cs: 

### Objects in Main Scene:
- [bhaptics]: 
- ExperimentController: 
- HapticFeedback: 
- Environment: 
- Canvas: 
- XR Interaction Setup: 
- Client - Optitrack: 
