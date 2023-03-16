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
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using System.Net;
using Syncfusion.Pdf;
using Syncfusion.Drawing;
using Syncfusion.Pdf.Graphics;

namespace PdfConversion
{
    public static class ImageToPdf
    {
        [FunctionName("ImageToPdf")]
        [OpenApiOperation]
  
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(MultiPartFormDataModel), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/pdf", bodyType: typeof(byte[]))]

        // вибираємо Anonymous - щоб кожен міг отримати доступ до вашої функціїї
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
     
             
            var imageFile = req.Form.Files[0];

          
            using var imageStream = new MemoryStream();
   
            await imageFile.CopyToAsync(imageStream);

            imageStream.Position = 0;

            using PdfDocument doc = new();
            PdfPage page = doc.Pages.Add();

            SizeF pageSize = page.GetClientSize();
            
            using PdfBitmap image = new(imageStream);

            page.Graphics.DrawImage(image, new RectangleF(0, 0, pageSize.Width, pageSize.Height));

            using var outputPdfStream = new MemoryStream();
            doc.Save(outputPdfStream);
        
            doc.Close();

            outputPdfStream.Position = 0;




            string contentType = "application/pdf";
            string fileName = "Document.pdf";
            req.HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;{fileName}");

            return new FileContentResult(outputPdfStream.ToArray(), contentType);
        }
    }
}
