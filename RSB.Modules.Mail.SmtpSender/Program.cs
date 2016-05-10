using System;
using NLog;
using RSB.Transports.RabbitMQ.Settings;
using Topshelf;

namespace RSB.Modules.Mail.SmtpSender
{
    class Program
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.SetServiceName("RSB.Modules.MailSender");
                x.SetDisplayName("RSB.Modules.MailSender");
                x.SetDescription("This service mail sending features for RSB.");

                x.StartAutomatically();

                x.UseNLog();

                x.Service<MailSenderService>(service =>
                {
                    service.ConstructUsing(srv => InitializeApiService());

                    service.WhenStarted(srv => srv.Start());
                    service.WhenStopped(srv => srv.Stop());
                });
            });
        }

        private static MailSenderService InitializeApiService()
        {
            Logger.Info("Starting RSB.Modules.MailSender service...");

            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var smtpSettings = new SmtpSettings()
            {
                Hostname = Properties.Settings.Default.Hostname,
                Password = Properties.Settings.Default.Password,
                Port = Properties.Settings.Default.Port,
                UseSsl = Properties.Settings.Default.UseSsl,
                Username = Properties.Settings.Default.Username
            };

            var rabbitMqTransportSettings = RabbitMqTransportSettings.FromConfigurationFile();

            return new MailSenderService(smtpSettings, rabbitMqTransportSettings, Properties.Settings.Default.InstanceName);
        }
    }
}