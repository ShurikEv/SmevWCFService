using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace SmevServiceEncoder
{
    public class SMEVTextMessageEncoder : MessageEncoder
    {
        private const string SmevActor = "http://smev.gosuslugi.ru/actors/smev";
        private const string RecipientActor = "http://smev.gosuslugi.ru/actors/recipient";
        private const string Soap11Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        private readonly SMEVTextMessageEncoderFactory _factory;
        private readonly string _contentType;
        private readonly MessageEncoder _innerEncoder;

        private string LogPath { get; set; }

        public override string ContentType
        {
            get
            {
                return _contentType;
            }
        }

        public override string MediaType
        {
            get
            {
                return _factory.MediaType;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return _factory.MessageVersion;
            }
        }

        public SMEVTextMessageEncoder(SMEVTextMessageEncoderFactory factory)
        {
            _factory = factory;
            _innerEncoder = factory.InnerMessageFactory.Encoder;
            _contentType = _factory.MediaType;
            LogPath = _factory.LogPath;
        }

        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            var msgContents = new byte[buffer.Count];
            Array.Copy(buffer.Array, buffer.Offset, msgContents, 0, msgContents.Length);
            var message = new UTF8Encoding().GetString(msgContents);
            var reader = XmlReader.Create(new MemoryStream(msgContents));
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(reader);
            var elementsByTagName = xmlDocument.GetElementsByTagName("Envelope", Soap11Namespace);
            if (elementsByTagName.Count == 0)
                throw new XmlException("Не найден узел Envelope");
            var prefixOfNamespace = elementsByTagName[0].GetPrefixOfNamespace(Soap11Namespace);
            if (string.IsNullOrEmpty(prefixOfNamespace))
                throw new XmlException(string.Format("Не найден префикс пространста имен {0}", Soap11Namespace));
            LogMessage(xmlDocument, true);
            // Убираем actors из входящего сообщения
            message = message.Replace(prefixOfNamespace + ":mustUnderstand=\"1\"", "");
            message = message.Replace(prefixOfNamespace + ":actor=\"" + SmevActor + "\"", "");
            var bytes = new UTF8Encoding().GetBytes(message.Replace(prefixOfNamespace + ":actor=\"" + RecipientActor + "\"", ""));
            var length = bytes.Length;
            var array = bufferManager.TakeBuffer(length);
            Array.Copy(bytes, 0, array, 0, length);
            buffer = new ArraySegment<byte>(array, 0, length);
            return _innerEncoder.ReadMessage(buffer, bufferManager, contentType);
        }

        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            return _innerEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
        }

        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            var arraySegment = _innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
            var buffer1 = new byte[arraySegment.Count];
            Array.Copy(arraySegment.Array, arraySegment.Offset, buffer1, 0, buffer1.Length);
            var reader = XmlReader.Create(new MemoryStream(buffer1));
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(reader);
            var prefixOfNamespace = xmlDocument.FirstChild.GetPrefixOfNamespace(Soap11Namespace);
            if (string.IsNullOrEmpty(prefixOfNamespace))
                throw new XmlException(string.Format("Не найден префикс пространста имен {0}", Soap11Namespace));
            var elementsByTagName = xmlDocument.GetElementsByTagName("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            if (elementsByTagName.Count == 1)
            {
                // Добавляем actor в исходящие сообщение
                var attribute = xmlDocument.CreateAttribute(prefixOfNamespace + ":actor", Soap11Namespace);
                attribute.Value = SmevActor;
                var xmlAttributeCollection = elementsByTagName[0].Attributes;
                if (xmlAttributeCollection != null)
                {
                    xmlAttributeCollection.Append(attribute);
                }
            }
            var memoryStream = new MemoryStream();
            var w = XmlWriter.Create(memoryStream, new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Encoding = new UTF8Encoding(false)
            });
            xmlDocument.Save(w);
            LogMessage(xmlDocument, false);
            w.Flush();
            var buffer2 = memoryStream.GetBuffer();
            var num = (int)memoryStream.Position;
            memoryStream.Close();
            var bufferSize = num + messageOffset;
            var array = bufferManager.TakeBuffer(bufferSize);
            Array.Copy(buffer2, 0, array, messageOffset, num);
            return new ArraySegment<byte>(array, messageOffset, num);
        }

        public override void WriteMessage(Message message, Stream stream)
        {
            _innerEncoder.WriteMessage(message, stream);
        }

        private void LogMessage(XmlDocument doc, bool input)
        {
            try
            {
                var body = doc.GetElementsByTagName("Body", Soap11Namespace);
                var child = body[0].FirstChild;
                SaveMessage(input, doc.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>",""), child.Name + "_");
            }
            catch
            {
                var s = input ? "Request_" : "Responce_";
                SaveMessage(input, doc.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ""), s);
            }
            
        }

        private void SaveMessage(bool input, string message, string name)
        {
            var dir = LogPath;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            dir = dir + "\\";
            var now = DateTime.Now.ToString("yy.MM.dd_HH.mm.ss.ffff_");
            var ending = input ? ".in.xml" : ".out.xml";
            try
            {
                var fileName = now + name + ending; 
                File.WriteAllText(dir + fileName, message);
            }
            catch 
            {
                try
                {
                    name = input ? "Request_" : "Responce_";
                    File.WriteAllText(dir + now + name + ".xml", message);
                }
                catch(Exception e)
                {
                    File.WriteAllText(dir + now + "_error" + ".txt", e.Message);
                }
                
            }
        }
    }
}
