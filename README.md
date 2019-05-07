# Discontinued

# RimWorld, Better Terrain

![](https://i.imgur.com/1SzUQXR.png)

Welcome!  
This mod showcases vast and sweeping changes all aimed at beautifying the terrain of RimWorld colonies.  It ties together the once independent 'terrain', 'plant' and 'mountain' generation systems of RimWorld into one coherent ecosystem. 

A single map can contain areas ranging from wet, lush, regions of vibrant green grass and flowers... to dense forests thick with underlying brush... to large fields of yellowish grass, interwoven with patches of beautifull flowers... to unfertile areas with little life short of sporadic flowers, hearty trees, and hearty shrubs...

Outside of that, there are tweaks to RimWorld's layering.  Originally rocks, people, and animals are all drawn on top of trees.  If a man walks 'behind' a tree the engine paints him last so it appears like he is walking on top of a 2-dimensional tree painted onto the ground.  This is jarring, now, trees can partially block out the view of rocks, people, and animals behind them. 

Finally, special thanks to Andreas Pardeike's and his beautifull, beautifull Harmony mod.  It allows all these vast and varying changes to be formed into one extremely compatible yet easy to install package.

-----

### Requirements  
* Rimworld A16:   
   Later Compatibilities not tested

-------------

### Compatibility
* _Vegetable Garden:_   
   Minor Conflicts.  Load BetterTerrain after Vegetable Garden. 
   * Vegetable Garden will not allow you to to plant some native plants, like agave.
   
No other Reported Conflicts.

-----------------

Player notes

# Biomes

### Temperate Forest  
   A wonderful melting pot of the land: meandering rivers, automnal forests and hearty plains.
   * Birch tress have been given a glorious update, they now cascade in color from area to area
   * Grass has been fluffed up, it is now larger and more luxurious
   * Plant density, plants are more dense around rivers and lakes, falling to scarce near most mountains.
   
   100% Completed   
   
![](https://i.imgur.com/1HCE5Ov.png)

### Tropical Forest   
   A dark, dense jungle:  large rivers, vast marshlands and reclusive mushroom patchs.
   * Cecropia trees, the staple of this jungle, they have been given a lushious green color and large, sun-shrouding canopies
   * Bushes, these have become serious ground cover, they turn most of the landscape into large flowing brush-life.
   * Mushrooms, rings of mushrooms are known to grow in breaks in the forest, a small relief from the otherwise inhospitable land
   
   80% Completed   
   * Finalizing art and colors.   
   
![](https://i.imgur.com/Y8gZNt0.png)

### Desert   
  50% Completed   
  * Adjusting landscape   
  * Searching for new art
    
### Boreal Forest   
  30% Completed   
   * Adjusting landscape   
   * Searching for new art
     
### Other   
  0% Completed   
  


-------------------------

## Overview Of Changes
  * __Terrain:__ The type of ground in a map is based on a random fertility map, areas of high fertility get lakes and fertile soil.  Originally, all other fertilies get the base ground for the region, this mod causes areas of lower fertility to get more barren and then hilly terrains.
  * __Mountains:__  Originally RimWorld's mountains are independently formed off of an elevation map.  Now mountains are tied into the terrain.  They will be more likely to form in hilly terrains, yet still randomly branch off into forests, plains, and lakes.
  * __Plants:__ Originally every plant in RimWorld is randomly scattered, possibly in patches, throughout a map.  Now plants are tied into the elevation and fertility of a map.  They will only be placed in fitting areas of a map and in propper quantities.
  * __Layering:__ Rewordered the the paint layers.  People, animals, and rocks are moved ahead of trees, so if a tree is large enough to block out the view of a person, animal, or rock, it will do so.
  * __Textures:__ Little hasn't been touched.  Most plants, trees, rocks, ground texture, mountain color all have been recrafted to fit the flowing landscapes of BetterTerrain.
  * __Harmony:__ This mod is built using Andreas Pardeike's beautifull Harmony, allowing compatibility in all but fringe cases and conflicting XML edits (typically from editing a default plants attributes).

---------------------
     
# Technical Notes of Changes

C# Functions Edited
* GenStep_ElevationFertility   
   * Tweaked the perlin maps for elevation and fertility.  Fertility is now broader, constant but less drastic fluctuations.  Elevation is slightly more spastic.
I added a perlin map for trees. Used in GenStep_Plants.

* GenStep_RocksFromGrid   
  Generate  
   * call in the fertility grid to use alongside the mountain perlin map for mountains placement   
2 perlin maps give much more interesting, flowing shapes.
To keep the map from just looking topographical and dull I multiply points on the elevation grid by ones on the fertility grid.
I set it up so mountains can spawn only when elevation is high and when fertility is high or very low.  
This means mountains will genererally form in areas away from low fertility areas, but occasionally pop up around lakes or rivers, exc.

* GenStep_Terrain
  TerrainFrom   
   * added in if statements that calculate a noise variable.  Noise is the value the biome XMLs will use when determining what terrain goes where.
Higher elevations calculate the XML value based more on elevation, lower elevations calculate it based more on fertility.

* GenStep_Plants   
   * Use Terrain variable, CanTakeFootPrint, as a notifier if a terrain is fertile.  en I used Sowtags in the ThingDef.plant class to tell the assemblies if the plant should go on fertile soil and if it should be based on the plant perlin map or not.  The first foreach method after the 5 list defines is just reading the tags from all the plants in the biome and putting them in the correct list.  The huge foreach after I define the canPlaceList takes a random tile in the map (vanilla methodology) exc after it determine if the ground was plantable, I added in my stuff.  I check if the land is fertile or not, then for each plant in the biome that should be placed on that terrain (fertile or unfertile)  I get the group of plants that should go there and should be placed using the perlin map (usually trees), choose a random one based off the perlin value.
Then I add in all the plants that are to be randomly scattered about that terrain (grass, debris, filler, exc exc)
