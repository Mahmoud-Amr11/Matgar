using Matgar.Api.Common;
using Matgar.Application.Features.Notifications.Commands.MarkRead;
using Matgar.Application.Features.Notifications.Queries.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetNotificationsQuery(), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{id}/mark-read")]
        public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
            return result.ToActionResult();
        }
    }
}
