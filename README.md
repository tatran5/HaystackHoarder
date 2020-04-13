# HaystackHoarder

Unity version used to create this project: 2019.2.13f1

When create a new map, please read through the following classes

### Haystack
When putting haystacks into scene, it is **important** to have some haystack [deactivate](https://docs.unity3d.com/Manual/DeactivatingGameObjects.html). When an active haystack runs out of hay, it will be set to be inactive, and try to randomly find an inactive haystack in the scene and set it to be active. If there's no haystack in the scene that is inactive, the program will run into an **infinite loop**

Haystack can be set to be invisible initially. An invisible haystack appears as a visible haystack runs out of hay (and becomes invisible). An invisible haystack ignores collision by other objects in the scene.