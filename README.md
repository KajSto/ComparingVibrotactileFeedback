# ComparingVibrotactileFeedback
Unity Project for the 'Comparing Vibrotactile Feedback Modes for Navigation' paper

Available at: **not yet published**


## To run the Experiment:
- Open the project through the Unity Hub in Unity Editor 2022.3.20f1
- Open the scene 'Main Scene.unity' in Assets/Comp-Vibro-Feedback/Scenes/
- You can change the Participant ID and whether this is the first or second experimental session in the Inspector for the 'Experiment Controller' Object (default settings are Participant 1 & Session 1)
- Press the 'Play' icon at the top of the Unity Editor. The game will automatically run in VR if a headset is detected.
- For debugging purposes, the experiment can also run on PC with WASD controls to move, and QE to rotate the character. To enable this, click on the 'XR Interaction Setup' object and enable the 'Player Movement PC' script in the Inspector.
- The experiment will automatically loop through experimental conditions in Latin Square order dependent on the Participant ID set in 'Experiment Controller'
- After every trial (practice excluded), three save file will be created in the 'Streaming Assets' folder. The first file contains the position of the VR camera over time; The second file contains collision timestamps and coordinates; and the third contains the positions of any detected Optitrack markers over time.
- (To enable ![Optitrack](https://optitrack.com/), set the object 'Client - Optitrack - modified variant' to active, and set the attached script settings according to the Optitrack guide.)


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


## Project Contents:
- folder: Comp-Vibro-Feedback: contains all the assets used in actual experiment



### Other assets from unity asset store:
- bHaptics
- Gridbox Materials
- Optitrack



### Objects in Main Scene:
