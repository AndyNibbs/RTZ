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

..\..\..\..\..\..\samplefiles\errors\Esoteric\EsotericRouteInfoError.rtz<br>
The element 'routeInfo' in namespace 'http://www.cirm.org/RTZ/1/1' has invalid child element 'extension' in namespace 'http://www.cirm.org/RTZ/1/1'. List of possible elements expected: 'extensions' in namespace 'http://www.cirm.org/RTZ/1/1'.<br>
validityPeriodStart >= validityPeriodStop<br>
Non-standard element (extension) found in routeInfo element<br>
FAILURE with 2 errors and 1 warnings<br>

..\..\..\..\..\..\samplefiles\errors\Esoteric\EsotericScheduleError.rtz<br>
The 'windDirection' attribute is invalid - The value '370' is invalid according to its datatype 'http://www.cirm.org/RTZ/1/1:CourseType' - The MaxExclusive constraint failed.<br>
The 'windSpeed' attribute is invalid - The value '-10' is invalid according to its datatype 'http://www.cirm.org/RTZ/1/1:SpeedType' - The MinInclusive constraint failed.<br>
The 'currentSpeed' attribute is invalid - The value '-10' is invalid according to its datatype 'http://www.cirm.org/RTZ/1/1:SpeedType' - The MinInclusive constraint failed.<br>
The 'currentDirection' attribute is invalid - The value '370' is invalid according to its datatype 'http://www.cirm.org/RTZ/1/1:CourseType' - The MaxExclusive constraint failed.<br>
FAILURE with 4 errors<br>

..\..\..\..\..\..\samplefiles\errors\MainlineErrors\DuplicateWaypointIdError.rtz<br>
Waypoint id 11 appears more than one once in waypoints element<br>
FAILURE with 1 errors<br>

..\..\..\..\..\..\samplefiles\errors\MainlineErrors\GeometryTypeError.rtz<br>
The 'geometryType' attribute is invalid - The value 'GreatCircle' is invalid according to its datatype 'http://www.cirm.org/RTZ/1/1:GeometryType' - The Enumeration constraint failed.<br>
{http://www.cirm.org/RTZ/1/1}leg attribute geometryType unexpected value (GreatCircle)<br>
FAILURE with 2 errors<br>

..\..\..\..\..\..\samplefiles\errors\MainlineErrors\MissingWaypointIdError.rtz<br>
The required attribute 'id' is missing.<br>
1 waypoints of 6 waypoints do not have an id attribute<br>
No id attribute on {http://www.cirm.org/RTZ/1/1}waypoint<br>
FAILURE with 3 errors<br>

..\..\..\..\..\..\samplefiles\errors\MainlineErrors\NegativeRevisionError.rtz<br>
The 'revision' attribute is invalid - The value '-1' is invalid according to its datatype 'http://www.w3.org/2001/XMLSchema:nonNegativeInteger' - Value '-1' was either too large or too small for NonNegativeInteger.<br>
FAILURE with 1 errors<br>

..\..\..\..\..\..\samplefiles\errors\MainlineErrors\NonsenseGeometryTypeError.rtz<br>
The 'geometryType' attribute is invalid - The value 'Nonsense' is invalid according to its datatype 'http://www.cirm.org/RTZ/1/1:GeometryType' - The Enumeration constraint failed.<br>
{http://www.cirm.org/RTZ/1/1}leg attribute geometryType unexpected value (Nonsense)<br>
FAILURE with 2 errors<br>

..\..\..\..\..\..\samplefiles\errors\MainlineErrors\RouteNameDoesNotMatchFilenameError.rtz<br>
routeName should equal the filename<br>
FAILURE with 1 errors<br>

..\..\..\..\..\..\samplefiles\errors\MainlineErrors\ScheduleError.rtz<br>
The 'etd' attribute is invalid - The value '2020-02-30T00:00:00z' is invalid according to its datatype 'http://www.w3.org/2001/XMLSchema:dateTime' - The string '2020-02-30T00:00:00z' is not a valid DateTime value.<br>
Schedule out-of-order eta after etd waypoints index 0, 5<br>
Contains an element calculated that does not contain all waypoint ids or order is wrong<br>
Contains an element calculated that does not contain all waypoint ids or order is wrong<br>
FAILURE with 4 errors<br>
