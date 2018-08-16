﻿using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
{
    [PipelineDisplayName("GenericTaxes.Block.TaxRegisteredPluginBlock")]
    public class TaxRegisteredPluginBlock : PipelineBlock<IEnumerable<RegisteredPluginModel>, IEnumerable<RegisteredPluginModel>, CommercePipelineExecutionContext>
    {
        public override Task<IEnumerable<RegisteredPluginModel>> Run(IEnumerable<RegisteredPluginModel> arg, CommercePipelineExecutionContext context)
        {
            if (arg == null)
            {
                return Task.FromResult(arg);
            }

            List<RegisteredPluginModel> list = arg.ToList();
            PluginHelper.RegisterPlugin(this, list);
            return Task.FromResult(list.AsEnumerable());
        }
    }
}
