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
            WarnOfDuplicateConsecutivePositions();

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
                XsdCheck(Properties.Resources.RTZ_Schema_version_1_0);
                _warnings.Add("Version of file is 1.1 but checked with 1.0 schema (TODO)");
                _namespace = XNamespace.Get("http://www.cirm.org/RTZ/1/1");
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
            if (el.Attribute(attName) is object)
            {
                string value = el.Attribute(attName).Value.Trim();
                if (!acceptable.Contains(value))
                    _errors.Add($"{el.Name} attribute {attName} unexpected value ({value})");
            }
        }

        private void AttributeExistenceCheck(XElement el, string attName)
        {
            if (el.Attribute(attName) is null)
                _errors.Add($"No {attName} attribute on {el.Name}");
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
            report.AppendLine();
            report.AppendLine($"Checked around {DateTime.UtcNow.ToString("G")}");
            report.AppendLine();


            if (HasErrors)
            {
                Errors.ForEach(e => report.AppendLine(e));
                report.AppendLine();
            }

            if (HasWarnings)
            {
                Warnings.ForEach(w => report.AppendLine(w));
                report.AppendLine();
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
    }
}
