using System.Net;
using System.Text.RegularExpressions;

public class HttpServer
{
    private readonly HttpListener _httpListener;
    private readonly IDictionary<String, Delegate> _handlers;

    private readonly CancellationTokenSource _serverStartedTokenSource;
    public readonly CancellationToken ServerStarted;

    public delegate void RequestHandlerDelegate(HttpListenerContext context);
    public delegate Task RequestAsyncHandlerDelegate(HttpListenerContext context);

    public HttpServer(int port)
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://*:{port}/");
        _handlers = new Dictionary<String, Delegate>();

        _serverStartedTokenSource = new CancellationTokenSource();
        ServerStarted = _serverStartedTokenSource.Token;
    }

    public void AddPatternHandler(String pattern, RequestHandlerDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        _handlers[pattern] = handler;
    }

    public void AddPatternHandler(String pattern, RequestAsyncHandlerDelegate handler)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        _handlers[pattern] = handler;
    }

    public void Run()
    {
        _httpListener.Start();
        _serverStartedTokenSource.Cancel();
        while (_httpListener.IsListening)
        {
            HttpListenerContext context = _httpListener.GetContext();

            String path = context.Request.Url!.AbsolutePath;
            if (path.Length > 1) path = path.TrimEnd('/');

            bool isMatched = false;
            foreach (String pattern in _handlers.Keys)
            {
                if (Regex.IsMatch(path, pattern))
                {
                    var handler = _handlers[pattern];

                    Task task = null!;
                    if (handler is RequestHandlerDelegate action)
                    {
                        task = Task.Run(() => action(context));
                    }
                    else if (handler is RequestAsyncHandlerDelegate actionAsync)
                    {
                        task = actionAsync(context);
                    }
                    task.ContinueWith((task) =>
                    {
                        context.Response.Close();

                        if (task.IsFaulted)
                            Console.WriteLine(task.Exception?.Message);
                    });

                    isMatched = true;
                    break;
                }
            }
            if (!isMatched)
                context.Response.Close();
        }
    }
}
