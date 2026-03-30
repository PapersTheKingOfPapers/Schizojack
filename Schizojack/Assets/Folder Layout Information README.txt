Each base folder contains a "what goes here" text file. It contains the info for what goes into those folders.

Keep the file layout the same for models; if you have a layout that looks like this:

Apple
-Model
--apple.fbx
--apple_dif.png
--apple.mat
--apple.blend

or in single lines:

Apple\Model\apple.fbx
Apple\Model\apple_dif.png
Apple\Model\apple.mat
Apple\Model\apple.blend

they should be foldered like this

Assets
-Blender
--Apple
---apple.blend

-Meshes
--Apple
---apple.fbx

-Materials
--Meshes
---Apple
----apple.mat

-Textures
--Meshes
---Apple
----apple_dif.png

or in single lines:

Assets\Blender\Apple\apple.blend
Assets\Meshes\Apple\apple.fbx
Assets\Materials\Meshes\Apple\apple.mat
Assets\Textures\Meshes\Apple\apple_dif.png

Notice that the Textures and Materials folder has a Meshes folder, it's there to seperate the mesh textures/materials from the sprite textures/materials.

Preferably we won't store .blend files in the git, because they also store materials, textures, and such in them. But in the beginning we will have them
because we don't have 500 different .blend files.