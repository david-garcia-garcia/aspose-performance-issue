using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using Aspose.Pdf;
using LoadOptions = System.Xml.Linq.LoadOptions;
using Page = System.Web.UI.Page;

public partial class Contact : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var tempFile = Path.GetTempFileName() + "_" + Guid.NewGuid() + ".pdf";
        string certificateLink = HttpContext.Current.Server.MapPath("~/Diploma.html");

        try
        {
            GeneratePdf(certificateLink, false, tempFile);

            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = "application/PDF";
            Response.AddHeader("Content-Disposition", $"attachment; filename=certificate.pdf");
            Response.AddHeader("Transfer-Encoding", "identity");
            Response.AppendHeader("Content-Length", new FileInfo(tempFile).Length.ToString());
            Response.TransmitFile(tempFile);
            Response.Flush();
            Response.End();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: " + ex.Message);
        }
        finally
        {
            try
            {
                File.Delete(tempFile);
            }
            catch
            {
            }
        }
    }

    public static void GeneratePdf(string localPath, bool bVertical, string strOutputPath)
    {
        HtmlLoadOptions htmlOptions = new HtmlLoadOptions(localPath);

        htmlOptions.PageInfo = new PageInfo();

        if (!bVertical)
        {
            htmlOptions.PageInfo.IsLandscape = true;
            htmlOptions.PageInfo.Width = 842;
            htmlOptions.PageInfo.Height = 595;
        }
        else
        {
            htmlOptions.PageInfo.IsLandscape = false;
            htmlOptions.PageInfo.Width = 595;
            htmlOptions.PageInfo.Height = 842;
        }

        htmlOptions.PageInfo.Margin = new MarginInfo();
        htmlOptions.PageInfo.Margin.Left = 0.00;
        htmlOptions.PageInfo.Margin.Right = 0.00;
        htmlOptions.HtmlMediaType = HtmlMediaType.Screen;
        htmlOptions.IsEmbedFonts = true;

        htmlOptions.CustomLoaderOfExternalResources = delegate (string uri)
        {
            if (uri.StartsWith("file://"))
            {
                var realPath = uri.Substring(8, uri.Length - 8);

                var fifo = new FileInfo(realPath);

                var directoryName = Path.GetDirectoryName(localPath)?.ToLower();

                if (fifo?.Directory?.FullName.ToLower().StartsWith(directoryName) == true)
                {
                    return new Aspose.Pdf.LoadOptions.ResourceLoadingResult(File.ReadAllBytes(realPath));
                }
            }

            return new Aspose.Pdf.LoadOptions.ResourceLoadingResult(null);
        };

        using (Document pdfDocument = new Document(localPath, htmlOptions))
        {
            pdfDocument.Save(strOutputPath, SaveFormat.Pdf);
        }
    }
}