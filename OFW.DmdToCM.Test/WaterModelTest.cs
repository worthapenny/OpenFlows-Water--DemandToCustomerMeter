using Haestad.Support.User;
using NUnit.Framework;
using OFW.DmdToCM.DemandDestribution;
using OFW.DmdToCM.Support;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OFW.DmdToCM.Test
{
    [TestFixture]
    public class WaterModelTests : OpenFlowsWaterTestFixtureBase
    {
        #region Constructor
        public WaterModelTests()
        {
        }
        #endregion

        #region Setup / Teardown
        protected override void SetupImpl()
        {
            string filename = Path.GetFullPath(BuildTestFilename($@"Germantown_mini\Germantown_mini_start.wtg"));
            OpenModel(filename);
        }
        protected override void TeardownImpl()
        {
        }
        #endregion


        #region Tests
        [Test, Order(1)]
        public void CreateCustomerMeterFromBuildingShpTest()
        {
            var shapefilePath = @"D:\Development\Data\Shapefiles\Germantown\Mini\Germantown_Buildings.shp";
            var shapefile = new Shapefile(WaterModel, shapefilePath);
            Assert.IsNotNull(shapefile);

            var cmBuilder = new CustomerMeterBuilder(WaterModel);
            var options = new PolygonShapeFileOptions()
            {
                LabelField = "Label",
                BillingIdField = "ALL_BLDS_",
                BillingIdType = typeof(int),
                ShapeFilepath = shapefilePath,
                UniqueField = "ALL_BLDS_"
            };

            // Make sure no CMs are there in the model
            Assert.AreEqual(0, WaterModel.Network.CustomerMeters.Count);

            cmBuilder.CreateCustomerMeters(options);
            var cms = WaterModel.Network.CustomerMeters.Elements();
            
            // Make sure CM created number is same is shapefile attribute table rows
            Assert.AreEqual(shapefile.AllValues().Rows.Count,  cms.Count);
            
            // BillingID Field
            var billingIdField = WaterModel.Network.CustomerMeters.InputFields.FieldByLabel("Billing ID");
            Assert.NotNull(billingIdField);

            // Make sure attributes are transfered to Billing Id
            var cm1 = cms.Where(cm => cm.Label == "Label-4549");
            Assert.IsTrue(cm1.Any());
            Assert.AreEqual("4549", billingIdField.GetValue<string>(cm1.First().Id));

            var cm2 = cms.Where(cm => cm.Label == "Label-4654");
            Assert.IsTrue(cm2.Any());
            Assert.AreEqual("4654", billingIdField.GetValue<string>(cm2.First().Id));

        }


        [Test, Order(2)]
        public void CreateThiessenPolygonTest()
        {
            var tp = new ThiessenPolygon(WaterModel);
            var ss = new SelectionSetBuilder(WaterModel).CreateDemandNodesSelectionSet(
                includeHydrants: true,
                includeTanks: true);

            ThiessenPolygonOptions = new ThiessenPolygonOptions(TempDir)
            {
                ElementTypes = new List<WaterNetworkElementType>() {
                    WaterNetworkElementType.Junction,
                    //WaterNetworkElementType.Hydrant,
                    //WaterNetworkElementType.Tank
                },
                SelectionSet = ss,
                Overwrite = true,
            };
            Assert.NotNull(ThiessenPolygonOptions);

            var tpFilePath = tp.Create(ThiessenPolygonOptions, new NullProgressIndicator());

            // Make sure the file is created
            Assert.IsTrue(File.Exists(tpFilePath));
            Assert.IsTrue(File.Exists(ThiessenPolygonOptions.FilePath));

            // Make sure the right number of polygons are created
            var tpShapefile = new Shapefile(WaterModel, tpFilePath);
            Assert.AreEqual(tpShapefile.AllValues().Rows.Count, ss.Count);

            // Delete the SS
            ss.Delete();

        }

        [Test, Order(3)]
        public void DemandRedistributionTest()
        {
            // Make sure the previous test updated the property
            Assert.NotNull(ThiessenPolygonOptions);

            var dr = new DemandRedistribution(WaterModel);
            Assert.IsNotNull(dr);

            var demandMap = dr.ExtractDemandNodeData(new NullProgressIndicator());
            Assert.AreEqual(WaterModel.Network.Junctions.Count, demandMap.Count);

            // Make sure each demand attribute has polygon geometry
            dr.InsertPolygonGeometryToDemandNodes(
                thiessenPolygonShapefilePath: ThiessenPolygonOptions.FilePath,
                elementIdFieldName: "ELEMENTID",
                demandMap: demandMap,
                new NullProgressIndicator());

            foreach (var demandRow in demandMap)
            {
                var nodeId = demandRow.Key;
                Assert.NotNull(WaterModel.Element(nodeId));

                var demandAttribute = demandRow.Value;
                Assert.IsTrue(demandAttribute.PloygonGeometry.Count() >= 3);
            }

            // Make sure demands are transfered correctly
            dr.TransferDemandToCustomerMeters(demandMap, new NullProgressIndicator());

            var junctions = WaterModel.Network.Junctions.Elements();
            var cms = WaterModel.Network.CustomerMeters.Elements();

            // Test Case: One Junction with two different demand patterns
            // Only first demand gets distributed
            // second demand is untouched
            var jnc2SameDmdDiffPatt = junctions.Where(j => j.Label == "J-1213");
            Assert.IsTrue(jnc2SameDmdDiffPatt.Any());
            var junction = jnc2SameDmdDiffPatt.First();
            var jctDemand = demandMap[junction.Id];

            var nearByCMs = jctDemand.CustomerMeters;
            Assert.AreEqual(5, nearByCMs.Count());

            var sumOfDemands = 0.0;
            var patternId = nearByCMs.First().Input.DemandPattern?.Id;
            foreach (var cm in nearByCMs)
            {
                sumOfDemands += cm.Input.BaseDemand;
                Assert.AreEqual(patternId, cm.Input.DemandPattern?.Id);
                Assert.AreEqual(junction.Input.Elevation, cm.Input.Elevation);
            }
            // First demand row should be distributed
            Assert.AreEqual(junction.Input.DemandCollection.Get()[0].BaseFlow, sumOfDemands, 0.001);
            // second demand row should be in the undistributed list
            Assert.NotNull(jctDemand.PartiallydistributedDemandNodes);
            Assert.AreEqual(jctDemand.PartiallydistributedDemandNodes.DemandNodeId, junction.Id);
            Assert.AreEqual(
                jctDemand.PartiallydistributedDemandNodes.UndistributedDemands.Select(d=> d.Key.Value).Sum(),
                junction.Input.DemandCollection.Get()[1].BaseFlow, 0.001);


            // Test Case: One demand distributed to 6 CMs
            var jncTo6CMs = junctions.Where(j => j.Label == "J-1248");
            Assert.IsTrue(jncTo6CMs.Any());
            junction = jncTo6CMs.First();
            jctDemand = demandMap[junction.Id];

            nearByCMs = jctDemand.CustomerMeters;
            
            Assert.AreEqual(6, nearByCMs.Count());

            sumOfDemands = 0.0;
            patternId = nearByCMs.First().Input.DemandPattern?.Id;
            foreach (var cm in nearByCMs)
            {
                sumOfDemands += cm.Input.BaseDemand;
                Assert.AreEqual(patternId, cm.Input.DemandPattern?.Id);
                Assert.AreEqual(junction.Input.Elevation, cm.Input.Elevation);
            }
            Assert.AreEqual(junction.Input.DemandCollection.Get()[0].BaseFlow, sumOfDemands, 0.001);
            // there should not be any undistributed demands
            Assert.Null(jctDemand.PartiallydistributedDemandNodes);


            // Test Case: Same demand, same pattern on a junction
            var jncTwoSameFixedDmds = junctions.Where(j => j.Label == "J-1252");
            Assert.IsTrue(jncTwoSameFixedDmds.Any());
            junction = jncTwoSameFixedDmds.First();
            jctDemand = demandMap[junction.Id];

            nearByCMs = jctDemand.CustomerMeters;

            Assert.AreEqual(1, nearByCMs.Count());

            sumOfDemands = 0.0;
            patternId = nearByCMs.First().Input.DemandPattern?.Id;
            foreach (var cm in nearByCMs)
            {
                sumOfDemands += cm.Input.BaseDemand;
                Assert.AreEqual(patternId, cm.Input.DemandPattern?.Id);
                Assert.AreEqual(junction.Input.Elevation, cm.Input.Elevation);
            }
            Assert.AreEqual(junction.Input.DemandCollection.Get().Select(d => d.BaseFlow).Sum(), sumOfDemands, 0.001);
            // there should not be any undistributed demands
            Assert.Null(jctDemand.PartiallydistributedDemandNodes);


            // Test Case: Same demand same pattern (not fixed)
            var jncTwoSamePattenDmds = junctions.Where(j => j.Label == "J-1309");
            Assert.IsTrue(jncTwoSamePattenDmds.Any());
            junction = jncTwoSamePattenDmds.First();
            jctDemand = demandMap[junction.Id];

            nearByCMs = jctDemand.CustomerMeters;

            Assert.AreEqual(0, nearByCMs.Count());
            Assert.Null(jctDemand.PartiallydistributedDemandNodes);


            // Test case: Same demand same pattern (not fixed)
            var jncTwoSamePattenDmds2 = junctions.Where(j => j.Label == "J-1320");
            Assert.IsTrue(jncTwoSamePattenDmds2.Any());
            junction = jncTwoSamePattenDmds2.First();
            jctDemand = demandMap[junction.Id];

            nearByCMs = jctDemand.CustomerMeters;

            Assert.AreEqual(4, nearByCMs.Count());

            sumOfDemands = 0.0;
            patternId = nearByCMs.First().Input.DemandPattern?.Id;
            foreach (var cm in nearByCMs)
            {
                sumOfDemands += cm.Input.BaseDemand;
                Assert.AreEqual(patternId, cm.Input.DemandPattern?.Id);
                Assert.AreEqual(junction.Input.Elevation, cm.Input.Elevation);
            }
            Assert.AreEqual(junction.Input.DemandCollection.Get().Select(d => d.BaseFlow).Sum(), sumOfDemands, 0.001);
            // there should not be any undistributed demands
            Assert.Null(jctDemand.PartiallydistributedDemandNodes);


            // Test Case: Junction demand but no CMs to distribute
            var jncNoCms = junctions.Where(j => j.Label == "J-1224");
            Assert.IsTrue(jncNoCms.Any());
            junction = jncNoCms.First();
            jctDemand = demandMap[junction.Id];

            nearByCMs = jctDemand.CustomerMeters;


            Assert.AreEqual(0, nearByCMs.Count());
            Assert.Null(jctDemand.PartiallydistributedDemandNodes);


            foreach (var demandItem in demandMap)
            {
                if(demandItem.Value.Notes.Length > 0)
                    System.Console.Write(demandItem.Value.Notes.ToString());
            }

            // For visual testing
            //WaterModel.SaveAs(Path.Combine(TempDir, "Test_DmdToCM.wtg"));
        }


        #endregion

        #region Private Properties
        ThiessenPolygonOptions ThiessenPolygonOptions { get; set; }
        #endregion
    }
}