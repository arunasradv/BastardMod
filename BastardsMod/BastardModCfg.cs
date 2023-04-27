using BastardsMod.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace BastardsMod
{
    public class BastardModCfg: XmlCfg
    {
        public BastardModConfiguration values = new BastardModConfiguration();

        public override object Load(string file_name, Type type)
        {
            values = (BastardModConfiguration)base.Load(file_name, type);

            return values;
        }
    }
}