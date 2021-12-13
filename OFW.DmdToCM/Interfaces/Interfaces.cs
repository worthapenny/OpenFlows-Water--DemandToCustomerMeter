using Haestad.Support.Support;
using OFW.DmdToCM.Support;
using OpenFlows.Domain.ModelingElements;
using OpenFlows.Water.Domain.ModelingElements;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFW.DmdToCM.Interfaces
{
    //public interface IInputOptions
    //{
    //    string ModelFilePath { get; set; }
    //    IPolygonShapeFileOptions ParcelShapeFileOptions { get; set; }

    //}

    //public interface IPolygonShapeFileOptions
    //{
    //    string ShapeFilepath { get; set; }
    //    string LabelField { get; set; }
    //    string BillingIdField { get; set; }
    //    Type BillingIdType { get; set; }
    //    string UniqueField { get; set; }
    //    string Address { get; set; }

    //    string AddressModelFieldName { get; set; }

    //}

    //public interface IThiessenPolygonOptions
    //{
    //    int BufferPercentage { get; set; }
    //    string FilePath { get; }
    //    bool Overwrite { get; set; }
    //    string PointLayerName { get; }
    //    string RootDir { get; set; }
    //    IWaterSelectionSet SelectionSet { get; set; }
    //    string ThiessenPolygonShapefileName { get; set; }
    //    List<WaterNetworkElementType> ElementTypes { get; set; }
        
    //}

    public interface IDemandDistributionDataAttribute
    {
        List<ICustomerMeter> CustomerMeters { get; set; }
        List<IDemand> Demands { get; set; }
        List<IUnitLoadDemand> UnitLoadDemands { get; set; }
        double Elevation { get; set; }
        int NodeId { get; set; }
        StringBuilder Notes { get; set; }
        GeometryPoint[] PloygonGeometry { get; set; }
        PartiallydistributedDemandNodes PartiallydistributedDemandNodes { get; set; }


        KeyValuePair<double?, int?> UnifyDemands();
        IUnitLoadDemand UnifiyUnitDemands();

    }
}
