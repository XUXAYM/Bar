using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace СocktailParser
{
    [DataContract]
    public class IngredientsData
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int Degree { get; set; }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;

            IngredientsData ingredient = (IngredientsData)obj;
            return Name == ingredient.Name && Description == ingredient.Description;
        }
    }
}
