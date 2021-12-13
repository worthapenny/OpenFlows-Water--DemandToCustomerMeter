using Haestad.Shapefile;
using Haestad.Support.Library;
using Haestad.Support.Support;
using Haestad.Support.User;
using OpenFlows.Domain.ModelingElements.NetworkElements;
using OpenFlows.Water.Domain;
using OpenFlows.Water.Domain.ModelingElements.NetworkElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace OFW.DmdToCM.Support
{
    public class Shapefile
    {
        #region Constructor
        public Shapefile(IWaterModel waterModel, string shapefilePath)
        {
            WaterModel = waterModel;

            if (!File.Exists(shapefilePath))
                throw new FileNotFoundException($"Give shapefile path '{shapefilePath}' is not valid.", nameof(Shapefile));


            FilePath = shapefilePath;
        }
        #endregion

        #region Static Methods
        public static GeometryPoint GetCentroid(GeometryPoint[] points)
        {
            var centroid = new GeometryPoint();
            var area = 0.0;
            MathLibrary.CalculatePolygonAreaAndCentroid(points, out area, out centroid);

            return centroid;
        }

        public static bool CreateShapefile(
            string fullFilePath,
            List<DBFFieldInfo> fields,
            DataTable data,
            List<GeometryPoint[]> pipeGeometryList,
            string sourceProjectionFilePath,
            ShapeType shapeType = ShapeType.Polyline)
        {
            var success = true;

            var writer = new ShapefileDataSourceWriter();
            try
            {
                var fileCreated = writer.CreateDataFile(
                    fileName: fullFilePath,
                    fieldsToCreate: fields.ToArray(),
                    overwrite: true,
                    shapefileShapeType: shapeType
                    );

                if (!fileCreated) return !success;

                for (int i = 0; (i < data.Rows.Count) && success; i++)
                {

                    foreach (var field in fields)
                    {
                        writer.AddRecordBegin();

                        success = writer.WriteFieldData(
                            data.Rows[i][field.FieldName, DataRowVersion.Current],
                            field.FieldName);

                        var geometry = pipeGeometryList[i].ToArray();
                        success = success && writer.WritePolylineGeometry(geometry);

                        writer.AddRecordEnd();
                    }
                }

                if (!string.IsNullOrEmpty(sourceProjectionFilePath))
                {
                    var projectionFileFileInfo = new FileInfo(sourceProjectionFilePath);
                    var newProjectionFilePath = Path.Combine(
                                Path.GetDirectoryName(fullFilePath),
                                $"{Path.GetFileNameWithoutExtension(fullFilePath)}.prj"
                                );

                    if (projectionFileFileInfo.Exists)
                    {
                        File.Copy(
                            sourceFileName: sourceProjectionFilePath,
                            destFileName: newProjectionFilePath,
                            true
                        );
                        File.SetCreationTime(newProjectionFilePath, DateTime.Now);
                        File.SetLastWriteTime(newProjectionFilePath, DateTime.Now);
                    }
                }
            }
            finally
            {
                writer.Close();
            }

            return success;
        }
        #endregion

        #region Public Methods

        public IDictionary GetGeometryMap(string fieldName)
        {
            try
            {
                if (!Reader.IsOpen)
                    Reader.OpenDataFile(FilePath);

               return Reader.GetGeometryMap(fieldName);
            }
            finally
            {
                Reader.Close();
            }
        }

        

        public List<T> NodesInsidePolygon<T>(GeometryPoint[] polygonCoords, List<T> elements)
        {
            var validNodes = new List<T>();
            elements.ForEach(e =>
            {
                if (MathLibrary.IsPointInPolygon(((IPointNodeInput)e).GetPoint(), polygonCoords))
                    validNodes.Add(e);
            });

            return validNodes;
        }
        public List<T> LinksInsidePolygon<T>(GeometryPoint[] polygonCoords)
        {
            var validLinks = new List<T>();

            if (typeof(T) is IPipe)
            {
                WaterModel.Network.Pipes.Elements().ForEach(l =>
                {
                    bool isInside = true;
                    l.Input.GetPoints().ForEach(p =>
                    {
                        isInside = isInside && MathLibrary.IsPointInPolygon(p, polygonCoords);
                    });

                    if (isInside)
                        validLinks.Add((T)l);
                });
            }

            if (typeof(T) is ILateral)
            {
                WaterModel.Network.Laterals.Elements().ForEach(l =>
                {
                    bool isInside = true;
                    l.Input.GetPoints().ForEach(p =>
                    {
                        isInside = isInside && MathLibrary.IsPointInPolygon(p, polygonCoords);
                    });

                    if (isInside)
                        validLinks.Add((T)l);
                });
            }

            return validLinks;
        }
        public List<T> PolygonsInsidePolygon<T>(GeometryPoint[] polygonCoords)
        {
            var validPolygons = new List<T>();

            if (typeof(T) is IPumpStation)
            {
                WaterModel.Network.PumpStations.Elements().ForEach(pumpStation =>
                {
                    bool isInside = true;
                    var rings = pumpStation.Input.GetRings();
                    foreach (var point in rings[0])
                        isInside = isInside && MathLibrary.IsPointInPolygon(point, polygonCoords);

                    if (isInside)
                        validPolygons.Add((T)pumpStation);
                });
            }

            return validPolygons;
        }

        public bool AddField(DBFFieldInfo fieldInfo)
        {
            if (!Writer.IsOpen)
                Writer.OpenDataFile(FilePath);

            if (Writer.CheckIfFieldExists(fieldInfo.FieldName))
                return true;

            bool success = Writer.AddField(fieldInfo);

            success = success && Writer.Flush();

            return success;
        }
        public string[] Fields(bool toLower = false)
        {
            if (!Reader.IsOpen)
                Reader.OpenDataFile(FilePath);

            string[] fields = Reader.GetFieldNames();

            if (toLower)
                fields = fields.Select(f => f.ToLower()).ToArray();

            Reader.Close();
            return fields;
        }
        public bool FieldExists(string fieldName)
        {
            if (!Reader.IsOpen)
                Reader.OpenDataFile(FilePath);

            bool exists = Reader.CheckIfFieldExists(fieldName);

            Reader.Close();
            return exists;
        }
        public bool AddField(DataColumn col)
        {
            if (!Writer.IsOpen)
                Writer.OpenDataFile(FilePath);

            if (Writer.CheckIfFieldExists(col.ColumnName))
            {
                System.Diagnostics.Debug.Print($"Field '{col.ColumnName}' already exists, returning.");
                return true;
            }

            DBFFieldInfo fieldInfo = new DBFFieldInfo();
            fieldInfo.FieldName = col.ColumnName;
            if (col.DataType == typeof(string))
            {
                fieldInfo.FieldType = DBFFieldType.String;
                fieldInfo.FieldLength = 100;
            }
            else if (col.DataType == typeof(int))
            {
                fieldInfo.FieldType = DBFFieldType.Numeric;

            }
            else if (col.DataType == typeof(double))
            {
                fieldInfo.FieldType = DBFFieldType.Numeric;
                fieldInfo.FieldLength = 20;
                fieldInfo.NumberOfDecimalPlaces = 6;
            }
            else if (col.DataType == typeof(float))
            {
                fieldInfo.FieldType = DBFFieldType.Numeric;
                fieldInfo.FieldLength = 20;
                fieldInfo.NumberOfDecimalPlaces = 6;
            }
            else if (col.DataType == typeof(bool))
            {
                fieldInfo.FieldType = DBFFieldType.Boolean;
            }
            else if (col.DataType == typeof(DateTime))
            {
                fieldInfo.FieldType = DBFFieldType.DateTime;
            }


            bool success = Writer.AddField(fieldInfo);
            success = success && Writer.Flush();

            return success;
        }
        public DataTable AllValues()
        {
            if (!Reader.IsOpen)
                Reader.OpenDataFile(FilePath);

            DataTable data = Reader?.GetDataTable("ShpData");

            Reader?.Close();
            return data;
        }
        public bool AddDataTable(string keyFieldName, DataTable dataTable, IProgressIndicator pi)
        {
            bool fileOpened = false;

            if (!Writer.IsOpen)
                fileOpened = Writer.OpenDataFile(FilePath);

            if (!fileOpened)
                throw new InvalidOperationException($"Failed to open up the file '{FilePath}'");

            if (!Writer.CheckIfFieldExists(keyFieldName))
                throw new InvalidOperationException($"Given key field name '{keyFieldName}' does not exists.");

            pi.AddTask("Adding data to the shapfile...");
            pi.IncrementTask();
            pi.BeginTask(1);


            var cols = new List<string>();
            foreach (DataColumn col in dataTable.Columns)
            {
                cols.Add(col.ColumnName);
                AddField(col);
            }

            var success = WriteRows(keyFieldName, cols.ToArray(), dataTable);

            pi.IncrementStep();
            pi.EndTask();

            success = success && Writer.Flush();
            Writer.Close();
            return success;
        }
        public bool WriteRows(string commonField, string[] fieldsToWrite, DataTable dataTable)
        {
            if (!Writer.IsOpen)
                Writer.OpenDataFile(FilePath);

            bool success = Writer.WriteDataTable(
                commonField,
                fieldsToWrite,
                dataTable
                );

            success = success && Writer.Flush();

            Writer.Close();
            return success;
        }

        public void Dispose()
        {
            if (Writer != null && Writer.IsOpen)
                Writer.Close();

        }
        #endregion

        #region Public Properties
        public ShapefileDataSourceReader Reader
        {
            get
            {
                if (reader == null)
                    reader = new ShapefileDataSourceReader(false);
                return reader;
            }
        }
        public ShapefileDataSourceWriter Writer
        {
            get
            {
                if (writer == null) { writer = new ShapefileDataSourceWriter(); }
                return writer;
            }
        }

        #endregion

        #region Private Properties
        private IWaterModel WaterModel { get; }
        private string FilePath { get; }
        #endregion

        #region Fields
        private ShapefileDataSourceReader reader;
        private ShapefileDataSourceWriter writer;
        #endregion
    }
}
