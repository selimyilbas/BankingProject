// BankingApp.API/Controllers/TransferController.cs
using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Transfer;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransferController : ControllerBase
    {
        private readonly ITransferService _transferService;

        public TransferController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TransferDto>>> CreateTransfer([FromBody] CreateTransferDto dto)
        {
            var result = await _transferService.CreateTransferAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{transferId}")]
        public async Task<ActionResult<ApiResponse<TransferDto>>> GetTransferById(int transferId)
        {
            var result = await _transferService.GetTransferByIdAsync(transferId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<ApiResponse<List<TransferDto>>>> GetTransfersByAccountId(int accountId)
        {
            var result = await _transferService.GetTransfersByAccountIdAsync(accountId);
            return Ok(result);
        }

        [HttpGet("account/{accountId}/paged")]
        public async Task<ActionResult<ApiResponse<PagedResult<TransferDto>>>> GetTransfersPaged(
            int accountId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _transferService.GetTransfersPagedAsync(accountId, pageNumber, pageSize);
            return Ok(result);
        }
    }
}