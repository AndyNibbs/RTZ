using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace rtz
{
    class Checker
    {
        public Checker(string filename)
        {
            Filename = filename;

            FilenameChecks();

            _doc = XDocument.Load(Filename);

            RootChecks();
            XsdCheck();
            CheckWaypointIdsAreUnique();
            CheckAllLegs();
            CheckAllWaypointsHaveBasics();
            DefaultWaypointChecks();
            WarnOfDuplicateConsecutivePositions();
            RouteInfoChecks();

            CheckSchedule();

            //NonStandardExtensionWarnings();

            Errors = new ReadOnlyCollection<string>(_errors);
            Warnings = new ReadOnlyCollection<string>(_warnings);
        }

        private XNamespace _namespace;

        private void XsdCheck()
        {
            string version = _doc.Root.Attribute("version").Value;

            if (version.Equals("1.0", StringComparison.InvariantCultureIgnoreCase))
            {
                XsdCheck(Properties.Resources.RTZ_Schema_version_1_0);
                _namespace = XNamespace.Get("http://www.cirm.org/RTZ/1/0");
            }
            else if (version.Equals("1.1", StringComparison.InvariantCultureIgnoreCase))
            {
                XsdCheck(Properties.Resources.RTZ_Schema_version_1_1);
                _namespace = XNamespace.Get("http://www.cirm.org/RTZ/1/1");

                _warnings.Add("Version of file is 1.1 but not checked with OFFICIAL SCHEMA");
            }
            else
            {
                _errors.Add($"Could not check with XSD for version {version}");
            }
        }

        private void XsdCheck(string xsd)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("http://www.cirm.org/RTZ/1/0", XmlReader.Create(new StringReader(xsd)));
           
            _doc.Validate(schemas, (o, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                    _errors.Add(e.Message);
                else
                    _warnings.Add(e.Message);
            });
        }

        private void FilenameChecks()
        {
            bool rtzpExtension = Path.GetExtension(Filename).Equals(".rtzp", StringComparison.InvariantCultureIgnoreCase);
            if (rtzpExtension)
                throw new NotSupportedException("Do not yet support checking RTZP, please unzip manually to check");

            bool rtzExtension = Path.GetExtension(Filename).Equals(".rtz", StringComparison.InvariantCultureIgnoreCase);

            if (!rtzExtension)
            {
                _warnings.Add("Does not have RTZ extension");
            }

            if (rtzExtension && new FileInfo(Filename).Length > (400 * 1024))
            {
                _errors.Add("Exceeds 400kb limit");
            }
        }

        private void RootChecks()
        {
            var root = _doc.Root;

            ElementNameCheck(root, "route");
            AttributeExistenceCheck(root, "version");
            AttributeContentsCheck(root, "version", "1.0", "1.1");
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

            foreach(var name in names)
            {
                if (!acceptableAttributeNames.Contains(name.LocalName))
                {
                    _warnings.Add($"Non-standard attribute name ({name.LocalName}) found in {el.Name.LocalName} element");
                }
            }
        }

        private void AcceptableElementsCheck(XElement el, params string[] acceptableNames)
        {
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

        private List<string> _errors = new List<string>();
        private List<string> _warnings = new List<string>();
        private XDocument _doc;

        public ReadOnlyCollection<string> Warnings { get; private set; } 
        public ReadOnlyCollection<string> Errors { get; private set; } 

        public string Report()
        {
            var report = new StringBuilder();

            report.AppendLine($"RTZ check report for {Filename}");
            report.AppendLine($"Checked around {DateTime.UtcNow.ToString("G")}");


            if (HasErrors)
            {
                Errors.ForEach(e => report.AppendLine(e));
            }

            if (HasWarnings)
            {
                Warnings.ForEach(w => report.AppendLine(w));
            }

            report.AppendLine(SuccessDescription());

            return report.ToString();
        }

        public bool HasErrors => Errors.Any();
        public bool HasWarnings => Warnings.Any();
        public bool Passed => !HasErrors;

        public string SuccessDescription()
        {
            if (HasErrors && HasWarnings)
            {
                return $"Failed with {Errors.Count} errors and {Warnings.Count} warnings";
            }

            if (HasErrors)
            {
                return $"Failed with {Errors.Count} errors";
            }
            
            if (HasWarnings)
            {
                return $"Success with {Warnings.Count} warnings";
            }

            return "Success. No errors or warnings.";
        }

        private void CheckWaypointIdsAreUnique()
        {
            var ids = _doc.Root.Element(_namespace + "waypoints").Elements(_namespace + "waypoint").Select(wp => wp.Attribute("id").Value);
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

            foreach(var wp in wps)
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
                    _warnings.Add($"WP index {n} position is very similar to position of WP index {n+1}. This is not forbidden but may affect schedule calculations");
            }
        }

        private void RouteInfoChecks()
        {
            var routeInfoElement = _doc.Root.Element(_namespace + "routeInfo");

            string routeName = (string)routeInfoElement.Attribute("routeName");
            string fileName = Path.GetFileNameWithoutExtension(Filename);
            if (!string.Equals(routeName, fileName, StringComparison.InvariantCultureIgnoreCase))
            {
                _errors.Add("routeName shall equal the filename");
            }

            WarnAboutLength(routeInfoElement, "routeName");
            WarnAboutLength(routeInfoElement, "routeAuthor");
            WarnAboutLength(routeInfoElement, "routeStatus");
            WarnAboutLength(routeInfoElement, "vesselName", 32);
            WarnAboutLength(routeInfoElement, "vesselVoyage", 16);
   
            DateTimeOffset? validityPeriodStart = routeInfoElement.OptionalAttributeTime("validityPeriodStart");
            DateTimeOffset? validityPeriodStop  = routeInfoElement.OptionalAttributeTime("validityPeriodStop");
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


            double? vesselGM = routeInfoElement.OptionalAttributeDouble("vesselGM");
            if (vesselGM.HasValue && vesselGM.Value < 0.15)
                _warnings.Add($"vesselGM is below that allowed by UK Department for Transport");


            double? vesselSpeedMax = routeInfoElement.OptionalAttributeDouble("vesselSpeedMax");
            double? vesselServiceMin = routeInfoElement.OptionalAttributeDouble("vesselServiceMin");
            double? vesselServiceMax = routeInfoElement.OptionalAttributeDouble("vesselServiceMax");
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
                _warnings.Add($"Route name is >{reasonableLength} chars which is unreasonably long");
            }
        }

        private void CheckSchedule()
        {
            var waypointIds = _doc.Root.Element(_namespace + "waypoints").Elements(_namespace + "waypoint").Select(wp => (int)wp.Attribute("id"));

            XElement schedulesNode = _doc.Root.Element(_namespace + "schedules");

            if (schedulesNode is null)
            {
                return; // the best schedule is no schedule
            }

            var schedules = schedulesNode.Elements("schedule");

            

            // All schedules should contain the same WP ids in the same order as in
            // the waypoints section 
        }

        // The only meaningful scheduleElement attributes ont he first leg are:
        /*  waypointID
         *  etd
         *  windSpeed
         *  windDirection
         *  Note
         *  
         *  All of the other ones refer to the "leg" which is N-1 to N
         *  so "previous" in line with the daftness of the standard
         *  it is at least consistent
         *  
         */


        
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

            AcceptableAttributesCheck(leg, "starboardXTD", "portsideXTD", "safetyContour", "safetyDepth", "geometryType", "planSpeedMin", "planSpeedMax",
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
    }


}

