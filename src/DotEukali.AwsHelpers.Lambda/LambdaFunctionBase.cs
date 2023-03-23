using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotEukali.AwsHelpers.Lambda
{
    public abstract class LambdaFunctionBase<TInput, TOutput>
    {
        private readonly IServiceProvider _serviceProvider;
        private IServiceCollection? _serviceCollection;

        protected LambdaFunctionBase(Action<IServiceCollection>? serviceCollection)
        {
            serviceCollection?.Invoke(BuildServiceCollection());
            _serviceProvider = BuildServiceProvider();
        }

        // endpoint to be called for the lambda function
        public async Task<TOutput> FunctionHandler(TInput input, ILambdaContext context)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            return await ExecuteFunction(input, context, scope.ServiceProvider);
        }

        /// <summary>
        /// When registering the lambda execution, use FunctionHandler
        /// </summary>
        protected abstract Task<TOutput> ExecuteFunction(TInput request, ILambdaContext context, IServiceProvider serviceProvider);

        private IServiceProvider BuildServiceProvider() => BuildServiceCollection().BuildServiceProvider();

        private IServiceCollection BuildServiceCollection() => _serviceCollection ??= GetServices();

        private IServiceCollection GetServices()
        {
            IConfiguration configuration = BuildConfig();

            IServiceCollection serviceCollection = new ServiceCollection();

            RegisterServices(serviceCollection, configuration);

            return serviceCollection;
        }

        protected abstract void RegisterServices(IServiceCollection services, IConfiguration configuration);

        protected virtual IConfiguration BuildConfig() =>
            new ConfigurationBuilder()
                .AddJsonFile(GetAppSettingsFilename(), true, true)
                .Build();

        private static string GetAppSettingsFilename()
        {
            string? environment = Environment.GetEnvironmentVariable("Environment");

            return !string.IsNullOrWhiteSpace(environment) 
                ? $"appsettings.{environment}.json" 
                : "appsettings.json";
        }
    }
}
