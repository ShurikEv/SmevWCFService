using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace CustomErrorHandler
{
    public class CustomErrorHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message msg)
        {
            var faultException = new FaultException(error.Message);
            var messageFault = faultException.CreateMessageFault();
            msg = Message.CreateMessage(version, messageFault, faultException.Action);
        }
    }
}
