using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace SmevServiceEncoder
{
    public class SMEVMessageEncodingBindingElementExtensionElement : BindingElementExtensionElement
    {
        private const string LogPathName = "logPath";

        [ConfigurationProperty(LogPathName, IsRequired = false,
            DefaultValue = "D:\\ServiceLog\\")]
        public string LogPath
        {
            get
            {
                return (string)base[LogPathName];
            }

            set
            {
                base[LogPathName] = value;
            }
        }

        protected override BindingElement CreateBindingElement()
        {
            var bindingElement =
                 new SMEVMessageEncodingBindingElement(LogPath);
            ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        public override Type BindingElementType
        {
            get { return typeof(SMEVMessageEncodingBindingElement); }
        }
    }
}
