using Haestad.Domain;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System.Collections.Generic;
using System.Linq;

namespace OFW.DmdToCM.Extensions
{
    public static class DomainDataSetExtensions
    {
        public static IDomainElementManager[] GetDomainElementManagers(this IDomainDataSet domainDataSet, List<WaterNetworkElementType> elementTypes)
        {
            if (elementTypes?.Count == 0) return null;
         
            var elementManagers = new List<IDomainElementManager>();
            
            elementManagers.AddRange(
                elementTypes.Select(
                    e => domainDataSet.DomainElementManager((int)e)
                    ).ToList()
                );

            return elementManagers.ToArray();
        }
    }
}
