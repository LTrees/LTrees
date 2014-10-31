# User Manual for XNA Procedural LTrees

## Overview of Classes

A tree is composed of three parts: A `TreeSkeleton`, a `TreeMesh` and a
`TreeLeafCloud`. The skeleton describes the topology of the tree, and here you 
can inspect the positions of each individual branch and leaf (if you wanted to). 
The tree mesh contains the geometry required to render the branches, but it must 
be given an `Effect` from outside to render itself - it sets no effect 
parameters on its own. Likewise, the tree leaf cloud contains the vertex buffer 
for the leaves.

The `SimpleTree` contains all you need to render a tree, including the skeleton, 
mesh, leaf cloud, textures, effects, and animation state. It will set effect 
parameters in its draw calls for you.

A `TreeAnimationState` contains the rotations of each bone in a tree, and is 
stored independently of the other parts, so one tree can be used in several 
copies.

`TreeGenerator` is the class that generates the random tree. It produces a tree 
skeleton, which can be used to create a tree mesh and a leaf cloud.

## Render States

The tree and leaf shaders set a couple of render states automatically, but they 
do not change them back. Managing render states is a central aspect of any game, 
and LTrees will not do it for you. Therefore, *it is your own responsibility to 
restore the previous render states*. If you are having graphical issues with the 
trees, it is most likely a render state issue.

## Applying Wind

You need two objects: A `WindSource` which describes the wind strength at 
different times and places, and the `TreeWindAnimator` helper class.
`WindSource` is an interface so you can provide your own definition of wind 
strength, but a simple implementation is included in the project.

To get started, add these two fields in your `Game` class or similar location:

``` csharp
public class Game1 : Game
{
    WindStrengthSin wind;
    TreeWindAnimator animator;
    // ...
```

Create them during initialization:

``` csharp
protected override void LoadContent()
{
    wind = new WindStrengthSin();
    animator = new TreeWindAnimator(wind);
    // ...
}
```

In the `Update` method, update the wind source so it knows what time it is, and 
then use it to animate your trees:

``` csharp
protected override void Update(GameTime gameTime)
{
    wind.Update(gameTime);
    // ...
}

protected override void Draw(GameTime gameTime)
{
    foreach (SimpleTree tree in MyCollectionOfTrees)
    {
        animator.Animate(tree.Skeleton, tree.AnimationState, gameTime);
    }
    // ...
}
```

Note that the `WindStrengthSin` was designed to demonstrate the wind feature. 
In practice, it can be tiresome when all the trees are blowing in such an 
irregular wind. Currently, you are encouraged to write your own implementation 
of `WindSource`, which also allows you to synchronize it with the wind that 
affects other scenery like grass and snowflakes.

* *Caution: The `BoundingSphere` property of the `TreeMesh` is only guaranteed to 
  enclose the tree in its initial animation state.*
* *Hint: Trees in your world can share the same mesh but still use different 
  animation states, so they can be animated independently.*

## Using Multiple Levels-of-Detail (LOD)

A common way to improve performance is to reduce the amount of detail on models 
the further they are from the camera. When sufficiently far away, you want to 
display a billboard instead of rendering the tree, but that is another topic
("Impostering"). Before turning the tree into a billboard, we can decrease the 
number of polygons in the trunk.

To create a low-detail model of a tree, try this:

``` csharp
TreeMesh lowPolyMesh = new TreeMesh(GraphicsDevice, tree.Skeleton, 4);
```

The third argument is the number of radial segments to use in the root branch. 
The default is 8 - a value of 4 makes it look like a square, but should 
hopefully not be noticeable at a distance. Note that the number of radial 
segments in branches will gradually decrease towards 3 (the minimum) as the 
radius gets smaller.

There is currently no way to decrease the level of detail on leaves.

## Using Features like Fog, Point Lights, and Shadows

The shaders provided in the project only allow for two directional lights, 
hardware-skinned animation, and nothing else. In modern games this is 
insufficient, but the exact requirements depends too strongly the individual 
application, which is why serious users are encouraged to write their own 
shaders and/or a replacement for `SimpleTree` to render their trees. The 
reference shaders are included in the content project bundled in LTreeLibrary.

To replace the default shaders inserted by the content loader, add this 
**before** loading any trees:

``` csharp
TreeProfileReader.DefaultTreeShaderAssetName = "Effects/<YourShaderHere>";
TreeProfileReader.DefaultLeafShaderAssetName= "Effects/<YourShaderHere>";
```

This way, you can still use the `TreeProfile` loaded by the content manager - 
nobody says you have to call `GenerateSimpleTree` to generate your trees.
`TreeProfile` has all the properties available required to make your own 
implementation.

Alternatively, you can change the effect loaded for each individual tree profile 
by settings its "Trunk Effect" and/or "Leaf Effect" properties in the property 
editor.

* *Note: The tree mesh contains holes and is self-intersecting, so stencil 
  shadows (aka. shadow volumes) cannot be used to draw shadow. Shadow mapping 
  is the recommended approach.*

## Avoiding the Content Pipeline

For those who don't like the content pipeline, an `.ltree`-file can be loaded at 
runtime using the static method `TreeGenerator.CreateFromXml`. Note that this 
method does not load textures or effects for you. Use the resulting
`TreeGenerator` to generate a `TreeSkeleton`, and from the skeleton create a 
`TreeMesh` and a `TreeLeafCloud`.

## Creating more Tree Profiles

This is not an easy task. One way is of course to copy an existing tree profile 
and simple replace the texture with new ones. To change the texture used, open 
the `.ltree` file in your favourite XML editor (double-clicking it in Visual
Studio will work). Find these two lines:

```
    <TrunkTexture>...</TrunkTexture>
    <LeafTexture>...</LeafTexture>
```

Replace the inner text of these two tags with the asset names of the trunk 
texture and leaf texture you want.

To understand how the structure of the tree is generated, you must know what an 
L-system is (LTrees got its name from "L-system"). I suggest starting with an 
[interactive example in 2D](http://lsysjs.qwert.ch). The tree generator works 
like this, only in three dimensions. Think of it as moving around a crayon in 
3D space, and this crayon paints tree branches in its wake. The L-system 
produces a long sequence of instructions for the crayon. Instructions include to
move forward, rotate around its own X or Y axis, scale, store the current state,
restore the previously stored state, place a leaf here, place a bone here, and
so on.

A *production* contains a sequence of instructions, and the instruction *Call*
makes the crayon execute the instructions in a specified production, much like a
method call in C#. The *Child* tag instructs the crayon to remember its current 
state, execute the nested instructions, and then restore the original state 
again. To avoid a tree that grows into infinity without ever stopping, the 
crayon knows its current *level* as part of its state. Executing a *Call* 
instruction by default decreases the level by one, and if the level ever would 
become negative, the crayon instead omits the call.

If you are up to the challenge, from here on you must use the existing profiles 
as reference to try through trial and error to create your own profile. The 
content pipeline will complain if there are syntactic errors in your profile.

## Collision Detection and Physics

To perform collision detection and physics simulation, it is required to know 
the general size and shape of the tree. For simple circle-based collision 
detection, `tree.Skeleton.TrunkRadius` is a good collision radius to use, as it 
is the radius at the bottom of the tree.

For more advanced physics, the `TreeSkeleton` should provide sufficient 
information about the tree's topology to build an appropriate representation of 
it. There is no official support for breakable branches, but it might be 
possible like this. Once your physics library has detected that a branch should 
break, split the tree skeleton into two parts: the descendants of that branch, 
and the rest. Then generate new meshes and leaf clouds for the two skeletons. 
The skeleton generated from the free-broken branch should then be transformed to 
the branch's original position on the tree.