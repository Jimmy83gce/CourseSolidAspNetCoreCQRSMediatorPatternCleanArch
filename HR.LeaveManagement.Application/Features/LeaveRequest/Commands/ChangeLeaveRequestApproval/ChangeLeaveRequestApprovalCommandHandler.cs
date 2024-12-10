using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Email;
using HR.LeaveManagement.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval
{
    public class ChangeLeaveRequestApprovalCommandHandler
        : IRequestHandler<ChangeLeaveRequestApprovalCommand, Unit>
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IAppLogger<ChangeLeaveRequestApprovalCommandHandler> _appLogger;

        public ChangeLeaveRequestApprovalCommandHandler(
            ILeaveRequestRepository leaveRequestRepository,
            ILeaveTypeRepository leaveTypeRepository,
            IMapper mapper,
            IEmailSender emailSender,
            IAppLogger<ChangeLeaveRequestApprovalCommandHandler> appLogger)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _mapper = mapper;
            _emailSender = emailSender;
            _appLogger = appLogger;
        }

        public async Task<Unit> Handle(ChangeLeaveRequestApprovalCommand request, CancellationToken cancellationToken)
        {
            var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

            if (leaveRequest is null)
                throw new NotFoundException(nameof(LeaveRequest), request.Id);

            leaveRequest.Approved = request.Approved;
            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            // if equest is approved, get and update employee's allocations

            try
            {
                // send confirmation email
                var email = new EmailMessage
                {
                    To = string.Empty, /* Get email from employee record */
                    Body = $"The approval status for your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D} has been updated.",
                    Subject = "Leave Request Approval Status Updated"
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
