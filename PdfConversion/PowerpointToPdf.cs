using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using Syncfusion.Pdf;
using Syncfusion.Presentation;
using Syncfusion.PresentationToPdfConverter;

namespace PdfConversion
{
    public static class PowerpointToPdf
    {
        [FunctionName("PowerpointToPdf")]
        [OpenApiOperation]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(MultiPartFormDataModel), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/pdf", bodyType: typeof(byte[]))]


        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {


            var presentationFile = req.Form.Files[0];

            using var pptxStream = new MemoryStream();
            await presentationFile.CopyToAsync(pptxStream);
            pptxStream.Position = 0;



            using IPresentation presentation = Presentation.Open(pptxStream);
            PresentationToPdfConverterSettings settings = new()
            {
                ShowHiddenSlides = true
            };

            PdfDocument pdfDocument =  PresentationToPdfConverter.Convert(presentation, settings);

      

            using var outputPdfStream = new MemoryStream();
            pdfDocument.Save(outputPdfStream);
            pdfDocument.Close();

            outputPdfStream.Position = 0;




            string contentType = "application/pdf";
            string fileName = "Document.pdf";
            req.HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;{fileName}");

            return new FileContentResult(outputPdfStream.ToArray(), contentType);
        }
    }
}
