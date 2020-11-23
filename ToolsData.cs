using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace СocktailParser
{
    [DataContract]
    public class ToolsData
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;

            ToolsData tool = (ToolsData)obj;
            return Name == tool.Name && Description == tool.Description;
        }
    }
}
