using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using doob.middler;
using doob.middler.Action.Scripting;
using doob.middler.Common.SharedModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.API.ExtensionMethods;
using middlerApp.API.TsDefinitions;
using middlerApp.DataAccess;
using middlerApp.DataAccess.Entities.Models;
using middlerApp.SharedModels;


namespace middlerApp.API.Controllers.Admin
{
    [ApiController]
    [Route("_api/endpoint-rules")]
    [AdminController]
    [Authorize(Policy = "Admin")]
    public class EndpointRulesController: Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMapper _mapper;
        private readonly InternalHelper _internalHelper;
        private readonly EndpointRuleRepository _endpointRuleRepository;

        public EndpointRulesController(IServiceProvider serviceProvider, IMapper mapper, InternalHelper internalHelper, EndpointRuleRepository endpointRuleRepository)
        {
            _serviceProvider = serviceProvider;
            _mapper = mapper;
            _internalHelper = internalHelper;
            _endpointRuleRepository = endpointRuleRepository;

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<EndpointRuleListDto>>> GetAll(bool actionDetails = false)
        {
            var rules = await _endpointRuleRepository.GetAllAsync();
            if (actionDetails)
            {
                return Ok(rules.Select(r => r.ToEndpointRuleDto(_internalHelper)));
            }
            return Ok(_mapper.Map<List<EndpointRuleListDto>>(rules));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EndpointRuleDto>> Get(Guid id)
        {
            var rule = await _endpointRuleRepository.Find(id);
            var dto = rule.ToEndpointRuleDto(_internalHelper);
            return Ok(dto);
        }


        [HttpPost]
        public async Task<ActionResult> Add([FromBody] EndpointRuleDto rule)
        {


            var entity = _mapper.Map<EndpointRuleEntity>(rule);
            foreach (var endpointActionEntity in entity.Actions)
            {
                UpdateAction(endpointActionEntity);
            }
            await _endpointRuleRepository.AddAsync(entity);

            //var dbModel = _mapper.Map<MiddlerRuleDbModel>(rule);
            //UpdateActions(dbModel);
            //await Repo.AddAsync(dbModel);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _endpointRuleRepository.RemoveAsync(id);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EndpointRuleDto>> Update(Guid id, [FromBody] EndpointRuleDto ruleDto)
        {
            var ruleInDb = await _endpointRuleRepository.Find(id);
             _mapper.Map(ruleDto, ruleInDb);
            foreach (var endpointActionEntity in ruleInDb.Actions)
            {
                endpointActionEntity.EndpointRuleEntityId = id;
                UpdateAction(endpointActionEntity);
            }
            
            
            await _endpointRuleRepository.UpdateAsync(ruleInDb);

            var updated = await _endpointRuleRepository.GetByIdAsync(id);
            return Ok(updated.ToEndpointRuleDto(_internalHelper));
        }

        [HttpPut("{id}/enabled/{enabled:bool}")]
        public async Task<ActionResult<EndpointRuleDto>> Enabled(Guid id, bool enabled)
        {
            var ruleInDb = await _endpointRuleRepository.Find(id);
            ruleInDb.Enabled = enabled;
           
            await _endpointRuleRepository.UpdateAsync(ruleInDb);

            var updated = await _endpointRuleRepository.GetByIdAsync(id);
            return Ok(updated.ToEndpointRuleDto(_internalHelper));
        }

        //[HttpPatch("{id}")]
        //public async Task<ActionResult<EndpointRuleEntity>> PartialUpdate(Guid id, [FromBody]JsonPatchDocument<EndpointRuleDto> patchDocument) {

        //    var ruleInDb = await _endpointRuleRepository.Find(id);

        //    var updDto = _mapper.Map<EndpointRuleDto>(ruleInDb);

        //    patchDocument.ApplyTo(updDto, ModelState);

        //    _mapper.Map(updDto, ruleInDb);
        //    //UpdateActions(ruleInDb);
        //    await _endpointRuleRepository.UpdateAsync(ruleInDb);
        //    var updated = await _endpointRuleRepository.GetByIdAsync(id);
        //    return Ok(updated);
        //}

        [HttpPost("order")]
        public async Task<IActionResult> UpdateRulesOrder([FromBody] Dictionary<Guid, decimal> order)
        {

            await _endpointRuleRepository.UpdateRulesOrder(order);
            return Ok();
        }

        private void UpdateAction(EndpointActionEntity actionEntity)
        {
            var mAction = _mapper.Map<MiddlerAction>(actionEntity);
            var action = _internalHelper.BuildConcreteActionInstance(mAction);
            if (action == null)
            {
                return;
            }

            if (action is ScriptingAction scriptAction)
            {
                actionEntity.Parameters["CompiledCode"] = scriptAction?.CompileScriptIfNeeded();
            }

        }


        [HttpGet("import-definitions")]
        public IActionResult GetImportDefinitions([FromServices] TsDefinitionService tsDefinitionService)
        {
            var list = tsDefinitionService.GetTsImports().ToList();
            return Ok(list);
        }

        [HttpGet("type-definitions")]
        public IActionResult GetTypeDefinitions([FromServices]TsDefinitionService tsDefinitionService)
        {

            var list = tsDefinitionService.GetTsDefinitions().ToList();
            return Ok(list);
        }
    }
}
