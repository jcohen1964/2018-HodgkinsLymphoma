using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL1
{
    public static class K
    {
        // Common list structures
        public enum ProbTimePair { PROB, YEARS };
        public enum ProbTimeTriple { START, STOP, PROB }

        // public static int YRS_TIME_PERIOD1 = 13;
        public static int NUM_PERSONS = 1000000;
        public static double DELTA_TIME = 1.0; // simulation time step in years
        public static double PROBTIMESTEP = 1.0; // Time period corresponding to transition probabilities below
        public static double timeStepOffsetAtDeath = 1;
        public static double tinyProb = 0.000001;

        public enum Tx { CHEMOONLY, CMT }; // List of treatments
        public static String[] treatmentNames = { "chemoonly", "cmt" };
        public enum RelapseHx { NEVER_RELAPSED, HAVE_RELAPSED};

        public enum Stage { A, B, C1, C2, C3, D };
        public static Stage deadStage = K.Stage.D; // Stage representing dead
        public static Stage initStage = K.Stage.A; // Initial stage
        public enum LateEffectSeverity { MILD, SEVERE};

        public static Dictionary<LateEffectSeverity, List<List<double>>> probC3toD_SMR;
        public enum probC3toD_SMR_offset { STARTYR, ENDYR, STARTSMR, ENDSMR };

        // Scenario number (0 to 3) governs which utility values are assigned to UtilityA, UtilityB, and
        // UtilityC.  UtilityC also depends on treatment type.
        public static int scenario = 0;

        public static double[] utilityA_Vals = new double[] { 0.77 }; // At risk state
        public static double[] utilityB_Vals = new double[] { 0.71 }; // Relapse state
        public static double[] utilityC1_Vals = new double[] { 0.74 }; // Cured state - with relapse
        public static double[] utilityC2_Vals = new double[] { 0.80 }; // Cured state - no relapse
        public static double[] utilityC3_Vals = new double[] { 0, 0 }; // Cured state - with late effects - dummy values
        public static double[] utilityD_Vals = new double[] { 0.00 }; // Dead

        // Pairs are specified as ChemoOnly and then CMT.
        // Each probability pair is (1) aggregate duration probability, (2) duration
        // Note - probAtoD_FromTx_Vals is mortality attributable to treatment only.  Background mortality
        // must be added to this mortality source to get total mortality.  See 
        public static List<double>[] probAtoB;
        public static List<double>[] probAtoD_FromTx;
        public static List<double>[] maxTimeInA;

        public enum probBtoD_dims { START, STOP, PROB};
        public static List<List<double>>[] probBtoD;
        public static List<double>[] maxTimeInB;

        public static List<List<double>>[] probC1toC3;
        public static List<List<double>>[] probC2toC3;

        public static double[,] probSeverityC3_Vals = new double[,] { { 0, 0 }, { 0, 0 } }; // dummy values
        public static double discountRate = 0.03;

        public static double[] backgroundNeutralDeathProb = new double[] {
                0.006123, 0.000428, 0.000275, 0.000211, 0.000158, 0.000145, 0.000128, 0.000114, 0.0001, 0.000087,
                0.000079, 0.000086, 0.000116, 0.000175, 0.000252, 0.000333, 0.000412, 0.000492, 0.000573, 0.000655,
                0.000744, 0.000829, 0.000892, 0.000925, 0.000934, 0.000936, 0.000943, 0.000953, 0.000971, 0.000998,
                0.001029, 0.001063, 0.001099, 0.001137, 0.001180, 0.001235, 0.001302, 0.001377, 0.001461, 0.001557,
                0.001663, 0.001793, 0.001962, 0.002177, 0.002423, 0.002676, 0.002931, 0.003205, 0.003505, 0.00383,
                0.004177, 0.004535, 0.004903, 0.005284, 0.005684, 0.006117, 0.006589, 0.007095, 0.007626, 0.00818,
                0.008767, 0.009397, 0.010085, 0.010863, 0.011758, 0.012810, 0.014011, 0.015290, 0.016601, 0.018005,
                0.019548, 0.021294, 0.023275, 0.025528, 0.028061, 0.030820, 0.033775, 0.037252, 0.041136, 0.045411,
                0.050146, 0.055445, 0.061272, 0.067764, 0.075818, 0.085319, 0.094975, 0.105525, 0.117007, 0.12945,
                0.142873, 0.157280, 0.172661, 0.188988, 0.206214, 0.224274, 0.243080, 0.262527, 0.282492, 0.302838,
                1
            };

        public static void setScenario(int scenario)
        {
            /////////////////////////////////////////////////////////////////////////////////////
            // Base case scenario
            /////////////////////////////////////////////////////////////////////////////////////

            maxTimeInA = new List<double>[(Enum.GetValues(typeof(K.Tx))).Length];
            maxTimeInA[(int)K.Tx.CHEMOONLY] = new List<double>() { 4 };
            maxTimeInA[(int)K.Tx.CMT] = new List<double>() { 4 };

            probAtoB = new List<double>[(Enum.GetValues(typeof(K.Tx))).Length];
            probAtoB[(int)K.Tx.CHEMOONLY] = new List<double>() { 0.09, 4 };
            probAtoB[(int)K.Tx.CMT] = new List<double>() { 0.03, 4 };

            probAtoD_FromTx = new List<double>[(Enum.GetValues(typeof(K.Tx))).Length];
            probAtoD_FromTx[(int)K.Tx.CHEMOONLY] = new List<double>() { 0.0022, 1 };
            probAtoD_FromTx[(int)K.Tx.CMT] = new List<double>() { 0.0022, 1 };

            probBtoD = new List<List<double>>[(Enum.GetValues(typeof(K.Tx))).Length];
            probBtoD[(int)K.Tx.CHEMOONLY] = new List<List<double>>();
            probBtoD[(int)K.Tx.CMT] = new List<List<double>>();

            probBtoD[(int)K.Tx.CHEMOONLY].Add(new List<double> { 0, 2, 0.15 });
            probBtoD[(int)K.Tx.CHEMOONLY].Add(new List<double> { 2, 5, 0.10 });

            probBtoD[(int)K.Tx.CMT].Add(new List<double> { 0, 2, 0.15 });
            probBtoD[(int)K.Tx.CMT].Add(new List<double> { 2, 5, 0.10 });

            maxTimeInB = new List<double>[(Enum.GetValues(typeof(K.Tx))).Length];
            maxTimeInB[(int)K.Tx.CHEMOONLY] = new List<double>() { 5 };
            maxTimeInB[(int)K.Tx.CMT] = new List<double>() { 5 };

            // Multi-year probability of late effect WITH RELAPSE (C1 to C3)
            // First entry is probability, while the second is the number of years
            probC1toC3 = new List<List<double>>[(Enum.GetValues(typeof(K.Tx))).Length];
            probC1toC3[(int)K.Tx.CHEMOONLY] = new List<List<double>>();
            probC1toC3[(int)K.Tx.CHEMOONLY].Add(new List<double> { 0, 10, 0.00 });
            probC1toC3[(int)K.Tx.CHEMOONLY].Add(new List<double> { 10, 45, 0.45 });
            probC1toC3[(int)K.Tx.CMT] = new List<List<double>>();
            probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
            probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.45 });

            probC2toC3 = new List<List<double>>[(Enum.GetValues(typeof(K.Tx))).Length];
            probC2toC3[(int)K.Tx.CHEMOONLY] = new List<List<double>>();
            probC2toC3[(int)K.Tx.CHEMOONLY].Add(new List<double> { 0, 10, 0.00 });
            probC2toC3[(int)K.Tx.CHEMOONLY].Add(new List<double> { 10, 45, 0.30 });
            probC2toC3[(int)K.Tx.CMT] = new List<List<double>>();
            probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
            probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.45 });

            // Probability that late effects are severe
            probSeverityC3_Vals[(int)Tx.CHEMOONLY, (int)RelapseHx.NEVER_RELAPSED] = 0.20;
            probSeverityC3_Vals[(int)Tx.CHEMOONLY, (int)RelapseHx.HAVE_RELAPSED] = 0.20;
            probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.NEVER_RELAPSED] = 0.20;
            probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.HAVE_RELAPSED] = 0.20;

            // SMR for late effects
            // Consists of a dictionary with one entry for each severity level in State C3.
            // Each entry is a list of lists.  Each sublist contains four values: (1) and (2) - range
            // of years since entering State C3, and (3) and (4) - corresponding start and stop 
            probC3toD_SMR = new Dictionary<LateEffectSeverity, List<List<double>>>(); // Create dictionary
            List<List<double>> myList = new List<List<double>>(); // Create dictionary SMR entry for MILD
            myList.Add(new List<double>() { 0, 1000, 1, 1 });
            probC3toD_SMR.Add(LateEffectSeverity.MILD, myList);

            myList = new List<List<double>>(); // create dictionary SMR entry for SEVERE
            myList.Add(new List<double>() { 0, 10, 1, 1 });
            myList.Add(new List<double>() { 10, 14, 15.8, 15.8 });
            myList.Add(new List<double>() { 14, 20, 15.8, 4.7 });
            myList.Add(new List<double>() { 20, 1000, 4.7, 4.7 });
            probC3toD_SMR.Add(LateEffectSeverity.SEVERE, myList);

            // Utility weights for late effects
            utilityC3_Vals[(int)LateEffectSeverity.MILD] = 0.73;
            utilityC3_Vals[(int)LateEffectSeverity.SEVERE] = 0.67;

            /// Case 0:  Patient 1.0 (base case)
            /// Case 11: Patient 1.1 (Proportion of late effects severe = 10% for chemo and CMT)
            /// Case 12: Patient 1.2 (Proportion of late effects severe = 5% for chemo and CMT)
            /// Case 20: Patient 2.0 (Late effects probability = 90% with CMT, with or without relapse,
            ///          C1 to C3, and C2 to C3 transitions)
            /// Case 21: Patient 2.1 (Same as Patient 2.0 plus: 40% of late effects are severe for CMT, 
            ///          with or without relapse.)
            /// Case 22: Patient 2.2 (Same as Patient 2.0 plus: 60% of late effects are severe for CMT, 
            ///          with or without relapse.)
            /// Case 23: Patient 2.3 (Same as Patient 2.0 plus: 60% of late effects are severe for CMT, 
            ///          with or without relapse.)

            switch (scenario)
            {
                case 0:
                    break;
                case 10:
                    break;
                case 11:
                    probSeverityC3_Vals[(int)Tx.CHEMOONLY, (int)RelapseHx.NEVER_RELAPSED] = 0.10;
                    probSeverityC3_Vals[(int)Tx.CHEMOONLY, (int)RelapseHx.HAVE_RELAPSED] = 0.10;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.NEVER_RELAPSED] = 0.10;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.HAVE_RELAPSED] = 0.10;
                    break;
                case 12:
                    probSeverityC3_Vals[(int)Tx.CHEMOONLY, (int)RelapseHx.NEVER_RELAPSED] = 0.05;
                    probSeverityC3_Vals[(int)Tx.CHEMOONLY, (int)RelapseHx.HAVE_RELAPSED] = 0.05;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.NEVER_RELAPSED] = 0.05;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.HAVE_RELAPSED] = 0.05;
                    break;

                case 20:
                    probC1toC3[(int)Tx.CMT].Clear();
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probC2toC3[(int)Tx.CMT].Clear();
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.NEVER_RELAPSED] = 0.20;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.HAVE_RELAPSED] = 0.20;
                    break;
                case 21:
                    probC1toC3[(int)Tx.CMT].Clear();
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probC2toC3[(int)Tx.CMT].Clear();
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.NEVER_RELAPSED] = 0.40;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.HAVE_RELAPSED] = 0.40;
                    break;
                case 22:
                    probC1toC3[(int)Tx.CMT].Clear();
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probC2toC3[(int)Tx.CMT].Clear();
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.NEVER_RELAPSED] = 0.60;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.HAVE_RELAPSED] = 0.60;
                    break;
                case 23:
                    probC1toC3[(int)Tx.CMT].Clear();
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC1toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probC2toC3[(int)Tx.CMT].Clear();
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 0, 10, 0.00 });
                    probC2toC3[(int)K.Tx.CMT].Add(new List<double> { 10, 45, 0.90 });
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.NEVER_RELAPSED] = 0.80;
                    probSeverityC3_Vals[(int)Tx.CMT, (int)RelapseHx.HAVE_RELAPSED] = 0.80;
                    break;
                default:
                    break;
            }
            return;
        }
    }
}
