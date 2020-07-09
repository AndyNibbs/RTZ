# Errors - bad RTZ 1.1 files
Systems should reject these files.
Note: I think it unfair to specify that a system should trap *all* these errors.

### "Mainline" Errors

These are errors in the aspects of RTZ that most systems will use (NonsenseGeometryTypeError.rtz especially - NO system should fail that)

- RouteNameDoesNotMatchFilenameError.rtz - routeName attribute does not match filename
- DuplicateWaypointIdError.rtz - first two waypoints have same id (11 in both cases)
- MissingWaypointIdError.rtz - first waypoint does not have id
- NegativeRevisionError.rtz - first waypoint has revision of -1
- GeometryTypeError.rtz - contains a leg geometry type of "GreatCircle"
- NonsenseGeometryTypeError.rtz - contains a leg geometry type of "Nonsense" (whilst a system might be tolerant of BadGeometryType absolutely no system should cope with this)

ScheduleError.rtz
- first schedule, calculated ids are just one based index (which to a programmer is vile)
- first schedule, calculated first waypoint etd is after all etas in following WP
- second schedule first etd is 30th of February
- owing to that broken date it after it finishes

### Esoteric Errors

These are errors in the areas of the standard that fewer systems will use. For example many systems unless they do a schema check will not pay attention to wind information left by a weather optimiser or extra non-standard extensions. Doing a schema check is fraught with risk for an implementor because it may deny import of a basically sound but not strictly compliant file. Customers will demand an urgent "fix" for this (someone else's error) and no one will care who is in the right. 

**EsotericRouteInfoError.rtz**
- validityPeriodStart is after validityPeriodStop
- extension element in routeInfo is not wrapped in a extensions element (and extending of routeInfo is not mentioned in std)

**EsotericScheduleError.rtz**
- second schedule contains wind and current directions of 370 and negative speeds
- defaultWaypoint leg safetyContour is zero 

# Check tool results for error files

A report for all sample files is in the root samplefiles folder