using OFW.DmdToCM.Form;
using OpenFlows.Application;
using OpenFlows.Water;
using OpenFlows.Water.Application;
using System;

namespace OFW.DmdToCM
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main()
        {
            ApplicationManagerBase.SetApplicationManager(new WaterApplicationManager());
            WaterApplicationManager.GetInstance().SetParentFormSurrogateDelegate(
                new ParentFormSurrogateDelegate((fm) =>
                {
                    return new DemandToCMParentForm(fm);
                }));

            OpenFlowsWater.StartSession(WaterProductLicenseType.WaterGEMS);

            WaterApplicationManager.GetInstance().Start();
            WaterApplicationManager.GetInstance().Stop();

            OpenFlowsWater.EndSession();
            return 0;
        }
    }
}
