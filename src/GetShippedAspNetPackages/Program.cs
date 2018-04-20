using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GetShippedAspNetPackages
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // TODO: Change this URL to whatever version you want to get data for
            //  1. Go to https://github.com/dotnet/versions/tree/master/build-info/dotnet/product/cli/release
            //  2. Find the release you want
            //  3. Click on the 'build.xml' file
            //  4. Click on the "Raw" button to get the raw URL
            //  5. Change the 'versionDataUrl' variable to the raw URL
            var versionDataUrl = "https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/product/cli/release/2.1-preview2/build.xml";

            var http = new HttpClient();

            using (var s = await http.GetStreamAsync(versionDataUrl))
            {
                var doc = XDocument.Load(s);
                var orchestratedBuildElement = doc.Root;
                var endpointElement = orchestratedBuildElement.Element(XName.Get("Endpoint"));
                var packageElements = endpointElement.Elements(XName.Get("Package"));
                var aspNetShippingElements =
                    packageElements
                        .Where(e =>
                            string.Equals(e.Attribute(XName.Get("OriginBuildName"))?.Value, "aspnet", StringComparison.Ordinal) &&
                            !string.Equals(e.Attribute(XName.Get("NonShipping"))?.Value, "true", StringComparison.Ordinal));

                foreach (var aspNetShippingElement in aspNetShippingElements)
                {
                    Console.WriteLine($"{GetPackageMonikerBaseString(aspNetShippingElement)},{aspNetShippingElement.Attribute(XName.Get("Id")).Value},{aspNetShippingElement.Attribute(XName.Get("Version")).Value}");
                }
            }
        }

        private const string AspNetPackageMonikerBaseString = "aspnetcore";
        private const string EFPackageMonikerBaseString = "efcore";

        private static string GetPackageMonikerBaseString(XElement packageElement)
        {
            var packageId = packageElement.Attribute(XName.Get("Id")).Value;

            if (string.Equals(packageId, "Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore", StringComparison.Ordinal) ||
                packageId.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
            {
                return EFPackageMonikerBaseString;
            }
            else
            {
                return AspNetPackageMonikerBaseString;
            }
        }
    }
}
