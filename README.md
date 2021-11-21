# Ray Caster
My ray casting engine project, most of the calculations came from this beautiful piece of art: https://lodev.org/cgtutor/raycasting.html

## **Simple Ray Caster**

#

## **Ray Caster**
![Textured Ray Caster](https://github.com/CallMeRZIBI/Ray_Casting/blob/Development/readme/textured_ray_caster.gif)
### When creating ray caster you must give it those arguments:
```C#
RayCasting.TexturedRayCaster rayCaster = new RayCasting.TexturedRayCaster(screenWidth, screenHeight, renderingScale);
```
- screenWidth - int 
- screenHeight - int
- renderingScale - int - scales the rendering width and size (default is 1) *isn't working at the moment

#

### To create map that you can walk on you have to call this method
```C#
rayCaster.CreateMap(map, StartingPosX, StartingPosY, dirX, dirY, planeX, planeY);
```
and give it those parameters
- map - 2D array of numbers which are corresponding to indexes of textures - 1 (0 isn't anything)
- StartingPosX - double which corresponds to position of player X axis
- StartingPosY - double which corresponds to position of player Y axis
- dirX - double (default is -1) correspond to rotation
- dirY - double (default is 0) correspond to rotation
- planeX - double (default is 0) correspond to
- planeY - double (default is 0.66) correspond to

#

### Set what kind of floor and ceiling you want
To have floor and ceiling only as color use this method
```C#
rayCaster.UseFloorCeilingColors(FloorColor, CeilingColor);
```
and give it those parameters as arguments
- FloorColor - 1D byte array with RGB values from 0 - 255
- CeilingColor - 1D byte array with RGB values from 0 - 255

#

And to have textured ceiling and floor use this method
```C#
rayCaster.UseFloorCeilingTextures(TexFloorIndex, TexCeilingIndex);
```
and give it those parameters as argument
- TexFloorIndex - int - index of texture in textures list that you give to ray caster
- TexCeilingIndex - int - index of texture in textures list that you give to ray caster

#

### Update frame
```C#
rayCaster.UpdateRayCast(W_Down, A_Down, S_Down, D_Down);
```
and give it those parameters as arguments for implemented movement
- W_Down - bool if W is pressed (true = pressed)
- A_Down - bool if A is pressed (true = pressed)
- S_Down - bool if S is pressed (true = pressed)
- D_Down - bool if D is pressed (true = pressed)

#

### Get frame from ray caster
```C#
rayCaster.GetRawBuffer();
```
returning value
- 3D byte array which you can directly use as RGB texture for OpenGL

#

## **Textures**
### You can load textures in two different ways
### From path
```C#
rayCaster.LoadTexturesFromPaths(paths);
```
You have to give it this parameter as argument
- paths - 1D string array which is containing paths to images

#

### Or load it yourself and then give List of textures to it
```C#
rayCaster.LoadTextures(textures);
```
You have to give it this parameter as argument
- textures - List of Textures, the Texture class is in the library

#

### Creating texture
```C#
RayCasting.Texture texture = new RayCasting.Texture(path);
```
You have to give it this parameter as argument
- path - string with path to image

#

## **Sprites**
### You can load sprites with this method
```C#
rayCaster.LoadSprites(sprites);
```
You have to give it this parameter as argument
- sprites - List of Sprites

#

## Creating sprite
```C#
RayCasting.Sprite sprite = new RayCasting.Sprite();
```
### This class have three public variables
- posX - double for position
- posY - double for position
- posZ - double for position (default is 0) *relies on resolution
- scaleX - double for scailing (default is 1)
- scaleY - double for scailing (default is 1)
- texture - Texture of the sprite
### You can give sprite texture simply like public variable or with this method
```C#
sprite.LoadTextureFromPath(path);
```
And give it this parameter as argument
- path - string with path to image