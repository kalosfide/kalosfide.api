using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public class JwtEventsHandlers : JwtBearerEvents
    {

        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        public new Func<AuthenticationFailedContext, Task> OnAuthenticationFailed { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        public new Func<MessageReceivedContext, Task> OnMessageReceived { get; set; } = context => Task.CompletedTask;

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        public new Func<TokenValidatedContext, Task> OnTokenValidated { get; set; } = context => Task.CompletedTask;


        /// <summary>
        /// Invoked before a challenge is sent back to the caller.
        /// </summary>
        public new Func<JwtBearerChallengeContext, Task> OnChallenge { get; set; } = context =>
        Task.CompletedTask;

    }
}
