using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Vraa.Div.Client;

public class MessageService
{

    public string ValidateXml(string xmlContent, params string[] xsdUrls)
    {
        bool isValid = true;
        string validationErrors = "";

        try
        {
            // Initialize XmlSchemaSet for loading schemas
            XmlSchemaSet schemas = new XmlSchemaSet();

            // Load each XSD from the URL and add it to the schema set
            foreach (var xsdUrl in xsdUrls)
            {
                if (Uri.IsWellFormedUriString(xsdUrl, UriKind.Absolute))
                {
                    // Download the schema from the URL
                    using (var webClient = new WebClient())
                    {
                        string xsdContent = webClient.DownloadString(xsdUrl);
                        using (StringReader sr = new StringReader(xsdContent))
                        {
                            schemas.Add(null, XmlReader.Create(sr));
                        }
                    }
                }
                else
                {
                    // If any local paths are added in the future
                    schemas.Add(null, xsdUrl);
                }
            }

            // Define settings for validation
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(schemas);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationEventHandler += (sender, args) =>
            {
                isValid = false;
                validationErrors += $"Validation error: {args.Message}\n";
            };

            // Perform validation
            using (StringReader stringReader = new StringReader(xmlContent))
            using (XmlReader reader = XmlReader.Create(stringReader, settings))
            {
                while (reader.Read()) { } // Read through the document for validation
            }
        }
        catch (Exception ex)
        {
            validationErrors = $"Exception during validation: {ex.Message}";
            isValid = false;
        }

        return isValid ? "XML is valid." : $"XML is invalid.\n{validationErrors}";
    }

    public string FormatXmlWithColor(string xmlContent)
    {
        try
        {
            // Parse XML and get it as a formatted string
            var xDocument = XDocument.Parse(xmlContent);
            var prettyXml = xDocument.ToString();

            // Escape special characters
            prettyXml = prettyXml.Replace("&", "&amp;")
                                 .Replace("<", "&lt;")
                                 .Replace(">", "&gt;");

            // Apply CSS classes with regex or string replacements
            prettyXml = System.Text.RegularExpressions.Regex.Replace(prettyXml, @"(&lt;/?)(\w+)", @"$1<span class='xml-element'>$2</span>");
            prettyXml = System.Text.RegularExpressions.Regex.Replace(prettyXml, @"(\w+)=&quot;", @"<span class='xml-attribute'>$1</span>=");
            prettyXml = System.Text.RegularExpressions.Regex.Replace(prettyXml, @"&quot;([^&]+)&quot;", @"<span class='xml-value'>""$1""</span>");

            return prettyXml;
        }
        catch (Exception)
        {
            // Return unformatted XML if there’s a parsing issue
            return xmlContent;
        }
    }

    public async Task<bool> SendMessageAsync(string from, string to, string title)
    {
        try
        {
            IntegrationMessage message = new IntegrationMessage();
            //uzstāda ziņojuma nosūtītāju
            message.From = "mail@domain1";
            //uzstāda adresātu
            message.To = "mail@domain2";
            //uzstāda metadatus
            message.Document.Authors.AddInstitution("My institution name");
            message.Document.Kind.Code = "DOC_EMPTY";
            message.Document.Title = "Document simple title";
            // Tiek izgūts Aploksnes XML
            // Aploksnes XML izgūšana nav obligāta
            string xml = message.Xml;

            using var client = new IntegrationClient();
            var messageId = await Task.Run(() => client.SendMessage(message));
            return !string.IsNullOrEmpty(messageId);
        }
        catch
        {
            return false;
        }
    }
    

    public string PrepareMessage()
    {
        IntegrationMessage message = new IntegrationMessage();

        // Set up sender and recipient, even if they are placeholders
        message.From = "test@domain.local";
        message.To = "recipient@domain.local";

        // Configure metadata
        message.Document.Authors.AddInstitution("Test Institution");
        message.Document.Kind.Code = "DOC_TEST";
        message.Document.Title = "Test Document Title";

        // Add a sample attachment
        byte[] sampleFile = new byte[] { 0x0A, 0x0B, 0x0C }; // Test binary data
        message.Document.Files.Add(sampleFile, "SampleFile.txt");

        // Optionally, retrieve the XML representation for validation
        string xmlContent = message.Xml;

        // Output or log the XML for verification (no actual sending involved)
        return xmlContent;
    }

    public string GenerateMessageXml()
    {
        IntegrationMessage message = new IntegrationMessage();
        message.From = "example@domain.local";
        message.To = "receiver@domain.local";
        message.Document.Authors.AddInstitution("Sample Institution");
        message.Document.Kind.Code = "DOC_SAMPLE";
        message.Document.Title = "Sample Document";

        // Get XML envelope without sending
        return message.Xml;
    }
    public string TestErrorHandling()
    {
        try
        {
            // Create message as usual
            IntegrationMessage message = new IntegrationMessage();
            message.From = "test@domain.local";
            message.To = "recipient@domain.local";
            message.Document.Authors.AddInstitution("Test Institution");
            message.Document.Kind.Code = "DOC_TEST";
            message.Document.Title = "Error Handling Test";

            // Create client with invalid address to simulate error
            ClientConfiguration clientConfig = new ClientConfiguration();
            clientConfig.ServiceAddress = "https://invalid.endpoint/vraa";
            IntegrationClient client = new IntegrationClient(clientConfig);

            // Attempt to send message (synchronously, will fail)
            string messageId = client.SendMessage(message);

            // Output result (not expected to succeed)
            return "messageId: " + messageId;
        }
        catch (Exception ex)
        {
            // Handle expected error here
            return "ex.Message: " + ex.Message;
        }
    }

    //public void RetrieveMessageList()
    //{
    //    IntegrationClient client = new IntegrationClient();
    //    long addresseeUnitId = 12345; // Dummy ID for testing
    //    int maxResultCount = 10; // Maximum number of results to retrieve

    //    var messageList = client.GetMessageList(addresseeUnitId, maxResultCount); // No await needed for synchronous methods

    //    Console.WriteLine("Retrieved Messages:");
    //    foreach (var message in messageList)
    //    {
    //        Console.WriteLine("Message ID: " + message.MessageId);
    //        Console.WriteLine("Title: " + message.Title);
    //    }
    //}

}
