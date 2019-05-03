# Discontinued

# RimWorld, Better Terrain

![](https://i.imgur.com/1SzUQXR.png)

Welcome!  
This mod showcases vast and sweeping changes to beautify the terrain of RimWorld colonies.  Each map will showcase multiple different 'mini-biomes' befitting the over-arching biome.  Temperate forests will have areas ranging from dense forests to praries to rocky plains, so on and so forth.
* __Plants:__ Now placed according to the 'mini-biome'.  Forests are dense with trees and bushes; plains have a thick layer of yellowish grass, patches of beautifull flowers and an overall splattering of shrubs.
* __Textures:__ Plants, trees, rocks, ground texture, mountain color... all recrafted for the sake of viewing pleasure.
* __Mountains:__  Now generally fallow the contours of land, tieing them into the overall immersive aesthetic.
* __Layering:__ Trees can now partially block out the view of rocks, people and animals; preventing the jarring appearance of them floating.
* __Harmony:__ Built using Andreas Pardeike's beautifull Harmony, allowing compatibility in all but fringe cases.

-----

### Requirements  
* Rimworld A16:
   Later Compatibilities not tested

-------------

### Compatibility
* _Vegetable Garden:_ 
   Minor Conflicts  
   * Load BetterTerrain after Vegetable Garden.
   * Both mods tweak some vanilla plants.
   
No other Reported Conflicts.

-----------------

Player notes

# Biomes

* Temperate Forest   
   100% Completed   
   A wonderful melting pot of the land: meandering rivers, automnal forests and hearty plains.
   * Birch tress have been given a glorious update, they now cascade in color from area to area
   * Grass has been fluffed up, it is now larger and more luxurious
   * Plant density, plants are more dense around rivers and lakes, falling to scarce near most mountains.
   
![](https://i.imgur.com/1HCE5Ov.png)

* Tropical Forest   
   80% Completed   
     * Finalizing art and colors.
   A dark, dense jungle:  large rivers, vast marshlands and reclusive mushroom patchs.
   * Cecropia trees, the staple of this jungle, they have been given a lushious green color and large, sun-shrouding canopies
   * Bushes, these have become serious ground cover, they turn most of the landscape into large flowing brush-life.
   * Mushrooms, rings of mushrooms are known to grow in breaks in the forest, a small relief from the otherwise inhospitable land
   
![](https://i.imgur.com/Y8gZNt0.png)

* Desert   
  50% Completed   
    * Adjusting landscape
    * Searching for new art
    
* Boreal Forest   
  30% Completed   
     * Adjusting landscape
     * Searching for new art
     
* Other   
  0% Completed   
     
# Technical Notes for Other Modders

C# Functions Edited
* GenStep_ElevationFertility   
   Tweaked the perlin maps for elevation and fertility.  Fertility is now broader, constant but less drastic fluctuations.  Elevation is slightly more spastic.
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
   Use Terrain variable, CanTakeFootPrint, as a notifier if a terrain is fertile.  en I used Sowtags in the ThingDef.plant class to tell the assemblies if the plant should go on fertile soil and if it should be based on the plant perlin map or not.  The first foreach method after the 5 list defines is just reading the tags from all the plants in the biome and putting them in the correct list.  The huge foreach after I define the canPlaceList takes a random tile in the map (vanilla methodology) exc after it determine if the ground was plantable, I added in my stuff.  I check if the land is fertile or not, then for each plant in the biome that should be placed on that terrain (fertile or unfertile)  I get the group of plants that should go there and should be placed using the perlin map (usually trees), choose a random one based off the perlin value.
Then I add in all the plants that are to be randomly scattered about that terrain (grass, debris, filler, exc exc)
