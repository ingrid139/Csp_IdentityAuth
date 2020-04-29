﻿using IdentityAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace IdentityAuth.Services
{
    public class EmailServices : IEmailServices
    {
        private SendGridOptions _sendGridOptions { get; }

        public EmailServices(IOptions<SendGridOptions> sendGridOptions)
        {
            _sendGridOptions = sendGridOptions.Value;
        }

        public async Task<EmailResponse> SendEmailBySendGridAsync(string email, string assunto, string mensagem)
        {
            try
            {
                // buscar SendGrid key
                var client = new SendGridClient(_sendGridOptions.SendGridKey);

                // obj de email
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(_sendGridOptions.FromEmail, _sendGridOptions.FromFullName),
                    Subject = assunto,
                    PlainTextContent = mensagem,
                    HtmlContent = mensagem
                };

                // email do usuario
                msg.AddTo(new EmailAddress("ingrid139@gmail.com", email));

                // envio do email
                var responseSend = await client.SendEmailAsync(msg);

                // obj retorno
                var retorno = new EmailResponse();
                retorno.Enviado = true;

                // verificação de envio
                if (!responseSend.StatusCode.Equals(System.Net.HttpStatusCode.Accepted))
                {
                    retorno.Enviado = false;
                    retorno.error = ErrorResponse.FromEmail(responseSend);
                }

                return retorno;
            }
            catch (Exception ex)
            {
                return new EmailResponse()
                {
                    Enviado = false,
                    error = ErrorResponse.From(ex)
                };
            }
        }
    }
}
