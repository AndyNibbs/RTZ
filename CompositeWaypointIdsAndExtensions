The standards are naturally pared-back descriptions to avoid confusion. 

This is how I see the use of id and revision working to allow systems to process extensions that they do not understand. Each waypoint element has a mandatory id attribute which should be unique within the file. Optionally each waypoint can also have a revison attribute. (Both should contain positive integers.)

It took some time before I realised the intent of revision attributes. I can still see issues and better ways to achieve the aim. But here is how it is intended to work...

Systems that work with RTZ (ECDISs, route optimisers, planning systems and so on) can work with (read, write and process) proprietary *extensions*.

Not all systems will understand a given extension but may *modify* things that have been extended. Systems that *do* understand that extension have to be able to detect these modifications because they may mess up the extension. 

Simple example:

For convenience if my system chose to compute the lengths of legs we could put that in an extension and export it.

```
<waypoint id="1" revision="0">
   <position whatever/>
 </waypoint>
 <waypoint id="66" revision=0">
   <position whatever/>
   <extensions>
     <extension manufacturer="chersoft" name="length" version="1.0.0">
       <length id="66" revision="0">200</length>
     </extension>
   <extensions/>
  </waypoint>
...
```

So when a system that doesn't understand my extension changes waypoint id 66 it should ensure that when it exports it as RTZ it increments the revision number (and *the original extension*).

When the original system that *does understand* the extension gets the file it can see that the length element refers to id 66 revision 0 but that revision is now 1. 

So it then knows that it cannot trust the contents of the extension.

I do feel that this might only partially solve the problem and by partially solving it create new classes of harder to diagnose problems. 








  
  
