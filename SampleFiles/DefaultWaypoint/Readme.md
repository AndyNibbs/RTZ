# Test route file focussing on DefaultWaypoint

[DefaultWaypoint.rtz](./DefaultWaypoint.rtz)

[Defaultwaypoint.png](./DefaultWaypoint.png)

T14: Make/or choose a test file to suit 

6.9.8
s)	Import a route that contains default waypoint and leg data. Add a new leg to the route. Confirm by observation that the default data is applied or not applied to the new waypoint and leg in accordance with the guidance given in the user manual. (Annex S.5.5 and 5.5.6; Test file )

The route file is a simple file that has a full defaultWaypoint. The most striking thing from a test PoV is that it redefines the default for leg geometryType to orthodrome (aka Great Circle) and leg elements on each most waypoints set this back to loxodrome/rhumb line. The obvious long transatlantic leg should be great circle (see picture).


QUESTIONS

What can a defaultWaypoint include? - I think quite a lot of things as this sample suggests but need to make sure that fits with Simon Cooke's schema knowledge

speedMin speedMax vs. planSpeedMin planSpeedMax

xsd:time or xsd:duration

