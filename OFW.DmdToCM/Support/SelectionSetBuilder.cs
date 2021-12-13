using OpenFlows.Water.Domain;
using OpenFlows.Water.Domain.ModelingElements;
using System.Collections.Generic;
using System.Linq;

namespace OFW.DmdToCM.Support
{
    public class SelectionSetBuilder
    {
        #region Constructor
        public SelectionSetBuilder(IWaterModel waterModel)
        {
            WaterModel = waterModel;
        }
        #endregion

        #region Public Methods
        public IWaterSelectionSet CreateDemandNodesSelectionSet(bool includeHydrants = true, bool includeTanks = false)
        {
            var selectionSet = WaterModel.SelectionSets.Create();
            selectionSet.Label = "DmdToCM__Demand Nodes";

            var ids = WaterModel.Network.Junctions.ElementIDs();

            if (includeTanks)
                ids.AddRange(WaterModel.Network.Tanks.ElementIDs());

            if (includeHydrants)
                ids.AddRange(WaterModel.Network.Hydrants.ElementIDs());

            selectionSet.Set(ids);
            
            return selectionSet;
        }
        public IWaterSelectionSet CreateCustomerMetersWithNoDemandSelectionSet()
        {
            var allCustomerMeters = WaterModel.Network.CustomerMeters.Elements();
            var selectionSet = WaterModel.SelectionSets.Create();
            var unassignedCMs = WaterModel.Network.CustomerMeters.Elements().Where(cm=>cm.Input.BaseDemand<=0).Select(cm=>cm.Id).ToList();
            

            // create selection set
            var ss = WaterModel.SelectionSets.Create();
            ss.Label = "DmdToCM__Customer Meters with no demand";
            ss.Set(unassignedCMs);

            return selectionSet;
        }
        public IWaterSelectionSet CreateFor(List<int> ids, string label)
        {
            var ss = WaterModel.SelectionSets.Create();
            ss.Label = $"DmdToCM__{label}";
            ss.Set(ids);
            return ss;
        }
        #endregion


        #region Private Properties
        private IWaterModel WaterModel { get; set; }
        #endregion

    }
}
