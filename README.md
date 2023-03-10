# Retro Road Visualizer

![](https://github.com/sfitzpatrickchapman/RetroRoadVisualizer/blob/main/DemoMedia/RR_BigSharp.png)

----
The goal of this personal project was to create a Unity 3D (HDRP) parallel of a [retro-themed Blender animation](https://www.youtube.com/watch?v=hnLsktA4gmY) created by Ducky 3D. The goal was to create a visualizer that is just as aesthetically pleasing, while having game engine capabilities such as procedural & configurable terrain generation, runtime capabilties, ray-tracing, and most importantly, scripting potential.<br><br>

![](https://github.com/sfitzpatrickchapman/RetroRoadVisualizer/blob/main/DemoMedia/RR_720p.gif)
![](https://github.com/sfitzpatrickchapman/RetroRoadVisualizer/blob/main/DemoMedia/RR_Bare_720p.gif)<br><br>

There are many possiblities that can make this project more interesting. Some future goals may be to:  
* Add mutating/randomized terrain generation as the camera travels through the landscape. This would make the visualizer intruiging for longer periods of time.
* Make a custom shader to replace the need for a wireframe mesh.
* Synchronize the terrain to music with mp3 frequency or MIDI data.
* Add new models or assets such as a retro themed car to create a focal point in the visualizer and make better use of ray-tracing.

## Getting Started

1. Pull this repository.
2. Open the `/RetrowaveRoad` project folder with Unity 2021.3.14f1.
3. Make sure HDRP is updated and working.
3. Make sure the `MainScene` is selected under `/Assets/Scenes`. If you have a slower machine, you may want to select the `BareScene`.
4. Make sure the materials in `/Assets/Materials` have valid shaders selected in the Inspector. The recommended shaders are in the `/Assets/Shaders` folder.
5. In the Hierarchy, select the `Global Volume` game object which is a child of `Lighting`. Open the `HDRI Sky` dropdown in the `Volume` component in the Inspector. Make sure that an HDRI is selected next to the `Hdri Sky` checkbox.
6. Press play to run.

* To change the speed of the camera, select the `Main Camera` in the Hierarchy and toggle the speed of the `CameraController` script component in the Inspector.
* To change the properties of the terrain, select `LandscapeGenerator` in the Hierarchy and change the values of the `TerrainGenerator` script component in the Inspector. Note that terrain setting presets can be selected by clicking the slider icon at the top of the component.
* To view changing terrain values during runtime, enable `Test Values Realtime` under the `Terrain Properties`. Note that this will stop camera movement and disable the wireframe. The slider values will now modify the terrain while the program is running.
* Contact scott@fitzpatrick.net for any questions or for a higher quality HDRI.

## References

* [Mesh Generation Basics by Brackeys](https://www.youtube.com/watch?v=eJEpeUH1EMg)
* [Procedural Terrain by Brackeys](https://www.youtube.com/watch?v=64NblGkAabk&t=699s)
* [Generating Terrain by Brackeys](https://www.youtube.com/watch?v=vFvwyu_ZKfU)
* [Combining Meshes by Tudvari](https://www.youtube.com/watch?v=5WbmDZohtJY)

## Built With
* Unity 3D 2021.3.14f1
* Unity HDRP
* C#

## Author
* [Scott Fitzpatrick](https://www.linkedin.com/in/scott-fitzpatrick-/)

