using MelonLoader;

[assembly: MelonInfo(typeof(StellarDrive_Tech.Core), "StellarDrive Tech", "1.0.0", "oneshade", null)]
[assembly: MelonGame("CuriousOwlGames", "StellarDrive")]

namespace StellarDrive_Tech
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
    }
}
