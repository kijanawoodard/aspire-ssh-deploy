#pragma warning disable ASPIREPIPELINES001

using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting.Docker.SshDeploy.Utilities;

/// <summary>
/// Collects all parameter resources referenced by the model, including those referenced
/// via environment variables and command-line arguments (not just explicit parameter resources).
/// This mirrors the logic in ParameterProcessor.CollectDependentParameterResourcesAsync.
/// </summary>
internal static class ParameterCollector
{
    /// <summary>
    /// Collects all parameter resources referenced by the model.
    /// </summary>
    public static async Task<List<ParameterResource>> CollectAllReferencedParametersAsync(
        DistributedApplicationModel model,
        DistributedApplicationExecutionContext executionContext,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var referencedParameters = new Dictionary<string, ParameterResource>();
        var currentDependencySet = new HashSet<object?>();

        foreach (var resource in model.Resources)
        {
            if (resource.IsExcludedFromPublish())
            {
                continue;
            }

            var configuration = await ExecutionConfigurationBuilder
                .Create(resource)
                .WithEnvironmentVariablesConfig()
                .WithArgumentsConfig()
                .BuildAsync(executionContext, logger, cancellationToken);

            if (configuration.Exception is not null)
            {
                throw new InvalidOperationException(
                    $"Failed to build execution configuration for resource '{resource.Name}'.",
                    configuration.Exception);
            }

            foreach (var (_, (unprocessed, _)) in configuration.EnvironmentVariablesWithUnprocessed)
            {
                TryAddDependentParameters(unprocessed, referencedParameters, currentDependencySet);
            }

            foreach (var (unprocessed, _, _) in configuration.ArgumentsWithUnprocessed)
            {
                TryAddDependentParameters(unprocessed, referencedParameters, currentDependencySet);
            }
        }

        // Combine explicit parameters with dependent parameters
        var explicitParameters = model.Resources.OfType<ParameterResource>();
        var dependentParameters = referencedParameters.Values.Where(p => !explicitParameters.Contains(p));
        return explicitParameters.Concat(dependentParameters).ToList();
    }

    private static void TryAddDependentParameters(
        object? value,
        Dictionary<string, ParameterResource> referencedParameters,
        HashSet<object?> currentDependencySet)
    {
        if (value is ParameterResource parameter)
        {
            referencedParameters.TryAdd(parameter.Name, parameter);
        }
        else if (value is IValueWithReferences objectWithReferences)
        {
            currentDependencySet.Add(value);
            foreach (var dependency in objectWithReferences.References)
            {
                if (!currentDependencySet.Contains(dependency))
                {
                    TryAddDependentParameters(dependency, referencedParameters, currentDependencySet);
                }
            }
            currentDependencySet.Remove(value);
        }
    }
}
