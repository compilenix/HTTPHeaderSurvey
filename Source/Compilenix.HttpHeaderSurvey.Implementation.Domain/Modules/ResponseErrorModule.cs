﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Compilenix.HttpHeaderSurvey.Implementation.Shared;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Entitys;
using Compilenix.HttpHeaderSurvey.Integration.DataAccess.Repositories;
using Compilenix.HttpHeaderSurvey.Integration.Domain.Modules;
using JetBrains.Annotations;

namespace Compilenix.HttpHeaderSurvey.Implementation.Domain.Modules
{
    [UsedImplicitly]
    public class ResponseErrorModule : BaseModule<IResponseErrorRepository, ResponseError>, IResponseErrorModule
    {
        [NotNull]
        private readonly IResponseErrorRepository _repository;

        public ResponseErrorModule([NotNull] IResponseErrorRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns true if this was a known error.
        /// </summary>
        // TODO get and match from persistent storage
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public async Task<bool> AddAsync(ResponseMessage messageWithError, Exception error, IUnitOfWork unit)
        {
            var messages = error.GetAllMessages();

            // ReSharper disable once PossibleMultipleEnumeration
            if (!messages.Any())
            {
                return false;
            }

            var errorCodes = unit.Resolve<IErrorCodeRepository>();

            if (messages.Any(m => m.Contains("The remote name could not be resolved")))
            {
                await HandleErrorDefault(message: "The remote server name could not be resolved, this may be a temporary error", errorCode: 0, messages: messages, messageWithError: messageWithError, error: error, errorCodes: errorCodes);
                return true;
            }

            if (messages.Any(m => m.Contains("A connection attempt failed because the connected party did not properly respond after a period of time")))
            {
                await HandleErrorDefault(message: "A connection attempt failed because the remote server did not properly respond after a period of time, this may be a temporary error", errorCode: 1, messages: messages, messageWithError: messageWithError, error: error, errorCodes: errorCodes);
                return true;
            }

            if (messages.Any(m => m.Contains("No connection could be made because the target machine actively refused")))
            {
                await HandleErrorDefault(message: "No connection could be made because the remote server actively refused it, this may be a permanent error", errorCode: 2, messages: messages, messageWithError: messageWithError, error: error, errorCodes: errorCodes);
                return true;
            }

            if (messages.Any(m => m.Contains("was forcibly closed by the remote host")))
            {
                await HandleErrorDefault(message: "The connection was forcibly closed by the remote server, this may be a permanent error", errorCode: 3, messages: messages, messageWithError: messageWithError, error: error, errorCodes: errorCodes);
                return true;
            }

            if (messages.Any(m => m.Contains("The connection was closed unexpectedly")))
            {
                await HandleErrorDefault(message: "The connection was closed unexpectedly, this may be a temporary error", errorCode: 4, messages: messages, messageWithError: messageWithError, error: error, errorCodes: errorCodes);
                return true;
            }

            if (messages.Any(m => m.Contains("Unable to read data from the transport connection")))
            {
                await HandleErrorDefault(message: "Unable to read data from the transport connection, this may be a temporary error", errorCode: 5, messages: messages, messageWithError: messageWithError, error: error, errorCodes: errorCodes);
                return true;
            }

            return false;
        }

        private async Task HandleErrorDefault([NotNull] string message, int errorCode, [ItemNotNull] [NotNull] IEnumerable<string> messages, [NotNull] ResponseMessage messageWithError, [NotNull] Exception error, [NotNull] IErrorCodeRepository errorCodes)
        {
            var addIfNotExistingAsync = errorCodes.AddIfNotExistingAsync(new ErrorCode { Code = errorCode, Message = message });
            _repository.Add(new ResponseError { ErrorCode = await addIfNotExistingAsync, Message = message, OriginalMessage = messages.FirstOrDefault(x => x.Contains(message)), ResponseMessage = messageWithError, StackTrace = error.StackTrace });
        }
    }
}