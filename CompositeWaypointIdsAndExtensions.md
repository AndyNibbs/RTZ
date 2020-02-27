The standards are naturally pared-back descriptions to avoid confusion and t took some time before I realised the intent of revision attributes. I can still see issues and better ways to achieve the aim. But here is how it is intended to work...

**Aim is to allow systems to process extensions that they do not understand.** Each waypoint element has a mandatory id attribute which should be unique within the file. Optionally each waypoint can also have a revision attribute. 

Systems that work with RTZ (ECDISs, route optimisers, planning systems and so on) can work with (read, write and process) proprietary *extensions*. But of course not all systems will understand a given extension but may *modify* waypoints that have been extended. Systems that *do* understand that extension have to be able to detect modifications because they may mess up the extension. 

Simple example:

For convenience my system chooses to compute the lengths of legs we and puts that in an extension (perhaps for another downstream system that doesn't have the ability to do the distance calcs, or maybe because it is just an example)

```
<waypoint id="1" revision="0">
   <position whatever/>
 </waypoint>
 <waypoint id="66" revision=3">
   <position whatever/>
   <extensions>
     <extension manufacturer="chersoft" name="length" version="1.0.0">
       <length ids="66.0,1.3">200</length>
     </extension>
   <extensions/>
  </waypoint>
...
```

Then a system that doesn't understand my extension changes the position waypoint of waypoint 66. The length in the extension is now wrong. 

But because it is a well behaved system when it exports RTZ it should contain that waypoint with an incremented revision number (and *the original extension, which is crucial*). **Systems should preserve extensions that they do not understand!** 

When the original system that *does understand* the extension gets the file it can see that the length element refers to id 66 revision 3 but that revision is now 4. 

**This is how it knows that it cannot trust the contents of the extension.**

I do feel that this might only partially solve the problems here and that a partial solution creates new classes of (harder to diagnose)  problems. 








  
  
