using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using Syncfusion.DocIORenderer;
using System.Net;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;

namespace PdfConversion
{
    public static class ExcelToPdf
    {
        [FunctionName("ExcelToPdf")]
        [OpenApiOperation]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(MultiPartFormDataModel), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/pdf", bodyType: typeof(byte[]))]

   
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
           
            var textFile = req.Form.Files[0];

         
            using var excelStream = new MemoryStream();
            await textFile.CopyToAsync(excelStream);

            excelStream.Position = 0;

            using ExcelEngine exelEngine = new();
            var application = exelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2013;
            var workbook = application.Workbooks.Open(excelStream, ExcelOpenType.Automatic, ExcelParseOptions.Default);

            XlsIORenderer renderer = new XlsIORenderer();

    
            var pdfDocument = renderer.ConvertToPDF(workbook);
    

     
            using MemoryStream pdfStream = new();
            pdfDocument.Save(pdfStream);
            pdfDocument.Close();
         
            pdfStream.Position = 0;


            string contentType = "application/pdf";
            string fileName = "Document.pdf";
            req.HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;{fileName}");

            return new FileContentResult(pdfStream.ToArray(), contentType);
        }
    }
}
