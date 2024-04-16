using Saunter;

namespace Gateway.Apis.Amqp.RegistrationExtensions;

public static class AsyncApiDocumentationGenerationBag
{
    private readonly static List<Action<AsyncApiOptions>> DocumentationActions = new();

    public static void AddDocumentation(Action<AsyncApiOptions> action)
    {
        DocumentationActions.Add(action);
    }

    public static void Apply(AsyncApiOptions options)
    {
        foreach (var action in DocumentationActions)
        {
            action(options);
        }
    }
}