using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConsumeSpecimens.Pages
{
    public class ValidateXMLModel : PageModel
    {
        private IHostingEnvironment _environment;
        private string result;

        public ValidateXMLModel(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        public void OnGet()
        {

        }

        public async Task OnPostAsync()
        {
            string fileName = Upload.FileName;
            XmlDocument doc = new XmlDocument();

            var file = Path.Combine(_environment.ContentRootPath, "uploads", fileName);
            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                await Upload.CopyToAsync(fileStream);
            }
            doc.Load(file);
            XmlNode node = doc.SelectSingleNode("/plant/specimens/specimen[latitude>0]");
            ValidateXML(file);
        }


        /// <summary>.
        /// Validate our XML file against the XSD
        /// </summary>
        public void ValidateXML(string fileName)
        {
            // Declare our validation preferences.'
            XmlReaderSettings settings = new XmlReaderSettings();
            // add schema
            var xsdPath = Path.Combine(_environment.ContentRootPath, "uploads", "plantsnamespace.xsd");
            XmlSchema schema = settings.Schemas.Add("http://www.plant" +
                "" +
                "places.com/Plant", xsdPath);
            // validate with XSD
            settings.ValidationType = ValidationType.Schema;
            // a couple more validation options.
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;

            settings.ValidationEventHandler +=
                new System.Xml.Schema.ValidationEventHandler(
                    this.ValidationEventHandler);

            XmlReader xmlReader = XmlReader.Create(fileName, settings);
            try
            {
                while (xmlReader.Read())
                {

                }
                // we only get here if no exception was thrown.
                result = "Validation successful";
            }
            catch (Exception e)
            {
                // we only get here if there was a validation error.
                result = e.Message;
            }
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs args)
        {
            result = "Validation failed.  Message = " + args.Message;
            throw new Exception("Validation failed.  Message: " + args.Message);
        }
    }
}