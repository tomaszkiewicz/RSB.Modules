﻿using System.Collections.Generic;

namespace RSB.Modules.Mail.Contracts
{
    public class SendMailMessage
    {
        public string FromMail { get; set; }
        public string FromName { get; set; }
        public List<Recipient> Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}