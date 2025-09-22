using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Extensions.Initialization;

public static class EnvironmentDependent
{
    public static async Task InvokeAsync(Dictionary<StartupMode, Func<Task>> actions)
    {
        var mode = Environment.Mode;

        if (mode == null)
            ArgumentNullException.ThrowIfNull(mode);

        var actionsToInvoke = actions.Where(x => (x.Key & mode) == x.Key);

        foreach (var action in actionsToInvoke)
        {
            await action.Value.Invoke();
        }
    }

    public static void Invoke(Dictionary<StartupMode, Action> actions)
    {
        var convertedActions = actions
            .ToDictionary<KeyValuePair<StartupMode, Action>, StartupMode, Func<Task>>(
                x => x.Key, x => () =>
                {
                    x.Value.Invoke();
                    return Task.CompletedTask;
                });
        InvokeAsync(convertedActions).Wait();
    }
}