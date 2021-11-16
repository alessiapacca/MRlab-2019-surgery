# MRlab-2019-surgery
<p align="center">
<img src="https://github.com/alessiapacca/MRlab-2019-surgery/blob/master/imgs/MRTK_Logo.png" width="71%"/>
</p>

## Project Description
Nowadays, surgeons operate complex fracture surgeries without being able to properly manipulate data during the procedures because of sterilization issues. Oftentimes, they need multiple assistants who will go through the data and tell/show them the relevant pieces of information throughout the surgery. This process is both inefficient and cumbersome as it detaches the surgeon from the actual data and wastes human resources: doctors and nurses who have to assist with data manipulation and who could otherwise be helping more objectively.<br>
This application aims to assist surgeons in fracture surgeries (and potentially in other procedures) by providing an intuitive and non-contaminant method for accessing and manipulating medical data through the **Microsoft Hololens 2 device**. Therefore, human resources can be better managed and surgeries can be performed more efficiently.

Project Collaborators: 
* A. Klaeger
* A. Paccagnella
* J. Lehner
* L. Albuquerque
* Y. Zhou

## Demo gifs
<p align="center">
<img src="https://github.com/alessiapacca/MRlab-2019-surgery/blob/master/imgs/video.gif" width="71%"/>
</p>

<p align="center">
<img src="https://github.com/alessiapacca/MRlab-2019-surgery/blob/master/imgs/video3.gif"/>
</p>

<p align="center">
<img src="https://github.com/alessiapacca/MRlab-2019-surgery/blob/master/imgs/video2.gif"/>
</p>

## Functionalities
### Hide adjustment
This function, that can be found on the menu, allows to exclude (and alternatively include) the adjusted bone mesh in the view.

### Translate and rotate the bone and his fragments
The bone can be grabbed, translated and rotated in the scene with only two fingers. It can also be decomposed into his small fragments parts. A menu button can then reset the initial position of the object. 

### Edit opacity
Another function that we implemented was the ability to change the transparency level of the object when needed to be hidden. This enables to have a better visualization of specific parts of the fracture without loosing track of the momentarily ”unimportant” ones. The initial opacity can be restored through a menu button.

### CT Scans
A core novel functionality of the App is the display of CT Scans in the Mixed Reality environment. This also represented the biggest technical challenge of the project, as a CT consists of large amount of 3D data, which needs to be processed in real time on the limited hardware of the Hololens.

### Slicing
This function can be used to request a render of a specific cross-sectional image (slice) of the bone. This can be done by sliding two axis-aligned planes directly on the bone, or directly with the hand. Indeed, any arbitrary slice can be requested with any orientation - even sheared/skewed slices if desired. The CTReader then renders the specified slice using a high speed compute shader running on the GPU hardware of the Hololens, using either a nearest-neighbor or a trilinear interpolation approach.


## Log of progress

### Install the tools

Noted that this project should be developed and run on devices with Microsoft SDK, so the suggestions for Mac or Linux users would be either *developing remotely on a Windows workstation (recommended)* or *using a virtual machine*.

Got prepared with the hardwares, the next step is installing the tool kits for developing, please refer to the [document](https://docs.microsoft.com/en-us/windows/mixed-reality/install-the-tools#mixed-reality-toolkit). Noted that the version of Visual Studio could be 2015 or 2017 as well, meantime the version of Unity can also be 2018 LTS or the newest, while neither is fully guaranteed, the safest option would always be the one recommended.

### Start with the first demo

To make things run on unity, some packages should be downloaded. Follow the [instruction](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/GettingStartedWithTheMRTK.html#import-mrtk-packages-into-your-unity-project) to import the unity packages properly. You may also have your first glance of what a MR project looks like by opening the scene in examples.

### Build and run our own project

You actually don't have to create a new unity project, since the project in the former step is actually an empty one ready to use, what you need to do is create a new scene and work on it. To make files and folder more organized, lessen all the tabs in the Hierachy inspector and create a new folder called "My scene", later you could put all your belongings within this folder.

Place the pieces of bones into the scene then we are ready to build.

Follow the [building instructions](https://docs.microsoft.com/en-us/windows/mixed-reality/mrlearning-base-ch1), be certain to have all the configurations correct. The instruction would lead you to generate a Visual Studio project from Unity, then build the VS project and deploy it to the Hololens2. 

Some tips: 

* The first time you have your computer connected with the Hololens, go through this [page](https://docs.microsoft.com/en-us/windows/mixed-reality/using-the-windows-device-portal) to pair these things up, that is to create a pin code from the Hololens, and it would be asked the first time you deploying a program.

* Should carefully follow the buiding instructions, such as setting the build mode to **Release** and specify the device name in the **Project Properties -> debugging -> machine name**. 

### Try it on Hololens

Once the deployment complete, the hololens may automatically start that application. Now's the time to check if things work as you think.

### Adding MRTK functions 

Going through the [MRTK tutorial](https://microsoft.github.io/MixedRealityToolkit-Unity/README.html). We added some control scripts to the bones, in order to enable dragging, scaling functions, and it works.

