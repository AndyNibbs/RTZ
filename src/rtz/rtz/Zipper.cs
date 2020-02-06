using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace rtz
{
    static class Zipper
    {
        public static void Zip(string target, string destination)
        {
            if (!Path.GetExtension(target).Equals(".rtz", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Must be a .RTZ file");
            }

            var doc = XDocument.Load(target);

            string root = Path.GetDirectoryName(target);
            var attachments = FindAttachments(doc.Root);
            var present = attachments.Where(f => File.Exists(Path.Combine(root, f)));
            var absent = attachments.Except(present);

            Console.WriteLine("RTZP will include:");
            Console.WriteLine($"\t{target}");
            present.ForEach(s => Console.WriteLine($"\t{s}"));

            if (absent.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Absent attachments?");
                absent.ForEach(s => Console.WriteLine($"\t{s}"));
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                string destPath = Path.GetDirectoryName(target);
                string destinationFilename = Path.GetFileNameWithoutExtension(target) + ".rtzp";
                destination = Path.Combine(destPath, destinationFilename);
            }

            Console.WriteLine();
            Console.WriteLine($"Destination {destination}");

            using (var stream = new FileStream(destination, FileMode.Create))
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntryFromFile(target, Path.GetFileName(target));

                    present.ForEach(attachment => archive.CreateEntryFromFile(Path.Combine(root, attachment), attachment));
                }
            }

            Console.WriteLine("Successful");
        }

        private static string[] FindAttachments(XElement root)
        {
            // Reference S.2 RTZP Data Container.
            // Details of attachments seems that is just a URI-style reference to "rtz://"
            // here we walk the DOM to find such things.
            var attachments = root.Descendants().Select(e => AttachmentsFromNode(e)).SelectMany(x => x).Distinct();
            return attachments.ToArray();
        }

        static IEnumerable<string> AttachmentsFromNode(XElement e)
        {
            string s = AttachmentFromString(e.Value);
            if (s is object)
                yield return s;

            foreach (string attVal in e.Attributes().Select(a => a.Value))
            {
                var sa = AttachmentFromString(attVal);
                if (sa is object)
                    yield return sa;
            }
        }

        static string AttachmentFromString(string s) // Might be null
        {
            if (s.Contains("rtz://", StringComparison.InvariantCultureIgnoreCase))
                return s.Replace("rtz://", string.Empty).Trim();
            else
                return null;
        }
    }
}
