using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;

namespace Binding
{
    /// <summary>
    /// Фабрика для кастомного кодировщика сообщений СМЭВ.
    /// 
    /// </summary>
    public class SMEVTextMessageEncoderFactory : MessageEncoderFactory
    {
        private MessageEncoder encoder;
        private MessageVersion version;
        private string mediaType;
        private string charSet;
        private MessageEncoderFactory innerMessageFactory;
        private string actor;

        public override MessageEncoder Encoder
        {
            get
            {
                return this.encoder;
            }
        }

        public override MessageVersion MessageVersion
        {
            get
            {
                return this.version;
            }
        }

        internal string Actor
        {
            get
            {
                return this.actor;
            }
            set
            {
                this.actor = value;
            }
        }

        internal MessageEncoderFactory InnerMessageFactory
        {
            get
            {
                return this.innerMessageFactory;
            }
        }

        internal string MediaType
        {
            get
            {
                return this.mediaType;
            }
        }

        internal string CharSet
        {
            get
            {
                return this.charSet;
            }
        }

        internal SMEVTextMessageEncoderFactory(string mediaType, string charSet, MessageVersion version, MessageEncoderFactory messageFactory)
            : this(mediaType, charSet, version, messageFactory, "")
        {
        }

        internal SMEVTextMessageEncoderFactory(string mediaType, string charSet, MessageVersion version, MessageEncoderFactory messageFactory, string actor)
        {
            this.version = version;
            this.mediaType = mediaType;
            this.charSet = charSet;
            this.actor = actor;
            this.innerMessageFactory = messageFactory;
            this.encoder = (MessageEncoder)new SMEVTextMessageEncoder(this);
        }
    }
}
