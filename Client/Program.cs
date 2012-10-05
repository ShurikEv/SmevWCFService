using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Client.Service;


namespace Client
{
    static class Program
    {
        static void Main()
        {
            var client = new Service1Client();

            const string clientThumbprint = "e1 57 ef d7 49 5a 3e 44 b9 07 e4 f6 a1 50 ea 76 76 24 20 e0";
            const string serviceThumbprint = "e1 57 ef d7 49 5a 3e 44 b9 07 e4 f6 a1 50 ea 76 76 24 20 e0";

            var sample = new SampleRequest
                                   {
                                       Header = new HeaderType
                                                {
                                                    MessageClass = MessageClassType.REQUEST,
                                                    TimeStamp = DateTime.Now,
                                                    MessageId = Guid.NewGuid().ToString(),
                                                    actor =
                                                        "http://smev.gosuslugi.ru/actors/recipient"
                                                },
                                       Message = new MessageType
                                                 {
                                                     Date = DateTime.Now,
                                                     Status = StatusType.REQUEST,
                                                     Originator = new orgExternalType
                                                                  {
                                                                      Code = "6666",
                                                                      Name = "FMS"
                                                                  },
                                                     Sender = new orgExternalType
                                                              {
                                                                  Code = "5555",
                                                                  Name = "MVD"
                                                              },
                                                     Recipient = new orgExternalType
                                                                 {
                                                                     Code = "3654",
                                                                     Name = "XXX"
                                                                 },
                                                     ExchangeType = "3"
                                                 },
                                       MessageData = new MessageDataType
                                                     {
                                                         AppData = new AppDataType
                                                                   {
                                                                       Request = new RequestType
                                                                                 {
                                                                                     request = "123"
                                                                                 }
                                                                   }
                                                     }
                                   };

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            // Клиентский сертификат
            var coll = store.Certificates.Find(X509FindType.FindByThumbprint, clientThumbprint, true);

            if (coll.Count == 0)
            {
                throw new FileNotFoundException(string.Format("Сертификат клиент не найден. Отпечаток {0}", clientThumbprint));
            }

            var clientCert = coll[0];

            coll = store.Certificates.Find(X509FindType.FindByThumbprint, serviceThumbprint, true);

            if (coll.Count == 0)
            {
                throw new FileNotFoundException(string.Format("Сертификат сервера не найден. Отпечаток {0}", clientThumbprint));
            }

            // Сервисный сертификат
            var serviceCert = coll[0];

            var serverName = serviceCert.GetNameInfo(X509NameType.SimpleName, false);

            if (serverName != null)
            {
                var endpointAddr = new EndpointAddress(new Uri("http://localhost:1622/Service1.svc"),
                                                         EndpointIdentity.CreateDnsIdentity(
                                                             serverName));
                client.Endpoint.Address = endpointAddr;
            }
            var binding = new CustomBinding(client.Endpoint.Binding);
            var textBindingElement = new Binding.SMEVMessageEncodingBindingElement();
            binding.Elements.Remove<TextMessageEncodingBindingElement>();
            binding.Elements.Insert(0, textBindingElement);

            // Не ищем метку времени в сообщениях от сервиса
            binding.Elements.Find<AsymmetricSecurityBindingElement>().LocalClientSettings.DetectReplays = false;

            // Не вставляем метку времени в заголовок Security
            binding.Elements.Find<AsymmetricSecurityBindingElement>().IncludeTimestamp = false;

            // Устанавливаем модифицированную привязку.
            client.Endpoint.Binding = binding;

            // Требуется только подпись сообщения.
            client.ChannelFactory.Endpoint.Contract.ProtectionLevel = System.Net.Security.ProtectionLevel.Sign;

            if (client.ClientCredentials != null)
            {
                client.ClientCredentials.ClientCertificate.Certificate = clientCert;

                client.ClientCredentials.ServiceCertificate.DefaultCertificate = serviceCert;

                client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
                client.ClientCredentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            }

            client.GetData(sample.Header, ref sample.Message, ref sample.MessageData);

            Console.WriteLine(sample.MessageData.AppData.Responce.responce);
            Console.WriteLine(sample.Message.Date.ToString());
            Console.WriteLine(sample.Message.Status);
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
