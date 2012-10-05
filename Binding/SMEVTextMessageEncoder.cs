using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Binding
{
    /// <summary>
    /// Кастомный кодировщик. Обертка над стандартным WCF кодировщиком.
    /// 
    /// </summary>
    public class SMEVTextMessageEncoder : MessageEncoder
    {
        private const string SMEVActor = "http://smev.gosuslugi.ru/actors/smev";
        private const string Soap11Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        private SMEVTextMessageEncoderFactory factory;
        private string contentType;
        private MessageEncoder innerEncoder;

        public override string ContentType
        {
            get
            {
                return contentType;
            }
        }

        public override string MediaType
        {
            get
            {
                return factory.MediaType;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return factory.MessageVersion;
            }
        }

        public SMEVTextMessageEncoder(SMEVTextMessageEncoderFactory factory)
        {
            this.factory = factory;
            innerEncoder = factory.InnerMessageFactory.Encoder;
            contentType = this.factory.MediaType;
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            var bytes = new byte[buffer.Count];
            Array.Copy(buffer.Array, buffer.Offset, bytes, 0, bytes.Length);
            var message = new UTF8Encoding(true).GetString(bytes);
            // Сохраняем ответ в файл содержащий actor
            File.WriteAllText("D:\\TrueClientResponseActor.xml", message);
            var m = message.Replace("s:actor=\"" + SMEVActor + "\"", "");
            var bytes2 = new UTF8Encoding().GetBytes(m);
            // Сохраняем ответ в файл уже без actor
            File.WriteAllBytes("D:\\TrueClientResponse.xml", bytes2);
            var length = bytes2.Length;
            const int zero = 0;
            var num = length + zero;
            var array = bufferManager.TakeBuffer(num);
            Array.Copy(bytes2, 0, array, zero, num);
            buffer = new ArraySegment<byte>(array, zero, num);
            return innerEncoder.ReadMessage(buffer, bufferManager, contentType);
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            return innerEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            
            var array = innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset).Array;
            var messageText = Encoding.UTF8.GetString(array);
            var xmlDocument = new XmlDocument();
            // Сохраняем запрос без actor
            File.WriteAllText("D:\\TrueClientRequest.xml", messageText);
            xmlDocument.LoadXml(messageText);
            var prefixOfNamespace = xmlDocument.FirstChild.GetPrefixOfNamespace(Soap11Namespace);
            if (string.IsNullOrEmpty(prefixOfNamespace))
                throw new XmlException(string.Format("Не найден префикс пространста имен {0}", Soap11Namespace));
            var elementsByTagName = xmlDocument.GetElementsByTagName("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            if (elementsByTagName.Count == 0)
                throw new NullReferenceException("Не найден подпись под документом.");
            var attribute = xmlDocument.CreateAttribute(prefixOfNamespace + ":actor", Soap11Namespace);
            attribute.Value = SMEVActor;
            var xmlAttributeCollection = elementsByTagName[0].Attributes;
            if (xmlAttributeCollection != null)
            {
                xmlAttributeCollection.Append(attribute);
            }
            var memoryStream = new MemoryStream();
            var w = XmlWriter.Create(memoryStream);
            xmlDocument.Save(w);
            // Сохраняем запрос с actor
            File.WriteAllText("D:\\TrueClientRequestActor.xml", xmlDocument.InnerXml);
            w.Flush();
            var buffer = memoryStream.GetBuffer();
            var num = (int)memoryStream.Position;
            memoryStream.Close();
            var bufferSize = num + messageOffset;
            var array2 = bufferManager.TakeBuffer(bufferSize);
            Array.Copy(buffer, 0, array2, messageOffset, num);
            return new ArraySegment<byte>(array2, messageOffset, num);
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            innerEncoder.WriteMessage(message, stream);
        }
    }
}
