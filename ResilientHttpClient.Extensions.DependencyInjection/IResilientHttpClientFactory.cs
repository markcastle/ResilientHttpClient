namespace ResilientHttpClient.Extensions.DependencyInjection
{
    /// <summary>
    /// A factory for creating named ResilientHttpClient instances.
    /// </summary>
    /// <remarks>
    /// This interface allows you to register multiple named HTTP clients in dependency injection
    /// and resolve them by name at runtime.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register multiple named clients
    /// services.AddNamedResilientHttpClient("GitHub", "https://api.github.com");
    /// services.AddNamedResilientHttpClient("MyAPI", "https://myapi.com");
    /// 
    /// // Inject and use
    /// public class MyService
    /// {
    ///     private readonly IResilientHttpClient _githubClient;
    ///     
    ///     public MyService(IResilientHttpClientFactory factory)
    ///     {
    ///         _githubClient = factory.CreateClient("GitHub");
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IResilientHttpClientFactory
    {
        /// <summary>
        /// Creates a ResilientHttpClient instance with the specified name.
        /// </summary>
        /// <param name="name">The name of the client to create.</param>
        /// <returns>A ResilientHttpClient instance configured for the specified name.</returns>
        /// <exception cref="System.ArgumentException">Thrown when no client is registered with the specified name.</exception>
        Core.IResilientHttpClient CreateClient(string name);
    }
}
