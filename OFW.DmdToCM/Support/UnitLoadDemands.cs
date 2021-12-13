using OpenFlows.Water.Domain.ModelingElements.Components;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;

namespace OFW.DmdToCM.Support
{
    public class UnitLoadDemandEx : IUnitLoadDemand
    {
        #region Constructor
        public UnitLoadDemandEx()
        {
        }
        #endregion

        #region Public Properties

        public IUnitDemandLoad UnitDemandLoad { get; set; }
        public double NumberOfLoadingUnits { get; set; }
        public double UnitDemandBaseFlow { get; set; }
        public IPattern UnitDemandPattern { get; set; }
        #endregion
    }
}
