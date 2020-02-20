# DetailErrors.rtz

- routeName attribute does not match filename
- validityPeriodStart is after validityPeriodStop
- vesselGM is 'm' appended (unit for metres, but is wrong)
- extension element in routeInfo is not wrapped in a extensions element (and extending of routeInfo is not mentioned in std)
- first two waypoints have same id
- the 3rd waypoint (named WP3) does not have an id
- the 4th waypoints has a revision of -1
- last waypoint has a leg element with geometryType of "ox"
- first schedule, manual waypointIds match waypoints (which have bad ids)
- first schedule, calculated ids are just one based index (which to a programmer is vile)
- first schedule, calculated first waypoint etd is after all etas in following WP
- second schedule first etd is 30th of February
- owing to that broken date it after it finishes
- second schedule contains wind and current directions of 370 and negative speeds
- defaultWaypoint leg safetyContour is zero 

Note: I think it unfair to specify that a system should trap *all* these errors.