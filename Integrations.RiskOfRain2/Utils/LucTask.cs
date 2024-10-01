using System.Runtime.CompilerServices;

namespace OpenShock.Integrations.RiskOfRain2;

public static class LucTask
{
    public static Task Run(Func<Task?> function, CancellationToken token = default, [CallerFilePath] string file = "",
        [CallerMemberName] string member = "", [CallerLineNumber] int line = -1) => Task.Run(function, token)
        .ContinueWith(
            t =>
            {
                if (!t.IsFaulted) return;
                var index = file.LastIndexOf('\\');
                if (index == -1) index = file.LastIndexOf('/');
                RiskOfPain.ModLogger.LogError(
                    $"Error during task execution. {file.Substring(index + 1, file.Length - index - 1)}::{member}:{line} {t.Exception}");
            }, TaskContinuationOptions.OnlyOnFaulted);
}