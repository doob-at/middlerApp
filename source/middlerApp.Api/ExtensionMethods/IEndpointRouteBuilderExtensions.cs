using System;
using System.Linq;
using doob.Reflectensions.ExtensionMethods;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace middlerApp.Api.ExtensionMethods
{
    public static class IEndpointRouteBuilderExtensions
    {
        private static EndpointDataSource _endpointDataSource;

        public static void MapControllersWithAttribute<T>(this IEndpointRouteBuilder endpoints) where T: Attribute
        {


            var dataSource = endpoints.GetEndpointDatasource();

            if (dataSource != null)
            {

                var filteredEndpoints = dataSource.Endpoints.Where(e => e.Metadata.Any(m => m.GetType().Equals<T>()));
                var d = new DefaultEndpointDataSource(filteredEndpoints);
                endpoints.DataSources.Add(d);
            }

        }

        public static EndpointDataSource GetEndpointDatasource(this IEndpointRouteBuilder endpoints)
        {

            if (_endpointDataSource == null)
            {
                var assembly = typeof(Microsoft.AspNetCore.Mvc.Routing.DynamicRouteValueTransformer).Assembly;

                var orderProviderType = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals("OrderedEndpointsSequenceProviderCache"));
                var orderProvider = endpoints.ServiceProvider.GetRequiredService(orderProviderType);
                var orderedEndpointsSequenceProvider = orderProviderType.GetMethod("GetOrCreateOrderedEndpointsSequenceProvider").Invoke(orderProvider, new[] { endpoints });

                var factoryType = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals("ControllerActionEndpointDataSourceFactory"));
                var factory = endpoints.ServiceProvider.GetRequiredService(factoryType);
                _endpointDataSource = (EndpointDataSource)factoryType.GetMethod("Create").Invoke(factory, new[] { orderedEndpointsSequenceProvider });
            }

            return _endpointDataSource;

        }
    }
}
