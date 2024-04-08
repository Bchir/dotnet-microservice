using Saunter;

namespace Gateway.Apis.Amqp.Tooling;

public static class AsyncApiDocumentationGenerationBag
{
    private readonly static Queue<Action<AsyncApiOptions>> DocumentationActions = new();

    public static void AddDocumentation(Action<AsyncApiOptions> action)
    {
        DocumentationActions.Enqueue(action);
    }

    public static void Apply(AsyncApiOptions options)
    {
        while (DocumentationActions.Count > 0)
        {
            var action = DocumentationActions.Dequeue();
            action(options);
        }
    }
}