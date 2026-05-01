using Aspire.Hosting.Docker.SshDeploy.Utilities;

namespace Aspire.Hosting.Docker.SshDeploy.Tests;

public class ImageRefRewriterTests
{
    [Theory]
    [InlineData("WEB_IMAGE", "host.docker.internal:5001/kaizen-tools/web:abc", "localhost:5001", "localhost:5001/kaizen-tools/web:abc")]
    [InlineData("PYTHON_API_IMAGE", "ghcr.io/myorg/api:v1", "mirror.example.com", "mirror.example.com/myorg/api:v1")]
    [InlineData("FOO_BAR_IMAGE", "registry-1.docker.io/lib/foo:latest", "private.example.com:5000", "private.example.com:5000/lib/foo:latest")]
    public void RewriteIfImage_RewritesPrefixOnImageVars(string varName, string value, string pullEndpoint, string expected)
    {
        Assert.Equal(expected, ImageRefRewriter.RewriteIfImage(varName, value, pullEndpoint));
    }

    [Theory]
    [InlineData("WEB_PORT", "80")]
    [InlineData("OTEL_SERVICE_NAME", "web")]
    [InlineData("ASPNETCORE_FORWARDEDHEADERS_ENABLED", "true")]
    [InlineData("TUNNEL_TOKEN", "host.docker.internal:5001/abc")]
    public void RewriteIfImage_LeavesNonImageVarsAlone(string varName, string value)
    {
        Assert.Null(ImageRefRewriter.RewriteIfImage(varName, value, "localhost:5001"));
    }

    [Theory]
    [InlineData("web_image")]   // lowercase
    [InlineData("WEB_imAGE")]   // mixed case
    [InlineData("Web_IMAGE")]   // leading uppercase only
    public void RewriteIfImage_RejectsNonUpperSnakeKeys(string varName)
    {
        // Aspire emits UPPER_SNAKE_CASE; anything else is some other producer and we shouldn't touch it.
        Assert.Null(ImageRefRewriter.RewriteIfImage(varName, "ghcr.io/repo/web:tag", "localhost:5001"));
    }

    [Fact]
    public void RewriteIfImage_NullWhenValueHasNoSlash()
    {
        // Plain `name:tag` (no registry prefix) — nothing to rewrite, leave it.
        Assert.Null(ImageRefRewriter.RewriteIfImage("WEB_IMAGE", "myimage:tag", "localhost:5001"));
    }

    [Fact]
    public void RewriteIfImage_NullWhenAlreadyAtPullEndpoint()
    {
        // Idempotent: if the registry prefix already matches the pull endpoint, returns null
        // (caller treats that as "no change needed").
        Assert.Null(ImageRefRewriter.RewriteIfImage("WEB_IMAGE", "localhost:5001/repo/web:tag", "localhost:5001"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RewriteIfImage_NullWhenValueMissing(string? value)
    {
        Assert.Null(ImageRefRewriter.RewriteIfImage("WEB_IMAGE", value, "localhost:5001"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RewriteIfImage_NullWhenPullEndpointMissing(string? pullEndpoint)
    {
        Assert.Null(ImageRefRewriter.RewriteIfImage("WEB_IMAGE", "ghcr.io/repo/web:tag", pullEndpoint!));
    }
}
