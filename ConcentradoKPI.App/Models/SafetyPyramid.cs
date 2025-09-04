namespace ConcentradoKPI.App.Models
{
    public class SafetyPyramid
    {
        public int UnsafeActs { get; set; }
        public int UnsafeConditions { get; set; }
        public int NearMisses { get; set; }
        public int FirstAids { get; set; }
        public int MedicalTreatments { get; set; }
        public int LostTimeInjuries { get; set; }
        public int Fatalities { get; set; }

        public int FindingsCorrected { get; set; }
        public int FindingsTotal { get; set; }

        public int TrainingsCompleted { get; set; }
        public int TrainingsPlanned { get; set; }
        public int AuditsCompleted { get; set; }
        public int AuditsPlanned { get; set; }
    }
}
