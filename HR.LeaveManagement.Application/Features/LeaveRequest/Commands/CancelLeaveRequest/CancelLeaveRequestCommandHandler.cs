using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest
{
    public class CancelLeaveRequestCommandHandler
        : IRequestHandler<CancelLeaveRequestCommand>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IEmailSender _emailSender;
        private readonly IAppLogger<CancelLeaveRequestCommandHandler> _appLogger;

        public CancelLeaveRequestCommandHandler(
            ILeaveRequestRepository leaveRequestRepository,
            IEmailSender emailSender,
            IAppLogger<CancelLeaveRequestCommandHandler> appLogger)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _emailSender = emailSender;
            _appLogger = appLogger;
        }

        public async Task<Unit> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if(leaveRequest is null)
                throw new NotFoundException(nameof(leaveRequest), request.Id);

            leaveRequest.Cancelled = true;

            // if already approved, re-evaluate the employee's allocations for the leave type

            try
            {
                // send confirmation email
                var email = new EmailMessage
                {
                    To = string.Empty, /* Get email from employee record */
                    Body = $"Your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D} has been cancelled successfully.",
                    Subject = "Leaeve Request Cancelled"
                };

                await _emailSender.SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                _appLogger.LogWarning(ex.Message);
            }
            
            return Unit.Value;
        }
    }
}
