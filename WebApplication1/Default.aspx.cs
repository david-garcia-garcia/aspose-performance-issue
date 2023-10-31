using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

public partial class _Default : Page
{
    protected string results;

    protected void Page_Load(object sender, EventArgs e)
    {
        int paralellism = 8;
        int certificateCount = 100;
        long totalSize = 0;
        List<Exception> errors = new List<Exception>();

        List<string> links = new List<string>();

        var currentUrl = new UriBuilder(HttpContext.Current.Request.Url);
        currentUrl.Path = "Contact.aspx";
        string certificateLink = currentUrl.ToString();

        for (int x = 0; x < certificateCount; x++)
        {
            links.Add(certificateLink);
        }

        // Let's do an initial request just to warm up aspose
        HttpWebRequest r = (HttpWebRequest)WebRequest.Create(links.First());
        r.ServicePoint.Expect100Continue = false;
        using (var b = r.GetResponse())
        {
            using (Stream s = b.GetResponseStream())
            using (MemoryStream memStream = new MemoryStream())
            {
                memStream.CopyTo(memStream);
            }
        }

        ParallelOptions pops = new ParallelOptions();

        pops.MaxDegreeOfParallelism = paralellism;

        Stopwatch sw = Stopwatch.StartNew();

        Parallel.ForEach(links, pops, (l) =>
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(l);

                // Disabling Expect100Continue (equivalent to client.DefaultRequestHeaders.ExpectContinue = false in HttpClient)
                request.ServicePoint.Expect100Continue = false;

                using (WebResponse response = request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (MemoryStream memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);
                    byte[] data = memStream.ToArray();
                    Interlocked.Add(ref totalSize, data.Length);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });

        sw.Stop();

        string newLine = "<br/>";

        results += $"ASPOSE VERSION: {typeof(Aspose.Pdf.Artifact).Assembly.GetName().Version}" + newLine;
        results += $"paralellism: {paralellism}" + newLine;
        results += $"certificateCount: {certificateCount}" + newLine;
        results += $"errors: {errors.Count}" + newLine;
        results += $"outputFileSize average: {Math.Round((totalSize / (double)certificateCount) / (1024 * 1024), 2)} Mb" + newLine;
        results += $"lapsedSeconds: {sw.Elapsed.TotalSeconds}" + newLine;
        results += $"rate: {Math.Round(certificateCount / sw.Elapsed.TotalSeconds, 2)} certs/s" + newLine;
    }
}