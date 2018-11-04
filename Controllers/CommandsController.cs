using System;
using System.Threading.Tasks;
using System.Web.Http.OData;
using Microsoft.AspNetCore.Mvc;
using Plugin.Sample.GenericTaxes.Commands;
using Sitecore.Commerce.Core;

namespace Plugin.Sample.GenericTaxes.Controllers
{
    public class CommandsController : CommerceController
    {
        public CommandsController(IServiceProvider serviceProvider, CommerceEnvironment globalEnvironment)
            : base(serviceProvider, globalEnvironment)
        {
        }

        [HttpPut]
        [Route("CreateGenericTaxesComposerView()")]
        public async Task<IActionResult> CreateGenericTaxesComposerView([FromBody] ODataActionParameters value)
        {

            var command = this.Command<CreateComposerTemplatesCommand>();
            var result = await command.Process(this.CurrentContext, "Placeholder");

            return new ObjectResult(command);
        }
    }
}