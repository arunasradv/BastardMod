using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BastardsMod.Helpers
{
    public class BastardModConfiguration
    {
        public float PregnancyDurationInDays = 28.0f;
        public float BastardPregnancyBaseChance = 0.05f;
        public bool BastardPregnancyEnabledForPlayerHero = true;
        public float ResistPregnancyWithBastardValor = 0.0f;
        public float ResistPregnancyWithBastardHonor = 0.0f;
        public float ResistPregnancyWithBastardCalculating = 1.0f;
        public float ResistPregnancyIncestValor = -1.0f;
        public float ResistPregnancyIncestHonor = -2.0f;
        public float ResistPregnancyIncestCalculating = 1.0f;
        public float BastardPregnancyHeroRelationMin = 0.0f;
        public int BastardPregnancyMaxChildren = 5;
        public float DeliveringTwinsProbability = 0.01f;
        public float StillbirthProbability = 0.05f;
        public float DeliveringFemaleOffspringProbability = 0.50f;
        public float MaternalMortalityProbabilityInLabor = 0.01f;
        public string file_name = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "BastardModCfg.xml");
    }
}
