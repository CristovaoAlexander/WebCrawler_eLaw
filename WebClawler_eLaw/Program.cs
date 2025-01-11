using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string startUrl = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";
        string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WebCrawler;Integrated Security=True;";
        string outputDirectory = "CrawlerOutput";

        // Inicializa pastas de saída
        Directory.CreateDirectory(outputDirectory);

        DateTime startTime = DateTime.Now;
        List<ProxyData> allProxies = new List<ProxyData>();
        int pageCount = 0;

        var client = new HttpClient();

        // Processa a primeira página e captura todas as URLs de paginação
        Console.WriteLine($"Processando: {startUrl}");
        string firstPageHtml = await client.GetStringAsync(startUrl);

        // Salva a primeira página HTML localmente
        string firstPageFileName = Path.Combine(outputDirectory, "page_1.html");
        await File.WriteAllTextAsync(firstPageFileName, firstPageHtml);
        pageCount++;

        // Extrai proxies da primeira página
        var proxies = ExtractProxies(firstPageHtml);
        allProxies.AddRange(proxies);

        // Extrai todas as URLs de paginação
        var paginationUrls = ExtractPaginationUrls(firstPageHtml, "https://proxyservers.pro");

        // Processa todas as páginas da paginação
        foreach (var pageUrl in paginationUrls)
        {
            Console.WriteLine($"Processando: {pageUrl}");
            string pageHtml = await client.GetStringAsync(pageUrl);

            // Salva a página HTML localmente
            string pageFileName = Path.Combine(outputDirectory, $"page_{pageCount + 1}.html");
            await File.WriteAllTextAsync(pageFileName, pageHtml);
            pageCount++;

            // Extrai proxies da página
            var pageProxies = ExtractProxies(pageHtml);
            allProxies.AddRange(pageProxies);
        }

        DateTime endTime = DateTime.Now;

        // Salva os resultados em JSON
        string jsonFileName = Path.Combine(outputDirectory, "proxies.json");
        File.WriteAllText(jsonFileName, JsonConvert.SerializeObject(allProxies, Formatting.Indented));

        // Salva os detalhes no banco de dados
        using (var connection = new SqlConnection(connectionString))
        {
            connection.Open();
            var command = new SqlCommand(
                "INSERT INTO WebCrawlerExecutionLog (StartTime, EndTime, PageCount, LinhasCount, JsonFilePath) " +
                "VALUES (@StartTime, @EndTime, @PageCount, @LinhasCount, @JsonFilePath)", connection);
            command.Parameters.AddWithValue("@StartTime", startTime);
            command.Parameters.AddWithValue("@EndTime", endTime);
            command.Parameters.AddWithValue("@PageCount", pageCount);
            command.Parameters.AddWithValue("@LinhasCount", allProxies.Count);
            command.Parameters.AddWithValue("@JsonFilePath", jsonFileName);
            command.ExecuteNonQuery();
        }

        Console.WriteLine("Crawler concluído com sucesso!");
        Console.WriteLine($"Páginas processadas: {pageCount}");
        Console.WriteLine($"Linhas extraídas: {allProxies.Count}");
        Console.WriteLine($"Arquivo JSON: {jsonFileName}");
    }


    static List<ProxyData> ExtractProxies(string html)
    {
        var proxies = new List<ProxyData>();
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var rows = document.DocumentNode.SelectNodes("//table[contains(@class, 'table-hover')]/tbody/tr");
        if (rows != null)
        {
            foreach (var row in rows)
            {
                var cells = row.SelectNodes("td");
                if (cells != null && cells.Count >= 8)
                {
                    // Captura o atributo data-port no <span> e decodifica
                    var portSpan = cells[2].SelectSingleNode(".//span[@data-port]");
                    string encodedPort = portSpan?.GetAttributeValue("data-port", string.Empty);
                    string decodedPort = DecodePort(encodedPort);

                    proxies.Add(new ProxyData
                    {
                        IPAddress = cells[1].InnerText.Trim(),
                        Port = decodedPort, // Porta decodificada
                        Country = cells[3].InnerText.Trim(),
                        Protocol = cells[6].InnerText.Trim()
                    });
                }
            }
        }

        return proxies;
    }

    static List<string> ExtractPaginationUrls(string html, string baseUrl)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        // Localiza todos os links de paginação
        var paginationNodes = document.DocumentNode.SelectNodes("//ul[contains(@class, 'pagination')]//a[contains(@class, 'page-link')]");
        var urls = new List<string>();

        if (paginationNodes != null)
        {
            foreach (var node in paginationNodes)
            {
                string pageUrl = node.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(pageUrl))
                {
                    // Constrói a URL absoluta (caso seja relativa)
                    string fullUrl = new Uri(new Uri(baseUrl), pageUrl).ToString();
                    urls.Add(fullUrl);
                }
            }
        }

        // Debugging: Mostra as URLs capturadas no console
        Console.WriteLine("Links de paginação encontrados:");
        foreach (var url in urls)
        {
            Console.WriteLine(url);
        }

        return urls.Distinct().ToList(); // Remove URLs duplicadas
    }
     
    static string DecodePort(string encodedPort)
    {
        if (string.IsNullOrEmpty(encodedPort) || encodedPort.Length % 2 != 0)
            return string.Empty;

        // Divide o valor hexadecimal em pares de 2 e inverte a ordem
        var portBytes = Enumerable.Range(0, encodedPort.Length / 2)
                                  .Select(i => encodedPort.Substring(i * 2, 2))
                                  .Reverse()
                                  .ToArray();

        // Concatena os pares e converte para decimal
        string portHex = string.Concat(portBytes);
        ulong portDecimal = Convert.ToUInt64(portHex, 16);

        return portDecimal.ToString();
    }

}

class ProxyData
{
    public string IPAddress { get; set; }
    public string Port { get; set; }
    public string Country { get; set; }
    public string Protocol { get; set; }
}
