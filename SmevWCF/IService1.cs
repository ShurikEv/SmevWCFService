using System.Net.Security;
using System.ServiceModel;

namespace SmevWCF
{
    [ServiceContract(Namespace = "http://test", ProtectionLevel = ProtectionLevel.Sign)]
    [XmlSerializerFormat]
    public interface IService1
    {

        [OperationContract]
        [XmlSerializerFormat]
        SampleRequestResponse GetData(SampleRequest sample);
    }

    [MessageContract(WrapperName = "GetData", IsWrapped = true)]
    [XmlSerializerFormat]
    public class SampleRequest
   {
        [MessageHeader(Name = "Header", Namespace = "http://smev.gosuslugi.ru/rev120315")]
        public HeaderType Header;

       [MessageBodyMember(Name = "Message", Namespace = "http://smev.gosuslugi.ru/rev120315")]
       public MessageType Message;

       [MessageBodyMember(Name = "MessageData", Namespace = "http://smev.gosuslugi.ru/rev120315")] 
       public MessageDataType MessageData;
   }

    [MessageContract(WrapperName = "GetDataResponse", IsWrapped = true)]
    [XmlSerializerFormat]
    public class SampleRequestResponse
    {
        [MessageBodyMember(Name = "Message", Namespace = "http://smev.gosuslugi.ru/rev120315")]
        public MessageType Message;

        [MessageBodyMember(Name = "MessageData", Namespace = "http://smev.gosuslugi.ru/rev120315")]
        public MessageDataType MessageData;
    }


}
