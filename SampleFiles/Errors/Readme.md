# DetailErrors.rtz


### "Mainline" Errors

These are errors in the aspects of RTZ that most systems will use (BadGeometryType especially - NO system should fail that)

- RouteNameDoesNotMatchFilename.rtz - routeName attribute does not match filename
- DuplicateWaypointId.rtz - first two waypoints have same id (11 in both cases)
- MissingWaypointId.rtz - first waypoint does not have id
- NegativeRevision.rtz - first waypoint has revision of -1
- BadGeometryType.rtz - contains a leg geometry type of "GreatCircle"
- NonsenseGeometryType.rtz - contains a leg geometry type of "Nonsense" (whilst a system might be tolerant of BadGeometryType absolutely no system should cope with this)



I've got this far...






### More esoteric routeInfo faults

- validityPeriodStart is after validityPeriodStop
- vesselGM is 'm' appended (unit for metres, but is wrong)
- extension element in routeInfo is not wrapped in a extensions element (and extending of routeInfo is not mentioned in std)

### Bad schedules

- first schedule, manual waypointIds match waypoints (which have bad ids)
- first schedule, calculated ids are just one based index (which to a programmer is vile)
- first schedule, calculated first waypoint etd is after all etas in following WP
- second schedule first etd is 30th of February
- owing to that broken date it after it finishes

### Esoteric schedule issues

- second schedule contains wind and current directions of 370 and negative speeds
- defaultWaypoint leg safetyContour is zero 



Note: I think it unfair to specify that a system should trap *all* these errors.