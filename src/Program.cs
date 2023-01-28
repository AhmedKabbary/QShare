using System.CommandLine;

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

var rootCommand = new RootCommand("File sharing tool build with .NET Core");

var includeOption = new Option<String[]>(
    aliases: new String[] { "-i", "--include" },
    description: "Include files for sharing"
);
var excludeOption = new Option<String[]>(
    aliases: new String[] { "-e", "--exclude" },
    description: "Exclude files from sharing"
);
var portOption = new Option<int>(
    aliases: new String[] { "-p", "--port" },
    description: "Specify http server port number"
);

includeOption.AllowMultipleArgumentsPerToken = true;
excludeOption.AllowMultipleArgumentsPerToken = true;

includeOption.SetDefaultValue(new String[] { "*" });
portOption.SetDefaultValue(8080);

rootCommand.AddOption(includeOption);
rootCommand.AddOption(excludeOption);
rootCommand.AddOption(portOption);

rootCommand.SetHandler(((include, exclude, port) =>
{
    Matcher matcher = new Matcher();
    matcher.AddIncludePatterns(include);
    matcher.AddExcludePatterns(exclude);

    PatternMatchingResult result = matcher.Execute(
        new DirectoryInfoWrapper(
            new DirectoryInfo(Environment.CurrentDirectory)
        )
    );

    if (result.HasMatches)
    {
        FilesServer filesServer = new FilesServer(port);

        Console.WriteLine("Serving the following files:");

        int n = 1;
        foreach (FilePatternMatch file in result.Files)
        {
            filesServer.AddFile(file.Path, new FileInfo(file.Path));
            Console.WriteLine($"  {n}) {file.Path}");
            n++;
        }
        Console.WriteLine();

        filesServer.Run();
    }
    else Console.WriteLine("No files to serve");
}),
includeOption, excludeOption, portOption);

rootCommand.InvokeAsync(args);
