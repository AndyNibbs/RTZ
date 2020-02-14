using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace rtz
{
    static class MiscFunctions
    {
        public static (double lat, double lon)[] ParsePositions(XDocument doc, XNamespace ns) //(TODO) should write tests for this sort of stuff
        {
            return doc.Root.Element(ns + "waypoints")
                           .Elements(ns + "waypoint")
                           .Select(wp => wp.Element(ns + "position"))
                           .Select(el => (lat: (double)el.Attribute("lat"), lon: (double)el.Attribute("lon")))
                           .ToArray();
        }

        public static IEnumerable<(T, T)> LegsFromRun<T>(this IEnumerable<T> source)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    yield break;

                T previous = iterator.Current;
                while (iterator.MoveNext())
                {
                    T current = iterator.Current;
                    yield return (previous, current);
                    previous = current;
                }
            }
        }

        public static bool Similar((double lat, double lon) a, (double lat, double lon) b, double diff)
        {
            double latdiff = Math.Abs(a.lat - b.lat);
            double londiff = Math.Abs(a.lon - b.lon);
            return latdiff < diff && londiff < diff;
        }
    }
}
