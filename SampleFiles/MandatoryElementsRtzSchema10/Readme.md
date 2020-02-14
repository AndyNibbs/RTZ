# Test route file conforming to the minimum mandatory elements of schema version 1.0

[Rtz10Mandatory.rtz](./Rtz10Mandatory.rtz)

[Picture](./RouteLinePicture.png) 


# Notes

This is for task T6 for sub-group working on ECDIS WG development of the RTZ1.1 standard

T6: Create a test route file conforming to the minimum mandatory elements of schema version 1.0 for Clause 6.9.8 (d)

Text from developing standard reads:

6.9.8 Route Exchange
d) Import a route conforming to the minimum mandatory elements of schema version 1.0. Confirm by observation that the file imports without error and that all parts of the route are correctly presented. (Annex S.1; Test file MISSING) 


NOTE: To flag up a potential pitfall for implementors Waypoint IDs are deliberately not in rising numeric order to avoid giving impression that id should equal the index (S.5.6 Comment "It does not have to be equal to index)

NOTE: Waypoints exist in with positive and negative Lat and Lon (all four quadrants) and route crosses the "date line" (180 degree Lon).


# TODO/QUESTIONS:

**is Leg mandatory?!**
(S.5.6 says that it is in the table. But std also gives default leg type as Loxodrome and permits a DefaultWaypoint so believ table in S.5.6 is misleading)
