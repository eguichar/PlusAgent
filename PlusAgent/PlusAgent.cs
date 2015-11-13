/*
 * A Simple Exchange Transport Protocol agent that implements plus email addressing
 * 
 * James DeVincentis 
 * https://github.com/jmdevince/
 * 
 */

using System;
using Microsoft.Exchange.Data.Transport;
using Microsoft.Exchange.Data.Transport.Smtp;

namespace PlusAgent
{
    public sealed class PlusFactory : SmtpReceiveAgentFactory
    {
        public override SmtpReceiveAgent CreateAgent(SmtpServer server)
        {
            return new PlusAgent(server.AddressBook);
        }
    }

    public class PlusAgent : SmtpReceiveAgent
    {

        private readonly AddressBook _addressBook;
        
        public PlusAgent(AddressBook addressBook)
        {
            _addressBook = addressBook;
            OnRcptCommand += RcptToHandler;
        }

        public void RcptToHandler(ReceiveCommandEventSource source, RcptCommandEventArgs rcptArgs)
        {
            // Check to see if this is an internal message
            if (!rcptArgs.SmtpSession.IsExternalConnection)
            {
                return;
            }
            
            string[] parts = rcptArgs.RecipientAddress.LocalPart.Split('+');

            if (parts.Length < 1 || (_addressBook == null) || (_addressBook.Find(rcptArgs.RecipientAddress) != null))
            {
                return;
            }

            RoutingAddress emailAddress = new RoutingAddress(String.Join("+", parts, 0, (parts.Length - 1)) + "@" + rcptArgs.RecipientAddress.DomainPart);
            if(!emailAddress.IsValid) {
                return;
            }

            rcptArgs.RecipientAddress = emailAddress;

            return;
        }
    }

}
