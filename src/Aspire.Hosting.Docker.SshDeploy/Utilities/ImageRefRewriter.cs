namespace Aspire.Hosting.Docker.SshDeploy.Utilities;

/// <summary>
/// Helpers for rewriting the registry endpoint on a single docker-compose <c>*_IMAGE</c>
/// environment-variable value, used by
/// <see cref="DockerPipelineExtensions.WithPullRegistry(IResourceBuilder{DockerComposeEnvironmentResource}, string)"/>.
/// </summary>
internal static class ImageRefRewriter
{
    /// <summary>
    /// If <paramref name="varName"/> looks like an Aspire-emitted image variable
    /// (<c>UPPER_SNAKE_CASE</c> ending with <c>_IMAGE</c>) and <paramref name="value"/> has a
    /// registry-prefix segment before the first <c>/</c>, returns the value with that prefix
    /// replaced by <paramref name="pullEndpoint"/>. Returns <c>null</c> in every other case
    /// (caller leaves the entry untouched).
    /// </summary>
    /// <remarks>
    /// The first slash-delimited segment of an Aspire-emitted <c>*_IMAGE</c> value is always the
    /// registry endpoint (Aspire constructs the string from the attached <c>ContainerRegistry</c>
    /// resource as <c>{endpoint}/{repo}/{name}:{tag}</c>). Replacing whatever appears before that
    /// slash with <paramref name="pullEndpoint"/> is sufficient — we don't need to know the push
    /// endpoint up front. Values without a slash (e.g. plain <c>name:tag</c>) are left alone.
    /// </remarks>
    public static string? RewriteIfImage(string varName, string? value, string pullEndpoint)
    {
        if (!IsImageVarName(varName)) return null;
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pullEndpoint)) return null;

        var slashIndex = value.IndexOf('/');
        if (slashIndex <= 0) return null;

        var normalizedPullEndpoint = pullEndpoint.TrimEnd('/');
        if (normalizedPullEndpoint.Length == 0) return null;

        var rewritten = normalizedPullEndpoint + value[slashIndex..];
        return string.Equals(rewritten, value, StringComparison.Ordinal) ? null : rewritten;
    }

    private static bool IsImageVarName(string name)
    {
        if (!name.EndsWith("_IMAGE", StringComparison.Ordinal)) return false;

        // Aspire emits keys as UPPER_SNAKE_CASE; reject anything that doesn't match so we don't
        // mistake an unrelated value for an image reference.
        foreach (var ch in name)
        {
            if (ch is not (>= 'A' and <= 'Z') and not (>= '0' and <= '9') and not '_') return false;
        }
        return true;
    }
}
