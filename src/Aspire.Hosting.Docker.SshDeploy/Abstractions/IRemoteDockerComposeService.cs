using Aspire.Hosting.Docker.SshDeploy.Models;

namespace Aspire.Hosting.Docker.SshDeploy.Abstractions;

/// <summary>
/// Provides high-level Docker Compose operations on remote servers.
/// </summary>
internal interface IRemoteDockerComposeService
{
    /// <summary>
    /// Stops all containers in a Docker Compose deployment.
    /// </summary>
    /// <param name="deployPath">Path to the deployment directory containing docker-compose.yaml</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the stop operation</returns>
    Task<ComposeOperationResult> StopAsync(string deployPath, CancellationToken cancellationToken);

    /// <summary>
    /// Authenticates with a container registry on the remote server.
    /// </summary>
    /// <param name="registryUrl">The registry URL (e.g., ghcr.io, docker.io)</param>
    /// <param name="username">Registry username</param>
    /// <param name="password">Registry password or token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the login operation</returns>
    Task<ComposeOperationResult> LoginToRegistryAsync(string registryUrl, string username, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Deploys services with minimal downtime by running <c>docker compose up -d --remove-orphans</c>
    /// on the remote, optionally adding <c>--pull always</c> or <c>--pull never</c> based on
    /// <paramref name="pullPolicy"/>. Recreates only changed containers without explicitly stopping
    /// first.
    /// </summary>
    /// <param name="deployPath">Path to the deployment directory containing docker-compose.yaml</param>
    /// <param name="pullPolicy">Whether to instruct compose to pull images before starting containers. Defaults to <see cref="PullPolicy.Always"/> to preserve historical behavior.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the deploy operation</returns>
    Task<ComposeOperationResult> UpAsync(string deployPath, PullPolicy pullPolicy = PullPolicy.Always, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prunes unused Docker images to free disk space.
    /// Should be called after deployments to clean up old images.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the prune operation</returns>
    Task<ComposeOperationResult> PruneImagesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets diagnostic logs for a specific service/container.
    /// </summary>
    /// <param name="containerName">The container name (from ComposeServiceInfo.Name)</param>
    /// <param name="tailLines">Number of lines to retrieve from the end of the logs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The log output</returns>
    Task<string> GetServiceLogsAsync(string containerName, int tailLines, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the current status of all services using docker compose ps --format json.
    /// </summary>
    /// <param name="deployPath">Path to the deployment directory containing docker-compose.yaml</param>
    /// <param name="host">The host address for generating service URLs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated status of all services</returns>
    Task<ComposeStatus> GetStatusAsync(string deployPath, string host, CancellationToken cancellationToken);

    /// <summary>
    /// Streams status updates at a specified interval until cancelled.
    /// </summary>
    /// <param name="deployPath">Path to the deployment directory containing docker-compose.yaml</param>
    /// <param name="host">The host address for generating service URLs</param>
    /// <param name="interval">Time between status polls</param>
    /// <param name="cancellationToken">Cancellation token to stop streaming</param>
    /// <returns>Async enumerable of status updates</returns>
    IAsyncEnumerable<ComposeStatus> StreamStatusAsync(string deployPath, string host, TimeSpan interval, CancellationToken cancellationToken);
}
