# LTrees for MonoGame

## Project Description

Creates natural-looking tree models for MonoGame in real-time. LTrees makes it 
easier for MonoGame game developers to create beautiful nature scenes.

The original XNA version is available in the
[v2.1](https://github.com/LTrees/LTrees/tree/v2.1) tag here on GitHub, as well
as on the [CodePlex site](http://ltrees.codeplex.com/).

## Screenshots

Here are some screenshots of a few generated trees. The demo application gives a 
better impression of them, though.

![](http://download-codeplex.sec.s-msft.com/Download?ProjectName=LTrees&DownloadId=56673)

![](http://download-codeplex.sec.s-msft.com/Download?ProjectName=LTrees&DownloadId=56677)

## Getting Started

*TODO: Installation instructions*

* Copy the .ltree-files and textures from the LTreeDemo project. (Note: You don't 
  need the Grass.jpg texture). Keep the same folder structure for now.
* For each .ltree-file, change its importer to `LTree Specification` and its 
  processor to `LTree Profile`.
* Enable mipmap generation for all the textures.
* Load a tree profile using the content manager:
  `TreeProfile profile = Content.Load<TreeProfile>("Trees/Graywood");`
* Generate a SimpleTree from the tree profile:
  `SimpleTree tree = profile.GenerateSimpleTree();`
* Draw the trunk:
  `tree.DrawTrunk(world, view, projection);`
* Draw the leaves:
  `tree.DrawLeaves(world, view, projection);`

This was a guide intended to get you started as fast as possible. More advanced 
behaviour is possible, but for that you need to know more about how it works. 
See the [User Manual](doc/user-manual.md) to learn more.

## So How Does It Work?

See the [User Manual](doc/user-manual.md).

## Changelog

[Changelog](CHANGELOG.md)

## Licence

[Licence](LICENCE.txt)