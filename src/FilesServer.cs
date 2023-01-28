using System.Collections.Concurrent;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;

public class FilesServer
{
    public int Port { get; init; }
    private readonly IDictionary<String, FileInfo> _filesMap;

    public FilesServer(int port)
    {
        Port = port;
        _filesMap = new ConcurrentDictionary<String, FileInfo>();
    }

    private bool _isRunning;

    public void AddFile(String path, FileInfo fileInfo)
    {
        if (_isRunning)
            throw new InvalidOperationException();

        _filesMap[path] = fileInfo;
    }

    public void Run()
    {
        _isRunning = true;

        HttpServer httpServer = new HttpServer(Port);
        httpServer.AddPatternHandler(@"^\/$", handler1);
        httpServer.AddPatternHandler(@"^\/files$", handler2);
        httpServer.AddPatternHandler(@"^\/files", handler3);
        httpServer.AddPatternHandler(@"^\/assets", handler4);

        httpServer.ServerStarted.Register(() =>
        {
            String hostname = Dns.GetHostName();
            IPAddress[] ips = Dns.GetHostEntry(hostname).AddressList;

            foreach (IPAddress ip in ips)
            {
                if (!IPAddress.IsLoopback(ip) && !ip.IsIPv6LinkLocal)
                {
                    switch (ip.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            Console.WriteLine($"Listening on http://{ip}:{Port}");
                            break;
                        case AddressFamily.InterNetworkV6:
                            Console.WriteLine($"Listening on http://[{ip}]:{Port}");
                            break;
                    }
                }
            }
            Console.WriteLine();
        });

        httpServer.Run();
    }

    private async Task handler1(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;

        String assemblyPath = Assembly.GetExecutingAssembly().Location;
        String basePath = Path.GetDirectoryName(assemblyPath)!;
        String filePath = Path.Combine(basePath, "assets", "index.html");
        FileInfo file = new FileInfo(filePath);
        response.ContentLength64 = file.Length;
        response.ContentType = MediaTypeNames.Text.Html;
        await file.OpenRead().CopyToAsync(response.OutputStream);
    }

    private async Task handler2(HttpListenerContext context)
    {
        HttpListenerResponse response = context.Response;

        List<Dictionary<String, String>> files = new List<Dictionary<String, String>>(_filesMap.Count);
        foreach (var item in _filesMap)
        {
            files.Add(
                new Dictionary<String, String>()
                {
                    {"name", item.Key},
                    {"size", ConvertBytesToHumanReadableSize(item.Value.Length)},
                }
            );
        }
        String json = JsonSerializer.Serialize(files);
        byte[] data = Encoding.UTF8.GetBytes(json);

        response.ContentLength64 = data.LongLength;
        response.ContentType = MediaTypeNames.Application.Json;
        await response.OutputStream.WriteAsync(data, 0, data.Length);
    }

    private async Task handler3(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        String path = request.Url!.AbsolutePath;

        String filePath = path.Substring(7);
        if (_filesMap.TryGetValue(filePath, out FileInfo? file))
        {
            response.ContentLength64 = file.Length;
            response.ContentType = MediaTypeNames.Application.Octet;
            await file.OpenRead().CopyToAsync(response.OutputStream);
        }
        else response.StatusCode = (int)HttpStatusCode.NotFound;
    }

    private async Task handler4(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        String path = request.Url!.AbsolutePath;

        String assemblyPath = Assembly.GetExecutingAssembly().Location;
        String basePath = Path.GetDirectoryName(assemblyPath)!;
        String filePath = Path.Combine(basePath, "assets", path.Substring(7));
        if (_filesMap.TryGetValue(filePath, out FileInfo? file))
        {
            response.ContentLength64 = file.Length;
            response.ContentType = MediaTypeNames.Application.Octet;
            await file.OpenRead().CopyToAsync(response.OutputStream);
        }
        else response.StatusCode = (int)HttpStatusCode.NotFound;
    }

    public static String ConvertBytesToHumanReadableSize(double length)
    {
        String[] sizes = { "B", "KB", "MB", "GB", "TB" };

        int order = 0;
        while (length >= 1024 && order < sizes.Length)
        {
            order++;
            length /= 1024;
        }
        return String.Format("{0:0.##} {1}", length, sizes[order]);
    }
}
