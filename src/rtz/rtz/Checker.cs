using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace rtz
{
    class Checker
    {
        public Checker(string filename, bool onlyWarnAboutRouteName)
        {
            Filename = filename;
            OnlyWarnAboutRouteName = onlyWarnAboutRouteName;

            ReadRTZFromFile();

            // Following checks mainly check on contents of _doc
            if (_doc is object)
            {
                RootChecks();
                XsdCheck();
                CheckWaypointIdsAreUnique();
                CheckAllLegs();
                CheckAllWaypointsHaveBasics();
                DefaultWaypointChecks();
                WarnOfDuplicateConsecutivePositions();
                RouteInfoChecks();
                CheckSchedules();
                TryParsingXsdDuration();
                NonStandardExtensionWarnings();
            }

            Errors = new ReadOnlyCollection<string>(_errors);
            Warnings = new ReadOnlyCollection<string>(_warnings);
        }

        private void ReadRTZFromFile()
        {
            bool rtzpExtension = Path.GetExtension(Filename).Equals(".rtzp", StringComparison.InvariantCultureIgnoreCase);
            if (rtzpExtension)
            {
                string name = Path.GetFileNameWithoutExtension(Filename);
                using (var zipArchive = ZipFile.OpenRead(Filename))
                {
                    var entry = zipArchive.GetEntry(name + ".rtz");

                    if (entry is null)
                    {
                        _errors.Add($"Did not find entry in RTZP called {name + ".rtz"}");
                        return;
                    }

                    using (var stream = entry.Open())
                    {
                        _doc = XDocument.Load(stream);
                    }
                }

                return;
            }

            bool rtzExtension = Path.GetExtension(Filename).Equals(".rtz", StringComparison.InvariantCultureIgnoreCase);

            if (!rtzExtension)
            {
                _warnings.Add("Does not have RTZ extension");
            }

            if (rtzExtension && new FileInfo(Filename).Length > (400 * 1024))
            {
                _errors.Add("Exceeds 400kb limit");
            }

            try
            {
                _doc = XDocument.Load(Filename);
            }
            catch (XmlException x)
            {
                _errors.Add(x.Message);
            }
        }

        private XNamespace _namespace;

        private void XsdCheck()
        {
            string version = _doc.Root.Attribute("version").Value;

            var stm = StmSchema.None;

            if (_doc.Root.Attributes().Select(a => a.Value).Where(v => v.Equals("http://stmvalidation.eu/STM/1/0/0", StringComparison.InvariantCulture)).Any())
            {
                stm = StmSchema.StandardSTM;
            }
            else if (_doc.Root.Attributes().Select(a => a.Value).Where(v => v.Equals("http://stmvalidation.eu/STM/1/2/0", StringComparison.InvariantCulture)).Any())
            {
                stm = StmSchema.NonStandardAlteredByAndyNibbs;
            }

            if (version.Equals("1.0", StringComparison.InvariantCultureIgnoreCase))
            {
                Version = "1.0";
                _namespace = XNamespace.Get("http://www.cirm.org/RTZ/1/0");
                XsdCheck(@"http://www.cirm.org/RTZ/1/0", Properties.Resources.RTZ_Schema_version_1_0, stm);
            }
            else if (version.Equals("1.1", StringComparison.InvariantCultureIgnoreCase))
            {
                Version = "1.1";
                _namespace = XNamespace.Get("http://www.cirm.org/RTZ/1/1");
                XsdCheck(@"http://www.cirm.org/RTZ/1/1", Properties.Resources.RTZ_Schema_version_1_1, stm);
            }
            else if (version.Equals("1.2", StringComparison.InvariantCultureIgnoreCase))
            {
                Version = "1.2";
                _namespace = XNamespace.Get("http://www.cirm.org/RTZ/1/2");
                XsdCheck(@"http://www.cirm.org/RTZ/1/2", Properties.Resources.RTZ_Schema_version_1_2, stm);
            }
            else
            {
                _errors.Add($"Could not check with XSD for version {version}");
            }
        }

        enum StmSchema
        {
            None,
            StandardSTM,
            NonStandardAlteredByAndyNibbs,
        }
        
        private void XsdCheck(string targetNamespace, string xsdContents, StmSchema stm)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add(targetNamespace, XmlReader.Create(new StringReader(xsdContents)));

            if (stm == StmSchema.StandardSTM)
            {
                schemas.Add("http://stmvalidation.eu/STM/1/0/0", XmlReader.Create(new StringReader(Properties.Resources.stm_extensions_29032017)));
            }

            if (stm == StmSchema.NonStandardAlteredByAndyNibbs)
            {
                schemas.Add("http://stmvalidation.eu/STM/1/2/0", XmlReader.Create(new StringReader(Properties.Resources.stm_extensions_29032017_amendedByAndy)));
            }

            _doc.Validate(schemas, (o, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                    _errors.Add(e.Message);
                else
                    _warnings.Add(e.Message);
            });
        }


        private void RootChecks()
        {
            var root = _doc.Root;

            ElementNameCheck(root, "route");
            AttributeExistenceCheck(root, "version");
            AttributeContentsCheck(root, "version", "1.0", "1.1", "1.2");
        }

        private void AttributeContentsCheck(XElement el, string attName, params string[] acceptable)
        {
            if (el is null)
            {
                return;
            }

            if (el.Attribute(attName) is object)
            {
                string value = el.Attribute(attName).Value.Trim();
                if (!acceptable.Contains(value))
                    _errors.Add($"{el.Name} attribute {attName} unexpected value ({value})");
            }
        }

        private void AcceptableAttributesCheck(XElement el, params string[] acceptableAttributeNames)
        {
            var names = el.Attributes().Select(att => att.Name);

            foreach (var name in names)
            {
                if (!acceptableAttributeNames.Contains(name.LocalName))
                {
                    _warnings.Add($"Non-standard attribute name ({name.LocalName}) found in {el.Name.LocalName} element");
                }
            }
        }

        private void AcceptableElementsCheck(XElement el, params string[] acceptableNames)
        {
            if (el == null)
            {
                return;
            }

            var names = el.Elements().Select(att => att.Name);

            foreach (var name in names)
            {
                if (!acceptableNames.Contains(name.LocalName))
                {
                    _warnings.Add($"Non-standard element ({name.LocalName}) found in {el.Name.LocalName} element");
                }
            }
        }

        private void AttributeExistenceCheck(XElement el, string attName)
        {
            if (el.Attribute(attName) is null)
                _errors.Add($"No {attName} attribute on {el.Name}");
        }

        private void ElementExistenceCheck(XElement el, string elementName)
        {
            if (el.Element(_namespace + elementName) is null)
                _errors.Add($"No {elementName} element on {el.Name.LocalName}");
        }

        private void ElementNameCheck(XElement element, string expected)
        {
            if (element is null)
            {
                _errors.Add($"Expected an element named {element}");
                return;
            }

            if (!element.Name.LocalName.Equals(expected, StringComparison.InvariantCulture))
            {
                _errors.Add($"Element name {element.Name} should have been {expected}");
            }
        }

        private void AttributeAbsenceCheck(XElement el, string attName, string errorMessage = "")
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = $"The presence of attribute {attName} on element {el.Name.LocalName} does not make sense";
            }

            if (el.Attribute(attName) is object)
            {
                _errors.Add(errorMessage);
            }
        }

        private void ElementAbsenceCheck(XElement el, string elementName, string errorMessage = "")
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = $"The presence of element {elementName} on element {el.Name.LocalName} does not make sense";
            }

            if (el.Element(_namespace + elementName) is object)
            {
                _errors.Add(errorMessage);
            }
        }

        public string Filename { get; private set; }
        private bool OnlyWarnAboutRouteName { get; }

        private List<string> _errors = new List<string>();
        private List<string> _warnings = new List<string>();
        private XDocument _doc;

        public ReadOnlyCollection<string> Warnings { get; private set; }
        public ReadOnlyCollection<string> Errors { get; private set; }

        public string Report()
        {
            var report = new StringBuilder();

            report.AppendLine($"{Filename}");

            if (HasErrors)
            {
                Errors.ForEach(e => report.AppendLine($"Error: {e}"));
            }

            if (HasWarnings)
            {
                Warnings.ForEach(w => report.AppendLine($"Warning: {w}"));
            }

            report.AppendLine(SuccessDescription());

            return report.ToString();
        }

        public bool HasErrors => Errors.Any();
        public bool HasWarnings => Warnings.Any();
        public bool Passed => !HasErrors;

        public string Version { get; private set; }

        public string SuccessDescription()
        {
            if (HasErrors && HasWarnings)
            {
                return $"FAILURE with {Errors.Count} errors and {Warnings.Count} warnings";
            }

            if (HasErrors)
            {
                return $"FAILURE with {Errors.Count} errors";
            }

            if (HasWarnings)
            {
                return $"SUCCESS with {Warnings.Count} warnings";
            }

            return "SUCCESS. No errors or warnings.";
        }

        private void CheckWaypointIdsAreUnique()
        {
            var waypoints = _doc.Root.Element(_namespace + "waypoints").Elements(_namespace + "waypoint");
            var ids = waypoints.Where(wp => wp.Attribute("id") is object).Select(wp => wp.Attribute("id").Value);

            if (waypoints.Count() != ids.Count())
            {
                _errors.Add($"{waypoints.Count() - ids.Count()} waypoints of {waypoints.Count()} waypoints do not have an id attribute");
            }

            var dupes = ids.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
            dupes.ForEach(dupe => _errors.Add($"Waypoint id {dupe} appears more than one once in waypoints element"));
        }

        private void CheckAllWaypointsHaveBasics()
        {
            var wps = _doc.Root.Element(_namespace + "waypoints").Elements(_namespace + "waypoint");

            if (wps.First().Element(_namespace + "leg") is object)
            {
                _warnings.Add("First waypoint has a leg element but this is meaningless as it would define a route leg prior to the first waypoint");
            }

            foreach (var wp in wps)
            {
                AttributeExistenceCheck(wp, "id");
                ElementExistenceCheck(wp, "position");
                var position = wp.Element(_namespace + "position");
                AttributeExistenceCheck(position, "lat");
                AttributeExistenceCheck(position, "lon");
                AcceptableAttributesCheck(position, "lat", "lon");
            }
        }

        private void WarnOfDuplicateConsecutivePositions()
        {
            // Check for very short route legs 0.00009 of a degree is 
            // one NM is 1 minute of lat == 1852m
            // So one deg is 111120‬m
            // 0.00009 deg is ~10m 
            // Testing two position's closeness by comparing lat and long in this way is rough and ready as change in latitude makes longitude coords "closer"

            (double lat, double lon)[] positions = MiscFunctions.ParsePositions(_doc, _namespace);

            for (int n = 0; n < positions.Length - 1; ++n)
            {
                if (MiscFunctions.Similar(positions[n], positions[n + 1], 0.00009))
                    _warnings.Add($"WP index {n} position is very similar to position of WP index {n + 1}. This is not forbidden but may affect schedule calculations");
            }
        }

        private void RouteInfoChecks()
        {
            var routeInfoElement = _doc.Root.Element(_namespace + "routeInfo");

            string routeName = (string)routeInfoElement.Attribute("routeName");
            string fileName = Path.GetFileNameWithoutExtension(Filename);
            if (!string.Equals(routeName, fileName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (OnlyWarnAboutRouteName)
                    _warnings.Add("warning routeName should equal the filename");
                else
                    _errors.Add("routeName should equal the filename");
            }

            WarnAboutLength(routeInfoElement, "routeName");
            WarnAboutLength(routeInfoElement, "routeAuthor");
            WarnAboutLength(routeInfoElement, "routeStatus");
            WarnAboutLength(routeInfoElement, "vesselName", 32);
            WarnAboutLength(routeInfoElement, "vesselVoyage", 128); // STM files can contain things like vesselVoyage="urn:mrn:stm:voyage:id:test:104"

            DateTimeOffset? validityPeriodStart = OptionalAttributeTime(routeInfoElement, "validityPeriodStart");
            DateTimeOffset? validityPeriodStop = OptionalAttributeTime(routeInfoElement, "validityPeriodStop");
            if (validityPeriodStart.HasValue && validityPeriodStop.HasValue && validityPeriodStart.Value >= validityPeriodStop.Value)
            {
                _errors.Add("validityPeriodStart >= validityPeriodStop");
            }

            string mmsi = routeInfoElement.OptionalAttributeString("vesselMMSI");
            if (!ValidMMSI(mmsi))
                _warnings.Add($"vesselMMSI ({mmsi}) does not look like an MMSI ");

            string imo = routeInfoElement.OptionalAttributeString("vesselIMO");
            if (!ValidIMO(imo))
                _warnings.Add($"vesselIMO ({imo}) does not look like an IMO number");


            double? vesselGM = OptionalAttributeDouble(routeInfoElement, "vesselGM");
            if (vesselGM.HasValue && vesselGM.Value < 0.15)
                _warnings.Add($"vesselGM is below that allowed by UK Department for Transport");


            double? vesselSpeedMax = OptionalAttributeDouble(routeInfoElement, "vesselSpeedMax");
            double? vesselServiceMin = OptionalAttributeDouble(routeInfoElement, "vesselServiceMin");
            double? vesselServiceMax = OptionalAttributeDouble(routeInfoElement, "vesselServiceMax");
            if (vesselServiceMin.HasValue && vesselServiceMax.HasValue && vesselServiceMax.Value < vesselServiceMin.Value)
            {
                _errors.Add("vesselServiceMax < vesselServiceMin");
            }
            if (vesselSpeedMax.HasValue && vesselServiceMax.HasValue && vesselSpeedMax.Value < vesselServiceMax)
            {
                _errors.Add("vesselSpeedMax < vesselServiceMax");
            }
            if (vesselSpeedMax.HasValue && vesselServiceMax.HasValue && vesselSpeedMax.Value < vesselServiceMin)
            {
                _errors.Add("vesselSpeedMax < vesselServiceMin");
            }
        }

        private DateTimeOffset? OptionalAttributeTime(XElement element, string attributeName)
        {
            var att = element.Attribute(attributeName);

            if (att is null)
            {
                return null;
            }

            try
            {
                return (DateTimeOffset)att;
            }
            catch (FormatException)
            {
                _errors.Add($"{attributeName} of {element.Name.LocalName} invalid datetime ({att.Value})");
                return null;
            }
        }

        private bool ValidMMSI(string mmsi)
        {
            if (string.IsNullOrWhiteSpace(mmsi))
                return true;

            if (mmsi.Length < 9)
                return false; // to short

            string first = mmsi.Substring(0, 1);
            int nFirst = int.Parse(first);
            if (nFirst < 2 || nFirst > 7)
                return false; // not a ship

            return true;
        }

        private bool ValidIMO(string imo)
        {
            if (string.IsNullOrWhiteSpace(imo))
                return true;

            string numeric;

            if (imo.StartsWith("IMO"))
            {
                if (imo.Length != 10)
                    return false;

                numeric = imo.Substring(3, 7);
            }
            else
            {
                if (imo.Length != 7)
                    return false;

                numeric = imo;
            }

            return int.TryParse(numeric, out int dummy);
        }

        private void WarnAboutLength(XElement element, string attributeName, int reasonableLength = 128)
        {
            string s = element.OptionalAttributeString(attributeName);
            if (string.IsNullOrEmpty(s))
                return;
            if (s.Length > reasonableLength)
            {
                _warnings.Add($"{attributeName} is >{reasonableLength} chars which is unreasonably long");
            }
        }

        private void CheckSchedules()
        {
            var waypointIds = _doc.Root.Element(_namespace + "waypoints").Elements(_namespace + "waypoint").Where(wp => wp.Attribute("id") is object).Select(wp => (int)wp.Attribute("id"));

            XElement schedulesNode = _doc.Root.Element(_namespace + "schedules");

            if (schedulesNode is null)
            {
                return; // the best schedule is no schedule
            }

            var schedules = schedulesNode.Elements(_namespace + "schedule");

            foreach (var schedule in schedules)
            {
                CheckSchedules(schedule, waypointIds.ToArray());
            }

            // All schedules should contain the same WP ids in the same order as in
            // the waypoints section 
        }

        private void CheckSchedules(XElement schedule, int[] waypointIds)
        {
            XElement manual = schedule.Element(_namespace + "manual");
            XElement calculated = schedule.Element(_namespace + "calculated");

            if (manual is object)
            {
                GeneralScheduleCheck(manual, waypointIds, warningIfIncomplete: false);
            }

            if (calculated is object)
            {
                GeneralScheduleCheck(calculated, waypointIds, warningIfIncomplete: true);
            }
        }

        private void GeneralScheduleCheck(XElement sch, int[] waypoints_ids_from_route, bool warningIfIncomplete)
        {
            string scheduleType = sch.Name.LocalName;

            var scheduleElements = sch.Elements(_namespace + "scheduleElement");

            if (!scheduleElements.Any())
            {
                _errors.Add($"Contains a {scheduleType} schedule with no elements");
                return;
            }

            CheckFirstScheduleElement(scheduleElements.First());

            foreach (var se in scheduleElements)
            {
                CheckScheduleElement(se);
            }

            var ids_from_schedule = scheduleElements.Where(el => el.Attribute("waypointId") is object).Select(el => (int)el.Attribute("waypointId"));

            if (ids_from_schedule.Count() != scheduleElements.Count())
            {
                _errors.Add($"A {scheduleType} schedule found that contains scheduleElements(s) without waypointId attribute");
            }

            bool everythingFromRouteIsInSchedule = true; // the next warning check is confusing unless this is true!
            foreach (var id_from_schedule in ids_from_schedule)
            {
                if (!waypoints_ids_from_route.Contains(id_from_schedule))
                {
                    _errors.Add($"A {scheduleType} schedule element contains id {id_from_schedule} which does not correspond to any of the waypoints");
                    everythingFromRouteIsInSchedule = false;
                }
            }

            var route_waypoint_subset = waypoints_ids_from_route.Where(id => ids_from_schedule.Contains(id));

            if (everythingFromRouteIsInSchedule && !route_waypoint_subset.SequenceEqual(ids_from_schedule))
            {
                _warnings.Add($"The waypoint ids in a {scheduleType} schedule are not in the same order as in the waypoints element");
            }

            if (warningIfIncomplete)
            {
                var a = ids_from_schedule.OrderBy(id => id);
                var b = waypoints_ids_from_route.OrderBy(id => id);
                if (!a.SequenceEqual(b))
                {
                    var missing = b.Except(a);
                    string missingText = string.Join(", ", missing);
                    _warnings.Add($"A {scheduleType} schedule does not contain ALL waypoints ids (is missing {missingText})");
                }
            }

            ScheduleGoesForwardsInTime(scheduleElements);
        }

        private void CheckScheduleElement(XElement se)
        {
            AttributeExistenceCheck(se, "waypointId");

            AcceptableAttributesCheck(se, "waypointId", "etd", "etdWindowBefore", "etdWindowAfter",
                                          "eta", "etaWindowBefore", "etaWindowAfter", "stay", "speed",
                                          "speedWindow", "windSpeed", "windDirection", "currentSpeed",
                                          "currentDirection", "windLoss", "waveLoss", "totalLoss",
                                          "rpm", "pitch", "fuel", "relFuelSave", "absFuelSave", "Note");
        }

        private void ScheduleGoesForwardsInTime(IEnumerable<XElement> scheduleElements)
        {
            // Make an ordered list...

            var times = new List<(int index, string type, DateTimeOffset time)>();

            int ind = 0;
            foreach (var se in scheduleElements)
            {
                DateTimeOffset? etd = OptionalAttributeTime(se, "etd");
                DateTimeOffset? eta = OptionalAttributeTime(se, "eta");

                //TODO: if has both etd and eta AND stay we could check stay time is diff between etd and eta

                if (eta.HasValue)
                {
                    times.Add((ind, "eta", eta.Value));
                }

                if (etd.HasValue)
                {
                    times.Add((ind, "etd", etd.Value));
                }

                ++ind;
            }

            // Check the it goes forward in time...

            for (int n = 0; n < times.Count - 1; ++n)
            {
                var ntime = times[n];
                var next = times[n + 1];

                if (ntime.index == next.index)
                {
                    if (next.time < ntime.time) // the eta and etd can be same 
                    {
                        _errors.Add($"Schedule out-of-order {next.type} after {ntime.type} for waypoint index {next.index}");
                    }
                }
                else
                {
                    if (next.time <= ntime.time)
                    {
                        _errors.Add($"Schedule out-of-order {next.type} after {ntime.type} waypoints index {ntime.index}, {next.index}");
                    }
                }
            }
        }

        private void CheckFirstScheduleElement(XElement scheduleElement)
        {
            // Check first schedule element with a small set of attributes allowed because many attributes refer to the "leg" which is "previous"
            // All of the other attributres refer to the "leg" which is N - 1 to N
            AcceptableAttributesCheck(scheduleElement, "waypointId", "etd", "eta", "windSpeed", "windDirection", "currentSpeed", "currentDirection", "Note", "etdWindowBefore", "etdWindowAfter");
        }

        // This is general around ensuring that default waypoint does not have information on it that
        // only makes sense for an actual waypoint
        private void DefaultWaypointChecks()
        {
            var defaultWaypoint = _doc.Root.Element(_namespace + "waypoints").Element(_namespace + "defaultWaypoint");

            if (defaultWaypoint is object)
            {
                ElementAbsenceCheck(defaultWaypoint, "position");
                AttributeAbsenceCheck(defaultWaypoint, "id");
                AttributeAbsenceCheck(defaultWaypoint, "revision");
                AttributeAbsenceCheck(defaultWaypoint, "name");
                CheckLeg(defaultWaypoint.Element(_namespace + "leg"));
            }
        }

        private void CheckLeg(XElement leg)
        {
            if (leg == null)
                return;

            AttributeContentsCheck(leg, "geometryType", "Orthodrome", "Loxodrome");

            AcceptableAttributesCheck(leg, "starboardXTD", "portsideXTD", "safetyContour", "safetyDepth", "geometryType", "speedMin", "speedMax",
                                           "draughtForward", "draughtAft", "staticUKC", "dynamicUKC", "masthead", "legReport", "legInfo", "legNote1", "legNote2");

        }

        private void CheckAllLegs()
        {
            var legs = _doc.Root.Element(_namespace + "waypoints")
                                .Elements(_namespace + "waypoint")
                                .Select(el => el.Element(_namespace + "leg"))
                                .Where(leg => leg is object);

            legs.ForEach(leg => CheckLeg(leg));
        }

        private void NonStandardExtensionWarnings()
        {
            // The code below is trying to check...

            //  The only thing that routeInfo can contain is extensions
            //
            //  waypoints can contain
            //  	defaultWaypoint
            //  	waypoint
            //  	extensions
            //  	
            //  defaultWaypoint can contain	
            //  	leg
            //  	extensions
            //  	
            //  waypoint can contain
            //  	position
            //  	leg
            //  	extensions
            //  	
            //  extensions can contain extension elements
            //  
            //  schedules can contain
            //  	schedule
            //  	extensions
            //  
            //  schedule can contain
            //  	manual
            //  		scheduleElement
            //  			extensions
            //  		extensions
            //  	calculated
            //  		scheduleElement
            //  			extensions
            //  		extensions
            //  	extensions

            var route = _doc.Root;
            AcceptableElementsCheck(route, "routeInfo", "waypoints", "schedules", "extensions");

            var routeInfo = route.Element(_namespace + "routeInfo");
            AcceptableElementsCheck(routeInfo, "extensions");

            var waypoints = route.Element(_namespace + "waypoints");
            AcceptableElementsCheck(waypoints, "defaultWaypoint", "waypoint", "extensions");

            var defaultWaypoint = waypoints.Element(_namespace + "defaultWaypoint");
            AcceptableElementsCheck(defaultWaypoint, "leg", "extensions");

            var allWaypoints = waypoints.Elements(_namespace + "waypoint");
            foreach (var wp in allWaypoints)
                AcceptableElementsCheck(wp, "position", "leg", "extensions");

            var schedules = route.Element(_namespace + "schedules");
            AcceptableElementsCheck(schedules, "schedule", "extensions");

            // Extensions should only contain <extension>s
            var extensions = route.Descendants(_namespace + "extensions");
            foreach (var el in extensions)
                AcceptableElementsCheck(el, "extension");

            if (schedules is object)
            {
                var allSchedules = schedules.Elements(_namespace + "schedule");
                foreach (var sch in allSchedules)
                    AcceptableElementsCheck(sch, "manual", "calculated", "extensions");

                var allManuals = allSchedules.Elements(_namespace + "manual");
                foreach (var man in allManuals)
                    AcceptableElementsCheck(man, "scheduleElement", "extensions");

                var allCalc = allSchedules.Elements(_namespace + "calculated");
                foreach (var calc in allCalc)
                    AcceptableElementsCheck(calc, "scheduleElement", "extensions");

                var allSchEl = schedules.Descendants(_namespace + "scheduleElement");
                foreach (var se in allSchEl)
                    AcceptableElementsCheck(se, "extensions");
            }
        }

        private double? OptionalAttributeDouble(XElement element, string attributeName)
        {
            var att = element.Attribute(attributeName);

            if (att is null)
            {
                return null;
            }

            if (double.TryParse(att.Value, out double result))
            {
                return result;
            }
            else
            {
                _errors.Add($"Failed to parse value {att.Value} from attribute {attributeName} from {element.Name.LocalName} element");
            }

            return null;
        }

        private void TryParsingXsdDuration()
        {
            if (!Version.Equals("1.1", StringComparison.InvariantCultureIgnoreCase))
                return;

            var schedElems = _doc.Descendants(_namespace + "scheduleElement");

            IEnumerable<XAttribute> stays = schedElems.Where(e => e.Attribute("stay") is object).Select(e => e.Attribute("stay"));
            IEnumerable<XAttribute> etdWindowBefore = schedElems.Where(e => e.Attribute("etdWindowBefore") is object).Select(e => e.Attribute("etdWindowBefore"));
            IEnumerable<XAttribute> etdWindowAfter = schedElems.Where(e => e.Attribute("etdWindowAfter") is object).Select(e => e.Attribute("etdWindowAfter"));
            IEnumerable<XAttribute> etaWindowBefore = schedElems.Where(e => e.Attribute("etaWindowBefore") is object).Select(e => e.Attribute("etaWindowBefore"));
            IEnumerable<XAttribute> etaWindowAfter = schedElems.Where(e => e.Attribute("etaWindowAfter") is object).Select(e => e.Attribute("etaWindowAfter"));

            IEnumerable<XAttribute>[] combine = { stays, etdWindowBefore, etdWindowAfter, etaWindowBefore, etaWindowAfter };
            var all = combine.SelectMany(x => x).ToArray();

            foreach (var att in all)
            {
                string value = att.Value;

                if (!value.StartsWith("P"))
                {
                    _errors.Add($"An attribute {att.Name.LocalName} had value {value} which is not an xsd:duration");
                    continue;
                }

                TimeSpan span = (TimeSpan)att;

                if (span > TimeSpan.FromDays(3))
                {
                    _warnings.Add($"An attribute {att.Name.LocalName} attribute contains duration longer than 3 days");
                }
            }
        }
    }
}