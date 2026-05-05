namespace Aspire.Hosting
{
    /// <summary>
    /// Controls whether <c>docker compose up</c> pulls images before starting containers on the remote.
    /// </summary>
    public enum PullPolicy
    {
        /// <summary>
        /// Always re-pull images from the registry. Emits <c>--pull always</c>. Default — preserves
        /// the historical behavior of this library.
        /// </summary>
        Always,

        /// <summary>
        /// Only pull when the image isn't present in the remote daemon's local image store. Emits no
        /// <c>--pull</c> flag, matching Docker's compose default.
        /// </summary>
        Missing,

        /// <summary>
        /// Never pull; rely entirely on images already present in the remote daemon's local image
        /// store. Emits <c>--pull never</c>. Use with workflows like unregistry that load images
        /// directly into the remote daemon.
        /// </summary>
        Never,
    }
}

namespace Aspire.Hosting.Docker.SshDeploy.Models
{
    internal static class PullPolicyExtensions
    {
        /// <summary>
        /// Returns the <c>--pull</c> CLI fragment for <paramref name="policy"/>, or empty when no
        /// flag should be emitted.
        /// </summary>
        public static string ToComposeFlag(this PullPolicy policy) => policy switch
        {
            PullPolicy.Always => "--pull always",
            PullPolicy.Never => "--pull never",
            PullPolicy.Missing => "",
            _ => "--pull always",
        };

        /// <summary>
        /// Parses a configuration value (case-insensitive) into a <see cref="PullPolicy"/>. Falls
        /// back to <see cref="PullPolicy.Always"/> when the value is null, empty, or unrecognized.
        /// </summary>
        public static PullPolicy ParsePullPolicy(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "always" => PullPolicy.Always,
            "missing" => PullPolicy.Missing,
            "never" => PullPolicy.Never,
            _ => PullPolicy.Always,
        };
    }
}
