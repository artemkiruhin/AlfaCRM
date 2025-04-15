using AlfaCRM.Api.Contracts.Response;
using AlfaCRM.Api.Extensions;
using AlfaCRM.Api.Hubs;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserValidator _userValidator;
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(IUserService userService, IUserValidator userValidator, IChatService chatService, IMessageService messageService, IHubContext<ChatHub> hubContext)
        {
            _userService = userService;
            _userValidator = userValidator;
            _chatService = chatService;
            _messageService = messageService;
            _hub = hubContext;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAllChats(CancellationToken ct)
        {
            var userId = await _userValidator.GetUserId(User, ct);
            if (!userId.IsSuccess) return Unauthorized();

            var userChats = await _chatService.GetByUserAsync(userId.Data, ct);
            return Ok(new {data = userChats});
        }

        [HttpGet("id/{id:guid}")]
        public async Task<IActionResult> GetChatById(Guid id, CancellationToken ct)
        {
            var chat = await _chatService.GetById(id, ct);
            return Ok(new {data = chat});
        }

        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat([FromBody] ChatCreateRequest request, CancellationToken ct)
        {
            var result = await _chatService.Create(request, ct);
            if (!result.IsSuccess) return BadRequest();
            
            return Ok(new {data = result.Data});
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] MessageCreateRequest request, CancellationToken ct)
        {
            var userId = await _userValidator.GetUserId(User, ct);
            if (!userId.IsSuccess) return Unauthorized();
            
            var chat = await _chatService.GetById(request.ChatId, ct);
            if (!chat.IsSuccess) return BadRequest();
            
            var result = await _messageService.Create(request, ct);
            if (!result.IsSuccess) return BadRequest();
            
            var message = await _messageService.GetById(result.Data, ct);

            await _hub.Clients.Group(chat.Data.Name).SendAsync("ReceiveMessage", new MessageResponse(
                Id: message.Data.Id,
                Content: message.Data.Content,
                Timestamp: message.Data.CreatedAt,
                Username: message.Data.Sender.Username,
                IsOwn: message.Data.Sender.Id == userId.Data
            ));
            
            return Ok(new {data = result.Data});
        }
        
        [HttpGet("messages")]
        public async Task<IActionResult> GetAllMessages(Guid chatId, CancellationToken ct)
        {
            var userId = await _userValidator.GetUserId(User, ct);
            if (!userId.IsSuccess) return Unauthorized();
            
            var chat = await _chatService.GetById(chatId, ct);
            if (!chat.IsSuccess) return BadRequest();
            
            var result = await _messageService.GetAll(chatId, ct);
            if (!result.IsSuccess) return BadRequest();
            
            return Ok(new {data = result.Data});
        }
    }
}
