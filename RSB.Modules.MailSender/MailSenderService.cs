using MailKit.Net.Smtp;
using MimeKit;
using RSB.Diagnostics;
using RSB.Modules.MailSender.Contracts;
using RSB.Transports.RabbitMQ;
using RSB.Transports.RabbitMQ.Settings;

namespace RSB.Modules.MailSender
{
    internal class MailSenderService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly RabbitMqTransportSettings _rabbitMqSettings;
        private readonly string _instanceName;
        private Bus _bus;

        public MailSenderService(SmtpSettings smtpSettings, RabbitMqTransportSettings rabbitMqSettings, string instanceName)
        {
            _smtpSettings = smtpSettings;
            _rabbitMqSettings = rabbitMqSettings;
            _instanceName = instanceName;
        }

        public void Start()
        {
            _bus = new Bus(new RabbitMqTransport(_rabbitMqSettings));

            _bus.UseBusDiagnostics("MailSender", _instanceName);

            _bus.RegisterQueueHandler<MailMessage>(HandleMailMessage, _instanceName);
        }

        public void Stop()
        {
            _bus.Shutdown();
        }

        public void HandleMailMessage(MailMessage msg)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(msg.FromName, msg.FromMail));
            message.To.Add(new MailboxAddress(msg.ToName, msg.ToMail));
            message.Subject = msg.Subject;

            message.Body = new TextPart("plain")
            {
                Text = msg.Body
            };

            using (var client = new SmtpClient())
            {
                client.Connect(_smtpSettings.Hostname, _smtpSettings.Port, _smtpSettings.UseSsl);

                if (string.IsNullOrWhiteSpace(_smtpSettings.Username))
                {
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
                }

                client.Send(message);

                client.Disconnect(true);
            }
        }
    }
}