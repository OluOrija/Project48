using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace WebCrawlerBot
{
    class Program
    {
        // Azure Storage connection string
        private static readonly string storageConnectionString = "Your_Azure_Storage_Connection_String";

        static async Task Main(string[] args)
        {
            // List of URLs to crawl
            List<string> urls = new List<string>
            {
                "https://example.com",
                "https://bbc.co.uk"
                // Add more URLs here
            };

            // Create a BlobServiceClient
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create a container (if it doesn't exist)
            string containerName = "webcrawlerresults";
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName, PublicAccessType.None);

            // Crawl each URL and upload the result to Azure Blob Storage
            foreach (var url in urls)
            {
                string content = await CrawlUrlAsync(url);
                await UploadContentToBlobAsync(containerClient, url, content);
            }
        }

        static async Task<string> CrawlUrlAsync(string url)
        {
            HttpClient httpClient = new HttpClient();
            string html = await httpClient.GetStringAsync(url);

            // Optionally, parse the HTML using HtmlAgilityPack
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            // Process the HTML as needed
            // For now, we'll just return the raw HTML content
            return document.DocumentNode.OuterHtml;
        }

        static async Task UploadContentToBlobAsync(BlobContainerClient containerClient, string url, string content)
        {
            // Generate a unique blob name based on the URL
            string blobName = $"{Uri.EscapeDataString(url)}.html";

            // Create a BlobClient
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Convert the content to a byte array
            byte[] byteArray = Encoding.UTF8.GetBytes(content);

            // Upload the content to the blob
            using (var stream = new MemoryStream(byteArray))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "text/html" });
            }

            Console.WriteLine($"Uploaded content of {url} to {blobName}");
        }
    }
}
