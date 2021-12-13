using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System.Collections.Generic;

namespace OFW.DmdToCM.Support
{
    public class PartiallydistributedDemandNodes
    {
        #region Constructor
        public PartiallydistributedDemandNodes()
        {
        }
        #endregion

        #region Public Properties
        public int DemandNodeId { get; set; }
        public List<KeyValuePair<double?, int?>> UndistributedDemands { get; private set; } = new List<KeyValuePair<double?, int?>>();
        public List<IUnitLoadDemand> UndistributedUnitDemands { get; private set; } = new List<IUnitLoadDemand>();
        #endregion
    }
}
