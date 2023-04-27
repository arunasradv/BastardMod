using BastardsMod.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BastardsMod
{
    public abstract class DialogHelperBase
    {
        public List<Line> lines = new List<Line>();
        public List<Dialog> dialogs = new List<Dialog>();

        public virtual void Load(string file_name)
        {
            DialogHelper x = new DialogHelper();
            var mySerializer = new XmlSerializer(typeof(DialogHelper));
            using (var myFileStream = new FileStream(file_name, FileMode.Open))
            {
                x = (DialogHelper)mySerializer.Deserialize(myFileStream);
            }
            this.lines = x.lines;
            this.dialogs = x.dialogs;
        }
        public virtual void Save(string file_name)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DialogHelper));
            TextWriter writer = new StreamWriter(file_name);
            serializer.Serialize(writer, this);
            writer.Close();
        }
    }
}