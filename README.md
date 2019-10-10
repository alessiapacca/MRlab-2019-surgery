# MRlab-2019-surgery

A collaboration of Surgery Assistant Project, using Microsoft Hololens 2 and mixed reality tools.

Project Owners ~add last names~: 

* Adrian
* Alessia
* Jonathan
* Leonardo
* Yang

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

Going through the [MRTK tutorial](https://microsoft.github.io/MixedRealityToolkit-Unity/README.html). We ~currently~ added some control scripts to the bones, in order to enable dragging, scaling functions, and it works.

More logs should come up then...