﻿using Microsoft.Extensions.DependencyInjection;
using Moq;
using Stratis.Bitcoin.Builder.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Stratis.Bitcoin.Tests.Builder.Feature
{
    public class FeatureRegistrationTest
    {
		[Fact]
		public void FeatureServicesAddServiceCollectionToDelegates()
		{
			var collection = new ServiceCollection();			
			var registration = new FeatureRegistration<FeatureRegistrationFullNodeFeature>();
			
			registration.FeatureServices(d => { d.AddSingleton<FeatureCollection>(); });

			Assert.Equal(typeof(FeatureRegistrationFullNodeFeature), registration.FeatureType);
			Assert.Equal(1, registration.ConfigureServicesDelegates.Count);
			registration.ConfigureServicesDelegates[0].Invoke(collection);
			var descriptors = collection as IList<ServiceDescriptor>;
			Assert.Equal(1, descriptors.Count);
			Assert.Equal(typeof(FeatureCollection), descriptors[0].ImplementationType);
			Assert.Equal(ServiceLifetime.Singleton, descriptors[0].Lifetime);
		}

		[Fact]
		public void UseStartupSetsFeatureStartupType()
		{			
			var registration = new FeatureRegistration<FeatureRegistrationFullNodeFeature>();
			Assert.Equal(null, registration.FeatureStartupType);

			registration.UseStartup<ServiceCollection>();

			Assert.Equal(typeof(ServiceCollection), registration.FeatureStartupType);		
		}

		[Fact]
		public void BuildFeatureWithoutFeatureStartupTypeBootstrapsStartup()
		{
			var collection = new ServiceCollection();
			var registration = new FeatureRegistration<FeatureRegistrationFullNodeFeature>();
			registration.FeatureServices(d => { d.AddSingleton<FeatureCollection>(); });

			registration.BuildFeature(collection);

			var descriptors = collection as IList<ServiceDescriptor>;
			Assert.Equal(3, descriptors.Count);			
			Assert.Equal(typeof(FeatureRegistrationFullNodeFeature), descriptors[0].ImplementationType);
			Assert.Equal(ServiceLifetime.Singleton, descriptors[0].Lifetime);
			Assert.Equal(typeof(IFullNodeFeature), descriptors[1].ServiceType);
			Assert.NotNull(descriptors[1].ImplementationFactory);
			Assert.Equal(ServiceLifetime.Singleton, descriptors[1].Lifetime);
			Assert.Equal(typeof(FeatureCollection), descriptors[2].ImplementationType);
			Assert.Equal(ServiceLifetime.Singleton, descriptors[2].Lifetime);
		}

		[Fact]
		public void BuildFeatureWithFeatureStartupTypeBootstrapsStartupAndInvokesStartupWithCollection()
		{						
			var collection = new ServiceCollection();
			var registration = new FeatureRegistration<FeatureRegistrationFullNodeFeature>();
			registration.FeatureServices(d => { d.AddSingleton<FeatureCollection>(); });
			registration.UseStartup<FeatureStartup>();

			registration.BuildFeature(collection);
		}

		[Fact]
		public void BuildFeatureWithFeatureStartupNotHavingStaticConfigureServicesMethodDoesNotCrash()
		{
			var collection = new ServiceCollection();
			var registration = new FeatureRegistration<FeatureRegistrationFullNodeFeature>();
			registration.FeatureServices(d => { d.AddSingleton<FeatureCollection>(); });
			registration.UseStartup<FeatureNonStaticStartup>();

			registration.BuildFeature(collection);
		}

		private class FeatureNonStaticStartup
		{
			public void ConfigureServices(IServiceCollection services)
			{			
			}
		}

		private class FeatureStartup
		{
			public static void ConfigureServices(IServiceCollection services)
			{
				var descriptors = services as IList<ServiceDescriptor>;
				Assert.Equal(3, descriptors.Count);
				Assert.Equal(typeof(FeatureRegistrationFullNodeFeature), descriptors[0].ImplementationType);
				Assert.Equal(ServiceLifetime.Singleton, descriptors[0].Lifetime);
				Assert.Equal(typeof(IFullNodeFeature), descriptors[1].ServiceType);
				Assert.NotNull(descriptors[1].ImplementationFactory);
				Assert.Equal(ServiceLifetime.Singleton, descriptors[1].Lifetime);
				Assert.Equal(typeof(FeatureCollection), descriptors[2].ImplementationType);
				Assert.Equal(ServiceLifetime.Singleton, descriptors[2].Lifetime);
			}
		}

		private class FeatureRegistrationFullNodeFeature : IFullNodeFeature
		{
			public void Start()
			{
				throw new NotImplementedException();
			}

			public void Stop()
			{
				throw new NotImplementedException();
			}
		}
	}
}

