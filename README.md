[![Build status](https://ci.appveyor.com/api/projects/status/d76n0ik2rmx7dxt3?svg=true)](https://ci.appveyor.com/project/mcneel/ghcanvasviewport)
[![Install package](https://img.shields.io/badge/dynamic/json?label=yak&query=version&url=https%3A%2F%2Fyak.rhino3d.com%2Fpackages%2FGhCanvasViewport)](rhino://package/search?q=GhCanvasViewport)

# GhCanvasViewport
A Rhino viewport control embedded in the Grasshopper canvas

### This has been overhauled with the following changes

#### Additions:

- New Menu Item "Locking Options" which contains sub menus allowing you to lock the rotation/orientation (Right Click) and dragging/panning (Left Click) functionality for the viewport. Additionally, there are now two icon sets to show the lock state for each lock option with a sub menu allowing you to switch between the icon sets. There is also an option to disable the icons, and an option to dock the icons to any corner of the viewport.
- New Menu Item "Set View" which sets the view perspective for the viewport to any of the following views (Top, Bottom, Left, Right, Front, Back, Perspective, TwoPointPerspective). This is set in a static manner so any additional Named Views will not be included in this list. Additionally, if any of these are undefined in your Rhino installation it may do weird things :)
- Three New Menu Items "Toggle Grid", "Toggle Grid Axes", "Toggle World Axes". These should be pretty self-explanatory. It is worth noting that they only affect this viewport though and will not change any of your viewports in Rhino. Also worth noting, these only work for Display Modes that allow for them. They will simply be ignored on Display Modes that do not and resume functionality on the ones that do when you switch back to those Display Modes. 
- New Menu Item "Reset Camera" which resets the camera location and re-centers the geometry in the viewport. This will not change the orientation if you have rotated the perspective. This will not change the zooming level either. You can use the "Zoom Extents" menu item for that if needed.
- New Menu Item "Reset View" which resets the viewport camera, perspective, and zooming to some extent. This will change the orientation and re-center the geometry along with zooming in or out to show the geometry as needed.
- New Menu Item "Restore Defaults" which resets the viewport to a set of default values. This will reset all of the values for the XML file mentioned below. The only setting this does not reset is the Open/Closed state of the viewport.

- New Feature "Persistence". All of the settings for the viewport will now write themselves to a new XML file created in the AppData->Grasshopper folder. This should happen automatically using the `GH_SettingsServer Class` and should not require any effort or adjustment. The file will been created upon opening Grasshopper and initialized with default settings. After that any settings you change will be reflected in the file and those settings will be restored to the viewport for you. These settings will persist through opening and closing Grasshopper as well as opening and closing the viewport itself. This includes the viewport size, display mode, view, dock location, all lock settings, grid and axes toggle settings, and most notably whether the viewport is open or closed. These settings are updated each time you make changes to anything they watch for.
- New Feature: The Grasshopper->Display menu item for this now has an icon and correctly highlights the icon based on the open/closed state of the viewport.
- New Feature: The Grasshopper->Display menu item for this now has a tooltip explaining what this is and mentions the right-click menu for those that were unaware it had a context menu. 
 
---

#### Images for those that prefer visuals and for the English Impaired :)

<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv1.png" alt="1" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv2.png" alt="2" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv3.png" alt="3" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv4.png" alt="4" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv5.png" alt="5" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv6.png" alt="6" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv7.png" alt="7" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv8.png" alt="8" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv9.png" alt="9" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv10.png" alt="10" />
<img src="https://github.com/GrimblyGorn/GhCanvasViewport/blob/master/pics/cv11.png" alt="11" />

---

#### Known Issues:

- Currently there are no known issues.
   
#### Notes & Other Thoughts:

- There is still some weirdness when repeatedly right-clicking the viewport in different locations. It tends to rotate the geometry around on each click. At times a similar effect occurs with left-clicking as well causing the geometry to pan around large areas quickly. This behavior was already an issue before my overhaul but was beyond the scope of my concern currently so I left it as it was. These quirks can be avoided with the new locking functions if you don't need the panning or rotation at the time, though that is really more of a bandage than a solution.
- During my testing there was a point where I was able to spawn a second viewport canvas and assign it separate settings from the first (docking location, size, view, etc.). This was, at the time, a bug and was creating additional menu items in the Grasshopper->Display menu so it is currently no longer possible to do. However, it is a possibility that could be explored later and potentially implemented as a menu item for those that wish to spawn additional viewports to have multiple views or display modes. That being said, it is currently not something I am concerned with so that is not an option for now.
- The "Display Modes" menu is and was already dynamically generated so any custom modes you may have will show up in the list of choices. It would probably be better if my "Set View" menu was dynamically generated as well in case anyone had custom views they wanted to use. This is a point for improvement later perhaps.       