using Haestad.Support.Support;
using OFW.DmdToCM.Interfaces;
using OFW.DmdToCM.Support;
using OpenFlows.Domain.ModelingElements;
using OpenFlows.Water.Domain;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OFW.DmdToCM.DemandDestribution
{
    [DebuggerDisplay("Demands: {Demands.Count}, CMs: {CustomerMeters.Count}")]
    public class DemandDistributionDataAttribute : IDemandDistributionDataAttribute
    {
        #region Constructor
        public DemandDistributionDataAttribute(IWaterModel waterModel)
        {
            WaterModel = waterModel;
        }
        #endregion

        #region Public Methods
        public IUnitLoadDemand UnifiyUnitDemands()
        {
            var unitLoadDemand = new UnitLoadDemandEx();
            if(UnitLoadDemands.Count == 0) return unitLoadDemand;

            var patternID = UnitLoadDemands[0].UnitDemandPattern?.Id;
            var unitLoadUnit = UnitLoadDemands[0].NumberOfLoadingUnits;
            var unitLoadID = UnitLoadDemands[0].UnitDemandLoad;

            for (int i = 0; i < UnitLoadDemands.Count; i++)
            {
                var newUnitLoadDemand = UnitLoadDemands[i];
                var newPatternID = newUnitLoadDemand.UnitDemandPattern?.Id;
                var newUnitLoadUnit = newUnitLoadDemand.NumberOfLoadingUnits;
                var newUnitLoadID = newUnitLoadDemand.UnitDemandLoad;
                var newUnitLoadValue = newUnitLoadDemand.UnitDemandBaseFlow;

                if (patternID == newPatternID && unitLoadID == newUnitLoadID)
                    unitLoadUnit += newUnitLoadUnit;
                else
                {
                    if (partiallyDistributedDemandNodes == null)
                        partiallyDistributedDemandNodes = new PartiallydistributedDemandNodes();

                    partiallyDistributedDemandNodes.DemandNodeId = NodeId;
                    partiallyDistributedDemandNodes.UndistributedUnitDemands.Add(newUnitLoadDemand);
                    Notes.AppendLine($"Node={NodeElement} => Undistributed unit demand, Unit demand of = {newUnitLoadValue}, Pattern = {newUnitLoadDemand.UnitDemandPattern}");
                }

            }

            return unitLoadDemand;
        }
        public KeyValuePair<double?, int?> UnifyDemands()
        {
            var demands = new KeyValuePair<double?, int?>();

            if (Demands.Count == 0)
                return demands;

            double? demandValue = Demands[0]?.BaseFlow;
            int? demandPatterId = Demands[0]?.DemandPattern?.Id;

            for (int i = 1; i < Demands.Count; i++)
            {
                var newPatternId = Demands[i].DemandPattern?.Id;
                var newDemand = Demands[i].BaseFlow;

                if (demandPatterId == newPatternId)
                    demandValue += newDemand;
                else
                {
                    if (partiallyDistributedDemandNodes == null)
                        partiallyDistributedDemandNodes = new PartiallydistributedDemandNodes();

                    partiallyDistributedDemandNodes.DemandNodeId = NodeId;
                    partiallyDistributedDemandNodes.UndistributedDemands.Add(new KeyValuePair<double?, int?>(newDemand, newPatternId));
                    Notes.AppendLine($"Node={NodeElement} => Undistributed demand, Demand = {newDemand}, Pattern = {WaterModel.Element(newPatternId.Value)}");
                }

            }

            return new KeyValuePair<double?, int?>(demandValue, demandPatterId);
        }

        #endregion

        #region Public Properties
        public int NodeId { get; set; }
        public IElement NodeElement => WaterModel.Element(NodeId);
        public double Elevation { get; set; }
        public List<IDemand> Demands { get; set; } = new List<IDemand>();
        public List<IUnitLoadDemand> UnitLoadDemands { get; set; } = new List<IUnitLoadDemand>();
        
        public StringBuilder Notes { get; set; } = new StringBuilder();
        public List<ICustomerMeter> CustomerMeters { get; set; } = new List<ICustomerMeter>();
        public GeometryPoint[] PloygonGeometry { get; set; }
        public PartiallydistributedDemandNodes PartiallydistributedDemandNodes
        {
            get { return partiallyDistributedDemandNodes; }
            set { partiallyDistributedDemandNodes = value; }
        }

        #endregion

        #region Private Properties
        private IWaterModel WaterModel { get; set; }
        #endregion

        #region Private Fields
        PartiallydistributedDemandNodes partiallyDistributedDemandNodes;

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
        #endregion

    }
}
