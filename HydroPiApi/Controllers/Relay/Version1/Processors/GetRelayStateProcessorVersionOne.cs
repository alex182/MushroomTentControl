﻿using HydroPiApi.Controllers.Common.Processor;
using HydroPiApi.Controllers.Relay.Version1.Processors.Request;
using HydroPiApi.Controllers.Relay.Version1.Processors.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RelayClient;
using System;
using System.Linq;

namespace HydroPiApi.Controllers.Relay.Version1.Processors
{
    public class GetRelayStateProcessorVersionOne : BaseProcessor<GetRelayStateProcessorRequestVersionOne>
    {
        private readonly ILogger _logger;
        private readonly IRelayClient _relayClient;

        public GetRelayStateProcessorVersionOne(
            GetRelayStateProcessorRequestVersionOne record, 
            ILoggerFactory loggerFactory,
            IRelayClient relayClient) : base(record, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetRelayStateProcessorVersionOne>();
            _relayClient = relayClient;
        }

        public override IValidationResult IsValid(GetRelayStateProcessorRequestVersionOne record)
        {
            var existingRelays = _relayClient.GetRelays();

            if (existingRelays.FirstOrDefault(r => r.GpioPin == record?.GpioPin) != null)
            {
                return null;
            }

            return new ValidationResult
            {
                Message = $"Could not find a relay with the gpio pin: {record.GpioPin}",
                StatusCode = System.Net.HttpStatusCode.NotFound
            }; 
        }

        public override IActionResult ProcessRequest(GetRelayStateProcessorRequestVersionOne record)
        {
            var relayState = _relayClient.GetRelayState(record.GpioPin);

            return new ObjectResult(new GetRelayStateProcessorResponseVersionOne
            {
                IsSuccess = true,
                State = relayState.Value,
                StatusCode = System.Net.HttpStatusCode.OK
            });;
        }
    }
}
