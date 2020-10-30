using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

/*
 * @author Connor Gaskell #23091622 (CG) and Scott Sewards #22918078 (SS)
 * @date 05/01/2018
 * 
 */

namespace Programming {
    public partial class MainWindow : Window {
        #region START
        public MainWindow() {
            InitializeComponent();
        }

        void KinectLoad(object sender, RoutedEventArgs e) {
            InitiateKinect();
        }

        void KinectClose(object sender, System.ComponentModel.CancelEventArgs e) {
            kinect.Stop(); //STOP KINECT SENSOR
        }
        #endregion

        #region INITIATE
        KinectSensor kinect; //KINECT CONNECTED IS STORED AS A VARIABLE

        void InitiateKinect() { //(CG, SS)
            try {
                if (KinectSensor.KinectSensors.Count > 0)
                    kinect = KinectSensor.KinectSensors[0];

                if (kinect.Status == KinectStatus.Connected) {
                    kinect.ColorStream.Enable();
                    kinect.DepthStream.Enable();
                    kinect.SkeletonStream.Enable();
                    kinect.DepthStream.Range = DepthRange.Near;
                    kinect.SkeletonStream.EnableTrackingInNearRange = true;
                    kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(AllFramesReady);
                    InitiatePeople();
                    kinect.Start();
                    InitiateSpeech(); //ALLOW KINECT TO LISTEN
                    BuildGrid(); //BUILD A GRID FOR BUTTONS GENERATED TO DISPLAY IN
                    SetUp(); //ASSIGN BUTTONS TO VARIABLES
                }
            }
            catch (Exception e) {
                kinect.Stop();
            }
        }

        void InitiatePeople() { //(CG, SS)
            for (int i = 0; i < skeletonsTracked; i++) {
                person[i] = new Person(i);

                List<Ellipse> Joints = person[i].getJoints();

                foreach (Ellipse Joint in Joints)
                    canDraw.Children.Add(Joint);

                List<Line> Bones = person[i].getBones();

                foreach (Line Bone in Bones)
                    canDraw.Children.Add(Bone);
            }
        }

        void InitiateSpeech() { //(CG, SS)
            RecognizerInfo ri = GetRecognizer();
            sre = new SpeechRecognitionEngine(ri.Id);
            var choices = new Choices();

            for (int i = 0; i < alphanumerical.Length; i++)
                choices.Add(alphanumerical[i]);

            for (int i = 0; i < commands.Length; i++)
                choices.Add(commands[i]);

            var gb = new GrammarBuilder();
            gb.Culture = ri.Culture;
            gb.Append(choices);

            var grammar = new Grammar(gb);
            sre.LoadGrammar(grammar);
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Recognise);

            thread = new Thread(Listen);
            thread.Start();
        }
        #endregion

        void AllFramesReady(object sender, AllFramesReadyEventArgs e) { //FRAMES ARE CALLED 30 TIMES PER SECOND (CG)
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
                if (colorFrame == null) return;
                byte[] pixels = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(pixels);
                //KINECT CAMERA OUTPUT IS UPDATED EACH TIME
                imgVideo.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, colorFrame.Width * 4);
            }

            //SKELETONS ARE DRAWN EACH FRAME ONTO THE CAMERA
            DrawSkeleton(e);
        }

        #region SKELETON
        const int skeletonCount = 6;
        static int skeletonsTracked = 2;
        Skeleton[] skeletons = new Skeleton[skeletonCount];
        Person[] person = new Person[skeletonsTracked];
        Point rightHandPos;

        void DrawSkeleton(AllFramesReadyEventArgs e) { //(CG)
            for (int i = 0; i < skeletonCount; i++) {
                Skeleton me = null;
                GetSkeleton(e, ref me, i);

                if (me == null) {
                    for (int x = 0; x < person[i].getJoints().Count(); x++) {
                        SolidColorBrush jointColorBrush = new SolidColorBrush();
                        jointColorBrush.Opacity = 1;
                        person[i].getJoints()[x].Fill = jointColorBrush;
                        person[i].DrawJoint(person[i].getJoints()[x], new Point(0, 0));
                    }

                    for (int y = 0; y < person[i].getBones().Count(); y++) {
                        person[i].boneColorBrush.Opacity = 0;
                        person[i].getBones()[y].Fill = person[i].boneColorBrush;
                    }
                }

                if (me == null)
                    return;

                int j = 0;
                foreach (Joint joint in me.Joints) {
                    SolidColorBrush jointColorBrush = new SolidColorBrush();
                    if (joint.TrackingState == JointTrackingState.Tracked) {
                        jointColorBrush.Color = Colors.Green;
                        person[i].getJoints()[j].Fill = jointColorBrush;
                    }
                    else if (joint.TrackingState == JointTrackingState.Inferred) {
                        jointColorBrush.Color = Colors.DarkOrange;
                        person[i].getJoints()[j].Fill = jointColorBrush;
                    }
                    else if (joint.TrackingState == JointTrackingState.NotTracked) {
                        jointColorBrush.Color = Colors.Red;
                        jointColorBrush.Opacity = 1;
                        person[i].getJoints()[j].Fill = jointColorBrush;
                    }

                    person[i].DrawJoint(person[i].getJoints()[j], GetColour(joint.Position));
                    j += 1;
                }

                foreach (Line bone in person[i].getBones()) {
                    person[i].boneColorBrush.Opacity = 1;
                    person[i].boneColorBrush.Color = Colors.Red;
                    bone.StrokeThickness = 3;
                    bone.Fill = person[i].boneColorBrush;
                    bone.Stroke = person[i].boneColorBrush;
                }

                person[i].DrawBone(person[i].Neck, GetColour(me.Joints[JointType.Head].Position), GetColour(me.Joints[JointType.ShoulderCenter].Position));
                person[i].DrawBone(person[i].LowerBack, GetColour(me.Joints[JointType.Spine].Position), GetColour(me.Joints[JointType.HipCenter].Position));
                person[i].DrawBone(person[i].Spine, GetColour(me.Joints[JointType.ShoulderCenter].Position), GetColour(me.Joints[JointType.Spine].Position));
                person[i].DrawBone(person[i].RightShoulder, GetColour(me.Joints[JointType.ShoulderCenter].Position), GetColour(me.Joints[JointType.ShoulderRight].Position));
                person[i].DrawBone(person[i].RightUpperArm, GetColour(me.Joints[JointType.ShoulderRight].Position), GetColour(me.Joints[JointType.ElbowRight].Position));
                person[i].DrawBone(person[i].RightForeArm, GetColour(me.Joints[JointType.ElbowRight].Position), GetColour(me.Joints[JointType.WristRight].Position));
                person[i].DrawBone(person[i].RightHand, GetColour(me.Joints[JointType.WristRight].Position), GetColour(me.Joints[JointType.HandRight].Position));
                person[i].DrawBone(person[i].RightHip, GetColour(me.Joints[JointType.HipCenter].Position), GetColour(me.Joints[JointType.HipRight].Position));
                person[i].DrawBone(person[i].RightThigh, GetColour(me.Joints[JointType.HipRight].Position), GetColour(me.Joints[JointType.KneeRight].Position));
                person[i].DrawBone(person[i].RightShin, GetColour(me.Joints[JointType.KneeRight].Position), GetColour(me.Joints[JointType.FootRight].Position));
                person[i].DrawBone(person[i].RightFoot, GetColour(me.Joints[JointType.AnkleRight].Position), GetColour(me.Joints[JointType.FootRight].Position));
                person[i].DrawBone(person[i].LeftShoulder, GetColour(me.Joints[JointType.ShoulderCenter].Position), GetColour(me.Joints[JointType.ShoulderLeft].Position));
                person[i].DrawBone(person[i].LeftUpperArm, GetColour(me.Joints[JointType.ShoulderLeft].Position), GetColour(me.Joints[JointType.ElbowLeft].Position));
                person[i].DrawBone(person[i].LeftForeArm, GetColour(me.Joints[JointType.ElbowLeft].Position), GetColour(me.Joints[JointType.WristLeft].Position));
                person[i].DrawBone(person[i].LeftHand, GetColour(me.Joints[JointType.WristLeft].Position), GetColour(me.Joints[JointType.HandLeft].Position));
                person[i].DrawBone(person[i].LeftHip, GetColour(me.Joints[JointType.HipCenter].Position), GetColour(me.Joints[JointType.HipLeft].Position));
                person[i].DrawBone(person[i].LeftThigh, GetColour(me.Joints[JointType.HipLeft].Position), GetColour(me.Joints[JointType.KneeLeft].Position));
                person[i].DrawBone(person[i].LeftShin, GetColour(me.Joints[JointType.KneeLeft].Position), GetColour(me.Joints[JointType.FootLeft].Position));
                person[i].DrawBone(person[i].LeftFoot, GetColour(me.Joints[JointType.AnkleLeft].Position), GetColour(me.Joints[JointType.FootLeft].Position));

                rightHandPos = GetColour(me.Joints[JointType.HandRight].Position);
                CheckTile(person[i]);
            }
        }

        void GetSkeleton(AllFramesReadyEventArgs e, ref Skeleton me, int person) { //(CG)
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame()) {
                if (skeletonFrameData == null)
                    return;

                skeletonFrameData.CopySkeletonDataTo(skeletons);
                List<Skeleton> temporarySkeleton = (from s in skeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).Distinct().ToList();

                if (temporarySkeleton.Count < person + 1)
                    return;

                me = temporarySkeleton[person];
            }
        }

        Point GetColour(SkeletonPoint me) { //(CG)
            ColorImagePoint colorPoint = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(me, ColorImageFormat.InfraredResolution640x480Fps30);
            return new Point(colorPoint.X, colorPoint.Y);
        }

        float GetDepth(SkeletonPoint me) { //(CG)
            DepthImagePoint depthPoint = kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(me, DepthImageFormat.Resolution640x480Fps30);
            return depthPoint.Depth;
        }
        #endregion

        #region SPEECH
        SpeechRecognitionEngine sre;
        Thread thread;
        double confidence = 0.5;

        string[] alphanumerical = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        string[] commands = { "select", "delete", "clear", "close", "caps", "underscore", "space" };

        static RecognizerInfo GetRecognizer() { //(SS)
            Func<RecognizerInfo, bool> matchingFunc = r => {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };

            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        void Listen() { //(SS)
            var audioSource = kinect.AudioSource;
            audioSource.AutomaticGainControlEnabled = false;
            Stream aStream = audioSource.Start();
            sre.SetInputToAudioStream(aStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm,
            16000, 16, 1, 32000, 2, null));
            sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        void Recognise(object sender, SpeechRecognizedEventArgs e) { //(SS)
            if (e.Result.Text.ToLower() == commands[0] && e.Result.Confidence >= confidence) //SELECT
                Select();
            else if (e.Result.Text.ToLower() == commands[1] && e.Result.Confidence >= confidence) //DELETE
                Delete();
            else if (e.Result.Text.ToLower() == commands[2] && e.Result.Confidence >= confidence) //CLEAR
                Clear();
            else if (e.Result.Text.ToLower() == commands[3] && e.Result.Confidence >= confidence) //CLOSE
                Show(true, false, false);
            else if (e.Result.Text.ToLower() == commands[4] && e.Result.Confidence >= confidence) //CAPS
                Caps();
            else if (e.Result.Text.ToLower() == commands[5] && e.Result.Confidence >= confidence) //UNDERSCORE
                Write(alphanumerical.Length + 1);
            else if (e.Result.Text.ToLower() == commands[6] && e.Result.Confidence >= confidence) //SPACE
                Write(alphanumerical.Length + 2);

            for (int i = 0; i < alphanumerical.Length; i++)
                if (e.Result.Text.ToLower() == alphanumerical[i] && e.Result.Confidence >= confidence)
                    Write(i);
        }
        #endregion

        #region INTERFACES
        void ClickButton(object sender, EventArgs e) { //ALL BUTTON CLICKS ARE HANDLED BY THIS METHOD SAVE ALPHANUMBERICAL INPUTS (SS)
            string name = (sender as Button).Name; //NAME OF PRESSED BUTTON IS STORED AND IT IS COMPARED

            if (name == "close") Show(true, false, false); //IF THE NAME MEETS A SET STRING, IT CALLS A METHOD
            else if (name == "delete") Delete();
            else if (name == "clear") Clear();
            else if (name == "caps") Caps();
            else if (name == "underscore") Write(alphanumerical.Length + 1);
            else if (name == "space") Write(alphanumerical.Length + 2);
            else if (name == "loops" || name == "variables" || name == "methods" || name == "shapes") UpdateLists(name);
            else if (name == "run") Run();
            else if (name == "if" || name == "for" || name == "while" || name == "math" || name == "print" || name == "eclipse" || name == "rectangle" || name == "text") AddThis(name);
            else if (name.Contains("var")) OpenKeyboard(name);
            else if (name.Contains("line")) Line(name);
        }

        #region KEYBOARD
        string input = ""; //TEXT FROM KEYBOARD TEXTBLOCK
        bool isShow = true;
        bool capsOn = false;

        void Select() { //HAND POSITION IS SET TO GRID CELL CALLED INDEX/BUTTONS ARE ALIGNED TO OR INSIDE CELLS/INDEX IS COMPARE TO POSITION AND BUTTONS ARE PRESSED (SS)
            int index = (((selected[1] - 1) * 12) + selected[0]); //CALCULATE GRID CELL SELECTED VIA COLUMN AND ROW
            
            if (isShow) { //INTERACT WITH KEYBOARD BUTTONS
                if (index == 13) Show();
                else if (index == 20 || index == 21) Delete();
                else if (index == 23 || index == 24) Clear();
                else if (index == 62 || index == 63) Caps();
                else if (index == 65 || index == 66) Write(alphanumerical.Length + 1);
                else if (index >= 68 && index <= 71) Write(alphanumerical.Length + 2);
                else if (index >= 73 && index <= 108) Write(index - 73);
            }
            else { //PROGRAMMING BUTTONS
                if (index == 1 || index == 2) UpdateLists("loops");
                else if (index == 13 || index == 14) UpdateLists("variables");
                else if (index == 25 || index == 26) UpdateLists("methods");
                else if (index == 37 || index == 38) UpdateLists("shapes");
                else if (index == 61 || index == 62) {
                    if (option == 0)  AddThis("if");
                    else if (option == 1) OpenKeyboard("var1");
                    else if (option == 2) AddThis("print");
                    else if (option == 3) AddThis("eclipse");
                }
                else if (index == 73 || index == 74) {
                    if (option == 0)  AddThis("for");
                    else if (option == 1) OpenKeyboard("var2");
                    else if (option == 3) AddThis("rectangle");
                }
                else if (index == 85 || index == 86) {
                    if (option == 0) AddThis("while");
                    else if (option == 1) OpenKeyboard("var3");
                }
                else if (index == 97 || index == 98) {
                    if (option == 1) OpenKeyboard("var4");
                }
                else if (index > 9 && index <= 12) Line("line1");
                else if (index > 21 && index <= 24) Line("line2");
                else if (index > 33 && index <= 36) Line("line3");
                else if (index > 45 && index <= 48) Line("line4");
                else if (index > 57 && index <= 60) Line("line5");
                else if (index > 69 && index <= 72) Line("line6");
                else if (index > 81 && index <= 84) Line("line7");
                else if (index > 93 && index <= 96) Line("line8");
                else if (index >= 106) Run();
            }
        }

        void ClickWrite(object sender, EventArgs e, string tag) { //TAG NUMBER IS PARSED TO DETERMINE WHICH 0-35 IS PRESSED AND WRITES IT (SS)
            int i = Int32.Parse(tag);

            if (i <= alphanumerical.Length)
                Write(i);
        }

        void Write(int i) { //WRITES CHARACTERS INTO INPUT VARIABLE (SS)
            if (i < alphanumerical.Length) {
                if (capsOn) input += alphanumerical[i].ToUpper();
                else input += alphanumerical[i];
            }
            else if (i == alphanumerical.Length + 1) input += "_";
            else if (i == alphanumerical.Length + 2) input += " ";
            inputText.Text = input;
        }

        void Delete() { //REMOVES LAST LETTER FROM INPUT VARIABLE (SS)
            if (input.Length >= 1) input = input.Remove(input.Length - 1);
            inputText.Text = input;
        }

        void Clear() { //REMOVES ALL CHARACTERS FROM INPUT VARIABLE (SS)
            input = "";
            inputText.Text = input;
        }

        void Caps() { //TOGGLE LOWERCASE OR UPPERCASE (SS)
            capsOn = !capsOn;
            Show(false, false, false);
        }

        void Show(bool overwrite, bool value, bool invert) { //UPDATE KEYBOARD TO SHOW OR HIDE AND SHOW OR HIDE PROGRAMMING ENVIRONEMNT (SS)
            if (overwrite) isShow = value;
            else if (invert) isShow = !isShow;

            if (capsOn && isShow) {
                coder.Visibility = Visibility.Collapsed;
                loopButtons.Visibility = Visibility.Collapsed;
                variableButtons.Visibility = Visibility.Collapsed;
                methodButtons.Visibility = Visibility.Collapsed;
                shapeButtons.Visibility = Visibility.Collapsed;
                lowerGrid.Visibility = Visibility.Collapsed;
                upperGrid.Visibility = Visibility.Visible;
                elements.Visibility = Visibility.Visible;
            }
            else if (isShow) {
                coder.Visibility = Visibility.Collapsed;
                loopButtons.Visibility = Visibility.Collapsed;
                variableButtons.Visibility = Visibility.Collapsed;
                methodButtons.Visibility = Visibility.Collapsed;
                shapeButtons.Visibility = Visibility.Collapsed;
                lowerGrid.Visibility = Visibility.Visible;
                upperGrid.Visibility = Visibility.Collapsed;
                elements.Visibility = Visibility.Visible;
            }
            else {
                coder.Visibility = Visibility.Visible;
                loopButtons.Visibility = Visibility.Visible;
                lowerGrid.Visibility = Visibility.Collapsed;
                upperGrid.Visibility = Visibility.Collapsed;
                elements.Visibility = Visibility.Collapsed;
                NewValue(temp);
                input = "";
            }
        }
        #endregion

        #region CODING
        int option = 0; //LOOPS/VARIALBES/METHODS/SHAPES REPRESENTED FROM 0-3
        int select = 1; //LINE OF CODE SELECTED
        string temp = ""; //USED FOR STORING VARIOUS INFORMATION TEMPORARILY
        Button[] buttons = new Button[9]; //LINES OF CODE BUTTONS REFERENCES
        string[] execute = { "size(650, 600);", "", "", "", "", "", "", "", "" }; //ALL CODE SET TO COMPILE
        string[,] vars = new string[,] { { "int", "Var1", "0" }, { "int", "Var2", "0" }, { "int", "Var3", "0" }, { "int", "Var4", "0" } }; //VARIABLES THAT CAN BE REGERENCED OR ALTERED
                
        void SetUp() { //ASSIGNS BUTTONS TO BUTTON ARRAY (SS)
            buttons[0] = run;
            buttons[1] = line1;
            buttons[2] = line2;
            buttons[3] = line3;
            buttons[4] = line4;
            buttons[5] = line5;
            buttons[6] = line6;
            buttons[7] = line7;
            buttons[8] = line8;
        }

        void UpdateLists(string function) { //OPTIONS FOR LOOPS/VARIABLES/METHODS/SHAPES ARE SWAPPED (CG, SS)
            if (function == "loops") {
                loopButtons.Visibility = Visibility.Visible;
                variableButtons.Visibility = Visibility.Collapsed;
                methodButtons.Visibility = Visibility.Collapsed;
                shapeButtons.Visibility = Visibility.Collapsed;
                option = 0;
            }
            else if (function == "variables") {
                loopButtons.Visibility = Visibility.Collapsed;
                variableButtons.Visibility = Visibility.Visible;
                methodButtons.Visibility = Visibility.Collapsed;
                shapeButtons.Visibility = Visibility.Collapsed;
                option = 1;
            }
            else if (function == "methods") {
                loopButtons.Visibility = Visibility.Collapsed;
                variableButtons.Visibility = Visibility.Collapsed;
                methodButtons.Visibility = Visibility.Visible;
                shapeButtons.Visibility = Visibility.Collapsed;
                option = 2;
            }
            else if (function == "shapes") {
                loopButtons.Visibility = Visibility.Collapsed;
                variableButtons.Visibility = Visibility.Collapsed;
                methodButtons.Visibility = Visibility.Collapsed;
                shapeButtons.Visibility = Visibility.Visible;
                option = 3;
            }
        }

        void UpdateCode() { //CONTENT OF LINE BUTTONS IS UPDATED TO RESPECTIVE CODE LINES WHEN UPDATED (CG, SS)
            for (int i = 0; i < buttons.Length; i++) {
                buttons[i].Content = execute[i];
                //STOPS BUTTON CONTENT BEING BLANK BY PROVIDING EMPTY MESSAGE
                if (execute[i] == "") buttons[i].Content = "Empty";
                //IF FILLER BUTTON SET TO PREVIOUS VALUE
                else if (i == 0) buttons[0].Content = "Run";
            }
        }

        void Run() { //PROCESSING CODE IS USED TO RUN THE PROGRAM (CG, SS)
            string[] convertedCode = new string[9];
            convertedCode[0] = execute[0];

            for (int i = 1; i < convertedCode.Length; i++) { //CONVERT CODE TO PROCESSING AND STORE IT IN AN ARRAY STRING
                convertedCode[i] = ConvertToProcessing(execute[i]);
            }

            File.WriteAllLines("..\\..\\proc\\work\\sketches\\sketches.pde", convertedCode); //WRITE THE STRING TO A FILE
            Process process = null;

            try {
                string batDir = string.Format(@"..\\..\\proc\\work\\");
                process = new Process();
                process.StartInfo.WorkingDirectory = batDir;
                process.StartInfo.FileName = "processing.bat";
                process.StartInfo.CreateNoWindow = false;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace.ToString());
            }
        }

        string ConvertToProcessing(string code) { //PSEUDO-CODE WRITTEN IN THE PROGRAM IS CONVERTED TO PROCESSING CODE (CG, SS)
            string convertedCode = code;
            Char delimiter = ' ';
            string[] split = code.Split(delimiter);

            if (code == "") {
                //DO NOTHING
            }
            else if (code.Contains("if")) convertedCode = "if (" + split[1] + " " + split[2] + " " + split[3] + ")";
            else if (code.Contains("for")) convertedCode = "for (int i = 0; i < " + split[3] + "; i++)";
            else if (code.Contains("while")) convertedCode = "while (" + split[1] + " < " + split[3] + ")";
            else if (code.Contains("print")) convertedCode = "print ('" + split[1] + "');";
            else if (code.Contains("eclipse")) convertedCode = "eclipse (" + split[1] + ", " + split[2] + ", " + split[3] + ", " + split[4] + ");";
            else if (code.Contains("rectangle")) convertedCode = "rectanlge (" + split[1] + ", " + split[2] + ", " + split[3] + ", " + split[4] + ");";

            return convertedCode;
        }

        void AddThis(string name) { //CLICKING BUTTONS ADDS CODE TO THE EXECUTE ARRAY (CG, SS)
            if (name == "if") execute[select] = "if " + var1b.Content + " < " + var2b.Content;
            else if (name == "for") execute[select] = "for i < " + var3b.Content;
            else if (name == "while") execute[select] = "while " + var4b.Content + " < " + var5b.Content;
            else if (name == "print") execute[select] = "print " + var1c.Content;
            else if (name == "eclipse") execute[select] = "eclipse " + var1d.Content + " " + var2d.Content + " " + var3d.Content + " " + var4d.Content;
            else if (name == "rectangle") execute[select] = "rectangle " + var5d.Content + " " + var6d.Content + " " + var7d.Content + " " + var8d.Content;
            UpdateCode();
        }

        void OpenKeyboard(string value) { //SHOWS THE KEYBOARD SO VARIABLES CAN BE RENAMED (SS)
            temp = value;
            Show(true, true, false);
        }

        void NewValue(string name) { //SETS NEW VALUE FOR VARIABLE NAME OR DATA
            if (name.Contains("1a")) {
                vars[0, 2] = input;
                var1a.Content = input;
            }
            else if (name.Contains("2a")) {
                vars[1, 2] = input;
                var2a.Content = input;
            }
            else if (name.Contains("3a")) {
                vars[2, 2] = input;
                var3a.Content = input;
            }
            else if (name.Contains("4a")) {
                vars[3, 2] = input;
                var4a.Content = input;
            }
            if (name.Contains("1b")) var1b.Content = input;
            else if (name.Contains("2b")) var2b.Content = input;
            else if (name.Contains("3b")) var3b.Content = input;
            else if (name.Contains("4b")) var4b.Content = input;
            else if (name.Contains("5b")) var5b.Content = input;
            else if (name.Contains("1c")) var1c.Content = input;
            else if (name.Contains("1d")) var1d.Content = input;
            else if (name.Contains("2d")) var2d.Content = input;
            else if (name.Contains("3d")) var3d.Content = input;
            else if (name.Contains("4d")) var4d.Content = input;
            else if (name.Contains("5d")) var5d.Content = input;
            else if (name.Contains("6d")) var6d.Content = input;
            else if (name.Contains("7d")) var7d.Content = input;
            else if (name.Contains("8d")) var8d.Content = input;
            else if (name.Contains("1")) {
                vars[0, 1] = input;
                var1.Content = input;
            }
            else if (name.Contains("2")) {
                vars[1, 1] = input;
                var2.Content = input;
            }
            else if (name.Contains("3")) {
                vars[2, 1] = input;
                var3.Content = input;
            }
            else if (name.Contains("4")) {
                vars[3, 1] = input;
                var4.Content = input;
            }
        }

        void Line(string name) { //SETS SELECT LINE OF CODE
            for (int i = 0; i < buttons.Length; i++) 
                if (name.Contains(i.ToString())) select = i;
        }
        #endregion
        #endregion

        #region GRID
        double[] border = { 0, 0, 0, 0 }; //TOP/RIGHT/BOTTOM/LEFT
        double[] box = new double[2]; //HEIGHT/WIDTH
        int[] size = { 12, 9, 480, 640 }; //COLUMNS/ROWS/HEIGHT/WIDTH
        int[] selected = { 1, 7 }; //COL/ROW

        void BuildGrid() { //CREATES THE GRID USING DEFENITIONS AT SIZE 12:9
            box[0] = ((size[2] - (border[0] + border[2])) / size[1]);
            box[1] = ((size[3] - (border[3] + border[1])) / size[0]);

            RowDefinition topBorderRowA = new RowDefinition();
            topBorderRowA.Height = new GridLength(border[0]);
            RowDefinition bottomBorderRowA = new RowDefinition();
            bottomBorderRowA.Height = new GridLength(border[2]);
            ColumnDefinition rightBorderColA = new ColumnDefinition();
            rightBorderColA.Width = new GridLength(border[1]);
            ColumnDefinition leftBorderColA = new ColumnDefinition();
            leftBorderColA.Width = new GridLength(border[3]);
            RowDefinition topBorderRowB = new RowDefinition();
            topBorderRowB.Height = new GridLength(border[0]);
            RowDefinition bottomBorderRowB = new RowDefinition();
            bottomBorderRowB.Height = new GridLength(border[2]);
            ColumnDefinition rightBorderColB = new ColumnDefinition();
            rightBorderColB.Width = new GridLength(border[1]);
            ColumnDefinition leftBorderColB = new ColumnDefinition();
            leftBorderColB.Width = new GridLength(border[3]);

            for (int i = 0; i < size[1]; i++) {
                RowDefinition defaultRow = new RowDefinition();
                defaultRow.Height = new GridLength(box[0]);
                lowerGrid.RowDefinitions.Add(defaultRow);
            }

            for (int i = 0; i < size[1]; i++) {
                RowDefinition defaultRow = new RowDefinition();
                defaultRow.Height = new GridLength(box[0]);
                upperGrid.RowDefinitions.Add(defaultRow);
            }

            for (int i = 0; i < size[0]; i++) {
                ColumnDefinition defaultCol = new ColumnDefinition();
                defaultCol.Width = new GridLength(box[1]);
                lowerGrid.ColumnDefinitions.Add(defaultCol);
            }

            for (int i = 0; i < size[0]; i++) {
                ColumnDefinition defaultCol = new ColumnDefinition();
                defaultCol.Width = new GridLength(box[1]);
                upperGrid.ColumnDefinitions.Add(defaultCol);
            }

            lowerGrid.RowDefinitions.Add(topBorderRowA);
            lowerGrid.RowDefinitions.Add(bottomBorderRowA);
            lowerGrid.ColumnDefinitions.Add(leftBorderColA);
            lowerGrid.ColumnDefinitions.Add(rightBorderColA);
            upperGrid.RowDefinitions.Add(topBorderRowB);
            upperGrid.RowDefinitions.Add(bottomBorderRowB);
            upperGrid.ColumnDefinitions.Add(leftBorderColB);
            upperGrid.ColumnDefinitions.Add(rightBorderColB);

            for (int i = 0; i < alphanumerical.Length; i++) { //CREATES BUTTONS INSIDE GRID FOR KEYBOARD
                Button buttonLower = new Button(); //LOWERCASE
                buttonLower.Content = alphanumerical[i];
                buttonLower.FontSize = 20;
                buttonLower.Height = box[0];
                buttonLower.Width = box[1];
                buttonLower.Tag = i;
                buttonLower.Click += (sender, e) => ClickWrite(sender, e, buttonLower.Tag.ToString());
                Grid.SetColumn(buttonLower, i % size[0]);
                Grid.SetRow(buttonLower, i / size[0] + 6);
                lowerGrid.Children.Add(buttonLower);

                Button buttonUpper = new Button(); //UPERCASE
                buttonUpper.Content = alphanumerical[i].ToUpper();
                buttonUpper.FontSize = 20;
                buttonUpper.Height = box[0];
                buttonUpper.Width = box[1];
                buttonUpper.Tag = i;
                buttonUpper.Click += (sender, e) => ClickWrite(sender, e, buttonUpper.Tag.ToString());
                Grid.SetColumn(buttonUpper, i % size[0]);
                Grid.SetRow(buttonUpper, i / size[0] + 6);
                upperGrid.Children.Add(buttonUpper);
            }
        }

        void CheckTile(Person person) { //TRACKS HAND POSITIONING
            double handX = rightHandPos.X;
            double handY = rightHandPos.Y;

            for (int i = 1; i <= size[1]; i++) {
                if (handY <= ((box[0] * i) + border[0]) && handY > ((box[0] * (i - 1)) + border[0]))
                    if (i != selected[1]) selected[1] = i;
            }

            for (int i = 1; i <= size[0]; i++) {
                if (handX <= ((box[1] * i) + border[3]) && handX > ((box[1] * (i - 1)) + border[3]))
                    if (i != selected[0]) selected[0] = i;
            }
        }
        #endregion
    }
}