﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.GenericTaxes
{
    using System.Reflection;
    using global::Plugin.Sample.GenericTaxes.Pipelines.Blocks;
    using Microsoft.Extensions.DependencyInjection;
    using Plugin.Sample.GenericTaxes.Pipelines;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
              .ConfigurePipeline<IRunningPluginsPipeline>(
                configure =>
                {
                    configure.Add<TaxRegisteredPluginBlock>();
                })
              .AddPipeline<ICreateComposerTemplatesPipeline, CreateComposerTemplatesPipeline>(
                configure =>
                {
                    configure.Add<CreateGenericTaxesComposerTemplatesBlock>();
                })
              .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>()));

            services.RegisterAllCommands(assembly);
        }
    }
}