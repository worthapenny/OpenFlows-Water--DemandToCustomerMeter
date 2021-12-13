using Haestad.Support.Support;
using OFW.DmdToCM.Interfaces;
using OpenFlows.Water.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OFW.DmdToCM.Support
{
    public class CustomerMeterBuilder
    {
        #region Constructor
        public CustomerMeterBuilder(IWaterModel waterModel)
        {
            WaterModel = waterModel;
        }
        #endregion

        #region Public Methods
        public bool CreateCustomerMeters(PolygonShapeFileOptions options)
        {
            bool success = true;

            var shapefile = new Shapefile(WaterModel, options.ShapeFilepath);
            var dataTable = shapefile.AllValues();

            // Extract all the geometries (as IDictionary)
            var geometryMap = shapefile.GetGeometryMap(options.UniqueField);

            // Get the Address UDX Field
            var addressUdxField = WaterModel.Network.CustomerMeters.InputFields.FieldByLabel(options.AddressModelFieldName);
            
            var hasAddressUDX = addressUdxField != null;
            if (!hasAddressUDX)
                Console.WriteLine($"Given address field {options.AddressModelFieldName} does not exists in the model");

            var hasUniqueField = !string.IsNullOrEmpty(options.UniqueField);
            var hasMeterField = !string.IsNullOrEmpty(options.BillingIdField);            

            // Create customer meter row by row
            foreach (DataRow row in dataTable.Rows)
            {
                var label = row[options.LabelField]?.ToString();
                var cm = WaterModel.Network.CustomerMeters.Create();
                cm.Label = label;

                if (hasUniqueField)
                {
                    var uniqueValue = row[options.UniqueField];
                    var geometries = (GeometryPoint[][])geometryMap[uniqueValue];
                    var centroid = Shapefile.GetCentroid(geometries[0]);
                    cm.Input.SetPoint(centroid);
                }

                if(hasMeterField)
                {
                    var meterId = row[options.BillingIdField];
                    cm.Input.InputFields.FieldByLabel("Billing ID")?.SetValue<string>(cm.Id, meterId.ToString());
                }

                if(hasAddressUDX)
                {
                    var address = row[options.Address];
                    addressUdxField.SetValue(cm.Id, address);
                }
            }

            return success;
        }
        #endregion

        #region Private Properties
        private IWaterModel WaterModel { get; set; }
        #endregion
    }
}
