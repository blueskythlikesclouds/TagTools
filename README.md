# Tag Tools
Tools for editing Havok 2016 1.0 binary tag files.  

# Releases
### [EXE Release](https://ci.appveyor.com/project/blueskythlikesclouds/tagtools/build/artifacts)

# Tools
## Tag Tools
This tool converts Havok files **(version <= 2012 2.0)** to 2016 1.0 tag binary files, and vice versa.
### Usage
``TagTools [source] [destination]``  
If the destination path is not included, the changes will be overwritten to the source file.  
This way, you can simply drag and drop your file to TagTools, and get your new file.
### Example
``TagTools chr_Sonic_HD.skl.hkx chr_sonic.skl.hkx``

## Collision Converter
This tool converts HKX files with rigid bodies to static compund shapes.  
For example, this can be used to convert Sonic Generations collision to Sonic Lost World / Sonic Forces collision.

**NOTE: This tool outputs HKX files in 2012 2.0 version. To get 2016 1.0 tag file, run the file with TagTools.**
### Usage
``CollisionConverter [source]``  
The changes will be overwritten to the original file.  
This way, you can simply drag and drop your file to CollisionConverter, and get your new file.
### Example
``CollisionConverter ghz200_col.phy.hkx``

This tool was originally made by NeKit, TwilightZoney and N69 for Sonic Lost World.  
Modified by me to work with Tag Tools.
