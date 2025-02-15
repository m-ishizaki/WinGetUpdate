using System.Text;

const string wingetupdate = "winget update";
const string titleid = " ID ";
IProcessService process = new ProcessService();

string update = "";
{
    var result = process.Start(wingetupdate);
    Console.WriteLine(result);
    update = result.Replace('\r', '\n').Replace("\n\n", "\n");
}

var lines = update.Split("\n").SkipWhile(line => !line.Contains(titleid)).ToArray();

int idStart = default;
{

    var title = lines.FirstOrDefault() ?? string.Empty;
    var titleName = title.Substring(0, (title.IndexOf(titleid) + 1));
    var titleNameMultiBytecCount = titleName.Count(c => System.Text.Encoding.UTF8.GetByteCount(c.ToString()) != 1);
    idStart = title.IndexOf(titleid) + 1 + titleNameMultiBytecCount;
}

var packages = lines.Skip(2).Reverse().Skip(2).Reverse();

var ids = packages.Select(package => new string(package.Skip(idStart).TakeWhile(c => c != ' ').ToArray())).ToArray();

Parallel.ForEach(ids, id =>
{
    Console.WriteLine($"{id} =>\n");
    Console.WriteLine(process.Start($"{wingetupdate} {id}"));
});

public interface IProcessService
{
    public string Start(string command);
}

internal class ProcessService : IProcessService
{
    public string Start(string command)
    {
        var filename = command.Split(" ").FirstOrDefault("");
        System.Diagnostics.ProcessStartInfo info = new()
        {
            FileName = filename,
            Arguments = command.Substring(filename.Length),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
        };
        using var process = System.Diagnostics.Process.Start(info);
        using var stream = process?.StandardOutput;
        var output = stream?.ReadToEnd();
        return output ?? "";
    }
}