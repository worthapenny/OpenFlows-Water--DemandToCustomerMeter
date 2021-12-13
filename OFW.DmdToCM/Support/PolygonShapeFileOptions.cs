using System;

namespace OFW.DmdToCM.Support
{
    public class PolygonShapeFileOptions /*: IPolygonShapeFileOptions*/
    {
        public string ShapeFilepath { get; set; }
        public string LabelField { get; set; }
        public string BillingIdField { get; set; }
        public string UniqueField { get; set; }
        public string Address { get; set; }
        public string AddressModelFieldName { get; set; }
        public Type BillingIdType { get; set; }
    }
}
