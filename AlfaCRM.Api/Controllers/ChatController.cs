using System.Globalization;
using AlfaCRM.Api.Contracts.Request;
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
        public async Task<IActionResult> GetChatById(Guid id, [FromQuery] bool byUser, CancellationToken ct)
        {
            var userId = await _userValidator.GetUserId(User, ct);
            if (!userId.IsSuccess) return Unauthorized();

            var chat = byUser
                ? await _chatService.GetById(id, userId.Data, ct)
                : await _chatService.GetById(id, null, ct);
            if (!chat.IsSuccess) return BadRequest($"{chat.ErrorMessage}");
            return Ok(new {data = chat});
        }

        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat([FromBody] ChatCreateRequest request, CancellationToken ct)
        {
            var result = await _chatService.Create(request, ct);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
            
            return Ok(new {data = result.Data});
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] MessageCreateRequest request, CancellationToken ct)
        {
            var userId = await _userValidator.GetUserId(User, ct);
            if (!userId.IsSuccess) return Unauthorized();
            
            var chat = await _chatService.GetById(request.ChatId, userId.Data, ct);
            if (!chat.IsSuccess) return BadRequest();
            
            var result = await _messageService.Create(request, ct);
            if (!result.IsSuccess) return BadRequest();
            
            var message = await _messageService.GetById(result.Data, ct);

            /*var culture = new CultureInfo("ru-RU");
            var d = message.Data.CreatedAt.ToString("dd MMMM yyyy 'в' HH:mm", culture);
            
            Console.WriteLine(d);
            
            await _hub.Clients.Group(chat.Data.Name).SendAsync("ReceiveMessage", new MessageResponse(
                Id: message.Data.Id,
                Content: message.Data.Content,
                CreatedAt: d,
                Username: message.Data.Sender.Username,
                IsOwn: message.Data.Sender.Id == userId.Data
            ), cancellationToken: ct);*/
            
            return Ok(new {data = result.Data});
        }
        
        [HttpGet("messages")]
        public async Task<IActionResult> GetAllMessages(Guid chatId, CancellationToken ct)
        {
            var userId = await _userValidator.GetUserId(User, ct);
            if (!userId.IsSuccess) return Unauthorized();
            
            var chat = await _chatService.GetById(chatId, userId.Data, ct);
            if (!chat.IsSuccess) return BadRequest();
            
            var result = await _messageService.GetAll(chatId, ct);
            if (!result.IsSuccess) return BadRequest();
            
            return Ok(new {data = result.Data});
        }
        
        [HttpPost("pin-message")]
        public async Task<IActionResult> PinMessage([FromBody] PinMessageApiRequest request, CancellationToken ct)
        {
            var userId = await _userValidator.GetUserId(User, ct);
            if (!userId.IsSuccess) return Unauthorized();
    
            var message = await _messageService.GetById(request.MessageId, ct);
            if (!message.IsSuccess) return BadRequest("Message not found");
    
           
            var result = await _messageService.PinMessage(request.MessageId, request.IsPinned, ct);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
    
            return Ok(new {data = result.Data});
        }
    }
}
