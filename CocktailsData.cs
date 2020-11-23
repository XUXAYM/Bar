using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace СocktailParser
{
    [DataContract]
    public class CocktailsData
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<string> Tags { get; set; }
        [DataMember]
        public List<(string, int)> Ingredients { get; set; }
        [DataMember]
        public List<string> Tools { get; set; }
        [DataMember]
        public string Recipe { get; set; }
        [DataMember]
        public string Description { get; set; }
        public CocktailsData()
        {
            Ingredients = new List<(string, int)>();
            Tools = new List<string>();
            Tags = new List<string>();
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;

            CocktailsData cocktail = (CocktailsData)obj;
            return (Name == cocktail.Name && Tags.SequenceEqual(cocktail.Tags) && Tools.SequenceEqual(cocktail.Tools) && Recipe == cocktail.Recipe && Description == cocktail.Description && Ingredients.SequenceEqual(cocktail.Ingredients));
        }
    }
}
