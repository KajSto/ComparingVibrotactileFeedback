using Bhaptics.SDK2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

//script used to manage conditions and data and such
public class ExperimentController : MonoBehaviour
{
    [Header(("References (do not touch)"))]
    public HapticFeedback hapticScript; //script that controls haptic feedback
    public PathFollowing wayfindScript; //script that controls waypoint condition
    public PlayerTracker trackerScript; //script that tracks position/orientation of participant
    public VRPlayerBody bodyScript;     //script that tracks collisions for participant
    public Camera vrCamera; //camera used by participant
    public AudioSource startSoundSource;
    public OptitrackPointCollector optitrackTracker;

    //references used to dis/enable environments
    public Transform practiceParent;
    public Transform mazeParent;
    public Transform hallwayParent;
    public Transform obstacleParent;

    [Header(("Exeriment Control"))]
    public int participantID; //manually set to keep track of participants
    public bool secondSession; //manually set to keep track of 1st or second session
    public bool enableControllerENTER; //enables the VR controller to press continue (ONLY ENABLE THIS IN DEBUG)
    public InputDevice rightController;
    public InputDevice leftController;
    public Transform EnvironmentFullParent;
    public float trialTime;

    public enum feedback { surround, directed, waypoint, visual }
    public enum environment { maze, hallway, obstacle }

    public feedback[] feedback_order;
    public environment[] environment_order;
    public int[] variant_order;

    public bool buttonContinue; //used by experiment leader to have experiment proceed to next stages --> activate by pressing spacebar
    //private bool wait; //only used in ending/setting the old familiarization phase --> remove?

    [Header("'Buttons' for debugging")]
    public bool buttonRecenterEnvironment = false;

    public bool togglePauseTracker = false;
    public bool buttonClearTracker = false;

    public bool buttonVisualize = false;
    public bool buttonPlayback = false;

    public bool buttonBlindness = false;
    public bool buttonUnblind = false;

    public bool buttonObstaclesInvisible;
    public bool buttonObstaclesVisible;

    public bool buttonRaycastInvisible;
    public bool buttonRaycastVisible;

    public bool buttonManualHapticFeedback;

    [Header("Playback prefabs")]
    public GameObject playbackHeadObject;
    public GameObject playbackBodyObject;
    public GameObject playbackControllerRightObject;
    public GameObject playbackControllerLeftObject;

    public GameObject playbackCollisionError1;
    public GameObject playbackCollisionError2;
    public GameObject playbackCollisionObstacle;


    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.seed = participantID; //set random seed based on PID --> this ensures that randomization can be repeated (just in case)

        //ShuffleConditionOrder();
        LatinSquareConditionOrder();
        StartCoroutine(ExperimentManager());
        StartCoroutine(ManualHapticFeedback()); //to test if vest is working on set up.
    }

    // Update loops to check button presses (mainly for debug)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            buttonContinue = true;
        }        

        //continue button for VR controller (SHOULD ONLY BE USABLE IN DEBUG MODE!)
        if (enableControllerENTER)
        {
            if (!rightController.isValid)
            {
                List<InputDevice> devices = new List<InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, devices);
                if (devices.Count > 0) { rightController = devices[0]; }
                if (devices.Count > 1) { rightController = devices[1]; }
            }

            rightController.TryGetFeatureValue(CommonUsages.gripButton, out bool value);
            if (value)
            {
                buttonContinue = true;
            }

            rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool valuee);
            if (valuee) { buttonRecenterEnvironment = true; }
        }

        //debug button to re-center the environment around the VR direction
        if (buttonRecenterEnvironment)
        {
            buttonRecenterEnvironment = false;

            Vector3 VRPositon = vrCamera.transform.position;

            EnvironmentFullParent.parent = vrCamera.transform;
            EnvironmentFullParent.localPosition = new Vector3(0,EnvironmentFullParent.transform.localPosition.y,0); //only change X and Z position
            EnvironmentFullParent.localEulerAngles = new Vector3(0, 0, 0);
            EnvironmentFullParent.parent = transform.parent; //rest back to global parent
            EnvironmentFullParent.localEulerAngles = new Vector3(0, EnvironmentFullParent.transform.localEulerAngles.y, 0); //only change Y orientation
        }


        //debug functions (mainly activated through checkboxes in inspector)
        if (buttonVisualize)
        {
            VisualizeTracker(trackerScript.locationStorage);
            StartCoroutine(VisualizeCollisions(bodyScript.collisionPosition, bodyScript.collisionObject, trackerScript.locationStorage.Count * 0.05f));
            buttonVisualize = false;
        }
        if (buttonPlayback)
        {
            StartCoroutine(PlaybackTracker(trackerScript.locationStorage, trackerScript.rotationStorage, 
                trackerScript.locationControllerLeftStorage, trackerScript.rotationControllerLeftStorage, 
                trackerScript.locationControllerRightStorage, trackerScript.rotationControllerRightStorage));
            buttonPlayback = false;
        }

        if (buttonObstaclesInvisible)
        {
            TurnObstaclesInvisible();
            buttonObstaclesInvisible=false;
        }
        if (buttonObstaclesVisible)
        {
            TurnObstaclesVisible();
            buttonObstaclesVisible = false;
        }
        if (buttonClearTracker)
        {
            ClearTrackerData();
            buttonClearTracker = false;
        }
        if (togglePauseTracker)
        {
            trackerScript.pauseTracking = true;
        }
        else
        {
            trackerScript.pauseTracking = false;
        }

        if (buttonRaycastInvisible)
        {
            TurnRaycastInvisible();
            buttonRaycastInvisible = false;
        }

        if (buttonRaycastVisible)
        {
            TurnRaycastVisible();
            buttonRaycastVisible = false;
        }

        if (buttonManualHapticFeedback)
        {
            StartCoroutine(ManualHapticFeedback());
        }
    }


    //Main code managing the experiment
    IEnumerator ExperimentManager()
    {
        //check starting stuff
        hapticScript.DisableAllSensors(); //turns off all sensors (might be accidentally turned on during debug)
        TurnObstaclesInvisible();


        print("Check if environment and VR headset location are synched. Press ENTER to proceed");

        while (!buttonContinue) { yield return new WaitForSeconds(0.5f); }
        yield return new WaitForSeconds(0.2f); //to avoid accidentally double-clicking the continue button
        buttonContinue = false; //reset button
        enableControllerENTER = false; 


        //wait until continue-button is pressed by experiment leader
        print("Waiting for experiment leader to press continue");
        while (!buttonContinue) { yield return new WaitForSeconds(0.5f); }
        yield return new WaitForSeconds(0.2f); //to avoid accidentally double-clicking the continue button
        buttonContinue = false; //reset button
        hapticScript.disableVest = true; //turns off haptic feedback in general, even when sensor is turned on later in script



        //MAIN FUNCTION: loop through the experimental conditions
        int trial_index = 1;
        for (int i_feedback = 0; i_feedback < 4; i_feedback++)
        {
            for (int i_environment = 0; i_environment < 3; i_environment++)
            {
                feedback con_feedback = feedback_order[i_feedback];
                environment con_environment = environment_order[i_environment];                
                print("Starting environment index " + i_environment + " (" + con_environment + ") for feedback mode: " + con_feedback);

                //practice with the current feedback-environment combination (except in case of visual condition)
                //Code is essentially a copy of the experimental trials, minus saving of data and visual settings
                if (con_feedback != feedback.visual)
                {
                    //shuffling the three practice environments
                    int[] practiceEnvironments = new int[] { 0, 1, 2 };
                    practiceEnvironments = ShuffleArray(practiceEnvironments);

                    for (int practiceIndex = 0; practiceIndex <= 1; practiceIndex++)
                    {
                        print("Practice trial " + (practiceIndex + 1));

                        int practiceEnvironmentID = practiceEnvironments[practiceIndex];

                        Transform environmentParent = SetUpCondition(con_feedback, con_environment, 12 - practiceEnvironmentID); 
                        #region copiedCode
                        //press button when participant is ready for trial
                        print("Instruct participant to walk towards start of environment. \n Press continue when ready to start trial.");
                        Transform startObject = environmentParent.GetChild(2).GetChild(2).GetChild(0);
                        float distance = DistanceParticipantToEnd(startObject);
                        buttonContinue = false; //extra check, in case button was accidentally clicked during
                        while (!buttonContinue | distance > 0.3f)
                        {
                            buttonContinue = false;
                            distance = DistanceParticipantToEnd(startObject);
                            //print(distance);
                            yield return new WaitForSeconds(0.5f);
                        }
                        yield return new WaitForSeconds(0.2f); //to avoid accidentally double-clicking the continue button
                        buttonContinue = false; //reset button

                        //for the first practice, enable visual feedback as well
                        if (practiceIndex == 0)
                        {
                            TurnObstaclesVisible();
                            TurnRaycastVisible();
                        }
                        //if (practiceIndex == 1) 
                        //{
                        //    TurnRaycastVisible();
                        //}

                        //after button press: enable haptic feedback and rest of environment
                        PlayStartSound(); //signal to participant that we start
                        yield return new WaitForSeconds(1.6f); //hard-coded to wait for audio source to stop playing

                        hapticScript.disableVest = false;


                        print("Environment & Feedback activated. Practice Trial Started; \n Waiting for participant to reach target position");
                        Transform endObject = environmentParent.GetChild(2).GetChild(2).GetChild(1);
                        distance = DistanceParticipantToEnd(endObject);
                        while (distance > 0.3f)
                        {
                            distance = DistanceParticipantToEnd(endObject);
                            //print(distance);
                            yield return new WaitForEndOfFrame();
                        }


                        print("Participant reached target. Now waiting for participant to reach back to start.");
                        PlayStartSound(); //inform participant they reached target
                        distance = DistanceParticipantToEnd(startObject);
                        wayfindScript.reverseDirection = true; //this only relevant for the waypoint condition; Reverses the direction pointed along path

                        while (distance > 0.3f)
                        {
                            distance = DistanceParticipantToEnd(startObject);
                            //print(distance);
                            yield return new WaitForEndOfFrame();
                        }


                        print("participant reached end! Trial over. Disabling feedback and environment. Saving data.");
                        PlayStartSound(); //inform participant they reached target


                        //reset settings of this condition 
                        TurnObstaclesInvisible();
                        TurnRaycastInvisible();

                        environmentParent.GetChild(0).GetChild(12 - practiceEnvironmentID - 1).gameObject.SetActive(false); //de-activates the variant //INDEX CURRENTLY HARDCODED, SHOULD BE A SPECIAL PRACTICE ENVs.
                        environmentParent.gameObject.SetActive(false); //de-activates the environment

                        hapticScript.DisableAllSensors();
                        hapticScript.disableVest = true;
                        wayfindScript.reverseDirection = false;
                        hapticScript.checkWithinObstacleHands = false;
                        #endregion
                    }
                }

                print(""); //add some white space in debug

                //do the experimental trials
                for (int i_variant = 0; i_variant < 9; i_variant++)
                {
                    //depending on whether this is the first or second session, we only do the first or second half of variants
                    if (!secondSession) // --> first session
                    {
                        if (i_variant + 1 > 4) { continue; } // --> skip variants 4,6,7,8 and 9
                    }
                    else if (secondSession)
                    {
                        if (i_variant + 1 < 5) { continue; } // --> skip variants 1,2,3 and 4
                    }

                    int con_variant = variant_order[i_variant];
                    print("Starting experimental trial " + trial_index + ": " + con_feedback + " " + con_environment + " variant " + con_variant);

                    //use a helper function to retrieve the environment-parent and set the feedback settings
                    Transform environmentParent = SetUpCondition(con_feedback, con_environment, con_variant);

                    //press button when participant is ready for trial
                    print("Instruct participant to walk towards start of environment. \n Press continue when ready to start trial.");
                    Transform startObject = environmentParent.GetChild(2).GetChild(2).GetChild(0);
                    float distance = DistanceParticipantToEnd(startObject);
                    buttonContinue = false; //extra check, in case button was accidentally clicked during
                    while (!buttonContinue | distance > 0.3f)
                    {
                        buttonContinue = false;
                        distance = DistanceParticipantToEnd(startObject);
                        //print(distance);
                        yield return new WaitForSeconds(0.5f);
                    }
                    yield return new WaitForSeconds(0.2f); //to avoid accidentally double-clicking the continue button
                    buttonContinue = false; //reset button


                    //after button press: enable haptic feedback and rest of environment
                    PlayStartSound(); //signal to participant that we start
                    yield return new WaitForSeconds(1.6f); //hard-coded to wait for audio source to stop playing
                    
                    hapticScript.disableVest = false;

                    //in case of visual: special case
                    if (con_feedback == feedback.visual)
                    {
                        hapticScript.disableVest = true;
                        TurnObstaclesVisible();
                    }

                    //reset and activate location tracker
                    togglePauseTracker = false;
                    ClearTrackerData();
                    optitrackTracker.ClearMarkerData();
                    optitrackTracker.trackerActive = true;

                    trialTime = 0;

                    print("Environment & Feedback activated. Tracker Activated. Trial Started; \n Waiting for participant to reach target position");
                    Transform endObject = environmentParent.GetChild(2).GetChild(2).GetChild(1);
                    distance = DistanceParticipantToEnd(endObject);
                    while (distance > 0.3f)
                    {
                        distance = DistanceParticipantToEnd(endObject);
                        //print(distance);
                        trialTime += Time.deltaTime;

                        if (trialTime > 120)
                        {
                            //print("Time limit of 120 seconds reached. Ending the Trial.");
                            distance = 0;
                        }
                        yield return new WaitForEndOfFrame();
                    }


                    print("Participant reached target. Now waiting for participant to reach back to start.");
                    PlayStartSound(); //inform participant they reached target
                    distance = DistanceParticipantToEnd(startObject);
                    wayfindScript.reverseDirection = true; //this only relevant for the waypoint condition; Reverses the direction pointed along path

                    while (distance > 0.3f)
                    {
                        distance = DistanceParticipantToEnd(startObject);
                        //print(distance);
                        trialTime += Time.deltaTime;
                        if (trialTime > 120)
                        {
                            print("Time limit of 120 seconds reached. Ending the Trial.");
                            distance = 0;
                        }
                        yield return new WaitForEndOfFrame();
                    }


                    print("participant reached end! Trial over. Disabling feedback and environment. Saving data.");
                    PlayStartSound(); //inform participant they reached target


                    //reset settings of this condition 
                    TurnObstaclesInvisible();

                    //special case for waypoint: turn the path inactive, so that it does not show up in the visual condition
                    if (con_feedback == feedback.waypoint) {
                        Transform variantParent = environmentParent.GetChild(0).GetChild(con_variant - 1);
                        variantParent.GetChild(variantParent.childCount-1).gameObject.SetActive(false);
                    }

                    environmentParent.GetChild(0).GetChild(con_variant-1).gameObject.SetActive(false); //de-activates the variant
                    environmentParent.gameObject.SetActive(false); //de-activates the environment

                    hapticScript.DisableAllSensors();
                    hapticScript.disableVest = true;
                    wayfindScript.reverseDirection = false;
                    hapticScript.checkWithinObstacleHands = false;
                    togglePauseTracker = true;
                    optitrackTracker.trackerActive = false;


                    //Save data for this trial (trajectory and collisions for this trial)
                    SaveTrackerData(con_feedback,con_environment,con_variant,trial_index);

                    print("finished with this experimental condition. Proceeding to next.");
                    trial_index++;
                }

                print(""); //add extra whitespace between environments for overview in debug
            }

            //after finishing all environments for this feedback type, take a break for the questionnaire
            if (secondSession)
            {
                print("Instruct the participant to take off the VR headset and fill in the survey");
                yield return new WaitForSeconds(10);
            }

            print("");

        }


        print("Completed all experimental conditions, the experiment is over :)");
        yield return null;
    }


    //sets the correct haptic feedback, environment and variant according to condition (and returns the correct environment parent)
    Transform SetUpCondition(feedback con_fb, environment con_env, int con_var) 
    {
        //environment:
        Transform environmentParent = null;
        if (con_env == environment.maze)
        {
            environmentParent = mazeParent;
        }
        else if (con_env == environment.hallway)
        {
            environmentParent = hallwayParent;

            //special case for hallway: the start/end positions have to be set manually --> base this on the waypoints/path                   
            Transform startObj = environmentParent.GetChild(2).GetChild(2).GetChild(0);
            Transform endObj = environmentParent.GetChild(2).GetChild(2).GetChild(1);

            Transform waypoints = hallwayParent.GetChild(0).GetChild(con_var-1).GetChild(0);

            Vector3 waypointStart = waypoints.GetChild(0).position;
            Vector3 waypointEnd = waypoints.GetChild(waypoints.childCount - 1).position;

            startObj.transform.position = new Vector3(waypointStart.x, 1.5f, waypointStart.z);
            endObj.transform.position = new Vector3(waypointEnd.x, 1.5f, waypointEnd.z);
        }
        else if (con_env == environment.obstacle)
        {
            environmentParent = obstacleParent;
        }

        environmentParent.gameObject.SetActive(true); //activate main object
        environmentParent.GetChild(0).GetChild(con_var - 1).gameObject.SetActive(true); //activate correct variant


        //feedback:
        if (con_fb == feedback.directed)
        {
            hapticScript.sensor_Directed.enableFeedback = true;
            hapticScript.sensor_Directed.debugVisualize = true;
            hapticScript.checkWithinObstacleHands = true;
        }
        else if (con_fb == feedback.surround)
        {
            hapticScript.sensor_Surround.enableFeedback = true;
            hapticScript.sensor_Surround.debugVisualize = true;
        }
        else if (con_fb == feedback.waypoint)
        {
            hapticScript.sensor_Compass.enableFeedback = true;
            hapticScript.sensor_Compass.debugVisualize = true;
            //special case: enable seperate waypoint script as well
            wayfindScript.LoadNewPath(environmentParent.GetChild(0).GetChild(con_var-1).GetChild(0)); //give the 'path' object as path
        }
        TurnRaycastInvisible(); //only works after activatin above

        return environmentParent; //return so that can be fully activated later.
    }



    // HELPER FUNCTIONS *********************************************************************************************************************************************************
    //helper function to compute distance of participant to end object (for tracking progress?)
    float DistanceParticipantToEnd(Transform endObject)
    {
        Vector3 playerPos = hapticScript.playerObject.transform.position;
        Vector3 endPos = endObject.position;

        Vector3 heading = endPos - playerPos;
        Vector2 heading2D = new Vector2(heading.x, heading.z);

        return heading2D.magnitude;
    }

    //send haptic feedback to lowest row (used as signal when starting/finishing task) (replaced with audio signal now)
    IEnumerator ManualHapticFeedback()
    {
        //imagined signal: x number of short bursts
        int number = 3;
        int signalLength = 200;

        //create signal: 80 strength on lowest row around body
        int[] feedbackArray = new int[40];
        feedbackArray[16] = 80;
        feedbackArray[17] = 80;
        feedbackArray[18] = 80;
        feedbackArray[19] = 80;
        feedbackArray[36] = 80;
        feedbackArray[37] = 80;
        feedbackArray[38] = 80;
        feedbackArray[39] = 80;

        for (int i = 0; i < number; i++)
        {
            BhapticsLibrary.PlayMotors(
                position: (int)Bhaptics.SDK2.PositionType.Vest,
                motors: feedbackArray,
                durationMillis: signalLength
            );

            //print("MANUAL HAPTIC SIGNAL SEND"); //debug message

            yield return new WaitForSeconds((float)signalLength / 1000);
            yield return new WaitForSeconds((float)signalLength / 1000);
        }

        yield return null;
    }

    //replacement for manual haptic feedback. less confusing.
    void PlayStartSound()
    {
        startSoundSource.Play();
    }


    // CODE RELATED TO CONDITION GENERATION **************************************************************************************************************************************
    
    //helper function that shuffles the order of feedback and environment based on the ParticipantID
    void ShuffleConditionOrder()
    {
        //first shuffle the feedback conditions (but keep visual last)
        int[] order_f = new int[] { 1, 2, 3 }; //each int (1,2,3) refers to a feedback mode
        order_f = ShuffleArray(order_f); //shuffle the order (1,2,3)
        for (int i = 0; i < order_f.Length; i++) //loop through new ordering and assign corresponding feedback modes
        {
            int f = order_f[i];
            feedback new_f = feedback.visual;
            if (f == 1) { new_f = feedback.surround; }
            if (f == 2) { new_f = feedback.directed; }
            if (f == 3) { new_f = feedback.waypoint; }
            feedback_order[i] = new_f;
        }

        //second shuffle environment conditions
        int[] order_e = new int[] { 1, 2, 3 }; //each int (1,2,3) refers to an environment type
        order_e = ShuffleArray(order_e); //shuffle the order (1,2,3)
        for (int i = 0; i < order_e.Length; i++) //loop through new ordering and assign corresponding environment types
        {
            int f = order_e[i];
            environment new_e = environment.maze;
            if (f == 1) { new_e = environment.maze; }
            if (f == 2) { new_e = environment.hallway; }
            if (f == 3) { new_e = environment.obstacle; }
            environment_order[i] = new_e;
        }

        //thirdly shuffle variant order
        variant_order = ShuffleArray(variant_order);

    }

    //helper-helper function for shuffling experimental conditions --> Knuth shuffle algorithm found on Wikipedia
    int[] ShuffleArray(int[] input)
    {
        for (int t = 0; t < input.Length; t++)
        {
            int tmp = input[t];
            int r = UnityEngine.Random.Range(t, input.Length);
            input[t] = input[r];
            input[r] = tmp;
        }
        return input;
    }

    //instead of random shuffle for condition order, utilize pseudo-random based on PID
    //goal: have ~equal sequence of feedback modes and environments for the 20 participants
    void LatinSquareConditionOrder()
    {
        //if 18 participants are done, can move to full random instead
        if (participantID > 18) { ShuffleConditionOrder(); return; }

        //first: the sequence of feedback --> have 3 of each possible combination
        if (participantID >= 1 && participantID <= 6)
        {
            feedback_order[0] = feedback.surround;

            if (participantID >= 1 && participantID <= 3)
            {
                feedback_order[1] = feedback.directed;
                feedback_order[2] = feedback.waypoint;
            }
            else if (participantID >= 4 && participantID <= 6)
            {
                feedback_order[1] = feedback.waypoint;
                feedback_order[2] = feedback.directed;
            }
        }
        else if (participantID >= 7 && participantID <= 12)
        {
            feedback_order[0] = feedback.directed;

            if (participantID >= 7 && participantID <= 9)
            {
                feedback_order[1] = feedback.surround;
                feedback_order[2] = feedback.waypoint;
            }
            else if (participantID >= 10 && participantID <= 12)
            {
                feedback_order[1] = feedback.waypoint;
                feedback_order[2] = feedback.surround;
            }
        }
        else if (participantID >= 13 && participantID <= 18)
        {
            feedback_order[0] = feedback.waypoint;

            if (participantID >= 13 && participantID <= 15)
            {
                feedback_order[1] = feedback.surround;
                feedback_order[2] = feedback.directed;
            }
            else if (participantID >= 16 && participantID <= 18)
            {
                feedback_order[1] = feedback.directed;
                feedback_order[2] = feedback.surround;
            }
        }

        //second: the sequence of environment --> cannot do all x feedback --> ensure each environment goes 1st/2nd/3rd at least once in the 3 feedback repetitions
        if (participantID % 6 == 1)
        {
            environment_order[0] = environment.maze;
            environment_order[1] = environment.hallway;
            environment_order[2] = environment.obstacle;
        }
        else if (participantID % 6 == 2)
        {
            environment_order[0] = environment.obstacle;
            environment_order[1] = environment.maze;
            environment_order[2] = environment.hallway;
        }
        else if (participantID % 6 == 3)
        {
            environment_order[0] = environment.hallway;
            environment_order[1] = environment.obstacle;
            environment_order[2] = environment.maze;
        }
        else if (participantID % 6 == 4)
        {
            environment_order[0] = environment.maze;
            environment_order[1] = environment.obstacle;
            environment_order[2] = environment.hallway;
        }
        else if (participantID % 6 == 5)
        {
            environment_order[0] = environment.obstacle;
            environment_order[1] = environment.hallway;
            environment_order[2] = environment.maze;
        }
        else if (participantID % 6 == 0)
        {
            environment_order[0] = environment.hallway;
            environment_order[1] = environment.maze;
            environment_order[2] = environment.obstacle;
        }

        //third: the order of variants can be full random. //Note that, because random seed is based on PID, the order will be the same for the second session :)
        variant_order = ShuffleArray(variant_order);
        
    }



    // CODE RELATED TO TRACKING, SAVING AND VISUALIZING DATA *********************************************************************************************************************

    //write two csv's. One with tracker position/orientation, another with collision data
        //Retrieve this info from the current tracker- and body-script
    public void SaveTrackerData(feedback con_fb, environment con_env, int con_variant, int con_index)
    {
        // position/orientation: ////////////////////////////////////////////////////////////
        // Each row: timestamp / position x y z / orientation w x y z

        List<float> timestampStorage = trackerScript.timestampStorage;
        List<Vector3> locationStorage = trackerScript.locationStorage;   
        List<Quaternion> rotationStorage = trackerScript.rotationStorage;

        List<Vector3> locationControllerLeftStorage = trackerScript.locationControllerLeftStorage;
        List<Quaternion> rotationControllerLeftStorage = trackerScript.rotationControllerLeftStorage;
        List<Vector3> locationControllerRightStorage = trackerScript.locationControllerRightStorage;
        List<Quaternion> rotationControllerRightStorage = trackerScript.rotationControllerRightStorage;

        StringBuilder sb = new System.Text.StringBuilder();

        //add headers
        sb.AppendLine("timestamp;" +
            "posX;posY;posZ;rotW;rotX;rotY;rotZ;" +
            "controllerLeftPosX;controllerLeftPosY;controllerLeftPosZ;controllerLeftRotW;controllerLeftRotX;controllerLeftRotY;controllerLeftRotZ;" +
            "controllerRightPosX;controllerRightPosY;controllerRightPosZ;controllerRightRotW;controllerRightRotX;controllerRightRotY;controllerRightRotZ;");

        //add rows
        for (int i = 0; i < timestampStorage.Count; i++)
        {
            sb.AppendLine(timestampStorage[i].ToString("0.000") + ';' + 
                locationStorage[i].x.ToString("0.000") + ";" + locationStorage[i].y.ToString("0.000") + ";" + locationStorage[i].z.ToString("0.000") + ";" +
                rotationStorage[i].w.ToString("0.000") + ";" + rotationStorage[i].x.ToString("0.000") + ";" + rotationStorage[i].y.ToString("0.000") + ";" + rotationStorage[i].z.ToString("0.000") + ";" +
                locationControllerLeftStorage[i].x.ToString("0.000") + ";" + locationControllerLeftStorage[i].y.ToString("0.000") + ";" + locationControllerLeftStorage[i].z.ToString("0.000") + ";" +
                rotationControllerLeftStorage[i].w.ToString("0.000") + ";" + rotationControllerLeftStorage[i].x.ToString("0.000") + ";" + rotationControllerLeftStorage[i].y.ToString("0.000") + ";" + rotationControllerLeftStorage[i].z.ToString("0.000") + ";" +
                locationControllerRightStorage[i].x.ToString("0.000") + ";" + locationControllerRightStorage[i].y.ToString("0.000") + ";" + locationControllerRightStorage[i].z.ToString("0.000") + ";" +
                rotationControllerRightStorage[i].w.ToString("0.000") + ";" + rotationControllerRightStorage[i].x.ToString("0.000") + ";" + rotationControllerRightStorage[i].y.ToString("0.000") + ";" + rotationControllerRightStorage[i].z.ToString("0.000") + ";"
                );
        }

        //complete string
        string contentData = sb.ToString();

        //create save file
        var folder = Application.streamingAssetsPath;
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        //generate name
        string conditionInfo = "";

        //if (con != null) //unsure if this was safe to remove, but seems useless? -january 2025
        //{
        conditionInfo += "Participant";
        conditionInfo += participantID;
        conditionInfo += "_";
        conditionInfo += "Session";
        if (secondSession) { conditionInfo += "2"; }
        else { conditionInfo += "1"; }
        conditionInfo += "_";
        conditionInfo += "trial";
        conditionInfo += con_index.ToString();
        conditionInfo += "_";
        conditionInfo += con_fb.ToString();
        conditionInfo += "_";
        conditionInfo += con_env.ToString();
        conditionInfo += "_";
        conditionInfo += "variant";
        conditionInfo += con_variant.ToString();
        //}

        string dateTime = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string saveName = conditionInfo + "_PositionData_" + dateTime + ".csv";

        var filePath = Path.Combine(folder, saveName);

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(contentData);
        }


        //collision information: ////////////////////////////////////////////////////////////
        // Each row: timestamp / object / coordinates x y z / rotation w x y z 

        List<float> collisionTimestamp = bodyScript.collisionTimestamp;
        List<string> collisionObject = bodyScript.collisionObject;
        List<Vector3> collisionPosition = bodyScript.collisionPosition;
        List<Quaternion> collisionRotation = bodyScript.collisionRotation;

        StringBuilder sb2 = new System.Text.StringBuilder();

        //add headers
        sb2.AppendLine("timestamp;object;posX;posY;posZ;rotW;rotX;rotY;rotZ");

        //add rows
        for (int i = 0; i < collisionTimestamp.Count; i++)
        {
            sb2.AppendLine(collisionTimestamp[i].ToString("0.000") + ';' + collisionObject[i].ToString() + ";" +
                collisionPosition[i].x.ToString("0.000") + ";" + collisionPosition[i].y.ToString("0.000") + ";" + collisionPosition[i].z.ToString("0.000") + ";" +
                collisionRotation[i].w.ToString("0.000") + ";" + collisionRotation[i].x.ToString("0.000") + ";" + collisionRotation[i].y.ToString("0.000") + ";" + collisionRotation[i].z.ToString("0.000"));
        }

        //complete string
        string contentData2 = sb2.ToString();

        //save
        saveName = conditionInfo + "_CollisionData_" + dateTime + ".csv";
        filePath = Path.Combine(folder, saveName);
        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(contentData2);
        }

        print("data saved succesfully");


        //Optitrack information: ////////////////////////////////////////////////////////////
        optitrackTracker.SaveMarkerData(conditionInfo);

    }

    //empty the currently stored tracker and collision data
    public void ClearTrackerData()
    {
        trackerScript.locationStorage.Clear();
        trackerScript.rotationStorage.Clear();
        trackerScript.timestampStorage.Clear();

        trackerScript.locationControllerLeftStorage.Clear();
        trackerScript.locationControllerRightStorage.Clear();
        trackerScript.rotationControllerLeftStorage.Clear();
        trackerScript.rotationControllerRightStorage.Clear();

        bodyScript.collisionPosition.Clear();
        bodyScript.collisionRotation.Clear();
        bodyScript.collisionTimestamp.Clear();
        bodyScript.collisionObject.Clear();

    }

    //given a list of coordinates, draw a line between the points
    public void VisualizeTracker(List<Vector3> tracker)
    {
        for (int i = 0; i < tracker.Count - 1; i++)
        {
            Debug.DrawLine(tracker[i], tracker[i + 1], Color.red, tracker.Count * 0.05f);
        }
    }

    //visualize collision with error zones and walls themselves
    //Note that these are visualized at the location of the player during collision, not the exact point of the collision object
    IEnumerator VisualizeCollisions(List<Vector3> positions, List<String> types, float time)
    {
        List<GameObject> playbackCollisionObjects = new List<GameObject>();

        for (int i = 0; i < positions.Count; i++)
        {
            //base object type (only difference is color) on what type of collision it what
            if (types[i].Equals("error zone 1"))
            {
                GameObject collisionObject = Instantiate(playbackCollisionError1, positions[i], Quaternion.identity, transform);
                playbackCollisionObjects.Add(collisionObject);
            }
            else if (types[i].Equals("error zone 2"))
            {
                GameObject collisionObject = Instantiate(playbackCollisionError2, positions[i], Quaternion.identity, transform);
                playbackCollisionObjects.Add(collisionObject);
            }
            else if (types[i].Equals("obstacle"))
            {
                GameObject collisionObject = Instantiate(playbackCollisionObstacle, positions[i], Quaternion.identity, transform);
                playbackCollisionObjects.Add(collisionObject);
            }
        }

        yield return new WaitForSeconds(time);

        for (int i = 0; i < playbackCollisionObjects.Count; i++)
        {
            Destroy(playbackCollisionObjects[i]);            
        }

        yield return null;

    }

    //visualize a player object moving along the provided track
    IEnumerator PlaybackTracker(List<Vector3> posTracker, List<Quaternion> oriTracker,
       List<Vector3> posControlLeftTracker, List<Quaternion> oriControlLeftTracker, List<Vector3> posControlRightTracker, List<Quaternion> oriControlRightTracker)
    {
        GameObject replayModel = Instantiate(playbackHeadObject, posTracker[0], Quaternion.identity);
        GameObject replayModelBody = Instantiate(playbackBodyObject, posTracker[0], Quaternion.identity);
        replayModelBody.GetComponent<VRPlayerBody>().CameraPosition = replayModel.transform; //tell the body to follow the head

        GameObject replayModelControlLeft = Instantiate(playbackControllerLeftObject, posTracker[0], Quaternion.identity);
        GameObject replayModelControlRight = Instantiate(playbackControllerRightObject, posTracker[0], Quaternion.identity);


        for (int i = 0; i < posTracker.Count; i++)
        {
            //currently very basic, just jumping from point to point
            replayModel.transform.position = posTracker[i];
            replayModel.transform.rotation = oriTracker[i];

            replayModelControlLeft.transform.position = posControlLeftTracker[i];
            replayModelControlRight.transform.position = posControlRightTracker[i];

            replayModelControlLeft.transform.rotation = oriControlLeftTracker[i];
            replayModelControlRight.transform.rotation = oriControlRightTracker[i];

            //The actual model used is in just slightly different coordinates
            replayModelControlLeft.transform.Rotate(0, 180, 0, Space.Self);
            replayModelControlRight.transform.Rotate(0, 180, 0, Space.Self);
            replayModelControlLeft.transform.Translate(0, 0, 0.05f, Space.Self);
            replayModelControlRight.transform.Translate(0, 0, 0.05f, Space.Self);

            yield return new WaitForSeconds(0.05f);
        }

        Destroy(replayModelBody);
        Destroy(replayModel);
        Destroy(replayModelControlLeft);
        Destroy(replayModelControlRight);

        yield return null;
    }


    // CODE RELATED CONTROLLING (OBSTACLE) VISIBILITY *****************************************************
    public void TurnObstaclesInvisible()
    {
        //originalLayerMask &= ~(1 << layerToRemove);
        vrCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Obstacles"));
    }

    public void TurnObstaclesVisible()
    {
        //originalLayerMask |= (1 << layerToAdd);
        vrCamera.cullingMask |= (1 << LayerMask.NameToLayer("Obstacles"));
    }

    public void TurnRaycastInvisible()
    {
        foreach (Sensor sensor in hapticScript.sensorList)
        {
            ObjectInvisibleToCamera(sensor.transform);
        }

        void ObjectInvisibleToCamera(Transform parentObject)
        {
            foreach(Transform child in parentObject)
            {
                ObjectInvisibleToCamera (child);
            }
            parentObject.gameObject.layer = LayerMask.NameToLayer("VRIgnore");
        }
    }

    public void TurnRaycastVisible()
    {
        foreach (Sensor sensor in hapticScript.sensorList)
        {
            ObjectVisibleToCamera(sensor.transform);
        }

        void ObjectVisibleToCamera(Transform parentObject)
        {
            foreach (Transform child in parentObject)
            {
                ObjectVisibleToCamera(child);
            }
            parentObject.gameObject.layer = LayerMask.NameToLayer("Default");
        }

    }

    // ***************************************************************************************************************************************************************************
}
