using Haestad.LicensingFacade;
using Haestad.Support.Support;
using NUnit.Framework;
using OpenFlows.Water;
using OpenFlows.Water.Domain;
using System.IO;
using System.Reflection;
using static OpenFlows.Water.OpenFlowsWater;

namespace OFW.DmdToCM.Test
{
    public abstract class OpenFlowsWaterTestFixtureBase
    {
        #region Constructor
        public OpenFlowsWaterTestFixtureBase()
        {

        }
        #endregion

        #region Setup/Tear-down
        [OneTimeSetUp]
        public void Setup()
        {
            if (!TestUnitPermission.IsTestUnitRunning())
                TestUnitPermission.Assert();

            StartSession(WaterProductLicenseType.WaterGEMS);

            if (!Directory.Exists(TempDir)) Directory.CreateDirectory(TempDir);
            Assert.IsTrue(Directory.Exists(TempDir));

            SetupImpl();
        }
        protected virtual void SetupImpl()
        {
        }
        [OneTimeTearDown]
        public void Teardown()
        {
            if (WaterModel != null)
                WaterModel.Dispose();
            WaterModel = null;
            
            TeardownImpl();

            EndSession();
            try
            {
                if (Directory.Exists(TempDir)) Directory.Delete(TempDir, true);
                Assert.IsFalse(Directory.Exists(TempDir));
            }
            catch{}

            if (TestUnitPermission.IsTestUnitRunning())
                TestUnitPermission.RevertAssert();

        }
        protected virtual void TeardownImpl()
        {
        }
        #endregion

        #region Protected Methods
        protected void OpenModel(string filename)
        {
            WaterModel = Open(filename);
        }
        protected virtual string BuildTestFilename(string baseFilename)
        {
            return Path.Combine(@"D:\Development\Data\ModelData", baseFilename);
        }
        protected string TempDir => tempDir ?? (tempDir = Path.Combine(Path.GetTempPath(), Assembly.GetCallingAssembly().GetName().Name));
        #endregion

        #region Protected Properties
        protected IWaterModel WaterModel { get; private set; }
        #endregion

        #region Fields
        string tempDir;
        #endregion
    }
}
