# Tag Tools
Tools for editing Havok 2016 1.0 binary tag files.

# Releases
### [EXE Release](https://ci.appveyor.com/project/blueskythlikesclouds/tagtools/build/artifacts)

# Tools
## Tag Tools
This tool can be used to convert Havok files (version <= 2012 2.0) to Havok 2016 1.0 binary tag files, and vice versa.

### Usage
``TagTools [source] [destination]``  
Destination is optional, meaning you can do a drag and drop, saving changes to the source file.

### Example
``TagTools chr_Sonic_HD.skl.hkx chr_sonic.skl.hkx``

## Collision Converter
This tool converts rigid bodies within a Havok file to static compound shapes.  
For example, this can be used to convert Sonic Generations collision to Sonic Lost World / Sonic Forces collision.

To apply collision properties, simply append "tags" at the end of the names of your collision meshes.  
**Example:** MyCollisionMesh -> MyCollisionMesh@Grass

Any of the following can be used as valid tags:  
```
@STONE
@EARTH
@WOOD
@GRASS
@IRON
@SAND
@PHANTOMCUBE
@FORD
@GLASS
@SNOW
@SANDFALL
@ICE
@WATER
@SEA
@WATERFALL
@DEAD
@WATERDEAD
@DAMAGE
@POOL
@AIR
@INVISIBLE
@WIREMESH
@DEAD_ANYDIR
@DAMAGE_THROUGH
@DRY_GRASS
@WETROAD
@SNAKE
```

**NOTE:** This tool saves files in 2012 2.0 packfile format. If you wish to get 2016 1.0 binary tag file, use TagTools after converting your collision.
### Usage
``CollisionConverter [source]``  
Since the tool accepts only one argument, you can do a drag and drop, saving changes to the source file.

### Example
``CollisionConverter ghz200_col.phy.hkx``

This tool was originally made by NeKit, TwilightZoney and N69 for Sonic Lost World.  
Modified by me to work with Tag Tools.

# File Format Documentation
You can find the documentation of Havok 2016 1.0 binary tag format in the [wiki](https://github.com/blueskythlikesclouds/TagTools/wiki/Format-Documentation) page.

# License
Tag Tools uses the MIT License.  
For details, see [LICENSE](https://github.com/blueskythlikesclouds/TagTools/blob/master/LICENSE).
