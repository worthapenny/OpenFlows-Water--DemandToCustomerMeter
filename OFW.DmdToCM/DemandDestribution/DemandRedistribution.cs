using Haestad.Support.Library;
using Haestad.Support.User;
using OFW.DmdToCM.Interfaces;
using OFW.DmdToCM.Support;
using OpenFlows.Domain.ModelingElements;
using OpenFlows.Water.Domain;
using OpenFlows.Water.Domain.ModelingElements.Components;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OFW.DmdToCM.DemandDestribution
{
    public class DemandRedistribution
    {
        #region Constructor
        public DemandRedistribution(IWaterModel waterModel)
        {
            WaterModel = waterModel;
        }
        #endregion

        #region Public Methods
        public IDictionary<int, IDemandDistributionDataAttribute> ExtractDemandNodeData(IProgressIndicator pi)
        {
            var extractedDemandData = new Dictionary<int, IDemandDistributionDataAttribute>();

            pi.AddTask($"Extracting demand, pattern, and elevation from {WaterNetworkElementType.Junction}");
            pi.AddTask($"Extracting demand, pattern, and elevation from {WaterNetworkElementType.Hydrant}");
            pi.AddTask($"Extracting demand, pattern, and elevation from {WaterNetworkElementType.Tank}");

            var managers = new List<IElementManager>() {
                WaterModel.Network.Junctions,
                WaterModel.Network.Hydrants,
                WaterModel.Network.Tanks
            };

            foreach (var manager in managers)
            {
                pi.IncrementTask();
                pi.BeginTask(manager.Count);

                foreach (var id in manager.ElementIDs())
                {
                    var element = WaterModel.Element(id) as IWaterElement;
                    var demands = (element as IDemandNodeInput).DemandCollection.Get();
                    var unitDemands = (element as IDemandNodeInput).UnitDemandLoadCollection.Get();

                    if (demands?.Count == 0 && unitDemands?.Count ==0)
                        continue;

                    var row = new DemandDistributionDataAttribute(WaterModel);
                    row.NodeId = element.Id;
                    row.Elevation = (element as IPhysicalNodeElementInput).Elevation;
                    
                    // fill the dictionary to be returned
                    extractedDemandData.Add(row.NodeId, row);

                    // Add regular demands
                    if (demands != null)
                        row.Demands.AddRange(demands);

                    // If we have both, unit demands gets undistributed
                    if(demands != null && unitDemands != null)
                    {

                    }

                    // Add regular Unit Demands
                    if(demands == null && unitDemands != null)
                        row.UnitLoadDemands.AddRange(unitDemands);
                    
                    pi.IncrementStep();

                }

                pi.EndTask();
            }

            return extractedDemandData;

        }

        public void InsertPolygonGeometryToDemandNodes(
            string thiessenPolygonShapefilePath,
            string elementIdFieldName,
            IDictionary<int, IDemandDistributionDataAttribute> demandMap,
            IProgressIndicator pi)
        {
            if (!File.Exists(thiessenPolygonShapefilePath))
                throw new FileNotFoundException(thiessenPolygonShapefilePath);

            var shapefile = new Shapefile(WaterModel, thiessenPolygonShapefilePath);
            if (!shapefile.Reader.IsOpen)
                shapefile.Reader.OpenDataFile(thiessenPolygonShapefilePath);

            try
            {
                pi.AddTask("Extracting thiessen polygon geometry information...");
                pi.IncrementTask();
                pi.BeginTask(shapefile.Reader.RecordCount);


                shapefile.Reader.MoveReset();
                for (int i = 0; i < shapefile.Reader.RecordCount; i++)
                {
                    var id = (int)shapefile.Reader.GetFieldValue(elementIdFieldName);

                    IDemandDistributionDataAttribute dataRow = null;
                    if (demandMap.TryGetValue(id, out dataRow))
                        dataRow.PloygonGeometry = shapefile.Reader.GetPolygonGeometryNonJaggedArray()[0];

                    shapefile.Reader.MoveNext();
                    pi.IncrementStep();
                }
                pi.EndTask();

            }
            finally
            {
                shapefile.Reader.Close();
            }
        }

        public void TransferDemandToCustomerMeters(IDictionary<int, IDemandDistributionDataAttribute> demandMap, IProgressIndicator pi)
        {
            var cms = WaterModel.Network.CustomerMeters.Elements();
            //var assignedCMs = new Dictionary<int, ICustomerMeter>();

            pi.AddTask("Assigning Customer Meters with demand, pattern, and elevation...");
            pi.IncrementTask();
            pi.BeginTask(demandMap.Count);

            foreach (var demandAtributeItem in demandMap)
            {
                var nodeId = demandAtributeItem.Key;
                var dddAttribute = demandAtributeItem.Value;

                // Find out CMs that are inside the polygons
                foreach (var cm in cms)
                {
                    var isInside = MathLibrary.IsPointInPolygon(cm.Input.GetPoint(), dddAttribute.PloygonGeometry);
                    if (isInside)
                        dddAttribute.CustomerMeters.Add(cm);
                }

                // no CMs to distribute the demand
                if( dddAttribute.CustomerMeters.Count == 0)
                {
                    OrpandedNodeIds.Add(nodeId);
                    var nodeElement = WaterModel.Element(nodeId);
                    dddAttribute.Notes.AppendLine($"Node={nodeElement} => There is no Customer Meters near by to distribute the demand");
                    continue;
                }

                // no demands, nothing to transfer
                if (dddAttribute.Demands?.Count == 0 && dddAttribute.UnitLoadDemands.Count == 0)
                    continue;


                // work on simple demands
                if(dddAttribute.Demands?.Count > 0)
                {
                    var demandItem = dddAttribute.UnifyDemands();

                    // Divide the demand to all CMs equally
                    var demand = demandItem.Key.Value / dddAttribute.CustomerMeters.Count;
                    IPattern pattern = demandItem.Value == null ? null : (IPattern)WaterModel.Element(demandItem.Value.Value);
                    dddAttribute.CustomerMeters.ForEach(cm =>
                    {
                        cm.Input.BaseDemand = demand;
                        cm.Input.DemandPattern = pattern;
                        cm.Input.Elevation = dddAttribute.Elevation;
                    });
                }

                // work on Unit demand ONLY when simple demands are not there
                if(dddAttribute.Demands?.Count == 0 && dddAttribute.UnitLoadDemands?.Count > 0)
                {
                    var demandItem = dddAttribute.UnifiyUnitDemands();

                    // Divide the demand to all CMs equally
                    var demand = demandItem.NumberOfLoadingUnits / dddAttribute.CustomerMeters.Count;
                    //IPattern pattern = demandItem.Value == null ? null : (IPattern)WaterModel.Element(demandItem.Value.Value);
                    dddAttribute.CustomerMeters.ForEach(cm =>
                    {
                        //cm.Input.UnitDemand.
                        
                        cm.Input.Elevation = dddAttribute.Elevation;
                    });
                }

                

                pi.IncrementStep();
            }

            pi.EndTask();



            pi.AddTask("Create Selection Sets...");
            pi.IncrementTask();
            pi.BeginTask(4);

            var ssBuilder = new SelectionSetBuilder(WaterModel);
            
            // 1: Demand Nodes
            ssBuilder.CreateDemandNodesSelectionSet(false, false);
            pi.IncrementStep();

            // 2: CMs with no demand
            ssBuilder.CreateCustomerMetersWithNoDemandSelectionSet();
            pi.IncrementStep();

            // 3: Junctions with no nearby CMs
            ssBuilder.CreateFor(OrpandedNodeIds, "No near by CMs to distribute the demand");
            pi.IncrementStep();

            // 4: Partially allocated nodes
            var partiallyDistributedNodeIDs = demandMap.Values.Where(ddda => ddda.PartiallydistributedDemandNodes?.UndistributedDemands.Count > 0).Select(ddda => ddda.NodeId).ToList();
            ssBuilder.CreateFor(partiallyDistributedNodeIDs, "Partially distributed demand nodes");
            pi.IncrementStep();

            pi.EndTask();


        }
        #endregion

        #region Private Properties
        private List<int> OrpandedNodeIds { get; set; } = new List<int>();
        private IWaterModel WaterModel { get; }
        #endregion

    }
}
