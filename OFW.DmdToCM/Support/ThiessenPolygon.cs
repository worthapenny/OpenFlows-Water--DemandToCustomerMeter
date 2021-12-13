using Haestad.Domain;
using Haestad.LoadBuilder.Calculations;
using Haestad.LoadBuilder.Calculations.Data;
using Haestad.Support.Units;
using Haestad.Support.User;
using Haestad.ThiessenPolygon.Application;
using Haestad.ThiessenPolygon.Domain;
using OFW.DmdToCM.Extensions;
using OFW.DmdToCM.Interfaces;
using OpenFlows.Domain.ModelingElements;
using OpenFlows.Water.Domain;
using OpenFlows.Water.Domain.ModelingElements;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace OFW.DmdToCM.Support
{
    /// <summary>
    /// Manually copy the c4ll.dll to debug dir from right x86/64 dir
    /// </summary>
    public class ThiessenPolygon
    {
        #region Constructor
        public ThiessenPolygon(IWaterModel waterModel)
        {
            WaterModel = waterModel;
        }
        #endregion

        #region Public Methods
        public string Create(ThiessenPolygonOptions options, DataObject dataObject, IDomainElementManager[] elementManagers, IProgressIndicator pi)
        {
            pi.AddTask("Creating thiessen polygon...");
            pi.IncrementTask();
            pi.BeginTask(1);

            
            var lengthUnit = (LengthUnit)WaterModel.Units.NetworkUnits.Pipe.LengthUnit.GetUnit().EnumValue;

            var inputContext = new InputContext(
                parentWindow: IntPtr.Zero, // input pointer
                dataTools: new StandardDataTools(),
                nodeManagers: elementManagers,
                selectedIDs: new Haestad.Support.Support.ElementIdentifier[0], // Element Identifier
                //selectedIDs: options.
                unit: lengthUnit // LengthUnit
                );

            LoadBuilderLibrary.SetActiveDomainDataSet(WaterModel.DomainDataSet);
            LoadBuilderLibrary.SetDisplayLengthUnit(lengthUnit);

            // Run the Engine
            EngineDriver.Run(dataObject, inputContext, pi);

            pi.IncrementStep();
            pi.EndTask();

            return options.FilePath;
        }

        /// <summary>
        /// Create a thiessen polygon based on Junctions and Hydrants node
        /// </summary>
        /// <param name="options"></param>
        /// <param name="pi"></param>
        /// <returns></returns>
        public string Create(ThiessenPolygonOptions options, IProgressIndicator pi)
        {
            if (!options.Overwrite && File.Exists(options.FilePath))
            {
                Debug.Print($"Returning, as overwrite is false and path '{options.FilePath}' exists.");
                return options.FilePath;
            }

            var dataObject = new DataObject();
            dataObject.UseBoundaryPercentageBuffer = options.BufferPercentage > 0;
            dataObject.BoundaryPercentage = options.BufferPercentage;
            dataObject.UseExternalPointLayer = true;
            dataObject.IDFieldName = "ElemID";
            dataObject.ActiveElementsOnly = false;
            dataObject.OutputLayerSerializedName = options.FilePath;
            dataObject.PointLayerName = options.PointLayerName;


            return Create(
                options,
                dataObject,
                WaterModel.DomainDataSet.GetDomainElementManagers(options.ElementTypes),
                pi);
        }
        public void AddDemandAndElevationData(string shapefilePath, DataTable dataTable, IProgressIndicator pi)
        {
            if (!File.Exists(shapefilePath))
                throw new FileNotFoundException($"Given file path '{shapefilePath}' doesn't exists.", nameof(AddDemandAndElevationData));

            if (dataTable == null || dataTable.Rows.Count <= 0)
                throw new ArgumentException("Data table doesn't have any roes", nameof(AddDemandAndElevationData));

            pi.AddTask("Adding Field to the thiessen polygon");
            pi.IncrementTask();
            pi.BeginTask(1);



            pi.IncrementStep();
            pi.EndTask();
        }
        #endregion

        #region Private Properties
        private IWaterModel WaterModel { get; }
        #endregion
    }



    public class ThiessenPolygonOptions /*: IThiessenPolygonOptions */
    {
        #region Constructor
        public ThiessenPolygonOptions()
        {
            BufferPercentage = 10;
            ThiessenPolygonShapefileName = "ThiessenPolygon.shp";
            Overwrite = false;
        }
        public ThiessenPolygonOptions(string rootDir)
            : this()
        {
            RootDir = rootDir;
        }
        #endregion

        #region Public Methods
        #endregion

        public int BufferPercentage { get; set; }

        /// <summary>
        /// This has to be in a format like: 
        /// 'Junction\All Elements' Or,
        /// 'Junction\SelectionSetLabel'
        /// </summary>
        public string PointLayerName
        {
            get
            {
                if (SelectionSet == null && ElementTypes.Count == 1)
                    return $@"{ElementTypes[0]}\All Elements";

                else if (SelectionSet == null && ElementTypes.Count > 1)
                    throw new InvalidOperationException($"{nameof(SelectionSet)} can not be null when {nameof(ElementTypes)} count > 0");

                else if (SelectionSet != null && ElementTypes.Count == 1)
                    return $@"{ElementTypes[0]}\{SelectionSet.Label}";

                else if (SelectionSet != null && ElementTypes.Count > 1)
                    return $@"{SelectionSet.Label}";

                throw new InvalidOperationException($"Either provide a {nameof(SelectionSet)} or {nameof(ElementTypes)}");
            }
        }
        //public WaterNetworkElementType DomainElementType { get; set; }
        public IWaterSelectionSet SelectionSet { get; set; }
        public string ThiessenPolygonShapefileName { get; set; }
        public string RootDir { get; set; }
        public bool Overwrite { get; set; }
        public List<WaterNetworkElementType> ElementTypes { get; set; } = new List<WaterNetworkElementType>();
        public string FilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_filePath))
                    return _filePath;

                if (!ThiessenPolygonShapefileName.ToLower().EndsWith(".shp"))
                    ThiessenPolygonShapefileName += ".shp";

                _filePath = Path.GetFullPath(Path.Combine(RootDir, ThiessenPolygonShapefileName));

                return _filePath;
            }
        }

        #region Private Fields

        private string _filePath;


        #endregion
    }
}
