using System;
using System.ServiceModel;

namespace SmevWCF
{
    public class Service1 : IService1
    {
        public SampleRequestResponse GetData(SampleRequest sample)
        {
            try
            {
                var simple = new SampleRequestResponse
                             {
                                 Message = new MessageType
                                           {
                                               Date = DateTime.Now,
                                               Status = StatusType.RESULT,
                                               Originator = new orgExternalType
                                                            {
                                                                Code =
                                                                    sample.Message.Originator.Code,
                                                                Name =
                                                                    sample.Message.Originator.Name
                                                            }
                                           }
                             };

                simple.Message.Sender = new orgExternalType
                                        {
                                            Code = sample.Message.Recipient.Code,
                                            Name = sample.Message.Recipient.Name
                                        };
                simple.Message.Recipient = new orgExternalType
                                           {
                                               Code = sample.Message.Sender.Code,
                                               Name = sample.Message.Sender.Name
                                           };

                simple.Message.OriginRequestIdRef = sample.Header.MessageId;
                simple.Message.RequestIdRef = sample.Header.MessageId;
                simple.Message.ExchangeType = "3";

                simple.MessageData = new MessageDataType
                                     {
                                         AppData = new AppDataType
                                                   {
                                                       errorCode = 0,
                                                       errorCodeSpecified = true,
                                                       Responce = new ResponceType
                                                                  {
                                                                      responce =
                                                                          sample.MessageData.AppData
                                                                              .Request.request +
                                                                          " It's Work!"
                                                                  }
                                                   }
                                     };


                return simple;
            }
            catch (Exception e)
            {
                throw new FaultException(e.Message);
            }
            
        }
    }
}
