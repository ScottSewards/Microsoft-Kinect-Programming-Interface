# Microsoft Kinect Programming Interface by Gaskell and Sewards
This project depends on the discontinued Microsoft Kinect, Microsoft.Speech.dll and Processing 3. Visit https://youtu.be/GG0vmP9NcRQ to watch an early demonstration of the then-to-be-complete project recorded in January 2018. The following text is taken from the project report.

## Introduction
This report documents the research, design and development of a gesture-based visual programming environment and programming language. The environment and language will be aimed at novice programmers.

The program is being developed as part of a team of two. Before development of the project, both team members assigned parts of the project to complete, the two projects will then be merged at the end of development to create the final program. The individually developed projects will be attached with the final submission. Detailed programming contributions can be found within the implementation section of this report.

## Research
### Kinect Techology
The Kinect sensor is a depth and motion-sending device originally developed for the Xbox 360 game console (Crawford, n.d). The Kinect provides a Natural User Interface (NUI) for interaction using gestures (Jana, 2012). Having been developed for the Xbox 360, the original purpose of the Kinect was for its use in video games, games such as Just Dance, Dead Space 3 and Elder Scrolls V: Skyrim all featured Kinect compatibility (Williams, 2018).

Microsoft have provided an SDK for developers to create experimental or commercial applications for the Kinect. The SDK provides an interface to interact with the Kinect programmatically, it allows the developer to interact with the camera, sensors, microphone array and motors (Jana, 2011). 

The Kinect is made up of multiple major components, colour camera, infrared (IR) emitter, IR depth sensor, microphone array and an LED. (Crawford, n.d) (Microsoft, n.d). There is also a small motor in the base of the Kinect to allow for tilting on the horizontal axis (Jana, 2012).

![Microsoft Kinect](https://tctechcrunch2011.files.wordpress.com/2017/09/ic584396.png)

The applications for Kinect expand beyond video games, companies and individuals have developed applications to aid health care, robotics, imaging, education, security and artificial intelligence (Jana, 2012).

The colour camera for the Kinect streams video at 30 frames per second with a maximum resolution of 640x480. An inverse relationship can be seen between the frame rate and the resolution, should the developer require the use of a higher resolution, this can be achieved by decreasing the frame rate to 12 frames per second, this allows for a resolution of 1280x960 (Jana, 2012).

The Kinect's depth sensor can capture a raw 3D video stream of objects within the room, regardless of the rooms lighting. To achieve this, the IR emitter and the IR depth sensor are utilised. The IR depth sensor is a monochrome CMOS (Complementary Metal-Oxide-Semiconductor) sensor. The IR emitter emits electromagnetic radiation (EMR), the wavelength of IR is greater than that of visible light on the electromagnetic spectrum (Jana, 2012).

### Programming Environments for Education
Today, popular novice programming tools include Scratch, Alice and Blockly. In the early days of novice programming tools, opponents of intelligent programming tutoring systems (ITS) argued that these types of systems did not fulfil their promise of aiding novice programmers to overcome the abstraction and difficulty in programming (Lemut, Du Boulay and Dettori, n.d). With the development of visual programming environments like Alice and Scratch, ITS have been adopted by many schools and universities across the country. Alice is being used by teachers across all levels from middle schools to universities, and even in subjects outside of computing, such as visual arts and language arts (Alice.org, 2008).

Dann et al., (2011) noted that “Alice makes use of program visualisation and enables students to immediately see how their animation programs run, enabling students to easily understand the relationship between each individual programming statement and construct.”. The ability to create seemingly complex programs using ITS gives the user the incentive to continue developing and learning. Creating basic text-based programs in general programming languages will not motivate user to continue.

Novice programming environments are to be regarded as a ‘learn as you use’ tool. Many users learn Scratch as they go, trying various commands from the palette or using example projects developed by other users. To encourage this self-directed learning, Scratch was designed to invite scripting and provide immediate feedback for script execution (Maloney et al., 2010). The command palette in Scratch is constantly visible to the user, and commands are divided into categories such as motion, looks and sound. The most self-explanatory and useful commands appear near to the top of the palette, each category is colour coded to aid users in finding related blocks (Maloney et al., 2010).

## Design
## Language Design
When designing the programming language, simplicity must be considered. The programming language was required to be developed in a way which would never produce any errors, no matter what the user input. Errors can be frustrating for novice programmers, creating an environment in which no errors can occur leads to further experimentation of various functions. This motivation for experimentation will aid the learning curve of programming.
Developing a programming language which is unable to return an error means creating tight restrictions. The initial programming language will contain seven functions, all the parameters must be customisable by the user. The language is based upon Pseudo code, an extremely high-level language which is used by programmers to design programs prior to implementation.
The program must also take variables into consideration, the user must be able to change values, in future versions of the application changing variables types will also be required, however in this first implementation only integer values will be available.
The first function is the If Statement, an if statement is a conditional statement which only executes the contained code when the condition is true.

```if [Variable] == [Int] then```

The for loop allows for iteration, a user can run a snippet of code for [Int] amount of times.

```for i < [Int] do```

The while loop, like the for loop, but a while loop is broken when a condition is met, in this case when [Variable] is less than [Int].

```while [Variable] < [Int] do```

Math statement, a simple mathematical expression which allows the user to perform a mathematical operation on a variable. The operator can be changed.

```math [Variable] + [Int]```

Rectangle, a simple drawing function which draw a rectangle to the screen, the parameters for the rectangle are (x, y, width, height).

```rect [Int] [Int] [Int] [Int]```

Ellipse, like a rectangle however an ellipse is drawn, because the application is aimed at younger users, this has been simply named to ‘circle’. The parameters for the ellipse are (x, y, width, height).

```circle [Int] [Int] [Int] [Int]```

Text, draws a text label to the screen. The output text is the value of [Variable]. The parameters for text are (variable, x, y).

```text [Variable] [Int] [Int]```

Due to the time constraints faced with the development of this project, each statement will match with one line of drawing code. This code will be entered a code box which has been selected by the user.

```for i < 200 do```
```circle   20 * i   20 * i   10   10```

The programs developed using this environment will be static programs. Creating fully animated programs would require further complexity within the way code is created using the environment, this was not a possibility due to time restrictions, however the program has been designed for this to be a potential addition in the future.

### Interface Design
The UI follows the Kinect for Windows Human Interface Guidelines. This includes objectives such as setting minimum size for buttons, so they can be interacted with accurately and hassle free.

The graphical design will utilise rule of three. To elaborate, alike figure 1, left is options such as loops, variables, methods, etc, in the centre and background to provide player feedback via the Kinect sensor is video feed, and on the right, are lines of code with page-up or down, run, and load or save buttons to activate the respective feature.

![Interface Design](https://i.ibb.co/GJxgFq8/Untitled.png)

The buttons will be used to select a line of code then an option. The option will then prompt inputs. For example, an if statement will take two arguments such as if x is less than y. Inputs will be entered using a virtual keyboard with letters and a number pad. 

## Implementation
The final program contains several features:
1. 	Write up to 8 lines of code using loops, variables, and methods. Including but not limited printing strings and drawing shapes.
2.	Run and execute the code via Processing in a new window.
3.	Use an advanced keyboard to rename variables and change values.

## Conclusion
Background research and additional reading was conducted at the beginning of the project and it helped the design of the visual programming language far easier. The end objective of the project was to create an errorless visual programming language. Investigating research already completed by Carnegie Mellon University for the development of the Alice software, aided the development of this program. It was discovered that prior to the development of visual programming languages, intelligent tutorials systems for programming were criticised for their lack of usability and under-delivering on their ability to teach programming to novices (Lemut, Du Boulay and Dettori, n.d) .

The implementation of the design was different from what was documented because the design evolved over the course of development.

The final implementation is a functional programming environment. Users can interact via 2 inputs: mouse and keyboard or Microsoft Kinect gesture. The programming environment includes a virtual keyboard that supports identical inputs (additional interacting includes voice commands). Keyboard interface is controlled by the program and opened only when needed.

## References
About Alice. Alice.org [online]. Available from: http://www.alice.org/about/ [Accessed 4 January 2018].

Processing.org. Processing.org [online]. Available from: https://processing.org/ [Accessed 4 January 2018].

CRAWFORD, S. How Microsoft Kinect Works. HowStuffWorks [online]. Available from: https://electronics.howstuffworks.com/microsoft-kinect2.htm [Accessed 4 January 2018].

DANN, W., COSGROVE, D., SLATER, D. and CULYBA, D., 2011. Mediated Transfer: Alice 3 to Java. Available from: http://www.alice.org/wp-content/uploads/2017/04/MediatedTransfer.pdf [Accessed 4 January 2018].

JANA, A., 2011. Development With Kinect .NET SDK (Part I) – Installation and Development Environment Setup. Abhijit's Blog [online]. Available from: https://abhijitjana.net/2011/09/14/development-with-kinect-net-sdk-part-i-installation-and-development-environment-setup/ [Accessed 4 January 2018].

JANA, A., 2012. Kinect for Windows SDK Programming Guide (Community Experience Distilled). Packt Publishing.

KARCH, M., 2017. Get Your Kids Started Coding with these Programming Languages. Lifewire[online]. Available from: https://www.lifewire.com/kids-programming-languages-4125938 [Accessed 4 January 2018].

LEMUT, E., DU BOULAY, B. and DETTORI, G. Cognitive models and intelligent environments for learning programming.

MALONEY, J., RESNICK, M., RUSK, N., SILVERMAN, B. and EASTMOND, E., 2010. The Scratch Programming Language and Environment. ACM Transactions on Computing Education [online]. 10 (4), pp. 1-15. [Accessed 5 January 2018].

MICROSOFT. Kinect for Windows Sensor Components and Specifications [image]. Available from: https://msdn.microsoft.com/en-us/library/jj131033.aspx [Accessed 4 January 2018].

WILLIAMS, A., 2018. The 10 Best Xbox 360 Kinect Games to Buy in 2018. Lifewire [online]. Available from: https://www.lifewire.com/best-x360-kinect-games-3563057 [Accessed 4 January 2018].
