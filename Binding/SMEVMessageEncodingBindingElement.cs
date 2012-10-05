using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;


namespace Binding
{
    /// <summary>
    /// Кастомный элемент привязки для кодирования сообщений СМЭВ.
    /// 
    /// </summary>
    public sealed class SMEVMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        private MessageVersion vers = MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.None);
        private string senderActor = "";
        private string recipientActor = "";
        private MessageEncodingBindingElement innerBindingElement;

        /// <summary>
        /// Получить/задать вложенный кодировщик
        /// 
        /// </summary>
        public MessageEncodingBindingElement InnerMessageEncodingBindingElement
        {
            get
            {
                return this.innerBindingElement;
            }
            set
            {
                this.innerBindingElement = value;
            }
        }

        /// <summary>
        /// Значение аттрибута actor в заголовке Security
        ///             (для исходящих сообщений)
        /// 
        /// </summary>
        public string SenderActor
        {
            get
            {
                return this.senderActor;
            }
            set
            {
                this.senderActor = value;
            }
        }

        /// <summary>
        /// Значение аттрибута actor в заголовке Security
        ///             (для входящих сообщений)
        /// 
        /// </summary>
        public string RecipientActor
        {
            get
            {
                return this.recipientActor;
            }
            set
            {
                this.recipientActor = value;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return this.innerBindingElement.MessageVersion;
            }
            set
            {
                this.innerBindingElement.MessageVersion = value;
            }
        }

        /// <summary>
        /// по-умолчанию будем использовать стандартный кодировщик
        /// 
        /// </summary>
        public SMEVMessageEncodingBindingElement()
            : this((MessageEncodingBindingElement)new TextMessageEncodingBindingElement())
        {
        }

        public SMEVMessageEncodingBindingElement(MessageEncodingBindingElement messageEncoderBindingElement)
        {
            this.innerBindingElement = messageEncoderBindingElement;
            this.innerBindingElement.MessageVersion = this.vers;
        }

        /// <summary>
        /// Точка входа в элемент привязки.
        ///             Вызывается WCF'ом для получения фабрики кодировщика.
        /// 
        /// </summary>
        /// 
        /// <returns/>
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return (MessageEncoderFactory)new SMEVTextMessageEncoderFactory("text/xml", "utf-8", this.vers, this.innerBindingElement.CreateMessageEncoderFactory());
        }

        public override BindingElement Clone()
        {
            return (BindingElement)new SMEVMessageEncodingBindingElement(this.innerBindingElement);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
                return this.innerBindingElement.GetProperty<T>(context);
            else
                return base.GetProperty<T>(context);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            context.BindingParameters.Add((object)this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            context.BindingParameters.Add((object)this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            context.BindingParameters.Add((object)this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }
    }
}
